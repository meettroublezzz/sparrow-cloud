<script setup lang="ts">
import { NButton, NCard, NForm, NFormItem, NInput, NSelect } from 'naive-ui'
import type { UserQueryParams } from '../../types/user'

interface SearchFormModel {
  keyword: string
  role: UserQueryParams['role']
  status: UserQueryParams['status']
}

const props = defineProps<{
  modelValue: SearchFormModel
  loading: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: SearchFormModel]
  search: []
  reset: []
}>()

function updateField<K extends keyof SearchFormModel>(key: K, value: SearchFormModel[K]): void {
  emit('update:modelValue', {
    ...valueOfModel(),
    [key]: value,
  })
}

function valueOfModel(): SearchFormModel {
  return {
    keyword: props.modelValue.keyword,
    role: props.modelValue.role,
    status: props.modelValue.status,
  }
}
</script>

<template>
  <n-card class="panel-card" bordered>
    <div class="search-bar">
      <n-form inline label-placement="left" :show-feedback="false">
        <n-form-item label="关键词">
          <n-input
            :value="modelValue.keyword"
            clearable
            placeholder="用户名 / 昵称 / 邮箱"
            style="width: 240px"
            @update:value="(value) => updateField('keyword', value)"
          />
        </n-form-item>
        <n-form-item label="角色">
          <n-select
            :value="modelValue.role"
            :options="[
              { label: '全部', value: 'All' },
              { label: '管理员', value: 'Admin' },
              { label: '普通用户', value: 'User' },
            ]"
            style="width: 160px"
            @update:value="(value) => updateField('role', value as SearchFormModel['role'])"
          />
        </n-form-item>
        <n-form-item label="状态">
          <n-select
            :value="modelValue.status"
            :options="[
              { label: '全部', value: 'All' },
              { label: '启用', value: 'active' },
              { label: '停用', value: 'inactive' },
            ]"
            style="width: 160px"
            @update:value="(value) => updateField('status', value as SearchFormModel['status'])"
          />
        </n-form-item>
      </n-form>

      <div class="search-actions">
        <n-button secondary @click="emit('reset')">重置</n-button>
        <n-button type="primary" :loading="loading" @click="emit('search')">查询</n-button>
      </div>
    </div>
  </n-card>
</template>

<style scoped>
.search-bar {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  gap: 16px;
}

.search-actions {
  display: flex;
  gap: 12px;
  align-items: center;
}
</style>
