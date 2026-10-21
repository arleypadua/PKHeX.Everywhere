import { useState } from 'react'
import { Flex, InputNumber, Modal, Slider, Typography } from 'antd'
import { useDraft } from './useDraft'

interface ExperienceModalProps {
  onClose: () => void
}

export function ExperienceModal({ onClose }: ExperienceModalProps) {
  const { details, submit } = useDraft()
  const { min, max } = details.experienceRange
  const [experience, setExperience] = useState(details.experience)
  const atMaxLevel = min === max

  const handleOk = () => {
    void submit({ experience })
    onClose()
  }

  return (
    <Modal open title="EXP" okText="Save" onOk={handleOk} onCancel={onClose}>
      <Flex vertical gap={12}>
        <Slider min={min} max={max} value={experience} disabled={atMaxLevel} onChange={setExperience} />
        <InputNumber<number>
          aria-label="EXP"
          min={min}
          max={max}
          precision={0}
          value={experience}
          disabled={atMaxLevel}
          onChange={(value) => value !== null && setExperience(value)}
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
