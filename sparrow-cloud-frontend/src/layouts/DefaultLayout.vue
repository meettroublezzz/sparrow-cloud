<script setup lang="ts">
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NLayout, NLayoutContent, NLayoutSider, NMenu } from 'naive-ui'
import type { MenuOption } from 'naive-ui'

const menuOptions = computed<MenuOption[]>(() => [
  {
    label: '文件库首页',
    key: 'file-library-home',
  },
  {
    label: '用户管理',
    key: 'user-management',
  },
  {
    label: '系统设置',
    key: 'system-settings',
  },
])

const router = useRouter()
const route = useRoute()

const activeMenuKey = computed(() => {
  return typeof route.name === 'string' ? route.name : 'file-library-home'
})

function handleMenuSelect(key: string): void {
  if (key !== route.name) {
    void router.push({ name: key })
  }
}
</script>

<template>
  <n-layout class="default-layout" has-sider>
    <n-layout-sider bordered class="default-layout-sider" collapse-mode="width" :width="260" :collapsed-width="72">
      <div class="layout-brand">
        <div class="layout-brand-mark">雀</div>
        <div class="layout-brand-text">
          <strong>麻雀云盘</strong>
          <span>Enterprise Workspace</span>
        </div>
      </div>

      <n-menu :options="menuOptions" :value="activeMenuKey" @update:value="handleMenuSelect" />
    </n-layout-sider>

    <n-layout-content class="default-layout-content panel-card">
      <router-view />
    </n-layout-content>
  </n-layout>
</template>

<style scoped>
.default-layout {
  min-height: 100vh;
}

.default-layout-sider {
  padding: 20px 16px;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(16px);
}

.layout-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 8px 20px;
}

.layout-brand-mark {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  color: #ffffff;
  background: linear-gradient(135deg, #2563eb 0%, #60a5fa 100%);
}

.layout-brand-text {
  display: flex;
  flex-direction: column;
}

.layout-brand-text strong {
  font-size: 16px;
}

.layout-brand-text span {
  font-size: 12px;
  color: #64748b;
}

.default-layout-content {
  min-height: 100vh;
  padding: 20px;
}
</style>