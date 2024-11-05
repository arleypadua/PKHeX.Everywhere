/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_FIREBASE_PROJECT_NAME: string
    readonly VITE_FIREBASE_API_KEY: string
    readonly VITE_FIREBASE_MESSAGING_SENDER_ID: string
    readonly VITE_FIREBASE_APP_ID: string
    readonly VITE_FIREBASE_ENABLED: string
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}