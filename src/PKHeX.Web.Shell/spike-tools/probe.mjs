import { chromium } from 'playwright-core'
import { serve } from './serve.mjs'

const [root, mode, runs = '5', throttle] = process.argv.slice(2)
const server = await serve(root, 0)
const base = `http://127.0.0.1:${server.address().port}`
const browser = await chromium.launch({ executablePath: `${process.env.HOME}/Library/Caches/ms-playwright/chromium-1243/chrome-mac-arm64/Google Chrome for Testing.app/Contents/MacOS/Google Chrome for Testing` })

const results = []
for (let i = 0; i < Number(runs); i++) {
  const context = await browser.newContext()
  await context.route(/^https?:\/\/(?!127\.0\.0\.1)/, (r) => r.abort())
  const page = await context.newPage()
  const cdp = await context.newCDPSession(page)
  await cdp.send('Network.enable')
  if (throttle) await cdp.send('Network.emulateNetworkConditions', { offline: false, latency: 40, downloadThroughput: (20 * 1024 * 1024) / 8, uploadThroughput: (5 * 1024 * 1024) / 8 })
  let bytes = 0
  cdp.on('Network.loadingFinished', (e) => (bytes += e.encodedDataLength))
  const errors = []
  page.on('pageerror', (e) => errors.push(e.message))
  page.on('console', (m) => m.type() === 'error' && errors.push(m.text()))
  const now = () => page.evaluate(() => Math.round(performance.now()))
  const r = { run: i }
  if (mode === 'baseline') {
    await page.goto(base + '/')
    await page.getByText('Demo', { exact: true }).first().waitFor({ timeout: 120000 })
    r.interactiveMs = await now()
  } else {
    await page.goto(base + '/items-blazor')
    await page.getByText('Load', { exact: true }).waitFor()
    r.shellPaintMs = await now()
    await page.waitForFunction(() => performance.getEntriesByName('engine-ready').length > 0, null, { timeout: 120000 })
    r.engineReadyMs = Math.round(await page.evaluate(() => performance.getEntriesByName('engine-ready')[0].startTime))
    await page.getByText('Items', { exact: true }).last().waitFor({ timeout: 120000 })
    r.interactiveMs = await now()
  }
  await page.waitForLoadState('networkidle')
  r.transferredKB = Math.round(bytes / 1024)
  r.errors = errors.slice(0, 3)
  results.push(r)
  await context.close()
}
console.log(JSON.stringify(results, null, 1))
const med = (k) => results.map((r) => r[k]).filter((x) => x != null).sort((a, b) => a - b)[Math.floor(results.length / 2)]
console.log('median', { interactiveMs: med('interactiveMs'), shellPaintMs: med('shellPaintMs'), engineReadyMs: med('engineReadyMs'), transferredKB: med('transferredKB') })
await browser.close()
server.close()
