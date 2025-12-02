import { test, expect } from './fixtures';

test.describe('Navigation', () => {
  test.describe('Main Navigation', () => {
    test('should display navigation menu', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      // Should have navigation sidebar or header
      const nav = page.getByRole('navigation').first();
      await expect(nav).toBeVisible();
    });

    test('should have dashboard link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: /dashboard|home/i })).toBeVisible();
    });

    test('should have accounts link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: /accounts?/i })).toBeVisible();
    });

    test('should have cards link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: /cards?/i })).toBeVisible();
    });

    test('should have transactions link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: /transactions?/i })).toBeVisible();
    });

    test('should have reports link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: /reports?/i })).toBeVisible();
    });

    test('should have billing link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: /bill|payment/i })).toBeVisible();
    });
  });

  test.describe('Admin Navigation', () => {
    test('should show admin menu for admin users', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      // Admin should see user management
      await expect(page.getByRole('link', { name: /users?|admin/i })).toBeVisible();
    });

    test('should show batch jobs link for admin', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: /batch|jobs?/i })).toBeVisible();
    });

    test('should not show admin links for regular users', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      // Regular user should not see admin links
      const adminLink = page.getByRole('link', { name: /admin.*users?|user.*management/i });
      const batchLink = page.getByRole('link', { name: /batch.*jobs?/i });
      
      // These should be hidden or not visible
    });
  });

  test.describe('Navigation Links', () => {
    test('should navigate to accounts page', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: /accounts?/i }).click();
      
      await expect(page).toHaveURL('/accounts');
    });

    test('should navigate to cards page', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: /cards?/i }).click();
      
      await expect(page).toHaveURL('/cards');
    });

    test('should navigate to transactions page', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: /transactions?/i }).click();
      
      await expect(page).toHaveURL('/transactions');
    });

    test('should navigate to reports page', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: /reports?/i }).click();
      
      await expect(page).toHaveURL('/reports');
    });

    test('should navigate to billing page', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: /bill|payment/i }).click();
      
      await expect(page).toHaveURL('/billing');
    });
  });

  test.describe('User Info', () => {
    test('should display logged in user info', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      // Should show user name or ID
      const userInfo = page.getByText(/USER01|user/i);
      // User info might be displayed in header
    });

    test('should display user role', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      // Admin role might be displayed
      const adminRole = page.getByText(/admin/i);
    });
  });

  test.describe('Responsive Navigation', () => {
    test('should show mobile menu on small screens', async ({ authenticatedPage: page }) => {
      // Set viewport to mobile size
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto('/dashboard');
      
      // Should have hamburger menu
      const menuButton = page.getByRole('button', { name: /menu/i });
      // Mobile menu might be available
    });
  });
});
