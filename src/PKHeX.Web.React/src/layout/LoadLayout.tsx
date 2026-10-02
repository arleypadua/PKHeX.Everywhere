import { Layout } from 'antd'
import { Footer } from './Footer'
import { RoutedContent } from './RoutedContent'

export function LoadLayout() {
  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Content style={{ margin: '24px 16px 0', display: 'flex', justifyContent: 'center' }}>
        <div
          style={{
            padding: 24,
            minHeight: 360,
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 20,
          }}
        >
          Open a save file
          <RoutedContent />
        </div>
      </Layout.Content>
      <Footer />
    </Layout>
  )
}
