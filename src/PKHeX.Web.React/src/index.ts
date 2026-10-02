import './startSentry'
import 'antd/dist/reset.css'
import './index.css'
import { start } from './main'

start({ autoLoad: import.meta.env.DEV })
