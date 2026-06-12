/// <reference types="vite/client" />
declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  // 适配 Vue3 + <script setup>，通用兜底类型
  const component: DefineComponent<{}, {}, any>
  export default component
}