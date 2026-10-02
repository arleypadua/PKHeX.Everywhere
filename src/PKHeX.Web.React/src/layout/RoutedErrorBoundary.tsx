import { Component, type ReactNode } from 'react'
import { Alert, Descriptions, Typography } from 'antd'
import type { Engine, SaveVersion } from '@pkhex-everywhere/engine'
import { issueLink, reportPageError, toPageError, type PageError } from './pageError'

interface Props {
  engine: Engine
  children: ReactNode
}

interface State {
  error?: PageError
  game?: SaveVersion | null
}

export class RoutedErrorBoundary extends Component<Props, State> {
  state: State = {}
  private unsubscribe?: () => void

  static getDerivedStateFromError(error: unknown): State {
    return { error: toPageError(error), game: undefined }
  }

  componentDidCatch(error: unknown) {
    const pageError = this.state.error ?? toPageError(error)
    void this.props.engine.game
      .version()
      .catch(() => null)
      .then((game) => {
        reportPageError(error, pageError, game)
        if (this.state.error === pageError) this.setState({ game })
      })
  }

  componentDidMount() {
    this.unsubscribe = this.props.engine.subscribe(['*'], this.dismiss)
  }

  componentWillUnmount() {
    this.unsubscribe?.()
  }

  dismiss = () => {
    if (this.state.error !== undefined) this.setState({ error: undefined, game: undefined })
  }

  render() {
    const { error, game } = this.state
    if (error === undefined) return this.props.children
    const link = game === undefined ? null : issueLink(error, game)

    return (
      <Alert
        type="error"
        title="Error"
        closable={{ closeIcon: 'Dismiss', onClose: this.dismiss }}
        description={
          <>
            <Typography.Paragraph>
              {error.unsupportedFormat
                ? 'This Pokémon format is not compatible with the currently loaded save file yet. Support for this workflow is on the roadmap.'
                : 'Whoops, some unexpected error happened.'}
            </Typography.Paragraph>
            {link && (
              <Typography.Paragraph>
                <a href={link} target="_blank" rel="noreferrer">
                  Click here
                </a>{' '}
                to file an issue in GitHub.
              </Typography.Paragraph>
            )}
            <Descriptions
              bordered
              size="small"
              column={{ xs: 1, md: 3 }}
              items={[
                { key: 'id', label: 'Id', children: error.id },
                { key: 'type', label: 'Type', children: error.type },
                { key: 'message', label: 'Message', children: error.message },
              ]}
            />
          </>
        }
      />
    )
  }
}
