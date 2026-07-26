<script setup lang="ts">
import { NCard, NPagination } from 'naive-ui'
defineProps<{
  page: number
  pageSize: number
  total: number
  loading: boolean
}>()

const emit = defineEmits<{
  change: [page: number]
  pageSizeChange: [pageSize: number]
}>()

function handlePageChange(page: number): void {
  emit('change', page)
}

function handlePageSizeChange(pageSize: number): void {
  emit('pageSizeChange', pageSize)
}
</script>

<template>
  <n-card class="panel-card pagination-card" bordered>
    <n-pagination
      :page="page"
      :page-size="pageSize"
      :item-count="total"
      :disabled="loading"
      show-size-picker
      show-quick-jumper
      :page-sizes="[5, 10, 20]"
      @update:page="handlePageChange"
      @update:page-size="handlePageSizeChange"
    />
  </n-card>
</template>

<style scoped>
.pagination-card {
  display: flex;
  justify-content: flex-end;
}
</style>
