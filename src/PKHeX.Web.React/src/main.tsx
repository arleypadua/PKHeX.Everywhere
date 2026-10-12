import { createRoot } from 'react-dom/client'
import { engine } from './app'
import { startGoogleAnalytics } from './googleAnalytics'
import { settings } from './host'
import { AppShell } from './layout/AppShell'
import { applyPkhemuPreviewParam } from './pages/load/pkhemuInvite'
import { watchEngine } from './sentry'

startGoogleAnalytics(engine)
watchEngine(engine)
applyPkhemuPreviewParam(location.search, settings)

export function start({ autoLoad }: { autoLoad: boolean }) {
  createRoot(document.getElementById('root')!).render(<AppShell autoLoad={autoLoad} />)
}
