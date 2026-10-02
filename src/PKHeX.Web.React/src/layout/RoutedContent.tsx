import { Suspense } from 'react'
import { Spin } from 'antd'
import { Outlet } from 'react-router'
import { engine } from '../app'
import { PageErrorBoundary } from '../PageErrorBoundary'
import { WhenPlugInsLoaded } from './WhenPlugInsLoaded'

export function RoutedContent() {
  return (
    <PageErrorBoundary engine={engine}>
      <Suspense fallback={<Spin />}>
        <WhenPlugInsLoaded>
          <Outlet />
        </WhenPlugInsLoaded>
      </Suspense>
    </PageErrorBoundary>
  )
}
