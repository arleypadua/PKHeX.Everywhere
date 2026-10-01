import { useRef, useState, type ChangeEvent } from 'react'
import { CalculatorOutlined, CopyOutlined, FolderOpenOutlined } from '@ant-design/icons'
import { App, Button, Flex, type TableColumnsType } from 'antd'
import type { PokemonSummary } from '@pkhex-everywhere/engine'
import { useBox, useEngine } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'
import { PokemonTable } from '../../components/PokemonTable'
import { useCopyShowdown } from '../../hooks/useCopyShowdown'
import { openCalculator, useNavigate } from '../../host'
import { routes } from '../../routes'
import { useLoadPokemonFile } from './useLoadPokemonFile'

const boxNumber = (pokemon: PokemonSummary) => (pokemon.at.box ?? 0) + 1

const boxColumns: TableColumnsType<PokemonSummary> = [
  {
    title: 'Box',
    key: 'box',
    render: (_, pokemon) => boxNumber(pokemon),
    sorter: (a, b) => boxNumber(a) - boxNumber(b) || a.at.slot - b.at.slot,
  },
]

export default function BoxPage() {
  const { box } = useBox()
  const engine = useEngine()
  const navigate = useNavigate()
  const copyShowdown = useCopyShowdown()
  const { notification } = App.useApp()
  const loadPokemonFile = useLoadPokemonFile()
  const fileInput = useRef<HTMLInputElement>(null)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const selected = box.filter((pokemon) => selectedIds.includes(pokemon.id))

  const openSelectedInCalculator = async () => {
    const showdown = selected.length
      ? (await Promise.all(selected.map((pokemon) => engine.pokemon.showdown(pokemon.at)))).join('\n\n')
      : await engine.box.showdown()
    openCalculator(showdown)
  }

  const copyBoxShowdown = async () => {
    if (box.length === 0) {
      notification.error({ title: 'No Pokémon found in your box' })
      return
    }
    await copyShowdown(await engine.box.showdown(), `${box.length} entries`)
  }

  const loadFile = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (file) await loadPokemonFile(file)
  }

  return (
    <Flex vertical gap={20}>
      <PageHeader
        title="Box"
        extra={
          <>
            <Button type="primary" onClick={() => navigate(routes.searchEncounter)}>
              Add
            </Button>
            <Button type="link" icon={<FolderOpenOutlined />} onClick={() => fileInput.current?.click()}>
              Load *.pk
            </Button>
            <Button type="link" icon={<CalculatorOutlined />} onClick={openSelectedInCalculator}>
              Calculator
            </Button>
            <Button type="link" icon={<CopyOutlined />} onClick={copyBoxShowdown}>
              Showdown
            </Button>
            <input ref={fileInput} type="file" hidden onChange={loadFile} />
          </>
        }
      />
      <PokemonTable
        pokemon={box}
        extraColumns={boxColumns}
        selected={selected}
        onSelectedChange={(rows) => setSelectedIds(rows.map((pokemon) => pokemon.id))}
      />
    </Flex>
  )
}
