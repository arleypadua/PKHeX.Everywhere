export interface Notice {
  name: string
  version?: string
  license: string
  source: string
}

export function frameworkNotices(paths: { host: string; framework: string; repo: string }): Notice[]
