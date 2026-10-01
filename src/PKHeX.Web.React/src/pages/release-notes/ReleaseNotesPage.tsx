import { Tag, Typography } from 'antd'

interface ReleaseNotesEntry {
  date: string
  items: string[]
}

interface ReleaseNotesPageProps {
  entries?: ReleaseNotesEntry[]
  since?: string | null
}

export default function ReleaseNotesPage({ entries = [], since }: ReleaseNotesPageProps) {
  return (
    <Typography>
      <h1>Release Notes</h1>
      {entries.map((entry) => (
        <section key={entry.date}>
          <h2>
            {entry.date} {since && entry.date > since && <Tag color="green">new</Tag>}
          </h2>
          <ul>
            {entry.items.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </section>
      ))}
    </Typography>
  )
}
