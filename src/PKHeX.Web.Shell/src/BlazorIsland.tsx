import { useEffect, useRef } from 'react'
import { boot } from './engine'

export function BlazorIsland({ page }: { page: string }) {
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    const element = ref.current!
    const component = boot().then(() => window.Blazor.rootComponents.add(element, 'pkhex-island', { page }))
    component.then(() => performance.mark(`island-${page}`))
    return () => {
      component.then((c) => c.dispose())
    }
  }, [page])

  return <div ref={ref} data-island={page} />
}
