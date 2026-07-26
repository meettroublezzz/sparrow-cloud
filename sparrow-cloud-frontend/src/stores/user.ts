import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { createUser, deleteUser, fetchUsers, updateUser } from '../api/user'
import type { PaginationResult, User, UserFormModel, UserQueryParams } from '../types/user'

function createDefaultQuery(): UserQueryParams {
  return {
    keyword: '',
    role: 'All',
    status: 'All',
    page: 1,
    pageSize: 5,
  }
}

function createDefaultForm(): UserFormModel {
  return {
    userName: '',
    displayName: '',
    email: '',
    role: 'User',
    status: 'active',
  }
}

export const useUserStore = defineStore('user', () => {
  const users = ref<User[]>([])
  const total = ref(0)
  const loading = ref(false)
  const errorMessage = ref('')
  const query = ref<UserQueryParams>(createDefaultQuery())
  const formState = ref<UserFormModel>(createDefaultForm())
  const formVisible = ref(false)
  const editingUserId = ref<number | null>(null)

  const isEmpty = computed(() => !loading.value && users.value.length === 0)

  function resetFormState(): void {
    formState.value = createDefaultForm()
    editingUserId.value = null
  }

  function openCreateForm(): void {
    resetFormState()
    formVisible.value = true
  }

  function openEditForm(user: User): void {
    editingUserId.value = user.id
    formState.value = {
      id: user.id,
      userName: user.userName,
      displayName: user.displayName,
      email: user.email,
      role: user.role,
      status: user.status,
    }
    formVisible.value = true
  }

  function closeForm(): void {
    formVisible.value = false
  }

  function updateQuery(patch: Partial<UserQueryParams>): void {
    query.value = {
      ...query.value,
      ...patch,
      page: patch.page ?? query.value.page,
      pageSize: patch.pageSize ?? query.value.pageSize,
    }
  }

  function resetQuery(): void {
    query.value = createDefaultQuery()
  }

  async function loadUsers(): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
      const result: PaginationResult<User> = await fetchUsers(query.value)
      users.value = result.items
      total.value = result.total
    } catch (error) {
      users.value = []
      total.value = 0
      errorMessage.value = error instanceof Error ? error.message : '加载用户列表失败'
    } finally {
      loading.value = false
    }
  }

  async function submitForm(): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
      if (editingUserId.value === null) {
        await createUser(formState.value)
      } else {
        await updateUser({ ...formState.value, id: editingUserId.value })
      }

      closeForm()
      resetFormState()
      await loadUsers()
    } catch (error) {
      errorMessage.value = error instanceof Error ? error.message : '保存用户失败'
      throw error
    } finally {
      loading.value = false
    }
  }

  async function removeUser(userId: number): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
      await deleteUser(userId)
      await loadUsers()
    } catch (error) {
      errorMessage.value = error instanceof Error ? error.message : '删除用户失败'
      throw error
    } finally {
      loading.value = false
    }
  }

  return {
    users,
    total,
    loading,
    errorMessage,
    query,
    formState,
    formVisible,
    editingUserId,
    isEmpty,
    loadUsers,
    updateQuery,
    resetQuery,
    openCreateForm,
    openEditForm,
    closeForm,
    resetFormState,
    submitForm,
    removeUser,
  }
})
