import { execFileSync } from 'node:child_process'
import { cpSync, rmSync } from 'node:fs'
import { join, resolve } from 'node:path'

const engine = resolve(import.meta.dirname, '..')
const host = resolve(engine, '../../../PKHeX.Everywhere.Engine.Host')
const published = join(host, 'bin/Release/net10.0/publish/wwwroot/_framework')
const framework = join(engine, '_framework')

execFileSync('dotnet', ['publish', host, '-c', 'Release'], { stdio: 'inherit' })
rmSync(framework, { recursive: true, force: true })
cpSync(published, framework, { recursive: true, filter: (source) => !/\.(br|gz)$/.test(source) })
