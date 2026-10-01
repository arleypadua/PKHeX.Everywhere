import { Pie, Treemap } from '@ant-design/plots'
import { useTheme } from '../../host'
import type { Slice } from './analytics'

export interface TreemapData {
  name: string
  value: number
  children: { name: string; value: number }[]
}

function useChartTheme() {
  return useTheme() === 'dark' ? 'classicDark' : 'classic'
}

export function PieChart({ data }: { data: Slice[] }) {
  const theme = useChartTheme()
  return (
    <Pie
      data={data}
      angleField="value"
      colorField="type"
      radius={0.8}
      label={{ text: 'type', position: 'inside' }}
      autoFit
      theme={theme}
    />
  )
}

export function TreemapChart({ data }: { data: TreemapData }) {
  const theme = useChartTheme()
  return <Treemap data={{ value: data }} autoFit theme={theme} />
}
