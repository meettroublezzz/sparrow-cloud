import { mockUsers } from '../mocks/user'
import type { User, UserFormModel, UserQueryParams } from '../types/user'

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'DELETE'

interface RequestOptions<TBody = unknown> {
  method?: HttpMethod
  body?: TBody
  headers?: HeadersInit
  params?: Record<string, string | number | boolean | undefined> | UserQueryParams
}

interface RequestResponse<T> {
  data: T
  status: number
  statusText: string
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api'

function resolveRequestUrl(url: string): string {
  if (/^https?:\/\//i.test(url)) {
    return url
  }

  const normalizedUrl = url.startsWith('/') ? url : `/${url}`
  const normalizedBaseUrl = apiBaseUrl.replace(/\/$/, '')

  if (!normalizedBaseUrl) {
    return normalizedUrl
  }

  if (normalizedUrl === normalizedBaseUrl || normalizedUrl.startsWith(`${normalizedBaseUrl}/`)) {
    return normalizedUrl
  }

  return `${normalizedBaseUrl}${normalizedUrl}`
}

function buildUrl(url: string, params?: RequestOptions['params']): string {
  const resolvedUrl = new URL(resolveRequestUrl(url), window.location.origin)

  if (params) {
    Object.entries(params as Record<string, string | number | boolean | undefined>).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        resolvedUrl.searchParams.set(key, String(value))
      }
    })
  }

  return resolvedUrl.toString()
}

function normalizeKeyword(keyword: string): string {
  return keyword.trim().toLowerCase()
}

function cloneUser(user: User): User {
  return { ...user }
}

function formatTimestamp(date: Date): string {
  const year = date.getFullYear()
  const month = `${date.getMonth() + 1}`.padStart(2, '0')
  const day = `${date.getDate()}`.padStart(2, '0')
  const hours = `${date.getHours()}`.padStart(2, '0')
  const minutes = `${date.getMinutes()}`.padStart(2, '0')
  const seconds = `${date.getSeconds()}`.padStart(2, '0')

  return `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`
}

function delay(milliseconds: number): Promise<void> {
  return new Promise((resolve) => {
    window.setTimeout(resolve, milliseconds)
  })
}

function isMockRequest(url: string): boolean {
  return import.meta.env.DEV && url.startsWith('/users')
}

async function handleMockRequest<T>(
  url: string,
  method: HttpMethod,
  body?: unknown,
  params?: RequestOptions['params'],
): Promise<RequestResponse<T>> {
  await delay(280)

  if (method === 'GET' && url === '/users') {
    const query = (params ?? body ?? {}) as Partial<UserQueryParams>
    const keyword = normalizeKeyword(query.keyword ?? '')
    const role = query.role ?? 'All'
    const status = query.status ?? 'All'
    const page = query.page ?? 1
    const pageSize = query.pageSize ?? 5

    const filteredUsers = mockUsers.filter((user) => {
      const keywordMatched =
        keyword.length === 0 ||
        [user.userName, user.displayName, user.email].join(' ').toLowerCase().includes(keyword)

      const roleMatched = role === 'All' || user.role === role
      const statusMatched = status === 'All' || user.status === status

      return keywordMatched && roleMatched && statusMatched
    })

    const startIndex = (page - 1) * pageSize
    const items = filteredUsers.slice(startIndex, startIndex + pageSize).map(cloneUser)

    return {
      data: {
        items,
        total: filteredUsers.length,
        page,
        pageSize,
      } as T,
      status: 200,
      statusText: 'OK',
    }
  }

  if (method === 'POST' && url === '/users') {
    const input = body as UserFormModel
    const nextId = mockUsers.reduce((maxId, user) => Math.max(maxId, user.id), 0) + 1
    const now = formatTimestamp(new Date())
    const newUser: User = {
      id: nextId,
      userName: input.userName,
      displayName: input.displayName,
      email: input.email,
      role: input.role,
      status: input.status,
      createdAt: now,
      updatedAt: now,
    }

    mockUsers.unshift(newUser)

    return {
      data: cloneUser(newUser) as T,
      status: 201,
      statusText: 'Created',
    }
  }

  if (method === 'PUT' && url.startsWith('/users/')) {
    const id = Number(url.split('/').at(-1))
    const input = body as UserFormModel

    if (Number.isNaN(id)) {
      throw new Error('无效的用户 ID')
    }

    const index = mockUsers.findIndex((user) => user.id === id)
    if (index < 0) {
      throw new Error('未找到待更新的用户')
    }

    mockUsers[index] = {
      ...mockUsers[index],
      userName: input.userName,
      displayName: input.displayName,
      email: input.email,
      role: input.role,
      status: input.status,
      updatedAt: formatTimestamp(new Date()),
    }

    return {
      data: cloneUser(mockUsers[index]) as T,
      status: 200,
      statusText: 'OK',
    }
  }

  if (method === 'DELETE' && url.startsWith('/users/')) {
    const id = Number(url.split('/').at(-1))

    if (Number.isNaN(id)) {
      throw new Error('无效的用户 ID')
    }

    const index = mockUsers.findIndex((user) => user.id === id)
    if (index < 0) {
      throw new Error('未找到待删除的用户')
    }

    mockUsers.splice(index, 1)

    return {
      data: undefined as T,
      status: 204,
      statusText: 'No Content',
    }
  }

  throw new Error(`未实现的 mock 请求: ${method} ${url}`)
}

async function parseResponse<T>(response: Response): Promise<T> {
  const contentType = response.headers.get('content-type') ?? ''

  if (response.status === 204) {
    return undefined as T
  }

  if (contentType.includes('application/json')) {
    return (await response.json()) as T
  }

  return (await response.text()) as T
}

export async function request<T>(url: string, options: RequestOptions = {}): Promise<T> {
  const method = options.method ?? 'GET'

  if (isMockRequest(url)) {
    const mockResult = await handleMockRequest<T>(url, method, options.body, options.params)
    return mockResult.data
  }

  const response = await fetch(buildUrl(url, options.params), {
    method,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
    credentials: 'include',
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || `请求失败: ${response.status}`)
  }

  return parseResponse<T>(response)
}

export function get<T>(url: string, params?: RequestOptions['params']): Promise<T> {
  return request<T>(url, { method: 'GET', params })
}

export function post<T, TBody = unknown>(url: string, body?: TBody): Promise<T> {
  return request<T>(url, { method: 'POST', body })
}

export function put<T, TBody = unknown>(url: string, body?: TBody): Promise<T> {
  return request<T>(url, { method: 'PUT', body })
}

export function del<T>(url: string): Promise<T> {
  return request<T>(url, { method: 'DELETE' })
}
