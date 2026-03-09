import apiClient from './apiClient';
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  RefreshTokenRequest,
} from '@/types/auth';

export async function login(data: LoginRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/auth/login', data);
  return response.data;
}

export async function register(data: RegisterRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/auth/register', data);
  return response.data;
}

export async function refreshToken(data: RefreshTokenRequest): Promise<AuthResponse> {
  const response = await apiClient.post<AuthResponse>('/auth/refresh', data);
  return response.data;
}

export async function logout(data: RefreshTokenRequest): Promise<void> {
  await apiClient.post('/auth/logout', data);
}
