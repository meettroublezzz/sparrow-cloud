import { del, get, post, put } from '../utils/request'
import type { PaginationResult, User, UserFormModel, UserQueryParams } from '../types/user'

export function fetchUsers(query: UserQueryParams): Promise<PaginationResult<User>> {
  return get<PaginationResult<User>>('/users', query)
}

export function createUser(input: UserFormModel): Promise<User> {
  return post<User, UserFormModel>('/users', input)
}

export function updateUser(input: UserFormModel): Promise<User> {
  if (typeof input.id !== 'number') {
    return Promise.reject(new Error('用户 ID 不能为空'))
  }

  return put<User, UserFormModel>(`/users/${input.id}`, input)
}

export function deleteUser(userId: number): Promise<void> {
  return del<void>(`/users/${userId}`)
}
