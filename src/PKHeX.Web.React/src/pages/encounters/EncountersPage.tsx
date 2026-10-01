import { Suspense, useState } from 'react'
import { App, Flex, Spin } from 'antd'
import { EngineError, type EncounterRow } from '@pkhex-everywhere/engine'
import { useEngine, useQuery } from '@pkhex-everywhere/react'
import { EncountersTable } from '../../components/EncountersTable'
import { PageHeader } from '../../components/PageHeader'
import { SpeciesSelect } from '../../components/SpeciesSelect'
import { VersionSelect } from '../../components/VersionSelect'
import { notifySuccessInHost, useNavigate } from '../../host'
import { routes } from '../../routes'

interface EncountersPageProps {
  version?: number | null
  species?: number | null
}

export default function EncountersPage(props: EncountersPageProps) {
  const { versions, default: defaultVersion } = useQuery('encounters.versions')
  const allSpecies = useQuery('species.list')
  const navigate = useNavigate()
  const version = versions.find((entry) => entry.id === props.version)?.id ?? defaultVersion
  const species = allSpecies.find((entry) => entry.id === props.species)?.id

  return (
    <Flex vertical gap={20}>
      <PageHeader title="Encounters" />
      <Flex gap="small" wrap>
        <div style={{ flex: '1 1 160px' }}>
          <VersionSelect
            value={version}
            onChange={(selected) => navigate(routes.encounters({ version: selected.id, species }), { replace: true })}
          />
        </div>
        <div style={{ flex: '3 1 240px' }}>
          <SpeciesSelect
            value={species}
            onChange={(selected) => navigate(routes.encounters({ version, species: selected.id }), { replace: true })}
          />
        </div>
      </Flex>
      {species !== undefined && (
        <Suspense fallback={<Spin />}>
          <Encounters version={version} species={species} />
        </Suspense>
      )}
    </Flex>
  )
}

function Encounters({ version, species }: { version: number; species: number }) {
  const encounters = useQuery('encounters.search', version, species)
  const engine = useEngine()
  const navigate = useNavigate()
  const { notification } = App.useApp()
  const [adding, setAdding] = useState<number>()

  const add = async (encounter: EncounterRow) => {
    setAdding(encounter.index)
    try {
      // The query cache can serve an earlier search, so search again to make the Session hold these rows.
      await engine.encounters.search(version, species)
      const added = await engine.box.addEncounter(encounter.index)
      await notifySuccessInHost(`${encounter.species} added to your box`)
      navigate(routes.pokemon(added))
    } catch (error) {
      if (!(error instanceof EngineError)) throw error
      notification.error({ title: "Couldn't add the Pokémon", description: error.message })
    } finally {
      setAdding(undefined)
    }
  }

  return <EncountersTable encounters={encounters} adding={adding} onAdd={add} />
}
