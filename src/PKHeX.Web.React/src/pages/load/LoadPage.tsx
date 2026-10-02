import { useRef, type ChangeEvent } from 'react'
import { EllipsisOutlined, ExportOutlined, FolderOpenOutlined } from '@ant-design/icons'
import { Button, Descriptions, Dropdown, Flex, Space } from 'antd'
import type { SaveSummary } from '@pkhex-everywhere/engine'
import { useEngine, useLoadedGame, useQuery } from '@pkhex-everywhere/react'
import { fromBase64 } from '../../base64'
import { AdSlot } from '../../components/AdSlot'
import { downloadFile, useNavigate } from '../../host'
import { journey } from '../../layout/journey'
import { routes } from '../../routes'
import { useLoadSave } from './useLoadSave'

const topAdSlot = '5784199745'
const bottomAdSlot = '5148940774'
const swordVersionId = 44

export default function LoadPage() {
  const { game } = useLoadedGame()
  const { openFile, openDemo } = useLoadSave()
  const fileInput = useRef<HTMLInputElement>(null)

  const openFilePicker = () => fileInput.current?.click()

  const loadFile = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''
    if (file) await openFile(file)
  }

  return (
    <Flex vertical align="center" gap={20} style={{ width: '100%' }}>
      <AdSlot slot={topAdSlot} />
      {game ? <LoadedGame game={game} onOpen={openFilePicker} /> : <NoGame onOpen={openFilePicker} onDemo={openDemo} />}
      <AdSlot slot={bottomAdSlot} />
      <input ref={fileInput} type="file" hidden onChange={loadFile} />
    </Flex>
  )
}

function LoadedGame({ game, onOpen }: { game: SaveSummary; onOpen: () => void }) {
  const engine = useEngine()

  const exportSave = async () => {
    const { bytes, fileName } = await engine.game.export()
    downloadFile(fromBase64(bytes), fileName)
  }

  return (
    <>
      <Descriptions
        title="Loaded Game"
        bordered
        size="small"
        style={{ width: '100%' }}
        items={[
          { key: 'file', label: 'File', children: game.fileName },
          { key: 'version', label: 'Version', children: game.version },
        ]}
      />
      <Flex vertical gap={20} style={{ width: '100%', maxWidth: 300 }}>
        <Button type="primary" icon={<ExportOutlined aria-hidden />} onClick={exportSave}>
          Export
        </Button>
        <Button icon={<FolderOpenOutlined aria-hidden />} onClick={onOpen}>
          Open new...
        </Button>
      </Flex>
    </>
  )
}

function NoGame({ onOpen, onDemo }: { onOpen: () => void; onDemo: () => void }) {
  const engine = useEngine()
  const navigate = useNavigate()
  const goHome = () => {
    journey.reachedHome()
    void navigate(routes.home)
  }
  const versions = useQuery('game.blankVersions')

  return (
    <Flex vertical align="center" gap={20} style={{ width: '100%', maxWidth: 300 }}>
      <Button style={{ minWidth: 150 }} onClick={goHome}>
        Home
      </Button>
      <Button type="primary" icon={<FolderOpenOutlined aria-hidden />} style={{ minWidth: 150 }} onClick={onOpen}>
        Open
      </Button>
      <Space.Compact style={{ minWidth: 150 }}>
        <Button style={{ flexGrow: 1 }} onClick={() => engine.game.loadBlank(swordVersionId)}>
          Empty
        </Button>
        <Dropdown
          trigger={['click']}
          menu={{
            items: versions.map((version) => ({ key: version.id, label: version.name })),
            onClick: ({ key }) => engine.game.loadBlank(Number(key)),
            style: { maxHeight: 250, overflowY: 'auto' },
          }}
        >
          <Button icon={<EllipsisOutlined />} aria-label="Empty save versions" />
        </Dropdown>
      </Space.Compact>
      <Button style={{ minWidth: 150 }} onClick={onDemo}>
        Demo
      </Button>
    </Flex>
  )
}
