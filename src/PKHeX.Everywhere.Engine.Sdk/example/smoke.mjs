import { createReadStream, existsSync, readFileSync, statSync } from 'node:fs'
import { createServer } from 'node:http'
import { extname, join, relative, resolve } from 'node:path'
import { chromium } from 'playwright'

const contentTypes = {
  '.html': 'text/html',
  '.js': 'text/javascript',
  '.wasm': 'application/wasm',
  '.json': 'application/json',
  '.dat': 'application/octet-stream',
}

function serve(root, prefix = '/') {
  const server = createServer((req, res) => {
    const path = decodeURIComponent(new URL(req.url, 'http://localhost').pathname)
    const file = join(root, path.startsWith(prefix) ? path.slice(prefix.length) : '\0')
    const found = !relative(root, file).startsWith('..') && existsSync(file) && statSync(file).isFile()
    const target = found ? file : prefix === '/' ? join(root, 'index.html') : null
    res.setHeader('Access-Control-Allow-Origin', '*')
    if (!target) {
      res.statusCode = 404
      return res.end()
    }
    res.setHeader('Content-Type', contentTypes[extname(target)] ?? 'application/octet-stream')
    createReadStream(target).pipe(res)
  })
  return new Promise((done) => server.listen(0, '127.0.0.1', () => done(server)))
}

const selfHosted = process.argv[2] === 'self-hosted'
const engineDir = resolve(import.meta.dirname, 'node_modules/@pkhex-everywhere/engine')
const { version } = JSON.parse(readFileSync(join(engineDir, 'package.json'), 'utf8'))
const cdnPrefix = `/npm/@pkhex-everywhere/engine@${version}/`

const app = await serve(resolve(import.meta.dirname, selfHosted ? 'dist-self-hosted' : 'dist'))
const cdn = await serve(engineDir, cdnPrefix)
const appUrl = `http://127.0.0.1:${app.address().port}/`
const cdnOrigin = `http://127.0.0.1:${cdn.address().port}`

const browser = await chromium.launch()
try {
  const page = await browser.newPage()
  const errors = []
  page.on('pageerror', (error) => errors.push(error.message))
  page.on('console', (message) => message.type() === 'error' && errors.push(message.text()))

  const cdnRequests = []
  await page.route('https://cdn.jsdelivr.net/**', async (route) => {
    const url = new URL(route.request().url())
    cdnRequests.push(url.pathname)
    if (selfHosted) return route.abort()
    await route.fulfill({ response: await route.fetch({ url: cdnOrigin + url.pathname }) })
  })
  const appRequests = []
  page.on('request', (request) => {
    if (request.url().startsWith(appUrl)) appRequests.push(new URL(request.url()).pathname)
  })

  await page.goto(appUrl)
  const party = await page
    .locator('#party li')
    .first()
    .waitFor({ timeout: 60_000 })
    .then(() => page.locator('#party li').allTextContents())
    .catch((error) => {
      throw new Error(`The party never showed up. Browser errors:\n${errors.join('\n')}`, { cause: error })
    })

  if (selfHosted) {
    if (cdnRequests.length) throw new Error(`The self-hosted app requested jsDelivr: ${cdnRequests.join(', ')}`)
    if (!appRequests.includes('/_framework/dotnet.js')) throw new Error("dotnet.js didn't load from the app's own site.")
  } else if (!cdnRequests.includes(`${cdnPrefix}_framework/dotnet.js`))
    throw new Error(`dotnet.js didn't load from jsDelivr. CDN requests: ${cdnRequests.join(', ') || 'none'}`)

  const expected = ['Torchic', 'Wurmple', 'Wingull']
  if (party.join() !== expected.join()) throw new Error(`Expected the party ${expected.join(', ')} but got ${party.join(', ')}`)

  console.log(`${selfHosted ? 'Self-hosted' : 'CDN'} party: ${party.join(', ')}`)
} finally {
  await browser.close()
  app.close()
  cdn.close()
}
