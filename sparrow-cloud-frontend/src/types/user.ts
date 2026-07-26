export type UserStatus = 'active' | 'inactive'

export interface User {
  id: number
  userName: string
  displayName: string
  email: string
  role: 'Admin' | 'User'
  status: UserStatus
  createdAt: string
  updatedAt: string
}

export interface UserQueryParams {
  keyword: string
  role: 'All' | User['role']
  status: 'All' | UserStatus
  page: number
  pageSize: number
}

export interface PaginationResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface UserFormModel {
  id?: number
  userName: string
  displayName: string
  email: string
  role: User['role']
  status: UserStatus
}

export interface UserFormState {
  userName: string
  displayName: string
  email: string
  role: User['role']
  status: UserStatus
}
