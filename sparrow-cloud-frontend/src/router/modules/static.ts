import DefaultLayout from '@/layouts/DefaultLayout.vue'
import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/login/index.vue')
  },
  {
    path: '/',
    component: DefaultLayout,
    redirect: { name: 'file-library-home' },
    children: [
      {
        path: '',
        name: 'file-library-home',
        component: () => import('@/views/FileLibraryHome.vue')
      },
      {
        path: 'users',
        name: 'user-management',
        component: () => import('@/views/UserManagement.vue')
      },
      {
        path: 'settings',
        name: 'system-settings',
        component: () => import('@/views/SystemSettings.vue')
      }
    ]
  }
]

export default routes
