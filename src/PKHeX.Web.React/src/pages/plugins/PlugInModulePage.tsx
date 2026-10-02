import { useEffect, useRef, type CSSProperties } from 'react'
import { Flex } from 'antd'
import type { PageContext } from '@pkhex-everywhere/plugin-sdk'
import { useEngine, useQuery } from '@pkhex-everywhere/react'
import { fromBase64, toBase64 } from '../../base64'
import { PageHeader } from '../../components/PageHeader'
import { useNavigate, useTheme } from '../../host'
import { mountPageModule, settingValue } from '../../plugins/pageModule'

interface PlugInModulePageProps {
  plugInId?: string
  path?: string
}

export default function PlugInModulePage({ plugInId, path }: PlugInModulePageProps) {
  const page = useQuery('plugins.pages').find((p) => p.plugInId === plugInId && p.path === path)
  if (!page) return null
  if (page.layout === 'empty') return <PageModule plugInId={page.plugInId} path={page.path} style={{ height: '100%' }} />

  return (
    <Flex vertical gap={20}>
      <PageHeader title={page.title ?? page.path} />
      <PageModule plugInId={page.plugInId} path={page.path} />
    </Flex>
  )
}

function PageModule({ plugInId, path, style }: { plugInId: string; path: string; style?: CSSProperties }) {
  const engine = useEngine()
  const navigate = useNavigate()
  const theme = useTheme()
  const themeRef = useRef(theme)
  themeRef.current = theme
  const element = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const ctx: PageContext = {
      plugInId,
      get theme() {
        return themeRef.current
      },
      async getSave() {
        const save = await engine.game.file()
        return save && { bytes: fromBase64(save.bytes), fileName: save.fileName, version: save.version }
      },
      getSetting: async (key) => settingValue(await engine.plugins.setting(plugInId, key)),
      async loadSave(bytes, fileName) {
        await engine.game.load(toBase64(bytes), fileName)
        await navigate('/')
      },
      navigate: (url) => void navigate(url),
    }

    return mountPageModule(element.current!, engine.plugins.pageModule(plugInId, path), ctx)
  }, [engine, navigate, plugInId, path])

  return <div ref={element} style={{ width: '100%', ...style }} />
}
