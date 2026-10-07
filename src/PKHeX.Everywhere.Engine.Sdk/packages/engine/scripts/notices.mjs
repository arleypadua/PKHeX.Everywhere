import { execFileSync } from 'node:child_process'
import { existsSync, readdirSync, readFileSync } from 'node:fs'
import { basename, join, relative, resolve, sep } from 'node:path'

const licenceTexts = { 'MIT License': 'MIT' }

// The opening of the Unicode License v3. Unicode-DFS-2016 says "Unicode data files" here instead.
const unicodeV3 =
  'Permission is hereby granted, free of charge, to any person obtaining a copy of data files and any associated documentation (the "Data Files") or software'


export function frameworkNotices({ host, framework, repo }) {
  const assets = JSON.parse(readFileSync(join(host, 'obj/project.assets.json'), 'utf8'))
  const packages = Object.keys(assets.packageFolders)[0]
  const [targetKey, target] = Object.entries(assets.targets).find(([key]) => key.includes('/'))
  const rid = targetKey.split('/')[1]

  const components = [
    { notice: null, files: [`${assets.project.restore.projectName}.wasm`] },
    ...libraryComponents(assets, target, packages, host, repo),
    ...runtimePackComponents(assets, packages, rid),
  ]
  return coverFramework(frameworkFiles(framework), components)
}

function coverFramework(files, components) {
  const shipped = new Set()
  const uncovered = files.filter((file) => {
    const component = components.find((candidate) => covers(candidate, file))
    if (component?.notice) shipped.add(component.notice)
    return !component
  })
  if (uncovered.length) {
    throw new Error(`No notice covers these _framework files:\n  ${uncovered.join('\n  ')}`)
  }

  const unique = new Map([...shipped].map((notice) => [JSON.stringify(notice), notice]))
  return [...unique.values()].sort((a, b) => a.name.localeCompare(b.name) || (a.version ?? '').localeCompare(b.version ?? ''))
}

function covers({ files }, file) {
  const name = basename(file)
  return files.includes(name) || files.includes(name.replace(/\.[0-9a-z]{10}(?=\.[^.]+$)/, ''))
}

function frameworkFiles(framework) {
  return readdirSync(framework, { recursive: true, withFileTypes: true })
    .filter((entry) => entry.isFile())
    .map((entry) => relative(framework, join(entry.parentPath, entry.name)).split(sep).join('/'))
}

const webcil = (file) => basename(file).replace(/\.dll$/, '.wasm')

function* libraryComponents(assets, target, packages, host, repo) {
  const submodules = readSubmodules(repo)
  for (const [key, library] of Object.entries(assets.libraries)) {
    const { runtime = {}, native = {} } = target[key] ?? {}
    const files = [...Object.keys(runtime), ...Object.keys(native)].filter((file) => !file.endsWith('/_._')).map(webcil)
    if (!files.length) continue

    if (library.type === 'package') {
      yield { notice: nugetNotice(readNuspec(join(packages, library.path))), files }
      continue
    }

    const project = resolve(host, library.path)
    const submodule = submodules.find(({ dir }) => project.startsWith(dir + sep))
    yield { notice: submodule ? submoduleNotice(submodule, project, key.split('/')[1]) : null, files }
  }
}

function* runtimePackComponents(assets, packages, rid) {
  for (const framework of Object.values(assets.project.frameworks)) {
    for (const { name, version } of framework.downloadDependencies ?? []) {
      const pack = join(packages, name.toLowerCase(), version.replace(/^\[([^,\]]+).*$/, '$1'))
      const runtimes = join(pack, 'runtimes', rid)
      const libs = listDir(join(runtimes, 'lib')).flatMap((tfm) => listDir(join(runtimes, 'lib', tfm)))
      const files = [...libs.filter((file) => file.endsWith('.dll')), ...listDir(join(runtimes, 'native'))].map(webcil)
      if (!files.length) continue

      const nuspec = readNuspec(pack)
      const icu = files.filter((file) => /^icudt.*\.dat$/.test(file))
      yield { notice: nugetNotice(nuspec), files: files.filter((file) => !icu.includes(file)) }
      if (icu.length) yield { notice: icuNotice(pack, nuspec), files: icu }
    }
  }
}

function icuNotice(pack, nuspec) {
  const notices = readFileSync(join(pack, 'THIRD-PARTY-NOTICES.TXT'), 'utf8').replace(/\s+/g, ' ')
  if (!notices.includes(unicodeV3)) {
    throw new Error(`${nuspec.id}'s THIRD-PARTY-NOTICES.TXT no longer has the Unicode-3.0 licence text that covers ICU`)
  }
  return { name: 'ICU', license: 'Unicode-3.0', source: `${nuspec.repository}/blob/${nuspec.commit}/src/runtime/eng/Version.Details.xml` }
}

function readNuspec(dir) {
  const file = readdirSync(dir).find((name) => name.endsWith('.nuspec'))
  const xml = readFileSync(join(dir, file), 'utf8')
  const element = (name) => xml.match(new RegExp(`<${name}>([^<]*)</${name}>`))?.[1]
  const repository = xml.match(/<repository\b[^>]*>/)?.[0] ?? ''
  const attribute = (name) => repository.match(new RegExp(`\\b${name}="([^"]*)"`))?.[1]
  return {
    id: element('id'),
    version: element('version'),
    license: xml.match(/<license type="expression">([^<]*)<\/license>/)?.[1],
    repository: attribute('url')?.replace(/\.git$/, ''),
    commit: attribute('commit'),
  }
}

function nugetNotice({ id, version, license, repository, commit }) {
  if (!license) throw new Error(`${id} declares no SPDX licence expression in its .nuspec`)
  if (!repository || !commit) throw new Error(`${id} declares no repository commit in its .nuspec`)
  return { name: id, version, license, source: `${repository}/tree/${commit}` }
}

function readSubmodules(repo) {
  const gitmodules = join(repo, '.gitmodules')
  if (!existsSync(gitmodules)) return []
  return readFileSync(gitmodules, 'utf8')
    .split(/^\[submodule /m)
    .slice(1)
    .map((section) => {
      const path = section.match(/^\s*path\s*=\s*(.+)$/m)[1].trim()
      const url = section.match(/^\s*url\s*=\s*(.+)$/m)[1].trim()
      return { dir: join(repo, path), url: url.replace(/\.git$/, '') }
    })
}

function submoduleNotice({ dir, url }, project, version) {
  const commit = execFileSync('git', ['rev-parse', 'HEAD'], { cwd: dir, encoding: 'utf8' }).trim()
  return { name: basename(dir), version, license: projectLicence(dir, project), source: `${url}/tree/${commit}` }
}

function projectLicence(dir, project) {
  const expression = readFileSync(project, 'utf8').match(/<PackageLicenseExpression>([^<]*)<\/PackageLicenseExpression>/)?.[1]
  if (expression) return expression

  const licence = join(dir, 'LICENSE')
  const heading = existsSync(licence) ? readFileSync(licence, 'utf8').trim().split('\n')[0].trim() : undefined
  const spdx = licenceTexts[heading]
  if (!spdx) throw new Error(`Can't tell the SPDX licence of ${basename(project)}: add a <PackageLicenseExpression> or map its LICENSE heading`)
  return spdx
}

function listDir(dir) {
  return existsSync(dir) ? readdirSync(dir) : []
}
