import { Suspense } from 'react'
import { Layout, Spin } from 'antd'
import { Outlet } from 'react-router'
import { engine } from '../app'
import { PageErrorBoundary } from '../PageErrorBoundary'
import { Footer } from './Footer'
import { WhenPlugInsLoaded } from './WhenPlugInsLoaded'

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
          <PageErrorBoundary engine={engine}>
            <Suspense fallback={<Spin />}>
              <WhenPlugInsLoaded>
                <Outlet />
              </WhenPlugInsLoaded>
            </Suspense>
          </PageErrorBoundary>
        </div>
      </Layout.Content>
      <Footer />
    </Layout>
  )
}
