import { Alert } from 'antd'
import type { Legality } from '@pkhex-everywhere/engine'

interface LegalityBannerProps {
  legality: Legality
}

export function LegalityBanner({ legality }: LegalityBannerProps) {
  if (legality.messages.length === 0) return null

  return (
    <Alert
      key={legality.messages.join('\n')}
      type="error"
      title="Not legal"
      closable
      description={
        <ul>
          {legality.messages.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </ul>
      }
    />
  )
}
