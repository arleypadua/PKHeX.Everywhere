import { Component, type ReactNode } from 'react'
import { Result } from 'antd'
import { EngineError, type Engine } from '@pkhex-everywhere/engine'
import { captureError } from './sentry'

interface Props {
  engine: Engine
  children: ReactNode
}

export class PageErrorBoundary extends Component<Props, { error?: unknown }> {
  state: { error?: unknown } = {}
  private unsubscribe?: () => void

  static getDerivedStateFromError(error: unknown) {
    return { error }
  }

  componentDidCatch(error: unknown) {
    if (!(error instanceof EngineError)) captureError(error)
  }

  componentDidMount() {
    this.unsubscribe = this.props.engine.subscribe(['*'], () => {
      if (this.state.error !== undefined) this.setState({ error: undefined })
    })
  }

  componentWillUnmount() {
    this.unsubscribe?.()
  }

  render() {
    if (this.state.error === undefined) return this.props.children
    const message = this.state.error instanceof Error ? this.state.error.message : String(this.state.error)
    return <Result status="error" title="Something went wrong" subTitle={message} />
  }
}
