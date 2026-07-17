<script setup lang="ts">
import { computed } from 'vue'
import { NConfigProvider, NDialogProvider, NGlobalStyle, NMessageProvider, dateZhCN, zhCN } from 'naive-ui'

const naiveThemeOverrides = computed(() => ({
  common: {
    primaryColor: '#2563eb',
    primaryColorHover: '#3b82f6',
    primaryColorPressed: '#1d4ed8',
    borderRadius: '8px',
  },
}))
</script>

<template>
  <n-config-provider :locale="zhCN" :date-locale="dateZhCN" :theme-overrides="naiveThemeOverrides">
    <n-message-provider>
      <n-dialog-provider>
        <n-global-style />
        <router-view />
      </n-dialog-provider>
    </n-message-provider>
  </n-config-provider>
</template>
