import { Suspense, useCallback, type ReactNode } from 'react'
import { createRoot, type Root } from 'react-dom/client'
import { App, ConfigProvider, Spin, theme as antdTheme } from 'antd'
import { blazorHost, createEngine, type EngineError } from '@pkhex-everywhere/engine'
import { EngineProvider } from '@pkhex-everywhere/react'
import { startGoogleAnalytics } from './googleAnalytics'
import { connectHost, settings, useTheme, type HostBridge } from './host'
import { checkUnseenNews as checkUnseenNewsIn, markNewsSeen as markNewsSeenIn } from './news'
import { PageErrorBoundary } from './PageErrorBoundary'
import { pages } from './pages'
import { createPlugIns } from './plugins/plugIns'
import { PlugInsProvider } from './plugins/PlugInsContext'
import { createPlugInStore } from './plugins/store'
import { watchEngine } from './sentry'

export { getTheme, onThemeChanged } from './host'
export { track } from './googleAnalytics'
export { captureBlazorError } from './sentry'

export const checkUnseenNews = () => checkUnseenNewsIn(settings, new Date())
export const markNewsSeen = () => markNewsSeenIn(settings)

const engine = createEngine({ host: blazorHost() })

startGoogleAnalytics(engine)
watchEngine(engine)

const plugIns = createPlugIns(engine, createPlugInStore())

const plugInsLoaded = engine.ready.then(async () => {
  await plugIns.registerStored().catch((error) => console.error("Couldn't load plug-ins.", error))
  plugIns.refresh().catch((error) => console.error("Couldn't refresh plug-ins.", error))
})

const roots = new WeakMap<HTMLElement, Root>()
const unmounted = new WeakSet<HTMLElement>()

const fontFamily = `'Pokemon GB', "Lucida Console", sans-serif`

function Page({ name, props }: { name: string; props: Record<string, unknown> }) {
  const Component = pages[name]
  if (!Component) throw new Error(`No React page named "${name}".`)
  return <Component {...props} />
}

function NotifyingEngineProvider({ children }: { children: ReactNode }) {
  const { notification } = App.useApp()
  const onUnhandledError = useCallback(
    (error: EngineError) => notification.error({ title: 'Something went wrong', description: error.message }),
    [notification],
  )
  return (
    <EngineProvider engine={engine} onUnhandledError={onUnhandledError}>
      {children}
    </EngineProvider>
  )
}

function PageShell({ name, props }: { name: string; props: Record<string, unknown> }) {
  const theme = useTheme()
  return (
    <ConfigProvider
      prefixCls="rx"
      iconPrefixCls="rxicon"
      theme={{
        algorithm: theme === 'dark' ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm,
        token: { fontFamily },
      }}
    >
      <App component={false}>
        <NotifyingEngineProvider>
          <PlugInsProvider value={plugIns}>
            <PageErrorBoundary engine={engine}>
              <Suspense fallback={<Spin />}>
                <Page name={name} props={props} />
              </Suspense>
            </PageErrorBoundary>
          </PlugInsProvider>
        </NotifyingEngineProvider>
      </App>
    </ConfigProvider>
  )
}

export async function mount(element: HTMLElement, name: string, props: Record<string, unknown>, host: HostBridge) {
  unmounted.delete(element)
  connectHost(host)
  await plugInsLoaded
  if (unmounted.has(element)) return

  // A new <ReactPage> brings a new element, so a new root and an empty cache. This is the migration backstop for Blazor writes that report no topics.
  const root = roots.get(element) ?? createRoot(element)
  roots.set(element, root)
  root.render(<PageShell name={name} props={props} />)
}

export function unmount(element: HTMLElement) {
  unmounted.add(element)
  roots.get(element)?.unmount()
  roots.delete(element)
}
