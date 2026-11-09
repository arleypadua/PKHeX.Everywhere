import { useEffect, useState } from 'react'
import { Alert, Flex } from 'antd'
import { track } from '../../googleAnalytics'
import { settings } from '../../host'
import { pkhemuStore, pkhemuStores } from './pkhemuInvite'

export function PkhemuCard({ generation, onClose }: { generation: number; onClose: () => void }) {
  const [store] = useState(() =>
    pkhemuStore({
      generation,
      userAgent: navigator.userAgent,
      maxTouchPoints: navigator.maxTouchPoints,
      dismissed: settings.readPkhemuCardDismissed(),
    }),
  )

  const report = (name: string) => track(name, { generation_id: generation, store })

  useEffect(() => {
    if (store) report('pkhemu_card_shown')
  }, [])

  if (!store) return null

  const { url, badge, badgeWidth, alt, devices } = pkhemuStores[store]

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
          <Flex vertical align="start" gap={12}>
            <span>
              pkhemu plays GB, GBC, GBA, DS and GameCube games on {devices} with save editing built in. Pause, edit your save,
              and keep playing, with no files to copy.
            </span>
            <a href={url} target="_blank" rel="noopener" onClick={join} style={{ alignSelf: 'center' }}>
              <img src={badge} alt={alt} width={badgeWidth} height={42} style={{ display: 'block' }} />
            </a>
          </Flex>
        }
      />
    </div>
  )
}
