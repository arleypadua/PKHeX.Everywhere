import { Descriptions, Grid, InputNumber, Radio, Space, Switch, Typography } from 'antd'
import type { DescriptionsProps } from 'antd'
import { draftHandle, type PokemonGender } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'
import { ItemSelect } from '../../components/ItemSelect'
import { ChoiceSelect } from './ChoiceSelect'
import { useDraft } from './useDraft'

const typesUrl = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/types/generation-viii/sword-shield'

const genders: { value: PokemonGender; label: string }[] = [
  { value: 'male', label: '♂' },
  { value: 'female', label: '♀' },
  { value: 'genderless', label: '⚲' },
]

export function DescriptionTab() {
  const { details, submit } = useDraft()
  const options = useQuery('pokemon.options', draftHandle)
  const natures = useQuery('game.natures')
  const balls = useQuery('game.balls')
  const languages = useQuery('game.languages')
  const heldItems = useQuery('game.heldItems')
  const screens = Grid.useBreakpoint()

  const items: DescriptionsProps['items'] = [
    {
      key: 'species',
      label: 'Species',
      children: (
        <ChoiceSelect label="Species" choices={options.species} value={details.species} onChange={(species) => submit({ species })} />
      ),
    },
    {
      key: 'types',
      label: 'Types',
      children: (
        <Space size={5} wrap>
          {details.types.map((type) => (
            <img key={type} alt={`Type ${type}`} src={`${typesUrl}/${type + 1}.png`} style={{ width: 75 }} />
          ))}
        </Space>
      ),
    },
    {
      key: 'pid',
      label: 'PID',
      children: <Typography.Text copyable>{details.pid.toString(16).toUpperCase().padStart(8, '0')}</Typography.Text>,
    },
    {
      key: 'nickname',
      label: 'Nickname',
      children: (
        <Typography.Text editable={{ text: details.nickname, onChange: (nickname) => submit({ nickname }) }}>
          {details.nickname}
        </Typography.Text>
      ),
    },
    {
      key: 'gender',
      label: 'Gender',
      children: (
        <Radio.Group
          aria-label="Gender"
          optionType="button"
          buttonStyle="solid"
          value={details.gender}
          options={genders.filter((gender) => gender.value !== 'genderless' || details.gender === 'genderless')}
          onChange={(event) => submit({ gender: event.target.value as PokemonGender })}
        />
      ),
    },
    {
      key: 'shiny',
      label: 'Shiny',
      children: <Switch aria-label="Shiny" checked={details.isShiny} onChange={(isShiny) => submit({ isShiny })} />,
    },
    ...(details.isAlpha === null
      ? []
      : [
          {
            key: 'alpha',
            label: 'Alpha',
            children: <Switch aria-label="Alpha" checked={details.isAlpha} onChange={(isAlpha: boolean) => submit({ isAlpha })} />,
          },
        ]),
    {
      key: 'level',
      label: 'Level',
      children: (
        <InputNumber
          aria-label="Level"
          min={1}
          max={100}
          value={details.level}
          onChange={(level) => level !== null && submit({ level })}
        />
      ),
    },
    ...(natures.length === 0
      ? []
      : [
          {
            key: 'nature',
            label: 'Nature',
            children: (
              <ChoiceSelect label="Nature" choices={natures} value={details.nature} onChange={(nature) => submit({ nature })} />
            ),
          },
        ]),
    ...(options.forms.length === 0
      ? []
      : [
          {
            key: 'form',
            label: 'Form',
            children: <ChoiceSelect label="Form" choices={options.forms} value={details.form} onChange={(form) => submit({ form })} />,
          },
        ]),
    ...(heldItems.length <= 1
      ? []
      : [
          {
            key: 'heldItem',
            label: 'Held Item',
            children: (
              <ItemSelect items={heldItems} value={details.heldItem} onChange={(item) => submit({ heldItem: item.id })} />
            ),
          },
        ]),
    ...(options.abilities.length === 0
      ? []
      : [
          {
            key: 'ability',
            label: 'Ability',
            children: (
              <ChoiceSelect
                label="Ability"
                choices={options.abilities}
                value={details.ability}
                onChange={(ability) => submit({ ability })}
              />
            ),
          },
        ]),
    ...(balls.length === 0
      ? []
      : [
          {
            key: 'ball',
            label: 'Ball',
            children: <ItemSelect items={balls} value={details.ball} onChange={(ball) => submit({ ball: ball.id })} />,
          },
        ]),
    {
      key: 'friendship',
      label: details.isEgg ? 'Hatch Counter' : 'Friendship',
      children: (
        <InputNumber
          aria-label={details.isEgg ? 'Hatch Counter' : 'Friendship'}
          min={0}
          max={255}
          value={details.friendship}
          onChange={(friendship) => friendship !== null && submit({ friendship })}
        />
      ),
    },
    ...(languages.length === 0
      ? []
      : [
          {
            key: 'language',
            label: 'Language',
            children: (
              <ChoiceSelect
                label="Language"
                choices={languages}
                value={details.language}
                onChange={(language) => submit({ language })}
              />
            ),
          },
        ]),
    {
      key: 'egg',
      label: 'Egg',
      children: <Switch aria-label="Egg" checked={details.isEgg} onChange={(isEgg) => submit({ isEgg })} />,
    },
    {
      key: 'infected',
      label: 'Infected',
      children: <Switch aria-label="Infected" disabled checked={details.isInfected} />,
    },
    {
      key: 'cured',
      label: 'Cured',
      children: <Switch aria-label="Cured" disabled checked={details.isCured} />,
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
