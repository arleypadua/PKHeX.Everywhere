import { Suspense } from 'react'
import { Spin } from 'antd'
import { Outlet } from 'react-router'
import { useEngineStatus } from '@pkhex-everywhere/react'
import { engine } from '../app'
import { RoutedErrorBoundary } from './RoutedErrorBoundary'
import { WhenPlugInsLoaded } from './WhenPlugInsLoaded'

export function RoutedContent() {
  return (
    <RoutedErrorBoundary engine={engine}>
      <Suspense fallback={<Loading />}>
        <WhenPlugInsLoaded>
          <Outlet />
        </WhenPlugInsLoaded>
      </Suspense>
    </RoutedErrorBoundary>
  )
}

function Loading() {
  const { state, loaded, total } = useEngineStatus()
  return <Spin percent={state === 'booting' && total ? (loaded / total) * 100 : undefined} />
}
