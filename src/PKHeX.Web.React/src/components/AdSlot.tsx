import { useEffect } from 'react'
import { adSenseClient } from '../constants'

declare global {
  var adsbygoogle: unknown[] | undefined
}

export function AdSlot({ slot, format = 'auto' }: { slot: string; format?: 'auto' | 'autorelaxed' | 'horizontal' }) {
  useEffect(() => {
    try {
      ;(globalThis.adsbygoogle ??= []).push({})
    } catch {}
  }, [])

  return (
    <ins
      className="adsbygoogle"
      style={{ display: 'block', width: '100%' }}
      data-ad-client={adSenseClient}
      data-ad-slot={slot}
      data-ad-format={format}
      data-full-width-responsive="true"
    />
  )
}
