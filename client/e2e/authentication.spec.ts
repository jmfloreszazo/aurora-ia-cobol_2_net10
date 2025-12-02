import { test, expect, TEST_USERS, loginAs } from './fixtures';

test.describe('Authentication', () => {
  test.describe('Login Page', () => {
    test('should display login form', async ({ page }) => {
      await page.goto('/login');
      
      await expect(page.getByRole('heading', { name: 'CardDemo' })).toBeVisible();
      await expect(page.getByText('Credit Card Management System')).toBeVisible();
      await expect(page.getByLabel('User ID')).toBeVisible();
      await expect(page.getByLabel('Password')).toBeVisible();
      await expect(page.getByRole('button', { name: 'Sign in' })).toBeVisible();
    });

    test('should show demo credentials', async ({ page }) => {
      await page.goto('/login');
      
      await expect(page.getByText('Demo credentials:')).toBeVisible();
      await expect(page.getByText('ADMIN / Admin@123')).toBeVisible();
      await expect(page.getByText('USER01 / User@123')).toBeVisible();
    });

    test('should login successfully with admin credentials', async ({ page }) => {
      await page.goto('/login');
      
      await page.getByLabel('User ID').fill(TEST_USERS.admin.userId);
      await page.getByLabel('Password').fill(TEST_USERS.admin.password);
      await page.getByRole('button', { name: 'Sign in' }).click();
      
      await expect(page).toHaveURL('/dashboard');
      await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
    });

    test('should login successfully with user credentials', async ({ page }) => {
      await page.goto('/login');
      
      await page.getByLabel('User ID').fill(TEST_USERS.user.userId);
      await page.getByLabel('Password').fill(TEST_USERS.user.password);
      await page.getByRole('button', { name: 'Sign in' }).click();
      
      await expect(page).toHaveURL('/dashboard');
    });

    test('should show error with invalid credentials', async ({ page }) => {
      await page.goto('/login');
      
      await page.getByLabel('User ID').fill('INVALID');
      await page.getByLabel('Password').fill('wrongpassword');
      await page.getByRole('button', { name: 'Sign in' }).click();
      
      // Wait for error response
      await page.waitForTimeout(1000);
      const errorDiv = page.locator('.bg-red-50, [role="alert"]');
      await expect(errorDiv).toBeVisible();
    });

    test('should show loading state during login', async ({ page }) => {
      await page.goto('/login');
      
      await page.getByLabel('User ID').fill(TEST_USERS.user.userId);
      await page.getByLabel('Password').fill(TEST_USERS.user.password);
      
      // Intercept the login request to slow it down
      await page.route('**/api/auth/login', async route => {
        await new Promise(resolve => setTimeout(resolve, 500));
        await route.continue();
      });
      
      await page.getByRole('button', { name: 'Sign in' }).click();
      
      // Should show loading state
      await expect(page.getByRole('button', { name: 'Signing in...' })).toBeVisible();
    });

    test('should require userId field', async ({ page }) => {
      await page.goto('/login');
      
      await page.getByLabel('Password').fill('password');
      await page.getByRole('button', { name: 'Sign in' }).click();
      
      // HTML5 validation should prevent submission
      const userIdInput = page.getByLabel('User ID');
      await expect(userIdInput).toHaveAttribute('required');
    });

    test('should require password field', async ({ page }) => {
      await page.goto('/login');
      
      await page.getByLabel('User ID').fill('USER01');
      await page.getByRole('button', { name: 'Sign in' }).click();
      
      // HTML5 validation should prevent submission
      const passwordInput = page.getByLabel('Password');
      await expect(passwordInput).toHaveAttribute('required');
    });
  });

  test.describe('Protected Routes', () => {
    test('should redirect to login when not authenticated', async ({ page }) => {
      await page.goto('/dashboard');
      
      await expect(page).toHaveURL('/login');
    });

    test('should redirect to login when accessing accounts without auth', async ({ page }) => {
      await page.goto('/accounts');
      
      await expect(page).toHaveURL('/login');
    });

    test('should redirect to login when accessing admin pages without auth', async ({ page }) => {
      await page.goto('/admin/users');
      
      await expect(page).toHaveURL('/login');
    });
  });

  test.describe('Logout', () => {
    test('should logout successfully', async ({ page }) => {
      await loginAs(page, TEST_USERS.user);
      
      // Find and click logout button
      await page.getByRole('button', { name: /logout|sign out/i }).click();
      
      await expect(page).toHaveURL('/login');
    });

    test('should clear session after logout', async ({ page }) => {
      await loginAs(page, TEST_USERS.user);
      await page.getByRole('button', { name: /logout|sign out/i }).click();
      
      // Try to access protected route
      await page.goto('/dashboard');
      
      await expect(page).toHaveURL('/login');
    });
  });
});
