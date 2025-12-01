import api from '@/lib/axios';
import { API_ENDPOINTS } from '@/config/constants';
import type { LoginRequest, LoginResponse } from '@/types';

export const authService = {
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const { data } = await api.post<LoginResponse>(API_ENDPOINTS.LOGIN, credentials);
    return data;
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  },

  getToken(): string | null {
    return localStorage.getItem('token');
  },

  isAuthenticated(): boolean {
    return !!this.getToken();
  },

  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  },

  setAuth(token: string, user: any) {
    localStorage.setItem('token', token);
    localStorage.setItem('user', JSON.stringify(user));
  }
};
