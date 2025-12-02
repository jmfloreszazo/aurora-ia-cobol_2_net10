import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { LoginPage } from './LoginPage';

// Mock useNavigate
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

// Mock useAuth
const mockLogin = vi.fn();
vi.mock('@/contexts/AuthContext', () => ({
  useAuth: () => ({
    login: mockLogin,
  }),
}));

describe('LoginPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  const renderLoginPage = () => {
    return render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>
    );
  };

  describe('rendering', () => {
    it('should render the login form', () => {
      renderLoginPage();

      expect(screen.getByRole('heading', { name: /carddemo/i })).toBeInTheDocument();
      expect(screen.getByText(/credit card management system/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/user id/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
    });

    it('should render demo credentials', () => {
      renderLoginPage();

      expect(screen.getByText(/demo credentials/i)).toBeInTheDocument();
      expect(screen.getByText(/ADMIN \/ Admin@123/i)).toBeInTheDocument();
      expect(screen.getByText(/USER01 \/ User@123/i)).toBeInTheDocument();
    });

    it('should have empty inputs initially', () => {
      renderLoginPage();

      const userIdInput = screen.getByLabelText(/user id/i) as HTMLInputElement;
      const passwordInput = screen.getByLabelText(/password/i) as HTMLInputElement;

      expect(userIdInput.value).toBe('');
      expect(passwordInput.value).toBe('');
    });
  });

  describe('form interactions', () => {
    it('should update user id input when typing', async () => {
      const user = userEvent.setup();
      renderLoginPage();

      const userIdInput = screen.getByLabelText(/user id/i);
      await user.type(userIdInput, 'ADMIN');

      expect(userIdInput).toHaveValue('ADMIN');
    });

    it('should update password input when typing', async () => {
      const user = userEvent.setup();
      renderLoginPage();

      const passwordInput = screen.getByLabelText(/password/i);
      await user.type(passwordInput, 'Admin@123');

      expect(passwordInput).toHaveValue('Admin@123');
    });

    it('should have password input of type password', () => {
      renderLoginPage();

      const passwordInput = screen.getByLabelText(/password/i);
      expect(passwordInput).toHaveAttribute('type', 'password');
    });
  });

  describe('form submission', () => {
    it('should call login with credentials on submit', async () => {
      const user = userEvent.setup();
      mockLogin.mockResolvedValueOnce({});
      renderLoginPage();

      await user.type(screen.getByLabelText(/user id/i), 'ADMIN');
      await user.type(screen.getByLabelText(/password/i), 'Admin@123');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalledWith({
          userId: 'ADMIN',
          password: 'Admin@123',
        });
      });
    });

    it('should navigate to dashboard on successful login', async () => {
      const user = userEvent.setup();
      mockLogin.mockResolvedValueOnce({});
      renderLoginPage();

      await user.type(screen.getByLabelText(/user id/i), 'ADMIN');
      await user.type(screen.getByLabelText(/password/i), 'Admin@123');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      await waitFor(() => {
        expect(mockNavigate).toHaveBeenCalledWith('/dashboard');
      });
    });

    it('should show loading state while submitting', async () => {
      const user = userEvent.setup();
      // Make login wait so we can check loading state
      mockLogin.mockImplementation(() => new Promise(() => {}));
      renderLoginPage();

      await user.type(screen.getByLabelText(/user id/i), 'ADMIN');
      await user.type(screen.getByLabelText(/password/i), 'Admin@123');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      expect(screen.getByRole('button', { name: /signing in/i })).toBeInTheDocument();
      expect(screen.getByRole('button')).toBeDisabled();
    });

    it('should display error message on login failure', async () => {
      const user = userEvent.setup();
      mockLogin.mockRejectedValueOnce({
        response: { data: { message: 'Invalid credentials' } },
      });
      renderLoginPage();

      await user.type(screen.getByLabelText(/user id/i), 'ADMIN');
      await user.type(screen.getByLabelText(/password/i), 'wrong');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      await waitFor(() => {
        expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
      });
    });

    it('should display default error message when response has no message', async () => {
      const user = userEvent.setup();
      mockLogin.mockRejectedValueOnce(new Error('Network error'));
      renderLoginPage();

      await user.type(screen.getByLabelText(/user id/i), 'ADMIN');
      await user.type(screen.getByLabelText(/password/i), 'wrong');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      await waitFor(() => {
        expect(screen.getByText('Login failed. Please check your credentials.')).toBeInTheDocument();
      });
    });

    it('should clear error state when form is resubmitted', async () => {
      const user = userEvent.setup();
      mockLogin
        .mockRejectedValueOnce({ response: { data: { message: 'Invalid credentials' } } })
        .mockResolvedValueOnce({});
      renderLoginPage();

      // First failed attempt
      await user.type(screen.getByLabelText(/user id/i), 'ADMIN');
      await user.type(screen.getByLabelText(/password/i), 'wrong');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      await waitFor(() => {
        expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
      });

      // Second successful attempt
      await user.clear(screen.getByLabelText(/password/i));
      await user.type(screen.getByLabelText(/password/i), 'Admin@123');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      await waitFor(() => {
        expect(screen.queryByText('Invalid credentials')).not.toBeInTheDocument();
      });
    });

    it('should not navigate on login failure', async () => {
      const user = userEvent.setup();
      mockLogin.mockRejectedValueOnce(new Error('Login failed'));
      renderLoginPage();

      await user.type(screen.getByLabelText(/user id/i), 'ADMIN');
      await user.type(screen.getByLabelText(/password/i), 'wrong');
      await user.click(screen.getByRole('button', { name: /sign in/i }));

      await waitFor(() => {
        expect(mockLogin).toHaveBeenCalled();
      });

      expect(mockNavigate).not.toHaveBeenCalled();
    });
  });

  describe('accessibility', () => {
    it('should have required attribute on inputs', () => {
      renderLoginPage();

      const userIdInput = screen.getByLabelText(/user id/i);
      const passwordInput = screen.getByLabelText(/password/i);

      expect(userIdInput).toBeRequired();
      expect(passwordInput).toBeRequired();
    });

    it('should associate labels with inputs', () => {
      renderLoginPage();

      const userIdInput = screen.getByLabelText(/user id/i);
      const passwordInput = screen.getByLabelText(/password/i);

      expect(userIdInput).toHaveAttribute('id', 'userId');
      expect(passwordInput).toHaveAttribute('id', 'password');
    });
  });
});
