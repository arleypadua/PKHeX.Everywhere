import { use, type ReactNode } from 'react'
import { plugInsLoaded } from '../app'

export function WhenPlugInsLoaded({ children }: { children: ReactNode }) {
  use(plugInsLoaded)
  return children
}
