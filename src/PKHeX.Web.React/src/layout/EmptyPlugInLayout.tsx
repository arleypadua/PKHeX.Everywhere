import { Suspense, useState } from 'react'
import { Spin } from 'antd'
import { Outlet } from 'react-router'
import { engine } from '../app'
import { PageErrorBoundary } from '../PageErrorBoundary'
import { WhenPlugInsLoaded } from './WhenPlugInsLoaded'

export function EmptyPlugInLayout() {
  const [hovered, setHovered] = useState(false)

  return (
    <>
      <div
        role="button"
        aria-label="Close"
        onClick={() => history.back()}
        onMouseEnter={() => setHovered(true)}
        onMouseLeave={() => setHovered(false)}
        style={{
          width: 25,
          height: 25,
          padding: 10,
          color: 'white',
          position: 'absolute',
          top: 0,
          left: 0,
          opacity: hovered ? 1 : 0.5,
          zIndex: 9999,
          cursor: 'pointer',
          transition: 'opacity 0.3s ease-in-out',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        ✖
      </div>
      <PageErrorBoundary engine={engine}>
        <Suspense fallback={<Spin />}>
          <WhenPlugInsLoaded>
            <Outlet />
          </WhenPlugInsLoaded>
        </Suspense>
      </PageErrorBoundary>
    </>
  )
}
