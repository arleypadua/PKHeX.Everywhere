import { defineConfig } from 'astro/config'
import starlight from '@astrojs/starlight'
import { createStarlightTypeDocPlugin } from 'starlight-typedoc'
import starlightLinksValidator from 'starlight-links-validator'
import { fileURLToPath } from 'node:url'

const [engineTypeDoc, engineSidebar] = createStarlightTypeDocPlugin()
const [reactTypeDoc, reactSidebar] = createStarlightTypeDocPlugin()
const [pluginSdkTypeDoc, pluginSdkSidebar] = createStarlightTypeDocPlugin()

const packages = '../packages'
const typeDocOptions = {
  excludeInternal: true,
  excludePrivate: true,
  readme: 'none',
  entryFileName: 'index',
  sourceLinkTemplate: 'https://github.com/arleypadua/PKHeX.Everywhere/blob/main/{path}#L{line}',
  disableGit: true,
  displayBasePath: fileURLToPath(new URL('../../..', import.meta.url)),
}

const measurementId = 'G-V7YFZ3P3JV'

const googleAnalytics = process.env.DOCS_GOOGLE_ANALYTICS === 'true'
  ? [
      {
        tag: 'script',
        content: `window.dataLayer = window.dataLayer || []
window.gtag = function () { dataLayer.push(arguments) }
gtag('consent', 'default', {
  ad_storage: 'denied',
  ad_user_data: 'denied',
  ad_personalization: 'denied',
  analytics_storage: 'granted',
})
gtag('consent', 'default', {
  analytics_storage: 'denied',
  region: ['AT', 'BE', 'BG', 'HR', 'CY', 'CZ', 'DK', 'EE', 'FI', 'FR', 'DE', 'GR', 'HU', 'IE', 'IT',
    'LV', 'LT', 'LU', 'MT', 'NL', 'PL', 'PT', 'RO', 'SK', 'SI', 'ES', 'SE', 'IS', 'LI', 'NO',
    'GB', 'CH'],
})
gtag('js', new Date())
gtag('config', '${measurementId}')`,
      },
      {
        tag: 'script',
        attrs: { async: true, src: `https://www.googletagmanager.com/gtag/js?id=${measurementId}` },
      },
    ]
  : []

export default defineConfig({
  site: 'https://docs.pkhex-everywhere.fyi',
  integrations: [
    starlight({
      title: 'PKHeX.Everywhere SDK',
      logo: { src: './public/favicon.svg', replacesTitle: false },
      favicon: '/favicon.svg',
      customCss: ['./src/styles/theme.css'],
      head: [
        {
          tag: 'script',
          content: `document.addEventListener('DOMContentLoaded', () => {
  for (const link of document.querySelectorAll('a[href^="https://github.com"], a[href^="https://pkhex-web.github.io"]')) {
    link.target = '_blank'
    link.rel = 'noopener'
  }
})`,
        },
        ...googleAnalytics,
      ],
      social: [
        { icon: 'github', label: 'GitHub', href: 'https://github.com/arleypadua/PKHeX.Everywhere' },
      ],
      editLink: {
        baseUrl: 'https://github.com/arleypadua/PKHeX.Everywhere/edit/main/src/PKHeX.Everywhere.Engine.Sdk/docs/',
      },
      components: {
        Footer: './src/components/DocsFooter.astro',
      },
      plugins: [
        engineTypeDoc({
          entryPoints: [`${packages}/engine/src/index.ts`],
          tsconfig: `${packages}/engine/tsconfig.json`,
          output: 'docs/reference/engine',
          sidebar: { label: '@pkhex-everywhere/engine', collapsed: true },
          typeDoc: typeDocOptions,
        }),
        reactTypeDoc({
          entryPoints: [`${packages}/react/src/index.ts`],
          tsconfig: `${packages}/react/tsconfig.json`,
          output: 'docs/reference/react',
          sidebar: { label: '@pkhex-everywhere/react', collapsed: true },
          typeDoc: typeDocOptions,
        }),
        pluginSdkTypeDoc({
          entryPoints: [`${packages}/plugin-sdk/index.d.ts`],
          tsconfig: `${packages}/plugin-sdk/tsconfig.json`,
          output: 'docs/reference/plugin-sdk',
          sidebar: { label: '@pkhex-everywhere/plugin-sdk', collapsed: true },
          typeDoc: typeDocOptions,
        }),
        starlightLinksValidator({ errorOnLocalLinks: false }),
      ],
      sidebar: [
        {
          label: 'Start here',
          items: [
            { label: 'Overview', link: '/docs/' },
            { label: 'Getting started', link: '/docs/getting-started/' },
          ],
        },
        {
          label: 'Guides',
          items: [
            { label: 'Engine', link: '/docs/guides/engine/' },
            { label: 'React', link: '/docs/guides/react/' },
            { label: 'Writing a plugin', link: '/docs/guides/plugins/' },
          ],
        },
        {
          label: 'API reference',
          items: [engineSidebar, reactSidebar, pluginSdkSidebar],
        },
      ],
    }),
  ],
})
