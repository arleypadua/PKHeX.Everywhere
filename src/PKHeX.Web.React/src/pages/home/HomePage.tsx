import { useState } from 'react'
import { App, ConfigProvider, Descriptions, Flex, Input, InputNumber, Radio, Space, Typography } from 'antd'
import { EngineError, type TrainerGender } from '@pkhex-everywhere/engine'
import { useLoadedGame, useQuery, useTrainer } from '@pkhex-everywhere/react'
import { AdSlot } from '../../components/AdSlot'
import { PlugInActionButton } from '../../components/PlugInActionButton'

const multiplexAdSlot = '7470710144'
const maxMoney = 999_999
const maxBattlePoints = 65_535
const genderColors: Record<TrainerGender, string> = { male: '#1890ff', female: '#b218ff' }

export default function HomePage() {
  const { game } = useLoadedGame()

  return (
    <Flex vertical gap={20} style={{ width: '100%' }}>
      {game && <TrainerCard />}
      <AdSlot slot={multiplexAdSlot} format="autorelaxed" />
      <QuickActions />
    </Flex>
  )
}

function TrainerCard() {
  const { trainer, setName, setGender, setMoney, setBattlePoints } = useTrainer()
  const edit = useTrainerEdit()
  const name = useDraft(trainer.name, (value) => edit(() => setName(value)))
  const money = useDraft(trainer.money, (value) => value !== null && edit(() => setMoney(value)))
  const battlePoints = useDraft(trainer.battlePoints, (value) => value !== null && edit(() => setBattlePoints(value)))

  return (
    <Descriptions
      title="Trainer"
      bordered
      size="small"
      column={{ xs: 1, sm: 2, md: 3, lg: 3, xl: 3, xxl: 3 }}
      items={[
        { key: 'id', label: 'TID/SID', children: trainer.id },
        {
          key: 'name',
          label: 'Name',
          children: (
            <Input
              value={name.value}
              onChange={(event) => name.onChange(event.target.value)}
              onBlur={name.onBlur}
              maxLength={trainer.maxNameLength}
              placeholder="Name"
              style={{ width: '100%', maxWidth: 170 }}
            />
          ),
        },
        {
          key: 'gender',
          label: 'Gender',
          children: (
            <ConfigProvider theme={{ token: { colorPrimary: genderColors[trainer.gender] } }}>
              <Radio.Group
                optionType="button"
                buttonStyle="solid"
                value={trainer.gender}
                onChange={(event) => edit(() => setGender(event.target.value as TrainerGender))}
                options={[
                  { value: 'male', label: '♂' },
                  { value: 'female', label: '♀' },
                ]}
              />
            </ConfigProvider>
          ),
        },
        {
          key: 'cash',
          label: 'Cash',
          children: (
            <InputNumber<number>
              aria-label="Cash"
              value={money.value}
              onChange={money.onChange}
              onBlur={money.onBlur}
              formatter={(value) => `$ ${value}`}
              parser={(value) => Number(value?.replace(/\$\s?/g, ''))}
              disabled={trainer.money === null}
              min={0}
              max={maxMoney}
              precision={0}
              style={{ width: '100%', maxWidth: 170 }}
            />
          ),
        },
        ...(trainer.battlePoints === null
          ? []
          : [
              {
                key: 'battlePoints',
                label: 'Battle Points',
                children: (
                  <InputNumber<number>
                    aria-label="Battle Points"
                    value={battlePoints.value}
                    onChange={battlePoints.onChange}
                    onBlur={battlePoints.onBlur}
                    min={0}
                    max={maxBattlePoints}
                    precision={0}
                    style={{ width: '100%', maxWidth: 170 }}
                  />
                ),
              },
            ]),
        { key: 'rival', label: 'Rival', children: trainer.rival },
      ]}
    />
  )
}

function QuickActions() {
  const actions = useQuery('plugins.actions', 'quick', null)
  if (actions.length === 0) return null

  return (
    <Flex vertical align="center" gap={20} style={{ marginTop: 20 }}>
      <Typography.Title level={1}>Quick Actions</Typography.Title>
      <Space wrap size={20} align="center">
        {actions.map((action) => (
          <PlugInActionButton key={action.id} action={action} />
        ))}
      </Space>
    </Flex>
  )
}

// The input shows its own value while focused, so an edit's refetch can't overwrite keystrokes that came after it.
function useDraft<T>(value: T, commit: (next: T) => unknown) {
  const [draft, setDraft] = useState<{ value: T } | null>(null)
  return {
    value: draft ? draft.value : value,
    onChange: (next: T) => {
      setDraft({ value: next })
      commit(next)
    },
    onBlur: () => setDraft(null),
  }
}

function useTrainerEdit() {
  const { notification } = App.useApp()

  return async (edit: () => Promise<void>) => {
    try {
      await edit()
    } catch (error) {
      if (!(error instanceof EngineError) || (error.code !== 'out-of-range' && error.code !== 'not-in-game')) throw error
      notification.error({ title: "Couldn't change the trainer", description: error.message })
    }
  }
}
