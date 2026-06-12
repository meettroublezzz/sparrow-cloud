import { createApp } from 'vue'
import './style.css'
import App from './App.vue'

// 创建项目对象
const app = createApp(App)

// 导入路由对象
import router from './router/index.ts'
// 挂载路由对象
app.use(router)

// 导入状态对象
import pinia from './stores/index.ts'
// 挂载状态对象
app.use(pinia)

// 挂载组件
app.mount('#app')