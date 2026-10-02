import { useEffect, useState } from 'react'
import { Alert } from 'antd'
import { Link } from 'react-router'
import { settings } from '../host'
import { checkUnseenNews, markNewsSeen } from '../news'
import { routes } from '../routes'

export function NewsBanner() {
  const [news] = useState(() => checkUnseenNews(settings, new Date()))
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    if (!news.unseen) return
    const show = setTimeout(() => setVisible(true), 2000)
    const dismiss = setTimeout(() => {
      setVisible(false)
      markNewsSeen(settings)
    }, 12000)
    return () => {
      clearTimeout(show)
      clearTimeout(dismiss)
    }
  }, [news])

  return (
    <div
      style={{
        position: 'absolute',
        top: 0,
        width: '100%',
        zIndex: 9,
        transition: 'opacity 0.5s ease-in-out',
        opacity: visible ? 1 : 0,
        pointerEvents: visible ? 'auto' : 'none',
      }}
    >
      <Alert
        type="info"
        banner
        showIcon={false}
        closable={{ closeIcon: <span style={{ fontSize: '0.7em' }}>Dismiss</span>, onClose: () => markNewsSeen(settings) }}
        title={
          <span style={{ fontSize: '0.7em', display: 'flex', justifyContent: 'center' }}>
            <div>
              PKHeX.Web just got updated
              <br />
              See <Link to={routes.releaseNotes(news.since)}>what's new</Link>.
            </div>
          </span>
        }
      />
    </div>
  )
}
