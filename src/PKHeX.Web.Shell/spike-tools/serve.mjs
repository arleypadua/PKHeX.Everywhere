import http from 'node:http'
import fs from 'node:fs'
import path from 'node:path'

const types = { '.html': 'text/html', '.js': 'text/javascript', '.mjs': 'text/javascript', '.css': 'text/css', '.json': 'application/json', '.wasm': 'application/wasm', '.svg': 'image/svg+xml', '.png': 'image/png', '.ttf': 'font/ttf', '.dat': 'application/octet-stream', '.dll': 'application/octet-stream' }

export function serve(root, port) {
  const server = http.createServer((req, res) => {
    let file = path.join(root, decodeURIComponent(new URL(req.url, 'http://x').pathname))
    if (!fs.existsSync(file) || fs.statSync(file).isDirectory()) file = path.join(root, 'index.html')
    const headers = { 'content-type': types[path.extname(file)] ?? 'application/octet-stream', 'cache-control': 'no-store' }
    if ((req.headers['accept-encoding'] ?? '').includes('br') && fs.existsSync(file + '.br')) {
      headers['content-encoding'] = 'br'
      file += '.br'
    }
    res.writeHead(200, headers)
    fs.createReadStream(file).pipe(res)
  })
  return new Promise((r) => server.listen(port, '127.0.0.1', () => r(server)))
}
