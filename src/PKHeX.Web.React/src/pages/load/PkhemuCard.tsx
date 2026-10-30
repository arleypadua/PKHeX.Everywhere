import { useEffect, useState } from 'react'
import { Alert, Flex } from 'antd'
import { track } from '../../googleAnalytics'
import { settings } from '../../host'
import { pkhemuPlayUrl, showPkhemuCard } from './pkhemuInvite'

export function PkhemuCard({ generation, onClose }: { generation: number; onClose: () => void }) {
  const [visible] = useState(() =>
    showPkhemuCard({ generation, userAgent: navigator.userAgent, dismissed: settings.readPkhemuCardDismissed() }),
  )

  const report = (name: string) => track(name, { generation_id: generation })

  useEffect(() => {
    if (visible) report('pkhemu_card_shown')
  }, [])

  if (!visible) return null

  const join = () => {
    settings.writePkhemuCardDismissed()
    report('pkhemu_card_click')
  }

  const dismiss = () => {
    settings.writePkhemuCardDismissed()
    report('pkhemu_card_dismiss')
    onClose()
  }

  return (
    <div style={{ width: '100%', maxWidth: 560 }}>
      <Alert
        type="info"
        showIcon
        closable={{ onClose: dismiss }}
        title="Edit and play in one app"
        description={
          <Flex vertical align="start" gap={8}>
            <span>
              pkhemu plays GB, GBC, GBA and DS games on Android with save editing built in. Pause, edit your save, and
              keep playing, with no files to copy.
            </span>
            {/* The badge PNG has transparent padding around it; the negative margin cancels it. */}
            <a
              href={pkhemuPlayUrl}
              target="_blank"
              rel="noopener"
              onClick={join}
              style={{ alignSelf: 'center', margin: -10 }}
            >
              <img src="/google-play-badge.png" alt="Get it on Google Play" height={62} />
            </a>
          </Flex>
        }
      />
    </div>
  )
}
