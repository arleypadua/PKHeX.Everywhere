import { useEffect } from 'react'
import { useLoadSave } from '../pages/load/useLoadSave'

export function AutoLoadDemo() {
  const { openDemo } = useLoadSave()

  useEffect(() => {
    void openDemo()
  }, [])

  return null
}
