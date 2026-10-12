import { useEffect, useState } from 'react'
import { Alert, Button, Flex } from 'antd'
import { track } from '../../googleAnalytics'
import { settings } from '../../host'
import { pkhemuJoinUrl, showPkhemuCard } from './pkhemuInvite'

export function PkhemuCard({ generation, onClose }: { generation: number; onClose: () => void }) {
  const [preview] = useState(() => settings.readPkhemuPreview())
  const [visible] = useState(() =>
    showPkhemuCard({ generation, userAgent: navigator.userAgent, preview, dismissed: settings.readPkhemuCardDismissed() }),
  )

  const report = (name: string) => {
    if (!preview) track(name, { generation_id: generation })
  }

  useEffect(() => {
    if (visible && !preview) track('pkhemu_card_shown', { generation_id: generation })
  }, [visible, preview, generation])

  if (!visible) return null

  const remember = () => {
    if (!preview) settings.writePkhemuCardDismissed()
  }

  const join = () => {
    remember()
    report('pkhemu_card_click')
  }

  const dismiss = () => {
    remember()
    report('pkhemu_card_dismiss')
    onClose()
  }

  return (
    <Alert
      type="info"
      showIcon
      closable={{ onClose: dismiss }}
      title="Edit and play in one app"
      description={
        <Flex vertical align="start" gap={8}>
          <span>
            pkhemu plays GB, GBC, GBA and DS games on Android with save editing built in. Pause, edit your save, and keep
            playing, with no files to copy. It's in closed testing, and we need testers.
          </span>
          <Button type="primary" href={pkhemuJoinUrl} target="_blank" rel="noopener" onClick={join}>
            Join the test
          </Button>
        </Flex>
      }
    />
  )
}
