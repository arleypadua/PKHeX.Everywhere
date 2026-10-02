import { useState } from 'react'
import { Alert, App, Button, Descriptions, Flex, Result, Space, Switch, Table, Tag, type TableColumnsType } from 'antd'
import { Link } from 'react-router'
import type { PlugInDetails, PlugInHook, PlugInSetting } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'
import { containsText } from '../../components/filters/containsText'
import { TextFilter } from '../../components/filters/TextFilter'
import { usePlugIns } from '../../plugins/PlugInsContext'
import { routes } from '../../routes'
import { PlugInSettingInput } from './PlugInSettingInput'

interface PlugInPageProps {
  id?: string
}

export default function PlugInPage({ id }: PlugInPageProps) {
  const installed = useQuery('plugins.installed').find((plugIn) => plugIn.id === id)

  if (!id || !installed || installed.needsReinstall)
    return (
      <Result
        status="404"
        title="Plug-in not installed"
        extra={
          <Link to={routes.plugIns}>
            <Button type="primary">Manage plug-ins</Button>
          </Link>
        }
      />
    )

  return <InstalledPlugIn id={id} />
}

function InstalledPlugIn({ id }: { id: string }) {
  const plugIn = useQuery('plugins.details', id)
  const plugIns = usePlugIns()
  const { notification } = App.useApp()
  const [updating, setUpdating] = useState(false)

  async function save(call: () => Promise<void>) {
    try {
      await call()
    } catch (error) {
      console.error(error)
      notification.error({ title: "Couldn't save the plug-in", description: error instanceof Error ? error.message : undefined })
    }
  }

  async function update() {
    setUpdating(true)
    try {
      if (await plugIns.update(id)) notification.success({ title: 'Plug-in updated' })
    } catch (error) {
      console.error(error)
      notification.error({ title: "Couldn't reach the plug-in source", description: 'Try again later.' })
    } finally {
      setUpdating(false)
    }
  }

  const changeSetting = (setting: PlugInSetting) => save(() => plugIns.updateSetting(id, setting))

  const hookColumns: TableColumnsType<PlugInHook> = [
    {
      title: 'Description',
      dataIndex: 'description',
      sorter: (a, b) => a.description.localeCompare(b.description),
      filterDropdown: (props) => <TextFilter {...props} placeholder="Description" />,
      onFilter: (value, hook) => containsText(hook.description, value),
    },
    {
      title: 'Enabled',
      key: 'enabled',
      render: (_, hook) => (
        <Switch
          checked={hook.enabled}
          disabled={!plugIn.enabled}
          onChange={(enabled) => void save(() => plugIns.setHookEnabled(id, hook.id, enabled))}
        />
      ),
    },
  ]

  return (
    <Flex vertical gap={20}>
      <PageHeader
        title={plugIn.name}
        extra={
          plugIn.hasNewerVersion && (
            <Button type="primary" loading={updating} onClick={() => void update()}>
              Update
            </Button>
          )
        }
      />
      <Descriptions
        bordered
        size="small"
        column={1}
        items={[
          { key: 'name', label: 'Name', children: <ProjectLink plugIn={plugIn} /> },
          { key: 'description', label: 'Description', children: plugIn.description },
          {
            key: 'version',
            label: 'Version',
            children: (
              <Space>
                {plugIn.version}
                {plugIn.hasNewerVersion && <Tag color="processing">Update available</Tag>}
              </Space>
            ),
          },
          {
            key: 'enabled',
            label: 'Enabled',
            children: <Switch checked={plugIn.enabled} onChange={(enabled) => void save(() => plugIns.setEnabled(id, enabled))} />,
          },
          ...(plugIn.publicKeyToken ? [{ key: 'publicKey', label: 'Public Key', children: plugIn.publicKeyToken }] : []),
          ...plugIn.settings.map((setting) => ({
            key: `setting:${setting.key}`,
            label: setting.key,
            children: <PlugInSettingInput setting={setting} onChange={changeSetting} />,
          })),
        ]}
      />
      <Table
        rowKey="id"
        dataSource={plugIn.hooks}
        columns={hookColumns}
        size="small"
        scroll={{ x: 'max-content' }}
        pagination={{ hideOnSinglePage: true }}
      />
      {plugIn.information?.trim() && <Alert type="info" title={plugIn.information} showIcon />}
    </Flex>
  )
}

function ProjectLink({ plugIn }: { plugIn: PlugInDetails }) {
  if (!plugIn.projectUrl) return plugIn.name
  return (
    <a href={plugIn.projectUrl} target="_blank" rel="noopener noreferrer">
      {plugIn.name}
    </a>
  )
}
