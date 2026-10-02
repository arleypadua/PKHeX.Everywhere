import { Alert, Button, Descriptions, Empty, Flex, Typography } from 'antd'
import { useEngine, useQuery } from '@pkhex-everywhere/react'
import { PageHeader } from '../../components/PageHeader'

export default function PlugInErrorsPage() {
  const engine = useEngine()
  const failures = useQuery('plugins.failures')

  return (
    <Flex vertical gap={20}>
      <PageHeader title="Plug-in errors" />
      {failures.length === 0 && <Empty description="No plug-in errors" />}
      {failures.map((failure) => (
        <Alert
          key={failure.id}
          type="error"
          title={failure.message}
          description={
            <Descriptions
              size="small"
              column={1}
              items={[
                { key: 'plugin', label: 'Plug-in', children: failure.plugInId },
                { key: 'hook', label: 'Hook', children: failure.hookId },
                {
                  key: 'stackTrace',
                  label: 'Stack trace',
                  children: (
                    <Typography.Text code style={{ whiteSpace: 'pre-wrap' }}>
                      {failure.stackTrace}
                    </Typography.Text>
                  ),
                },
              ]}
            />
          }
          action={
            <Button size="small" onClick={() => void engine.plugins.dismissFailure(failure.id)}>
              Dismiss
            </Button>
          }
        />
      ))}
    </Flex>
  )
}
