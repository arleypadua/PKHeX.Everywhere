import { useState } from 'react'
import { App } from 'antd'
import { EngineError, type ItemHandle } from '@pkhex-everywhere/engine'
import { useInventory } from '@pkhex-everywhere/react'

export function useSetItem() {
  const { setItem } = useInventory()
  const { notification } = App.useApp()
  const [saving, setSaving] = useState(false)

  const save = async (at: ItemHandle, count: number) => {
    setSaving(true)
    try {
      await setItem(at, count)
      return true
    } catch (error) {
      if (!(error instanceof EngineError) || (error.code !== 'out-of-range' && error.code !== 'pouch-full')) throw error
      notification.error({ title: "Couldn't change the item", description: error.message })
      return false
    } finally {
      setSaving(false)
    }
  }

  return { save, saving }
}
