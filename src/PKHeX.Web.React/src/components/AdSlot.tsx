import { useEffect } from 'react'
import { adSenseClient } from '../constants'

declare global {
  var adsbygoogle: unknown[] | undefined
}

export function AdSlot({ slot, format = 'auto' }: { slot: string; format?: 'auto' | 'autorelaxed' }) {
  useEffect(() => {
    ;(globalThis.adsbygoogle ??= []).push({})
  }, [])

  return (
    <ins
      className="adsbygoogle"
      style={{ display: 'block', width: '100%', maxHeight: format === 'auto' ? 300 : undefined }}
      data-ad-client={adSenseClient}
      data-ad-slot={slot}
      data-ad-format={format}
      data-full-width-responsive="true"
    />
  )
}
