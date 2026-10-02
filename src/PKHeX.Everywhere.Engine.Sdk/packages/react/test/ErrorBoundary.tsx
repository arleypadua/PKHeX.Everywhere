import { Component, type ReactNode } from 'react'

export class ErrorBoundary extends Component<{ children: ReactNode; onError?: (error: Error) => void }, { error?: Error }> {
  state: { error?: Error } = {}

  static getDerivedStateFromError(error: Error) {
    return { error }
  }

  componentDidCatch(error: Error) {
    this.props.onError?.(error)
  }

  render() {
    return this.state.error ? <p>error: {this.state.error.message}</p> : this.props.children
  }
}
