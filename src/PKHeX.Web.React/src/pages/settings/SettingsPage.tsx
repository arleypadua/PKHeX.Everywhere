import { useState } from 'react'
import { AutoComplete, Descriptions, Flex, Switch } from 'antd'
import { changeCalculatorUrl, changeTheme, getCalculators, useCalculatorUrl, useTheme } from '../../host'

function CalculatorInput() {
  const calculators = getCalculators()
  const calculatorUrl = useCalculatorUrl()
  const [text, setText] = useState(() => calculators.find((c) => c.url === calculatorUrl)?.name ?? calculatorUrl)

  const handleChange = (value: string | undefined) => {
    const next = value ?? ''
    setText(next)
    const preset = calculators.find((c) => c.name === next)
    void changeCalculatorUrl(preset?.url ?? next).catch(console.error)
  }

  return (
    <AutoComplete
      aria-label="Calculator"
      style={{ width: '100%' }}
      value={text}
      onChange={handleChange}
      options={calculators.map((c) => ({ value: c.name, label: c.name }))}
      filterOption={false}
      placeholder="Calculator or URL (Default: Showdown)"
      allowClear
    />
  )
}

export default function SettingsPage() {
  const theme = useTheme()

  return (
    <Flex vertical gap={20}>
      <Descriptions
        title="General"
        bordered
        size="small"
        column={1}
        items={[{ key: 'calculator', label: 'Calculator', children: <CalculatorInput /> }]}
      />
      <Descriptions
        title="Appearance"
        bordered
        size="small"
        column={1}
        items={[
          {
            key: 'theme',
            label: 'Theme',
            children: (
              <Switch
                checked={theme === 'dark'}
                onChange={(dark) => void changeTheme(dark ? 'dark' : 'light').catch(console.error)}
                checkedChildren="Dark"
                unCheckedChildren="Light"
              />
            ),
          },
        ]}
      />
    </Flex>
  )
}
