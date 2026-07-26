<script setup lang="ts">
import { h } from 'vue'
import { NButton, NCard, NDataTable, NEmpty, NPopconfirm, NSpace, NTag } from 'naive-ui'
import type { User } from '../../types/user'

const props = defineProps<{
  loading: boolean
  users: User[]
}>()

const emit = defineEmits<{
  edit: [user: User]
  remove: [user: User]
}>()

function renderStatusTag(status: User['status']): 'success' | 'warning' {
  return status === 'active' ? 'success' : 'warning'
}

function renderActions(row: User) {
  return h(NSpace, null, {
    default: () => [
      h(NButton, { quaternary: true, size: 'small', onClick: () => emit('edit', row) }, { default: () => '编辑' }),
      h(
        NPopconfirm,
        { onPositiveClick: () => emit('remove', row) },
        {
          trigger: () => h(NButton, { quaternary: true, size: 'small', type: 'error' }, { default: () => '删除' }),
          default: () => '确认删除该用户？',
        },
      ),
    ],
  })
}

const columns = [
  {
    title: '用户名',
    key: 'userName',
    width: 140,
  },
  {
    title: '昵称',
    key: 'displayName',
    width: 160,
  },
  {
    title: '邮箱',
    key: 'email',
    minWidth: 220,
  },
  {
    title: '角色',
    key: 'role',
    width: 120,
  },
  {
    title: '状态',
    key: 'status',
    width: 120,
    render(row: User) {
      return h(
        NTag,
        { type: renderStatusTag(row.status), size: 'small' },
        { default: () => (row.status === 'active' ? '启用' : '停用') },
      )
    },
  },
  {
    title: '更新时间',
    key: 'updatedAt',
    width: 180,
  },
  {
    title: '操作',
    key: 'actions',
    width: 180,
    render: renderActions,
  },
]
</script>

<template>
  <n-card class="panel-card" bordered>
    <n-data-table
      :columns="columns"
      :data="users"
      :loading="props.loading"
      :single-line="false"
      :bordered="false"
      :row-key="(row: User) => row.id"
    >
      <template #empty>
        <div class="empty-state">
          <n-empty description="暂无用户数据" />
        </div>
      </template>
    </n-data-table>
  </n-card>
</template>

<style scoped>
.empty-state {
  padding: 40px 0;
}
</style>
