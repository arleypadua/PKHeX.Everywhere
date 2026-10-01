import { useEffect, useRef, useState } from 'react'
import { Link, Route, Routes, useLocation, useNavigate } from 'react-router-dom'
import { App as AntApp, Button, ConfigProvider, Layout, Menu, Space, Table, Tag, Typography, Upload } from 'antd'
import { BlazorIsland } from './BlazorIsland'
import { boot, engine, onBlazorNavigate, subscribe, type PartySlotDto, type SaveSummaryDto } from './engine'

const prefixOff = new URLSearchParams(location.search).get('prefix') === 'off'

function useSave() {
  const [save, setSave] = useState<SaveSummaryDto>()
  useEffect(() => {
    const refresh = () => engine.getSave().then((r) => setSave(r.ok ? r.value : undefined))
    refresh()
    return subscribe(refresh)
  }, [])
  return save
}

function LoadPage() {
  const { message } = AntApp.useApp()
  const load = async (bytes: Uint8Array, name: string) => {
    const result = await engine.loadSave(bytes, name)
    if (result.ok) message.success(`Loaded ${result.value.trainer} (${result.value.version})`)
    else message.error(`${result.error.code}: ${result.error.message}`)
  }
  return (
    <Space direction="vertical">
      <Typography.Title level={3}>Load a save</Typography.Title>
      <Upload
        beforeUpload={async (file) => {
          await load(new Uint8Array(await file.arrayBuffer()), file.name)
          return false
        }}
        showUploadList={false}
      >
        <Button type="primary">Choose save file</Button>
      </Upload>
      <Button
        data-testid="load-demo"
        onClick={async () => load(new Uint8Array(await (await fetch('data/emerald.sav')).arrayBuffer()), 'emerald.sav')}
      >
        Load demo save (emerald.sav)
      </Button>
    </Space>
  )
}

function ReactParty() {
  const [party, setParty] = useState<PartySlotDto[]>([])
  const [error, setError] = useState<string>()
  useEffect(() => {
    const refresh = () =>
      engine.getParty().then((r) => {
        if (r.ok) {
          setParty(r.value)
          setError(undefined)
        } else setError(r.error.message)
      })
    refresh()
    return subscribe(refresh)
  }, [])
  if (error) return <Typography.Text type="warning">{error}</Typography.Text>
  return (
    <Table
      rowKey="slot"
      dataSource={party}
      pagination={false}
      columns={[
        { title: 'Species', dataIndex: 'species' },
        { title: 'Nickname', dataIndex: 'nickname' },
        { title: 'Level', dataIndex: 'level' },
        { title: 'Shiny', dataIndex: 'isShiny', render: (s: boolean) => (s ? <Tag color="gold">Shiny</Tag> : null) },
        { title: 'Held item', dataIndex: 'heldItem' },
      ]}
    />
  )
}

function SideBySide() {
  return (
    <Space direction="vertical" style={{ width: '100%' }}>
      <Typography.Title level={4}>React (engine.getParty)</Typography.Title>
      <Space>
        <Button type="primary">React primary</Button>
        <Button>React default</Button>
        <Tag color="blue">React tag</Tag>
      </Space>
      <ReactParty />
      <Typography.Title level={4}>Blazor island (Party.razor)</Typography.Title>
      <BlazorIsland page="party" />
    </Space>
  )
}

const pages = [
  { key: '/', label: 'Load', element: <LoadPage /> },
  { key: '/party-react', label: 'Party (React)', element: <ReactParty /> },
  { key: '/party-blazor', label: 'Party (Blazor island)', element: <BlazorIsland page="party" /> },
  { key: '/items-blazor', label: 'Items (Blazor island)', element: <BlazorIsland page="items" /> },
  { key: '/side-by-side', label: 'Side by side', element: <SideBySide /> },
]

function useNavigationBridge() {
  const location = useLocation()
  const navigate = useNavigate()
  const current = location.pathname + location.search
  const routed = useRef(current)
  routed.current = current

  useEffect(() => {
    onBlazorNavigate((path) => {
      if (path !== routed.current) navigate(path, { replace: true })
    })
  }, [navigate])

  useEffect(() => {
    boot().then(() => window.Blazor.navigateTo(current, { replaceHistoryEntry: true }))
  }, [current])
}

export function App() {
  const location = useLocation()
  useNavigationBridge()
  const save = useSave()
  return (
    <ConfigProvider prefixCls={prefixOff ? undefined : 'rx'} iconPrefixCls={prefixOff ? undefined : 'rxicon'}>
      <AntApp>
        <Layout style={{ minHeight: '100vh' }}>
          <Layout.Sider theme="light">
            <Menu
              selectedKeys={[location.pathname]}
              items={pages.map((p) => ({ key: p.key, label: <Link to={p.key}>{p.label}</Link> }))}
            />
          </Layout.Sider>
          <Layout>
            <Layout.Header style={{ background: 'transparent' }}>
              <span data-testid="save-summary">
                {save ? `${save.trainer} · ${save.version} · ${save.fileName}` : 'No save loaded'}
              </span>
            </Layout.Header>
            <Layout.Content style={{ padding: 24 }}>
              <Routes>
                {pages.map((p) => (
                  <Route key={p.key} path={p.key} element={p.element} />
                ))}
                <Route path="*" element={<BlazorIsland page="router" />} />
              </Routes>
            </Layout.Content>
          </Layout>
        </Layout>
      </AntApp>
    </ConfigProvider>
  )
}
