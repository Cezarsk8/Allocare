'use client';

import { createContext, useCallback, useState } from 'react';
import type { UserDto } from '@/types/auth';
import * as authService from '@/app/services/authService';

function getStoredUser(): UserDto | null {
  if (typeof window === 'undefined') return null;
  const storedUser = localStorage.getItem('user');
  const accessToken = localStorage.getItem('accessToken');
  if (storedUser && accessToken) {
    return JSON.parse(storedUser) as UserDto;
  }
  return null;
}

interface AuthContextValue {
  user: UserDto | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, firstName: string, lastName: string) => Promise<void>;
  logout: () => Promise<void>;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(getStoredUser);
  const [isLoading] = useState(false);

  const storeAuth = useCallback((accessToken: string, refreshToken: string, userData: UserDto) => {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    localStorage.setItem('user', JSON.stringify(userData));
    setUser(userData);
  }, []);

  const clearAuth = useCallback(() => {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    setUser(null);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await authService.login({ email, password });
    storeAuth(response.accessToken, response.refreshToken, response.user);
  }, [storeAuth]);

  const register = useCallback(async (email: string, password: string, firstName: string, lastName: string) => {
    const response = await authService.register({ email, password, firstName, lastName });
    storeAuth(response.accessToken, response.refreshToken, response.user);
  }, [storeAuth]);

  const logout = useCallback(async () => {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      try {
        await authService.logout({ refreshToken });
      } catch {
        // Ignore logout errors — clear local state regardless
      }
    }
    clearAuth();
  }, [clearAuth]);

  return (
    <AuthContext.Provider value={{ user, isAuthenticated: !!user, isLoading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
