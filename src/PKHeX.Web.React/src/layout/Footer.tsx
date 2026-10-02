import { Divider, Layout, Space } from 'antd'
import { Link } from 'react-router'
import { applicationName, gitHubRepository } from '../constants'

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
        <Link to="/privacy-policy">Privacy Policy</Link>
        <Link to="/terms-of-use">Terms of Use</Link>
        <Link to="/credits">Credits</Link>
        <Link to="/release-notes">Release Notes</Link>
        <a href="/blog">Blog</a>
      </Space>
    </Layout.Footer>
  )
}
