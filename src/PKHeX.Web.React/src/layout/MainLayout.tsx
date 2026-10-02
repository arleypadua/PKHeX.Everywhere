import { Suspense, type ReactNode } from 'react'
import {
  ApiOutlined,
  FlagOutlined,
  HomeOutlined,
  InboxOutlined,
  LineChartOutlined,
  SaveOutlined,
  SettingOutlined,
  ShopOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import { Button, Layout, Menu, type MenuProps } from 'antd'
import { Link, useLocation } from 'react-router'
import { useLoadedGame, useQuery } from '@pkhex-everywhere/react'
import { useNavigate } from '../host'
import { routes } from '../routes'
import { Footer } from './Footer'
import { RoutedContent } from './RoutedContent'
import { menuEntries, type MenuEntry, type MenuIcon } from './menu'
import { NewsBanner } from './NewsBanner'

const icons: Record<MenuIcon, ReactNode> = {
  home: <HomeOutlined />,
  team: <TeamOutlined />,
  inbox: <InboxOutlined />,
  shop: <ShopOutlined />,
  flag: <FlagOutlined />,
  api: <ApiOutlined />,
  'line-chart': <LineChartOutlined />,
  save: <SaveOutlined />,
}

type MenuItem = Required<MenuProps>['items'][number]

function toMenuItem({ route, label, icon, children }: MenuEntry): MenuItem {
  if (children) return { key: `${route}#group`, icon: icon && icons[icon], label, children: children.map(toMenuItem) }
  return { key: route, icon: icon && icons[icon], label: <Link to={route}>{label}</Link> }
}

function SideMenu() {
  const { pathname } = useLocation()
  const { game } = useLoadedGame()
  const installed = useQuery('plugins.installed')

  return (
    <Menu theme="dark" mode="inline" selectedKeys={[pathname]} items={menuEntries(game, installed).map(toMenuItem)} />
  )
}

export function MainLayout() {
  const { pathname } = useLocation()
  const navigate = useNavigate()

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <NewsBanner />
      <Layout.Sider collapsible breakpoint="lg" collapsedWidth={64}>
        <div style={{ color: 'white', marginTop: 10, padding: 16, textAlign: 'center' }}>PKHeX.Web</div>
        <Suspense fallback={null}>
          <SideMenu />
        </Suspense>
      </Layout.Sider>
      <Layout>
        <Layout.Header style={{ display: 'flex', alignItems: 'center', width: '100%' }}>
          <div style={{ flexGrow: 1 }} />
          <Button
            icon={<SettingOutlined />}
            size="large"
            shape="round"
            aria-label="Settings"
            onClick={() => navigate(routes.settings)}
          />
        </Layout.Header>
        <Layout.Content style={{ margin: '24px 16px 0' }}>
          <div style={{ padding: 24, minHeight: 360 }}>
            <RoutedContent key={pathname} />
          </div>
        </Layout.Content>
        <Footer />
      </Layout>
    </Layout>
  )
}
