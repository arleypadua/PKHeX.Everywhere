import { lazy, useCallback, useEffect, type ReactNode } from 'react'
import { App, ConfigProvider, theme as antdTheme } from 'antd'
import { BrowserRouter, Navigate, Route, Routes, useParams, useSearchParams } from 'react-router'
import type { EngineError, PageLayout } from '@pkhex-everywhere/engine'
import { EngineProvider, useLoadedGame } from '@pkhex-everywhere/react'
import { engine, plugIns } from '../app'
import { useTheme } from '../host'
import { PlugInsProvider } from '../plugins/PlugInsContext'
import { routes } from '../routes'
import { EmptyPlugInLayout } from './EmptyPlugInLayout'
import { AutoLoadDemo } from './AutoLoadDemo'
import { GameJourney } from './GameJourney'
import { journey } from './journey'
import { LoadLayout } from './LoadLayout'
import { MainLayout } from './MainLayout'
import { PlugInOutcomes } from './PlugInOutcomes'

const AnalyticsPage = lazy(() => import('../pages/analytics/AnalyticsPage'))
const BoxPage = lazy(() => import('../pages/box/BoxPage'))
const CreditsPage = lazy(() => import('../pages/static/CreditsPage'))
const EncountersPage = lazy(() => import('../pages/encounters/EncountersPage'))
const EventsPage = lazy(() => import('../pages/events/EventsPage'))
const HomePage = lazy(() => import('../pages/home/HomePage'))
const ItemsPage = lazy(() => import('../pages/items/ItemsPage'))
const LoadPage = lazy(() => import('../pages/load/LoadPage'))
const PartyPage = lazy(() => import('../pages/party/PartyPage'))
const PlugInPage = lazy(() => import('../pages/plugins/PlugInPage'))
const PlugInsPage = lazy(() => import('../pages/plugins/PlugInsPage'))
const PlugInModulePage = lazy(() => import('../pages/plugins/PlugInModulePage'))
const PlugInErrorsPage = lazy(() => import('../pages/plugins/PlugInErrorsPage'))
const PokemonClonePage = lazy(() => import('../pages/pokemon-editor/PokemonClonePage'))
const PokemonEditorPage = lazy(() => import('../pages/pokemon-editor/PokemonEditorPage'))
const PrivacyPolicyPage = lazy(() => import('../pages/static/PrivacyPolicyPage'))
const ReleaseNotesPage = lazy(() => import('../pages/release-notes/ReleaseNotesPage'))
const SettingsPage = lazy(() => import('../pages/settings/SettingsPage'))
const TermsOfUsePage = lazy(() => import('../pages/static/TermsOfUsePage'))

const fontFamily = `'Pokemon GB', "Lucida Console", sans-serif`

function useTitle(title: string | undefined) {
  useEffect(() => {
    if (!title) return
    const previous = document.title
    document.title = title
    return () => {
      document.title = previous
    }
  }, [title])
}

function Page({ title, requiresSave, children }: { title?: string; requiresSave?: boolean; children: ReactNode }) {
  useTitle(title)
  return requiresSave ? <RequireSave>{children}</RequireSave> : children
}

function RequireSave({ children }: { children: ReactNode }) {
  const { game } = useLoadedGame()
  return game ? children : null
}

const integer = (value: string | null) => (value && /^-?\d+$/.test(value) ? Number(value) : null)

function HomeRoute() {
  const { game } = useLoadedGame()
  const redirect = journey.redirectsToLoad(game !== null)
  useEffect(() => journey.reachedHome(), [])
  return redirect ? <Navigate to={routes.load} replace /> : <HomePage />
}

function LoadRoute({ autoLoad }: { autoLoad: boolean }) {
  const { game } = useLoadedGame()
  if (game) return <Navigate to={routes.home} replace />
  return (
    <>
      {autoLoad && <AutoLoadDemo />}
      <LoadPage />
    </>
  )
}

