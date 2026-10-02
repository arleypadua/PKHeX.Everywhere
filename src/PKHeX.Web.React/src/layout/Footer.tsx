import { Divider, Layout, Space } from 'antd'
import { Link } from 'react-router'
import { applicationName, gitHubRepository } from '../constants'
import { routes } from '../routes'

export function Footer() {
  return (
    <Layout.Footer style={{ textAlign: 'center' }}>
      <p>{applicationName} - edit pokemon save files.</p>
      <p>
        Issues or feedback on{' '}
        <a href={gitHubRepository} target="_blank">
          GitHub
        </a>
      </p>
      <Space wrap separator={<Divider orientation="vertical" />}>
        <Link to={routes.privacyPolicy}>Privacy Policy</Link>
        <Link to={routes.termsOfUse}>Terms of Use</Link>
        <Link to={routes.credits}>Credits</Link>
        <Link to={routes.releaseNotes()}>Release Notes</Link>
        <a href="/blog">Blog</a>
      </Space>
    </Layout.Footer>
  )
}
