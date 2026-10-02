import './startSentry'
import './index.css'
import { start } from './main'

start({ autoLoad: import.meta.env.DEV })
