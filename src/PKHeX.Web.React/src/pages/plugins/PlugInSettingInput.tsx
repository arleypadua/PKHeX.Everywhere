import { useRef, type ChangeEvent } from 'react'
import { CloseOutlined } from '@ant-design/icons'
import { App, Button, Input, InputNumber, Space, Switch } from 'antd'
import type { PlugInSetting } from '@pkhex-everywhere/engine'
import { toBase64 } from '../../base64'

const maxFileSize = 24 * 1024 * 1000

interface PlugInSettingInputProps {
  setting: PlugInSetting
  onChange: (setting: PlugInSetting) => Promise<void>
}

export function PlugInSettingInput({ setting, onChange }: PlugInSettingInputProps) {
  if (setting.fileName !== null) return <FileSettingInput setting={setting} onChange={onChange} />

  if (setting.integerValue !== null)
    return (
      <InputNumber
        key={setting.integerValue}
        defaultValue={setting.integerValue}
        precision={0}
        disabled={setting.readOnly}
        onBlur={(event) => {
          const value = Number.parseInt(event.target.value, 10)
          if (Number.isInteger(value) && value !== setting.integerValue) void onChange({ ...setting, integerValue: value })
        }}
      />
    )

  if (setting.booleanValue !== null)
    return (
      <Switch
        checked={setting.booleanValue}
        disabled={setting.readOnly}
        onChange={(value) => void onChange({ ...setting, booleanValue: value })}
      />
    )

  return (
    <Input
      key={setting.stringValue}
      defaultValue={setting.stringValue ?? ''}
      readOnly={setting.readOnly}
      onBlur={(event) => {
        if (!setting.readOnly && event.target.value !== setting.stringValue) void onChange({ ...setting, stringValue: event.target.value })
      }}
      onPressEnter={(event) => event.currentTarget.blur()}
    />
  )
}

function FileSettingInput({ setting, onChange }: PlugInSettingInputProps) {
  const fileInput = useRef<HTMLInputElement>(null)
  const { notification } = App.useApp()

  const upload = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (!file) return
    if (file.size > maxFileSize) {
      notification.error({ title: 'File too large', description: 'Files can be up to 24 MB.' })
      return
    }
    await onChange({ ...setting, fileName: file.name, file: toBase64(new Uint8Array(await file.arrayBuffer())) })
  }

  if (setting.fileName)
    return (
      <Space>
        {setting.fileName}
        {!setting.readOnly && (
          <Button
            type="link"
            danger
            icon={<CloseOutlined />}
            aria-label={`Remove ${setting.fileName}`}
            onClick={() => void onChange({ ...setting, fileName: '', file: null })}
          />
        )}
      </Space>
    )

  return (
    <>
      <Button type="link" disabled={setting.readOnly} onClick={() => fileInput.current?.click()}>
        Upload
      </Button>
      <input ref={fileInput} type="file" hidden onChange={(event) => void upload(event)} />
    </>
  )
}
