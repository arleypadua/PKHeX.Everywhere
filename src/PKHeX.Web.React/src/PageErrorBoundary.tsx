import { Component, type ReactNode } from 'react'
import { Result } from 'antd'

export class PageErrorBoundary extends Component<{ children: ReactNode }, { error?: unknown }> {
  state: { error?: unknown } = {}

  static getDerivedStateFromError(error: unknown) {
    return { error }
  }

  render() {
    if (this.state.error === undefined) return this.props.children
    const message = this.state.error instanceof Error ? this.state.error.message : String(this.state.error)
    return <Result status="error" title="Something went wrong" subTitle={message} />
  }
}
