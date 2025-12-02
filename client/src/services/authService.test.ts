import { describe, it, expect, vi, beforeEach } from 'vitest';
import { authService } from '@/services/authService';

// Mock axios
vi.mock('@/lib/axios', () => ({
  default: {
    post: vi.fn(),
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('authService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Clear localStorage mock
    (localStorage.getItem as any).mockReturnValue(null);
  });

  describe('getToken', () => {
    it('should return null when no token is stored', () => {
      (localStorage.getItem as any).mockReturnValue(null);

      const result = authService.getToken();

      expect(result).toBeNull();
      expect(localStorage.getItem).toHaveBeenCalledWith('token');
    });

    it('should return token when stored', () => {
      const token = 'test-jwt-token';
      (localStorage.getItem as any).mockReturnValue(token);

      const result = authService.getToken();

      expect(result).toBe(token);
    });
  });

  describe('isAuthenticated', () => {
    it('should return false when no token', () => {
      (localStorage.getItem as any).mockReturnValue(null);

      const result = authService.isAuthenticated();

      expect(result).toBe(false);
    });

    it('should return true when token exists', () => {
      (localStorage.getItem as any).mockReturnValue('valid-token');

      const result = authService.isAuthenticated();

      expect(result).toBe(true);
    });
  });

  describe('getCurrentUser', () => {
    it('should return null when no user is stored', () => {
      (localStorage.getItem as any).mockReturnValue(null);

      const result = authService.getCurrentUser();

      expect(result).toBeNull();
    });

    it('should return parsed user when stored', () => {
      const user = { userId: 'ADMIN', fullName: 'Admin User', userType: 'ADMIN' };
      (localStorage.getItem as any).mockReturnValue(JSON.stringify(user));

      const result = authService.getCurrentUser();

      expect(result).toEqual(user);
    });
  });

  describe('setAuth', () => {
    it('should store token and user in localStorage', () => {
      const token = 'new-token';
      const user = { userId: 'USER01', fullName: 'Test User', userType: 'USER' };

      authService.setAuth(token, user);

      expect(localStorage.setItem).toHaveBeenCalledWith('token', token);
      expect(localStorage.setItem).toHaveBeenCalledWith('user', JSON.stringify(user));
    });
  });

  describe('logout', () => {
    it('should remove token and user from localStorage', () => {
      authService.logout();

      expect(localStorage.removeItem).toHaveBeenCalledWith('token');
      expect(localStorage.removeItem).toHaveBeenCalledWith('user');
    });
  });

  describe('login', () => {
    it('should call API and return response', async () => {
      const credentials = { userId: 'ADMIN', password: 'Admin@123' };
      const mockResponse = {
        token: 'jwt-token',
        userId: 'ADMIN',
        fullName: 'Admin User',
        userType: 'ADMIN',
      };

      const axios = await import('@/lib/axios');
      (axios.default.post as any).mockResolvedValue({ data: mockResponse });

      const result = await authService.login(credentials);

      expect(result).toEqual(mockResponse);
    });

    it('should throw error on invalid credentials', async () => {
      const credentials = { userId: 'INVALID', password: 'wrong' };
      const error = new Error('Invalid credentials');

      const axios = await import('@/lib/axios');
      (axios.default.post as any).mockRejectedValue(error);

      await expect(authService.login(credentials)).rejects.toThrow();
    });
  });
});
