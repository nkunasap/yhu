const API_BASE = '/api'

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  fullName: string
  email: string
  password: string
  role: string
}

export interface AuthResponse {
  token: string
  email: string
  role: string
}

export interface RegisterResponse {
  message: string
  userId: number
  fullName: string
  email: string
  role: string
}

export interface ApiError {
  message: string
}

async function handleResponse<T>(res: Response): Promise<T> {
  if (!res.ok) {
    const err: ApiError = await res.json().catch(() => ({ message: 'Unknown error' }))
    throw new Error(err.message ?? 'Request failed')
  }
  return res.json() as Promise<T>
}

export async function loginApi(data: LoginRequest): Promise<AuthResponse> {
  const res = await fetch(`${API_BASE}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  return handleResponse<AuthResponse>(res)
}

export async function registerApi(data: RegisterRequest): Promise<RegisterResponse> {
  const res = await fetch(`${API_BASE}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  })
  return handleResponse<RegisterResponse>(res)
}
