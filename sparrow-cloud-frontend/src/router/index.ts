import { createRouter, createWebHashHistory } from 'vue-router'
import type { RouteRecordRaw, Router } from 'vue-router'

const modules = import.meta.glob<{ default: RouteRecordRaw | RouteRecordRaw[] }>('./modules/*.ts', {
  eager: true
})

const routes = Object.values(modules).flatMap((module) => {
  const value = module.default
  return Array.isArray(value) ? value : [value]
})

export const router: Router = createRouter({
  history: createWebHashHistory(),
  routes,
  strict: true,
  scrollBehavior(_to, _from, savedPosition) {
    return savedPosition ?? { left: 0, top: 0 }
  }
})

router.beforeEach((to, _from, next) => {
  const isAuthenticated = true
  if (to.name !== 'Login' && !isAuthenticated) next({ name: 'Login' })
  else next()
})

export default router