function EncountersRoute() {
  const { game } = useLoadedGame()
  const [query] = useSearchParams()
  if (!game) return <Navigate to={routes.load} replace />
  return <EncountersPage version={integer(query.get('version'))} species={integer(query.get('species'))} />
}

function ReleaseNotesRoute() {
  const [query] = useSearchParams()
  return <ReleaseNotesPage since={query.get('since') || null} />
}

function PokemonEditorRoute() {
  const { source, id } = useParams()
  return <PokemonEditorPage source={source} id={id} />
}

function PokemonCloneRoute() {
  const { source, id } = useParams()
  return <PokemonClonePage source={source} id={id} />
}

function PlugInRoute() {
  const { id } = useParams()
  return <PlugInPage id={id} />
}

function PlugInModuleRoute({ layout }: { layout: PageLayout }) {
  const { plugInId, path } = useParams()
  return <PlugInModulePage key={`${plugInId}/${path}`} plugInId={plugInId} path={path} layout={layout} />
}

function AppRoutes({ autoLoad }: { autoLoad: boolean }) {
  return (
    <Routes>
      <Route element={<MainLayout />}>
        <Route index element={<Page title="Home"><HomeRoute /></Page>} />
        <Route path="party" element={<Page title="Party" requiresSave><PartyPage /></Page>} />
        <Route path="pokemon-box" element={<Page title="Pokemon Box" requiresSave><BoxPage /></Page>} />
        <Route path="items" element={<Page title="Items" requiresSave><ItemsPage /></Page>} />
        <Route path="events" element={<Page title="Events" requiresSave><EventsPage /></Page>} />
        <Route path="pokemon/search-encounter" element={<Page title="Encounters"><EncountersRoute /></Page>} />
        <Route path="pokemon/:source/:id" element={<Page title="Pokemon" requiresSave><PokemonEditorRoute /></Page>} />
        <Route path="pokemon/:source/:id/clone" element={<Page title="Clone Pokemon" requiresSave><PokemonCloneRoute /></Page>} />
        <Route path="plugins" element={<PlugInsPage />} />
        <Route path="plugins/errors" element={<PlugInErrorsPage />} />
        <Route path="plugins/:id" element={<PlugInRoute />} />
        <Route path="plugins/:plugInId/:path" element={<PlugInModuleRoute layout="standard" />} />
        <Route path="plugins/:plugInId/:path/standard" element={<PlugInModuleRoute layout="standard" />} />
        <Route path="analytics" element={<AnalyticsPage />} />
        <Route path="save" element={<Page title="Save"><LoadPage /></Page>} />
        <Route path="settings" element={<Page title="Settings"><SettingsPage /></Page>} />
        <Route path="credits" element={<Page title="Credits"><CreditsPage /></Page>} />
        <Route path="privacy-policy" element={<Page title="Privacy Policy"><PrivacyPolicyPage /></Page>} />
        <Route path="terms-of-use" element={<Page title="Terms of Use"><TermsOfUsePage /></Page>} />
        <Route path="release-notes" element={<Page title="Release Notes"><ReleaseNotesRoute /></Page>} />
        <Route path="*" element={<Page title="Not found"><p role="alert">Sorry, there's nothing at this address.</p></Page>} />
      </Route>
      <Route element={<LoadLayout />}>
        <Route path="load" element={<LoadRoute autoLoad={autoLoad} />} />
      </Route>
      <Route element={<EmptyPlugInLayout />}>
        <Route path="plugins/:plugInId/:path/empty" element={<PlugInModuleRoute layout="empty" />} />
      </Route>
    </Routes>
  )
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

export function AppShell({ autoLoad }: { autoLoad: boolean }) {
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
            <BrowserRouter>
              <GameJourney />
              <PlugInOutcomes />
              <AppRoutes autoLoad={autoLoad} />
            </BrowserRouter>
          </PlugInsProvider>
        </NotifyingEngineProvider>
      </App>
    </ConfigProvider>
  )
}
