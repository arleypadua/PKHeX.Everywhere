import { Suspense, useCallback, type ReactNode } from 'react'
import { createRoot, type Root } from 'react-dom/client'
import { ConfigProvider, notification, Spin, theme as antdTheme } from 'antd'
import { blazorHost, createEngine, type EngineError } from '@pkhex-everywhere/engine'
import { EngineProvider } from '@pkhex-everywhere/react'
import { connectHost, useTheme, type HostBridge } from './host'
import { PageErrorBoundary } from './PageErrorBoundary'
import { pages } from './pages'

export { setTheme } from './host'

const engine = createEngine({ host: blazorHost() })
const roots = new WeakMap<HTMLElement, Root>()
const unmounted = new WeakSet<HTMLElement>()

const fontFamily = `'Pokemon GB', "Lucida Console", sans-serif`

function Page({ name, props }: { name: string; props: Record<string, unknown> }) {
  const Component = pages[name]
  if (!Component) throw new Error(`No React page named "${name}".`)
  return <Component {...props} />
}

function NotifyingEngineProvider({ children }: { children: ReactNode }) {
  const [api, contextHolder] = notification.useNotification()
  const onUnhandledError = useCallback(
    (error: EngineError) => api.error({ title: 'Something went wrong', description: error.message }),
    [api],
  )
  return (
    <EngineProvider engine={engine} onUnhandledError={onUnhandledError}>
      {contextHolder}
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
      <NotifyingEngineProvider>
        <PageErrorBoundary engine={engine}>
          <Suspense fallback={<Spin />}>
            <Page name={name} props={props} />
          </Suspense>
        </PageErrorBoundary>
      </NotifyingEngineProvider>
    </ConfigProvider>
  )
}

export async function mount(element: HTMLElement, name: string, props: Record<string, unknown>, host: HostBridge) {
  unmounted.delete(element)
  connectHost(host)
  await engine.ready
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
