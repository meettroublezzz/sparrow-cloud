<script setup>
  import { useUserStore } from '@/stores/UserStore.ts';
  const user = useUserStore()

  import HelloWorld from '@/components/HelloWorld.vue'

  import { NButton, NRate } from 'naive-ui'
</script>

<template>
  <p>{{ user.userId }}</p>
  <router-link to="/login"> 去登录 </router-link>
  <HelloWorld />
  <NButton @click="user.userId = -1">set -1</NButton>
  <n-rate />
</template>

<style scoped>

</style>