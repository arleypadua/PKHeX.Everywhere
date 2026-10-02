import { Tag, Typography } from 'antd'
import { settings } from '../../host'
import { news } from '../../news'

interface ReleaseNotesPageProps {
  since?: string | null
}

export default function ReleaseNotesPage(props: ReleaseNotesPageProps) {
  const since = props.since ?? settings.readLastDateNewsSeen()
  return (
    <Typography>
      <h1>Release Notes</h1>
      {news.map((entry) => (
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
