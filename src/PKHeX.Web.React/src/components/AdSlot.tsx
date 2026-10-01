import { useEffect } from 'react'
import { adSenseClient } from '../constants'

declare global {
  var adsbygoogle: unknown[] | undefined
}

export function AdSlot({ slot }: { slot: string }) {
  useEffect(() => {
    ;(globalThis.adsbygoogle ??= []).push({})
  }, [])

  return (
    <ins
      className="adsbygoogle"
      style={{ display: 'block', width: '100%', maxHeight: 300 }}
      data-ad-client={adSenseClient}
      data-ad-slot={slot}
      data-ad-format="auto"
      data-full-width-responsive="true"
    />
  )
}
