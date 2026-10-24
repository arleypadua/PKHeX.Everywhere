import { useRef, useState } from 'react'
import { Flex, InputNumber, Modal, Slider, Typography } from 'antd'
import { useDraft } from './useDraft'

interface ExperienceModalProps {
  onClose: () => void
}

interface ExperienceFieldsProps {
  min: number
  max: number
  initial: number
  disabled: boolean
  onChange: (value: number) => void
}

function ExperienceFields({ min, max, initial, disabled, onChange }: ExperienceFieldsProps) {
  const [experience, setExperience] = useState(initial)

  const set = (value: number) => {
    setExperience(value)
    onChange(value)
  }

  return (
    <>
      <Slider min={min} max={max} value={experience} disabled={disabled} tooltip={{ open: false }} onChange={set} />
      <InputNumber<number>
        aria-label="EXP"
        min={min}
        max={max}
        precision={0}
        value={experience}
        disabled={disabled}
        onChange={(value) => value !== null && set(value)}
      />
    </>
  )
}

export function ExperienceModal({ onClose }: ExperienceModalProps) {
  const { details, submit } = useDraft()
  const { min, max } = details.experienceRange
  const experience = useRef(details.experience)
  const atMaxLevel = min === max

  const handleOk = () => {
    void submit({ experience: experience.current })
    onClose()
  }

  return (
    <Modal open title="EXP" okText="Save" onOk={handleOk} onCancel={onClose}>
      <Flex vertical gap={12}>
        <ExperienceFields
          min={min}
          max={max}
          initial={details.experience}
          disabled={atMaxLevel}
          onChange={(value) => (experience.current = value)}
        />
        <Typography.Text type="secondary">
          {atMaxLevel
            ? 'Level 100 is the most EXP this Pokémon can have.'
            : `Level ${details.level} runs from ${min} to ${max} EXP. Change the level to go beyond that.`}
        </Typography.Text>
      </Flex>
    </Modal>
  )
}
