import { lazy, Suspense, useEffect, useState, type ReactNode } from 'react'
import { Alert, Card, Flex, Spin } from 'antd'
import type { CatalogName } from '@pkhex-everywhere/engine'
import { useQuery } from '@pkhex-everywhere/react'
import { fetchAnalyticsResults, type AnalyticsResults, type Counted } from './analytics'
import type { TreemapData } from './charts'

const PieChart = lazy(() => import('./charts').then((charts) => ({ default: charts.PieChart })))
const TreemapChart = lazy(() => import('./charts').then((charts) => ({ default: charts.TreemapChart })))

export default function AnalyticsPage() {
  const [results, setResults] = useState<AnalyticsResults | 'failed'>()

  useEffect(() => {
    let active = true
    fetchAnalyticsResults().then(
      (loaded) => active && setResults(loaded),
      () => active && setResults('failed'),
    )
    return () => {
      active = false
    }
  }, [])

  if (results === undefined) return <Spin />
  if (results === 'failed')
    return <Alert type="error" title="Couldn't load analytics results. Try again later." showIcon />

  return (
    <Flex vertical gap={20} style={{ width: '100%' }}>
      <ChartDeck>
        {results.byCountry.length > 0 && (
          <ChartCard title="Edits by country">
            <PieChart data={results.byCountry} />
          </ChartCard>
        )}
        {results.byVersion.length > 0 && (
          <ChartCard title="Edits by version">
            <PieChart data={results.byVersion} />
          </ChartCard>
        )}
      </ChartDeck>
      <Suspense fallback={<Spin />}>
        <NamedTreemaps species={results.species} items={results.items} />
      </Suspense>
    </Flex>
  )
}

function NamedTreemaps({ species, items }: { species: Counted[]; items: Counted[] }) {
  const names = useQuery('catalog.names', {
    speciesIds: species.map((entry) => entry.id),
    itemIds: items.map((entry) => entry.id),
  })
  const speciesData = treemapData(species, names.species)
  const itemsData = treemapData(items, names.items)

  return (
    <ChartDeck>
      {speciesData.children.length > 0 && (
        <ChartCard title="Species changed">
          <TreemapChart data={speciesData} />
        </ChartCard>
      )}
      {itemsData.children.length > 0 && (
        <ChartCard title="Items changed">
          <TreemapChart data={itemsData} />
        </ChartCard>
      )}
    </ChartDeck>
  )
}

function treemapData(counts: Counted[], names: CatalogName[]): TreemapData {
  const nameById = new Map(names.map((entry) => [entry.id, entry.name]))
  const children = counts.flatMap(({ id, count }) => {
    const name = nameById.get(id)
    return name === undefined ? [] : [{ name, value: count }]
  })
  return { name: 'root', value: children.reduce((sum, child) => sum + child.value, 0), children }
}

function ChartDeck({ children }: { children: ReactNode }) {
  return (
    <Flex wrap gap={20} style={{ width: '100%' }}>
      {children}
    </Flex>
  )
}

function ChartCard({ title, children }: { title: string; children: ReactNode }) {
  return (
    <Card variant="borderless" title={title} style={{ flex: 1, minWidth: 250 }}>
      <Suspense fallback={<Spin />}>{children}</Suspense>
    </Card>
  )
}
