import { useState } from 'react'
import { Descriptions, Flex, Grid, Input, InputNumber, Radio } from 'antd'
import type { PokemonHandler } from '@pkhex-everywhere/engine'
import { TrainerGenderRadio } from '../../components/TrainerGenderRadio'
import { useDraft } from './useDraft'

const columns = { xs: 1, sm: 2, md: 3 }
const maxId = 4_294_967_295

export function TrainerTab() {
  const { details, submit } = useDraft()
  const layout = Grid.useBreakpoint().sm ? 'horizontal' : 'vertical'
  const handledBy = (handler: PokemonHandler) => details.currentHandler === handler

  return (
    <Flex vertical gap={20}>
      <Descriptions
        bordered
        size="small"
        layout={layout}
        column={columns}
        items={[
          {
            key: 'handler',
            label: 'Handler',
            children: (
              <Radio.Group
                optionType="button"
                buttonStyle="solid"
                value={details.currentHandler}
                onChange={(event) => submit({ currentHandler: event.target.value as PokemonHandler })}
                options={[
                  { value: 'originalTrainer', label: 'OT' },
                  { value: 'handlingTrainer', label: 'Other' },
                ]}
              />
            ),
          },
        ]}
      />
      <Descriptions
        title={handledBy('originalTrainer') ? 'Original Trainer ✅' : 'Original Trainer'}
        bordered
        size="small"
        layout={layout}
        column={columns}
        items={[
          {
            key: 'tid',
            label: 'TID',
            children: (
              <InputNumber<number>
                aria-label="TID"
                placeholder="TID"
                value={details.trainerId}
                onChange={(trainerId) => trainerId !== null && submit({ trainerId })}
                min={0}
                max={maxId}
                precision={0}
              />
            ),
          },
          {
            key: 'sid',
            label: 'SID',
            children: (
              <InputNumber<number>
                aria-label="SID"
                placeholder="SID"
                value={details.secretId}
                onChange={(secretId) => secretId !== null && submit({ secretId })}
                min={0}
                max={maxId}
                precision={0}
              />
            ),
          },
          {
            key: 'name',
            label: 'Name',
            children: (
              <NameInput
                value={details.originalTrainerName}
                onCommit={(originalTrainerName) => submit({ originalTrainerName })}
              />
            ),
          },
          {
            key: 'gender',
            label: 'Gender',
            children: (
              <TrainerGenderRadio
                value={details.originalTrainerGender}
                onChange={(originalTrainerGender) => submit({ originalTrainerGender })}
              />
            ),
          },
        ]}
      />
      <Descriptions
        title={handledBy('handlingTrainer') ? 'Current Handler ✅' : 'Current Handler'}
        bordered
        size="small"
        layout={layout}
        column={columns}
        items={[
          {
            key: 'name',
            label: 'Name',
            children: (
              <NameInput
                value={details.handlingTrainerName}
                onCommit={(handlingTrainerName) => submit({ handlingTrainerName })}
              />
            ),
          },
          {
            key: 'gender',
            label: 'Gender',
            children: (
              <TrainerGenderRadio
                value={details.handlingTrainerGender}
                onChange={(handlingTrainerGender) => submit({ handlingTrainerGender })}
              />
            ),
          },
        ]}
      />
    </Flex>
  )
}

interface NameInputProps {
  value: string
  onCommit: (name: string) => unknown
}

function NameInput({ value, onCommit }: NameInputProps) {
  const [draft, setDraft] = useState<string | null>(null)
  const commit = () => {
    if (draft !== null && draft !== value) onCommit(draft)
    setDraft(null)
  }

  return (
    <Input
      value={draft ?? value}
      onChange={(event) => setDraft(event.target.value)}
      onBlur={commit}
      onPressEnter={commit}
      placeholder="Name"
      style={{ maxWidth: 200 }}
    />
  )
}
