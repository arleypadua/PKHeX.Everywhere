const covers = (path: string, other: string) => path === '*' || other === path || other.startsWith(`${path}/`)

const overlaps = (a: string, b: string) => covers(a, b) || covers(b, a)

export const affects = (changed: readonly string[], read: readonly string[]) =>
  changed.some((c) => read.some((r) => overlaps(c, r)))
