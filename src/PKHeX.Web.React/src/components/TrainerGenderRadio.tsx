import { ConfigProvider, Radio } from 'antd'
import type { TrainerGender } from '@pkhex-everywhere/engine'

const genderColors: Record<TrainerGender, string> = { male: '#1890ff', female: '#b218ff' }

interface TrainerGenderRadioProps {
  value: TrainerGender
  onChange: (gender: TrainerGender) => void
}

export function TrainerGenderRadio({ value, onChange }: TrainerGenderRadioProps) {
  return (
    <ConfigProvider theme={{ token: { colorPrimary: genderColors[value] } }}>
      <Radio.Group
        optionType="button"
        buttonStyle="solid"
        value={value}
        onChange={(event) => onChange(event.target.value as TrainerGender)}
        options={[
          { value: 'male', label: '♂' },
          { value: 'female', label: '♀' },
        ]}
      />
    </ConfigProvider>
  )
}
