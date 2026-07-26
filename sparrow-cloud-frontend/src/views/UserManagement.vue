<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { NAlert, NButton, NSpace, useMessage } from 'naive-ui'
import UserFormModal from '@/components/user/UserFormModal.vue'
import UserPagination from '@/components/user/UserPagination.vue'
import UserSearchBar from '@/components/user/UserSearchBar.vue'
import UserTable from '@/components/user/UserTable.vue'
import { useUserStore } from '@/stores/user'
import type { User, UserFormState, UserQueryParams } from '@/types/user'

const userStore = useUserStore()
const message = useMessage()

const formTitle = computed(() => (userStore.editingUserId === null ? '新建用户' : '编辑用户'))

function getErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}

async function refreshUsers(): Promise<void> {
  await userStore.loadUsers()
  if (userStore.errorMessage) {
    message.error(userStore.errorMessage)
  }
}

function handleQueryUpdate(value: Pick<UserQueryParams, 'keyword' | 'role' | 'status'>): void {
  userStore.updateQuery(value)
}

async function handleSearch(): Promise<void> {
  userStore.updateQuery({ page: 1 })
  await refreshUsers()
}

async function handleReset(): Promise<void> {
  userStore.resetQuery()
  await refreshUsers()
}

async function handlePageChange(page: number): Promise<void> {
  userStore.updateQuery({ page })
  await refreshUsers()
}

async function handlePageSizeChange(pageSize: number): Promise<void> {
  userStore.updateQuery({ page: 1, pageSize })
  await refreshUsers()
}

function handleFormModelUpdate(value: UserFormState): void {
  userStore.formState = value
}

async function handleSubmit(): Promise<void> {
  try {
    await userStore.submitForm()
    message.success('用户信息已保存')
  } catch (error) {
    message.error(getErrorMessage(error, '保存用户失败'))
  }
}

async function handleRemove(user: User): Promise<void> {
  try {
    await userStore.removeUser(user.id)
    message.success('用户已删除')
  } catch (error) {
    message.error(getErrorMessage(error, '删除用户失败'))
  }
}

onMounted(() => {
  void refreshUsers()
})
</script>

<template>
  <section class="page-shell">
    <div class="page-header">
      <div>
        <p class="page-kicker">用户中心</p>
        <h1 class="page-title">用户管理</h1>
        <p class="page-description">管理系统用户、角色和启停状态。</p>
      </div>
      <NButton type="primary" round @click="userStore.openCreateForm">新建用户</NButton>
    </div>

    <NSpace vertical size="large">
      <NAlert v-if="userStore.errorMessage" type="error" :show-icon="false">
        {{ userStore.errorMessage }}
      </NAlert>

      <UserSearchBar
        :model-value="userStore.query"
        :loading="userStore.loading"
        @update:model-value="handleQueryUpdate"
        @search="handleSearch"
        @reset="handleReset"
      />

      <UserTable
        :loading="userStore.loading"
        :users="userStore.users"
        @edit="userStore.openEditForm"
        @remove="handleRemove"
      />

      <UserPagination
        :page="userStore.query.page"
        :page-size="userStore.query.pageSize"
        :total="userStore.total"
        :loading="userStore.loading"
        @change="handlePageChange"
        @page-size-change="handlePageSizeChange"
      />
    </NSpace>

    <UserFormModal
      :show="userStore.formVisible"
      :loading="userStore.loading"
      :model-value="userStore.formState"
      :title="formTitle"
      @update:show="userStore.formVisible = $event"
      @update:model-value="handleFormModelUpdate"
      @submit="handleSubmit"
    />
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
</style>
