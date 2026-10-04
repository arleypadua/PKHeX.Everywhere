import { Flex, Layout } from 'antd'
import { Link } from 'react-router'
import { applicationName, gitHubRepository, sdkDocs } from '../constants'
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
      <Flex wrap justify="center" gap="8px 16px">
        <Link to={routes.privacyPolicy}>Privacy Policy</Link>
        <Link to={routes.termsOfUse}>Terms of Use</Link>
        <Link to={routes.credits}>Credits</Link>
        <Link to={routes.releaseNotes()}>Release Notes</Link>
        <a href="/blog">Blog</a>
        <a href={sdkDocs} target="_blank">
          SDK
        </a>
      </Flex>
    </Layout.Footer>
  )
}
