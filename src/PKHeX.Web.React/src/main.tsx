import { createRoot } from 'react-dom/client'
import { engine } from './app'
import { startGoogleAnalytics } from './googleAnalytics'
import { AppShell } from './layout/AppShell'
import { watchEngine } from './sentry'

startGoogleAnalytics(engine)
watchEngine(engine)

export function start({ autoLoad }: { autoLoad: boolean }) {
  createRoot(document.getElementById('root')!).render(<AppShell autoLoad={autoLoad} />)
}
