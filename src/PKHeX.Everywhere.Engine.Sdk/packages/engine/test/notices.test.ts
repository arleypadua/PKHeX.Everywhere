import { execFileSync } from 'node:child_process'
import { mkdirSync, mkdtempSync, rmSync, writeFileSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { dirname, join } from 'node:path'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { frameworkNotices } from '../scripts/notices.mjs'

const unicodeLicence = `License notice for Unicode data
-------------------------------

Permission is hereby granted, free of charge, to any person obtaining a
copy of data files and any associated documentation (the "Data Files") or
software and any associated documentation (the "Software") to deal in the
Data Files or Software without restriction.`

let root: string
let repo: string
let host: string
let framework: string
let packages: string
let libCommit: string

const write = (path: string, contents = '') => {
  mkdirSync(dirname(path), { recursive: true })
  writeFileSync(path, contents)
}

const git = (cwd: string, ...args: string[]) =>
  execFileSync('git', ['-c', 'user.name=test', '-c', 'user.email=test@example.com', ...args], { cwd, encoding: 'utf8' }).trim()

const nuspec = (id: string, version: string, license?: string) => `<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>${id}</id>
    <version>${version}</version>
    ${license ? `<license type="expression">${license}</license>` : ''}
    <repository type="git" url="https://github.com/dotnet/dotnet" commit="c0ffee" />
  </metadata>
</package>`

const writeAssets = (libraries: Record<string, { type: string; path: string; runtime?: string[] }>) =>
  write(
    join(host, 'obj/project.assets.json'),
    JSON.stringify({
      targets: {
        'net10.0': {},
        'net10.0/browser-wasm': Object.fromEntries(
          Object.entries(libraries).map(([key, { type, runtime }]) => [
            key,
            { type, ...(runtime && { runtime: Object.fromEntries(runtime.map((file) => [file, {}])) }) },
          ]),
        ),
      },
      libraries: Object.fromEntries(Object.entries(libraries).map(([key, { type, path }]) => [key, { type, path }])),
      packageFolders: { [packages + '/']: {} },
      project: {
        restore: { projectName: 'Host' },
        frameworks: {
          'net10.0': { downloadDependencies: [{ name: 'Microsoft.NETCore.App.Runtime.Mono.browser-wasm', version: '[10.0.5, 10.0.5]' }] },
        },
      },
    }),
  )

const ship = (...files: string[]) => files.forEach((file) => write(join(framework, file)))

beforeEach(() => {
  root = mkdtempSync(join(tmpdir(), 'pkhex-engine-notices-'))
  repo = join(root, 'repo')
  host = join(repo, 'src/Host')
  framework = join(root, '_framework')
  packages = join(root, 'packages')

  write(join(repo, '.gitmodules'), '[submodule "external/Lib"]\n\tpath = external/Lib\n\turl = https://github.com/someone/Lib.git\n')
  const lib = join(repo, 'external/Lib')
  write(join(lib, 'Lib.Core/Lib.Core.csproj'), '<Project><PropertyGroup><PackageLicenseExpression>GPL-3.0-or-later</PackageLicenseExpression></PropertyGroup></Project>')
  write(join(lib, 'Lib.Extra/Lib.Extra.csproj'), '<Project />')
  write(join(lib, 'LICENSE'), 'MIT License\n\nCopyright (c) someone\n')
  git(lib, 'init', '-q')
  git(lib, 'add', '.')
  git(lib, 'commit', '-q', '-m', 'init')
  libCommit = git(lib, 'rev-parse', 'HEAD')

  write(join(packages, 'some.package/2.0.0/some.package.nuspec'), nuspec('Some.Package', '2.0.0', 'MIT'))
  write(join(packages, 'build.only/1.0.0/build.only.nuspec'), nuspec('Build.Only', '1.0.0', 'MIT'))

  const runtime = join(packages, 'microsoft.netcore.app.runtime.mono.browser-wasm/10.0.5')
  write(join(runtime, 'microsoft.netcore.app.runtime.mono.browser-wasm.nuspec'), nuspec('Microsoft.NETCore.App.Runtime.Mono.browser-wasm', '10.0.5', 'MIT'))
  write(join(runtime, 'THIRD-PARTY-NOTICES.TXT'), unicodeLicence)
  write(join(runtime, 'runtimes/browser-wasm/lib/net10.0/System.Runtime.dll'))
  write(join(runtime, 'runtimes/browser-wasm/native/System.Private.CoreLib.dll'))
  write(join(runtime, 'runtimes/browser-wasm/native/dotnet.js'))
  write(join(runtime, 'runtimes/browser-wasm/native/dotnet.native.wasm'))
  write(join(runtime, 'runtimes/browser-wasm/native/icudt_CJK.dat'))

  writeAssets({
    'Some.Package/2.0.0': { type: 'package', path: 'some.package/2.0.0', runtime: ['lib/net10.0/Some.Package.dll'] },
    'Build.Only/1.0.0': { type: 'package', path: 'build.only/1.0.0' },
    'Lib.Core/1.2.3': { type: 'project', path: '../../external/Lib/Lib.Core/Lib.Core.csproj', runtime: ['bin/placeholder/Lib.Core.dll'] },
    'Lib.Extra/1.2.4': { type: 'project', path: '../../external/Lib/Lib.Extra/Lib.Extra.csproj', runtime: ['bin/placeholder/Lib.Extra.dll'] },
    'Engine/1.0.0': { type: 'project', path: '../Engine/Engine.csproj', runtime: ['bin/placeholder/Engine.dll'] },
  })

  ship(
    'Some.Package.abcdefghij.wasm',
    'Lib.Core.0123456789.wasm',
    'Lib.Extra.klmnopqrst.wasm',
    'Engine.uvwxyz0123.wasm',
    'Host.a1b2c3d4e5.wasm',
    'System.Private.CoreLib.f6g7h8i9j0.wasm',
    'System.Runtime.4567abcdef.wasm',
    'dotnet.js',
    'dotnet.native.ghijklmnop.wasm',
    'icudt_CJK.qrstuvwxyz.dat',
  )
})

afterEach(() => rmSync(root, { recursive: true, force: true }))

const notices = () => frameworkNotices({ host, framework, repo })

describe('frameworkNotices', () => {
  it('lists every third-party component that has files in _framework', () => {
    expect(notices()).toEqual([
      {
        name: 'ICU',
        license: 'Unicode-3.0',
        source: 'https://github.com/dotnet/dotnet/blob/c0ffee/src/runtime/eng/Version.Details.xml',
      },
      {
        name: 'Lib',
        version: '1.2.3',
        license: 'GPL-3.0-or-later',
        source: `https://github.com/someone/Lib/tree/${libCommit}`,
      },
      { name: 'Lib', version: '1.2.4', license: 'MIT', source: `https://github.com/someone/Lib/tree/${libCommit}` },
      {
        name: 'Microsoft.NETCore.App.Runtime.Mono.browser-wasm',
        version: '10.0.5',
        license: 'MIT',
        source: 'https://github.com/dotnet/dotnet/tree/c0ffee',
      },
      { name: 'Some.Package', version: '2.0.0', license: 'MIT', source: 'https://github.com/dotnet/dotnet/tree/c0ffee' },
    ])
  })

  it('gives every entry a name, licence and https source', () => {
    for (const notice of notices()) {
      expect(notice.name).toBeTruthy()
      expect(notice.license).toBeTruthy()
      expect(notice.source).toMatch(/^https:\/\//)
    }
  })

  it('leaves out a component with no file in _framework', () => {
    rmSync(join(framework, 'Some.Package.abcdefghij.wasm'))

    expect(notices().map((notice) => notice.name)).not.toContain('Some.Package')
  })

  it('fails naming a _framework file no component covers', () => {
    ship('Unknown.Library.abcdefghij.wasm')

    expect(notices).toThrow(/Unknown\.Library\.abcdefghij\.wasm/)
  })

  it('fails when a package declares no SPDX licence expression', () => {
    write(join(packages, 'some.package/2.0.0/some.package.nuspec'), nuspec('Some.Package', '2.0.0'))

    expect(notices).toThrow(/Some\.Package/)
  })

  it('fails when the runtime pack no longer ships ICU under the Unicode licence', () => {
    write(join(packages, 'microsoft.netcore.app.runtime.mono.browser-wasm/10.0.5/THIRD-PARTY-NOTICES.TXT'), 'License notice for zlib')

    expect(notices).toThrow(/Unicode-3\.0/)
  })
})
