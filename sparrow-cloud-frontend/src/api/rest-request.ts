const REQUEST_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

type RequestOptions = RequestInit & {
  query?: Record<string, string | number | boolean | null | undefined>
}

function normalizePath(path: string) {
  if (/^https?:\/\//i.test(path)) {
    return path
  }

  const normalizedPath = path.startsWith('/') ? path : `/${path}`
  const normalizedBaseUrl = REQUEST_BASE_URL.replace(/\/$/, '')

  if (!normalizedBaseUrl) {
    return normalizedPath
  }

  if (normalizedPath === normalizedBaseUrl || normalizedPath.startsWith(`${normalizedBaseUrl}/`)) {
    return normalizedPath
  }

  return `${normalizedBaseUrl}${normalizedPath}`
}

function buildUrl(path: string, query?: RequestOptions['query']) {
  const url = new URL(normalizePath(path), window.location.origin)

  if (query) {
    Object.entries(query).forEach(([key, value]) => {
      if (value !== null && value !== undefined) {
        url.searchParams.set(key, String(value))
      }
    })
  }

  return url.toString()
}

export async function restRequest<T>(
  path: string,
  options: RequestOptions = {}
): Promise<T> {
  const { query, headers, body, ...fetchOptions } = options

  const response = await fetch(buildUrl(path, query), {
    ...fetchOptions,
    headers: {
      Accept: 'application/json',
      ...headers,
    },
    body,
  })

  if (!response.ok) {
    const message = await response.text()
    throw new Error(message || `Request failed: ${response.status}`)
  }

  const contentType = response.headers.get('content-type')

  if (contentType?.includes('application/json')) {
    return await response.json()
  }

  return (await response.text()) as T
}

export function get<T>(
  path: string,
  query?: RequestOptions['query']
) {
  return restRequest<T>(path, {
    method: 'GET',
    query,
  })
}

export function postForm<T>(
  path: string,
  data: Record<string, string | number | boolean>
) {
  const formData = new FormData()

  Object.entries(data).forEach(([key, value]) => {
    formData.append(key, String(value))
  })

  return restRequest<T>(path, {
    method: 'POST',
    body: formData,
  })
}
