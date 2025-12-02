import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, act } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { AuthProvider, useAuth } from './AuthContext';
import { authService } from '@/services/authService';

// Mock authService
vi.mock('@/services/authService', () => ({
  authService: {
    login: vi.fn(),
    logout: vi.fn(),
    getToken: vi.fn(),
    getCurrentUser: vi.fn(),
    setAuth: vi.fn(),
  },
}));

// Test component that exposes auth context values
const TestConsumer: React.FC<{ onMount?: (auth: ReturnType<typeof useAuth>) => void }> = ({ onMount }) => {
  const auth = useAuth();
  
  if (onMount) {
    onMount(auth);
  }

  return (
    <div>
      <span data-testid="is-authenticated">{auth.isAuthenticated.toString()}</span>
      <span data-testid="is-loading">{auth.isLoading.toString()}</span>
      <span data-testid="user-id">{auth.user?.userId || 'null'}</span>
      <span data-testid="user-name">{auth.user?.fullName || 'null'}</span>
      <span data-testid="user-type">{auth.user?.userType || 'null'}</span>
      <button data-testid="login-btn" onClick={() => auth.login({ userId: 'TEST', password: 'test123' })}>
        Login
      </button>
      <button data-testid="logout-btn" onClick={auth.logout}>
        Logout
      </button>
    </div>
  );
};

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('AuthProvider', () => {
    it('should initialize with loading state', () => {
      vi.mocked(authService.getToken).mockReturnValue(null);
      vi.mocked(authService.getCurrentUser).mockReturnValue(null);

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>
      );

      // After initialization, loading should be false
      expect(screen.getByTestId('is-loading').textContent).toBe('false');
    });

    it('should initialize as not authenticated when no token', async () => {
      vi.mocked(authService.getToken).mockReturnValue(null);
      vi.mocked(authService.getCurrentUser).mockReturnValue(null);

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(screen.getByTestId('is-authenticated').textContent).toBe('false');
        expect(screen.getByTestId('user-id').textContent).toBe('null');
      });
    });

    it('should restore user from storage when token exists', async () => {
      const storedUser = {
        userId: 'ADMIN',
        fullName: 'Admin User',
        userType: 'ADMIN',
      };
      vi.mocked(authService.getToken).mockReturnValue('valid-token');
      vi.mocked(authService.getCurrentUser).mockReturnValue(storedUser);

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(screen.getByTestId('is-authenticated').textContent).toBe('true');
        expect(screen.getByTestId('user-id').textContent).toBe('ADMIN');
        expect(screen.getByTestId('user-name').textContent).toBe('Admin User');
        expect(screen.getByTestId('user-type').textContent).toBe('ADMIN');
      });
    });

    it('should remain unauthenticated if token exists but no user', async () => {
      vi.mocked(authService.getToken).mockReturnValue('valid-token');
      vi.mocked(authService.getCurrentUser).mockReturnValue(null);

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>
      );

      await waitFor(() => {
        expect(screen.getByTestId('is-authenticated').textContent).toBe('false');
      });
    });
  });

  describe('login', () => {
    it('should login user and update state', async () => {
      const user = userEvent.setup();
      const loginResponse = {
        userId: 'TEST',
        fullName: 'Test User',
        userType: 'USER',
        token: 'new-token',
      };
      vi.mocked(authService.getToken).mockReturnValue(null);
      vi.mocked(authService.getCurrentUser).mockReturnValue(null);
      vi.mocked(authService.login).mockResolvedValue(loginResponse);

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>
      );

      // Initially not authenticated
      expect(screen.getByTestId('is-authenticated').textContent).toBe('false');

      // Perform login
      await user.click(screen.getByTestId('login-btn'));

      await waitFor(() => {
        expect(authService.login).toHaveBeenCalledWith({ userId: 'TEST', password: 'test123' });
        expect(authService.setAuth).toHaveBeenCalledWith('new-token', {
          userId: 'TEST',
          fullName: 'Test User',
          userType: 'USER',
          token: 'new-token',
        });
        expect(screen.getByTestId('is-authenticated').textContent).toBe('true');
        expect(screen.getByTestId('user-id').textContent).toBe('TEST');
        expect(screen.getByTestId('user-name').textContent).toBe('Test User');
      });
    });

    it('should throw error on login failure', async () => {
      const user = userEvent.setup();
      const error = new Error('Invalid credentials');
      vi.mocked(authService.getToken).mockReturnValue(null);
      vi.mocked(authService.getCurrentUser).mockReturnValue(null);
      vi.mocked(authService.login).mockRejectedValue(error);

      // Suppress console error for this test
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>
      );

      // The error should propagate
      await expect(async () => {
        await user.click(screen.getByTestId('login-btn'));
        await waitFor(() => {
          expect(authService.login).toHaveBeenCalled();
        });
      }).rejects.toThrow;

      consoleSpy.mockRestore();
    });
  });

  describe('logout', () => {
    it('should logout user and clear state', async () => {
      const user = userEvent.setup();
      const storedUser = {
        userId: 'ADMIN',
        fullName: 'Admin User',
        userType: 'ADMIN',
      };
      vi.mocked(authService.getToken).mockReturnValue('valid-token');
      vi.mocked(authService.getCurrentUser).mockReturnValue(storedUser);

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>
      );

      // Initially authenticated
      await waitFor(() => {
        expect(screen.getByTestId('is-authenticated').textContent).toBe('true');
      });

      // Perform logout
      await user.click(screen.getByTestId('logout-btn'));

      await waitFor(() => {
        expect(authService.logout).toHaveBeenCalled();
        expect(screen.getByTestId('is-authenticated').textContent).toBe('false');
        expect(screen.getByTestId('user-id').textContent).toBe('null');
      });
    });
  });

  describe('useAuth hook', () => {
    it('should throw error when used outside AuthProvider', () => {
      // Suppress console error for this test
      const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      expect(() => {
        render(<TestConsumer />);
      }).toThrow('useAuth must be used within an AuthProvider');

      consoleSpy.mockRestore();
    });
  });
});
