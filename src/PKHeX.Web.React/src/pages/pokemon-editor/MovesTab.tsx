import { useState } from 'react'
import { Checkbox, Descriptions, Flex, Grid } from 'antd'
import { draftHandle, type Choice, type MoveSlot } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'
import { ChoiceSelect } from './ChoiceSelect'
import { useDraft } from './useDraft'

const none: Choice = { id: 0, name: '(None)' }

export function MovesTab() {
  const { details, submit } = useDraft()
  const options = useQuery('pokemon.options', draftHandle)
  const allMoves = useQuery('game.moves')
  const [showAllMoves, setShowAllMoves] = useState(false)
  const layout = Grid.useBreakpoint().sm ? 'horizontal' : 'vertical'

  const assigned = new Set(details.moves.map((move) => move.id).filter((id) => id !== none.id))
  const available = (showAllMoves ? allMoves : options.moves).filter((move) => !assigned.has(move.id))
  const choicesFor = (slot: MoveSlot) => [...(slot.id === none.id ? [] : [slot]), ...available, none]

  const change = (index: number, id: number) =>
    submit({ moves: details.moves.map((move, i) => (i === index ? id : move.id)) })

  return (
    <Flex vertical gap={20}>
      <Checkbox checked={showAllMoves} onChange={(event) => setShowAllMoves(event.target.checked)}>
        Show all moves
      </Checkbox>
      {details.moves.map((slot, index) => (
        <Descriptions
          key={index}
          title={`Move ${index + 1}`}
          bordered
          size="small"
          layout={layout}
          items={[
            {
              key: 'move',
              label: 'Move',
              style: { maxWidth: 250 },
              children: (
                <ChoiceSelect
                  label={`Move ${index + 1}`}
                  choices={choicesFor(slot)}
                  value={slot.id}
                  onChange={(id) => change(index, id)}
                />
              ),
            },
            { key: 'pp', label: 'PP', children: `${slot.pp}/${slot.maxPp}` },
          ]}
        />
      ))}
    </Flex>
  )
}
