import { Suspense } from 'react'
import { createRoot, type Root } from 'react-dom/client'
import { ConfigProvider, Spin, theme as antdTheme } from 'antd'
import { blazorHost, createEngine } from '@pkhex-everywhere/engine'
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
      <EngineProvider engine={engine}>
        <PageErrorBoundary>
          <Suspense fallback={<Spin />}>
            <Page name={name} props={props} />
          </Suspense>
        </PageErrorBoundary>
      </EngineProvider>
    </ConfigProvider>
  )
}

export async function mount(element: HTMLElement, name: string, props: Record<string, unknown>, host: HostBridge) {
  unmounted.delete(element)
  connectHost(host)
  await engine.ready
  if (unmounted.has(element)) return

  // Each mounted page gets a new root, so a new EngineProvider with an empty cache. This backs up Blazor writes that report no topics.
  const root = roots.get(element) ?? createRoot(element)
  roots.set(element, root)
  root.render(<PageShell name={name} props={props} />)
}

export function unmount(element: HTMLElement) {
  unmounted.add(element)
  roots.get(element)?.unmount()
  roots.delete(element)
}
