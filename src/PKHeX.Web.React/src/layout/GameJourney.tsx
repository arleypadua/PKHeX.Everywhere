import { useEffect } from 'react'
import { useEngine } from '@pkhex-everywhere/react'
import { useNavigate } from '../host'
import { useLoadSave } from '../pages/load/useLoadSave'
import { routeAfter } from './journey'

export function GameJourney() {
  const engine = useEngine()
  const navigate = useNavigate()

  useEffect(
    () =>
      engine.onEvent((event) => {
        const route = routeAfter(event)
        if (route) void navigate(route)
      }),
    [engine, navigate],
  )

  return null
}

export function AutoLoadDemo() {
  const { openDemo } = useLoadSave()

  useEffect(() => {
    void openDemo()
  }, [])

  return null
}
