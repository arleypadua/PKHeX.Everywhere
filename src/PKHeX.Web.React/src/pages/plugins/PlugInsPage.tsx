import { useEffect, useState, type ReactNode } from 'react'
import { Alert, App, Button, Flex, Space, Table, Tag, Tooltip, Typography, type TableColumnsType } from 'antd'
import { Link } from 'react-router'
import type { InstalledPlugIn } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'
import { containsText } from '../../components/filters/containsText'
import { TextFilter } from '../../components/filters/TextFilter'
import { useNavigate } from '../../host'
import type { AvailablePlugIn } from '../../plugins/plugIns'
import { usePlugIns } from '../../plugins/PlugInsContext'
import { routes } from '../../routes'

function nameColumn<T extends { name: string }>(render: (plugIn: T) => ReactNode): TableColumnsType<T>[number] {
  return {
    title: 'Name',
    key: 'name',
    sorter: (a, b) => a.name.localeCompare(b.name),
    filterDropdown: (props) => <TextFilter {...props} placeholder="Name" />,
    onFilter: (value, plugIn) => containsText(plugIn.name, value),
    render: (_, plugIn) => render(plugIn),
  }
}

export default function PlugInsPage() {
  const plugIns = usePlugIns()
  const installed = useQuery('plugins.installed')
  const navigate = useNavigate()
  const { notification } = App.useApp()
  const [available, setAvailable] = useState<AvailablePlugIn[] | 'failed'>([])
  const [busy, setBusy] = useState<string>()

  useEffect(() => {
    let active = true
    plugIns.available().then(
      (loaded) => active && setAvailable(loaded),
      (error) => {
        console.error("Couldn't list the plug-ins of the sources.", error)
        if (active) setAvailable('failed')
      },
    )
    return () => {
      active = false
    }
  }, [plugIns, installed])

  async function whileBusy<T>(id: string, failure: string, call: () => Promise<T>): Promise<T | undefined> {
    setBusy(id)
    try {
      return await call()
    } catch (error) {
      console.error(error)
      notification.error({ title: failure, description: 'Try again later.' })
      return undefined
    } finally {
      setBusy(undefined)
    }
  }

  const reachingTheSource = <T,>(id: string, call: () => Promise<T>) => whileBusy(id, "Couldn't reach the plug-in source", call)

  async function install(plugIn: AvailablePlugIn) {
    const id = await reachingTheSource(plugIn.id, () => plugIns.install(plugIn.sourceUrl, plugIn.id))
    if (!id) return
    notification.success({ title: 'Plug-in installed' })
    await navigate(routes.plugIn(id))
  }

  async function update(id: string) {
    if (await reachingTheSource(id, () => plugIns.update(id))) notification.success({ title: 'Plug-in updated' })
  }

  const uninstall = (id: string) => whileBusy(id, "Couldn't uninstall the plug-in", () => plugIns.uninstall(id))

  const installedColumns: TableColumnsType<InstalledPlugIn> = [
    nameColumn((plugIn) => (plugIn.needsReinstall ? plugIn.id : <Link to={routes.plugIn(plugIn.id)}>{plugIn.name}</Link>)),
    {
      title: 'Version',
      key: 'version',
      render: (_, plugIn) =>
        plugIn.needsReinstall ? (
          <Tooltip title="This plug-in doesn't work with this version of the app. Uninstall it and install it again.">
            <Tag color="warning">Needs reinstall</Tag>
          </Tooltip>
        ) : (
          <Space>
            {plugIn.version}
            {plugIn.hasNewerVersion && <Tag color="processing">Update available</Tag>}
          </Space>
        ),
    },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, plugIn) => (
        <Space>
          {plugIn.hasNewerVersion && !plugIn.needsReinstall && (
            <Button type="link" disabled={!!busy} loading={busy === plugIn.id} onClick={() => void update(plugIn.id)}>
              Update
            </Button>
          )}
          <Button type="link" danger disabled={!!busy} onClick={() => void uninstall(plugIn.id)}>
            Uninstall
          </Button>
        </Space>
      ),
    },
  ]

  const availableColumns: TableColumnsType<AvailablePlugIn> = [
    nameColumn((plugIn) =>
      plugIn.projectUrl ? (
        <a href={plugIn.projectUrl} target="_blank" rel="noopener noreferrer">
          {plugIn.name}
        </a>
      ) : (
        plugIn.name
      ),
    ),
    { title: 'Description', dataIndex: 'description' },
    { title: 'Version', dataIndex: 'version' },
    { title: 'Source', dataIndex: 'sourceName', sorter: (a, b) => a.sourceName.localeCompare(b.sourceName) },
    {
      title: 'Actions',
      key: 'actions',
      render: (_, plugIn) => (
        <Button type="link" disabled={!!busy} loading={busy === plugIn.id} onClick={() => void install(plugIn)}>
          Install
        </Button>
      ),
    },
  ]

  return (
    <Flex vertical gap={20}>
      <PageHeader title="Manage Plug-Ins" />
      {installed.length > 0 && (
        <>
          <Typography.Title level={5}>Installed</Typography.Title>
          <Table
            rowKey="id"
            dataSource={installed}
            columns={installedColumns}
            size="small"
            scroll={{ x: 'max-content' }}
            pagination={{ hideOnSinglePage: true }}
          />
        </>
      )}
      {available === 'failed' ? (
        <Alert type="warning" title="Couldn't reach the plug-in sources. Try again later." showIcon />
      ) : (
        available.length > 0 && (
          <>
            <Typography.Title level={5}>Discover</Typography.Title>
            <Table
              rowKey={(plugIn) => `${plugIn.sourceUrl}#${plugIn.id}`}
              dataSource={available}
              columns={availableColumns}
              size="small"
              scroll={{ x: 'max-content' }}
              pagination={{ hideOnSinglePage: true }}
            />
          </>
        )
      )}
    </Flex>
  )
}
