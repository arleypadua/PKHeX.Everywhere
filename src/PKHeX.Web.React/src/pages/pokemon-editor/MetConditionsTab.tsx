import { DatePicker, Descriptions, Grid, InputNumber, Switch } from 'antd'
import type { DescriptionsProps } from 'antd'
import dayjs from 'dayjs'
import { draftHandle } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'
import { ItemSelect } from '../../components/ItemSelect'
import { ChoiceSelect } from './ChoiceSelect'
import { useDraft } from './useDraft'

export function MetConditionsTab() {
  const { details, submit } = useDraft()
  const options = useQuery('pokemon.options', draftHandle)
  const originGames = useQuery('game.originGames')
  const balls = useQuery('game.balls')
  const screens = Grid.useBreakpoint()

  const items: DescriptionsProps['items'] = [
    ...(originGames.length === 0
      ? []
      : [
          {
            key: 'version',
            label: 'Origin Game',
            children: (
              <ChoiceSelect
                label="Origin Game"
                choices={originGames}
                value={details.version}
                onChange={(version) => submit({ version })}
              />
            ),
          },
        ]),
    ...(options.metLocations.length === 0
      ? []
      : [
          {
            key: 'metLocation',
            label: 'Location',
            children: (
              <ChoiceSelect
                label="Location"
                choices={options.metLocations}
                value={details.metLocation}
                onChange={(metLocation) => submit({ metLocation })}
              />
            ),
          },
          {
            key: 'metLevel',
            label: 'Level',
            children: (
              <InputNumber
                aria-label="Met Level"
                min={0}
                max={100}
                value={details.metLevel}
                onChange={(metLevel) => metLevel !== null && submit({ metLevel })}
              />
            ),
          },
        ]),
    ...(balls.length === 0
      ? []
      : [
          {
            key: 'ball',
            label: 'Captured With',
            children: <ItemSelect items={balls} label="Ball" value={details.ball} onChange={(ball) => submit({ ball: ball.id })} />,
          },
        ]),
    ...(details.metDate === null
      ? []
      : [
          {
            key: 'metDate',
            label: 'Date',
            children: (
              <DatePicker
                aria-label="Met Date"
                format="DD/MM/YYYY"
                allowClear={false}
                value={dayjs(details.metDate)}
                onChange={(date) => date && submit({ metDate: date.format('YYYY-MM-DD') })}
              />
            ),
          },
        ]),
    {
      key: 'fatefulEncounter',
      label: 'Fateful Encounter',
      children: (
        <Switch
          aria-label="Fateful Encounter"
          checked={details.fatefulEncounter}
          onChange={(fatefulEncounter) => submit({ fatefulEncounter })}
        />
      ),
    },
  ]

  return (
    <Descriptions
      bordered
      size="small"
      layout={screens.sm ? 'horizontal' : 'vertical'}
      column={{ xs: 1, sm: 2, md: 3 }}
      items={items}
    />
  )
}
