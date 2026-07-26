<script setup lang="ts">
import { NCard, NEmpty, NButton } from 'naive-ui'

function saveSettings(): void {
  console.log('save settings')
}
</script>

<template>
  <section class="page-shell">
    <div class="page-header">
      <div>
        <p class="page-kicker">系统配置</p>
        <h1 class="page-title">系统设置</h1>
        <p class="page-description">先提供统一页面容器，后面再接主题、权限和基础参数。</p>
      </div>
      <NButton secondary round @click="saveSettings">保存配置</NButton>
    </div>

    <NCard class="page-card" bordered>
      <NEmpty description="系统设置页面的基础框架已就绪，下一步接入表单项与状态保存。" />
    </NCard>
  </section>
</template>

<style scoped>
.page-shell {
  min-height: calc(100vh - 40px);
  padding: 8px;
}

.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 20px;
}

.page-kicker {
  margin: 0 0 6px;
  color: #64748b;
  font-size: 12px;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.page-title {
  margin: 0;
  font-size: 28px;
  line-height: 1.2;
}

.page-description {
  margin: 8px 0 0;
  color: #475569;
}

.page-card {
  min-height: 320px;
  display: grid;
  place-items: center;
}
</style>