import { Suspense } from 'react'
import { Spin } from 'antd'
import { Outlet } from 'react-router'
import { engine } from '../app'
import { RoutedErrorBoundary } from './RoutedErrorBoundary'
import { WhenPlugInsLoaded } from './WhenPlugInsLoaded'

export function RoutedContent() {
  return (
    <RoutedErrorBoundary engine={engine}>
      <Suspense fallback={<Spin />}>
        <WhenPlugInsLoaded>
          <Outlet />
        </WhenPlugInsLoaded>
      </Suspense>
    </RoutedErrorBoundary>
  )
}
