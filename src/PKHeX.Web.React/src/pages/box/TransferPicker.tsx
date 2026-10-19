import { useEffect, useMemo, useState } from 'react'
import { App, Button, Flex, Modal, Segmented, Space, Typography, type TableColumnsType } from 'antd'
import { EngineError, type PokemonSummary, type TransferPreview, type TransferSummary } from '@pkhex-everywhere/engine'
import { useEngine } from '@pkhex-everywhere/react'
import { gameName } from '../../capabilities'
import { PokemonTable } from '../../components/PokemonTable'
import { downloadFile } from '../../host'
import { boxColumns } from './boxColumns'
import { describeChange, refusalReasons } from './transferLabels'
import { transferPick } from './transferPick'

type View = 'party' | 'box'

const pickKey = (pokemon: PokemonSummary) => JSON.stringify(pokemon.at)

function previewColumn(preview: TransferPreview | undefined): TableColumnsType<PokemonSummary>[number] {
  return {
    key: 'transfer',
    render: (_, pokemon) => {
      const key = pickKey(pokemon)
      const refused = preview?.refused.find((refusal) => JSON.stringify(refusal.at) === key)
      const offered = preview?.offers.find((offer) => pickKey(offer.from) === key)
      return (
        <div style={{ maxWidth: 280 }}>
          {refused ? (
            <Typography.Text type="danger">{refusalReasons[refused.reason]}</Typography.Text>
          ) : (
            offered?.changes.map((change, index) => (
              <Typography.Text key={index} type="secondary" style={{ display: 'block' }}>
                {describeChange(change)}
              </Typography.Text>
            ))
          )}
        </div>
      )
    },
  }
}

export function TransferPicker({ transfer, onClose }: { transfer: TransferSummary; onClose: () => Promise<void> }) {
  const engine = useEngine()
  const { notification } = App.useApp()
  const [view, setView] = useState<View>('party')
  const [partnerPokemon, setPartnerPokemon] = useState<PokemonSummary[]>([])
  const [pickedKeys, setPickedKeys] = useState<string[]>([])
  const [previewed, setPreviewed] = useState<{ of: PokemonSummary[]; preview: TransferPreview }>()
  const [moving, setMoving] = useState(false)

  useEffect(() => {
    let current = true
    void engine.transfer.partnerBoxes().then((pokemon) => current && setPartnerPokemon(pokemon))
    return () => {
      current = false
    }
  }, [engine])

  const picked = useMemo(
    () => partnerPokemon.filter((pokemon) => pickedKeys.includes(pickKey(pokemon))),
    [partnerPokemon, pickedKeys],
  )

  useEffect(() => {
    if (picked.length === 0) return
    let current = true
    void engine.transfer
      .preview({ send: [], receive: picked.map((pokemon) => pokemon.at) })
      .then((preview) => current && setPreviewed({ of: picked, preview }))
    return () => {
      current = false
    }
  }, [engine, picked])

  const preview = previewed?.of === picked ? previewed.preview : undefined
  const pick = transferPick(
    picked.map((pokemon) => pokemon.at),
    transfer.room,
    preview,
  )
  const shown = partnerPokemon.filter((pokemon) => pokemon.at.source === view)

  const changePicked = (rows: PokemonSummary[]) =>
    setPickedKeys([...pickedKeys.filter((key) => !shown.some((pokemon) => pickKey(pokemon) === key)), ...rows.map(pickKey)])

  const move = async () => {
    setMoving(true)
    try {
      const result = await engine.transfer.commit({ send: [], receive: picked.map((pokemon) => pokemon.at) })
      notification.success({
        title: `${result.arrived.length} Pokémon moved to your box`,
        description: 'Export this save from the Save page to keep them.',
        actions: (
          <Button type="link" onClick={() => downloadFile(result.partner.bytes, result.partner.fileName)}>
            {`Download ${result.partner.fileName}`}
          </Button>
        ),
      })
      await onClose()
    } catch (error) {
      if (!(error instanceof EngineError)) throw error
      notification.error({ title: error.message })
      setMoving(false)
    }
  }

  return (
    <Modal
      open
      width={720}
      title={
        <Flex vertical>
          {`Transfer from ${transfer.partner.fileName}`}
          <Typography.Text type="secondary" style={{ fontWeight: 'normal' }}>
            {`${gameName(transfer.partner)} · ${transfer.trainer.name}`}
          </Typography.Text>
        </Flex>
      }
      onCancel={onClose}
      footer={
        <Flex justify="space-between" align="center" gap={8} wrap>
          <Typography.Text type="secondary">{pick.summary}</Typography.Text>
          <Space>
            <Button onClick={onClose}>Cancel</Button>
            <Button type="primary" disabled={!pick.canMove} loading={moving} onClick={move}>
              {pick.moveLabel}
            </Button>
          </Space>
        </Flex>
      }
    >
      <Flex vertical gap={12}>
        <Segmented<View>
          options={[
            { label: 'Party', value: 'party' },
            { label: 'Box', value: 'box' },
          ]}
          value={view}
          onChange={setView}
        />
        <div style={{ maxHeight: '55vh', overflowY: 'auto' }}>
          <PokemonTable
            pokemon={shown}
            extraColumns={[...(view === 'box' ? boxColumns : []), previewColumn(preview)]}
            actions={false}
            selected={picked}
            onSelectedChange={changePicked}
          />
        </div>
      </Flex>
    </Modal>
  )
}
