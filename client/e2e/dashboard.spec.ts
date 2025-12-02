import { test, expect } from './fixtures';

test.describe('Dashboard', () => {
  test.describe('Dashboard Page', () => {
    test('should display dashboard after login', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('heading', { name: /Dashboard/i }).first()).toBeVisible();
    });

    test('should show user type badge', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      // User badge appears in header - look for exact USER text in badge
      await expect(page.getByRole('banner').getByText('USER', { exact: true })).toBeVisible();
    });

    test('should have logout button', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('button', { name: 'F3=Exit' })).toBeVisible();
    });

    test('should display welcome message', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByText(/Welcome/i)).toBeVisible();
    });
  });

  test.describe('Navigation Links', () => {
    test('should have accounts link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: 'Accounts' })).toBeVisible();
    });

    test('should have cards link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: 'Cards' })).toBeVisible();
    });

    test('should have transactions link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: 'Transactions' })).toBeVisible();
    });

    test('should have bill payment link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: 'Bill Payment' })).toBeVisible();
    });

    test('should have reports link', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: 'Reports' })).toBeVisible();
    });
  });

  test.describe('Logout', () => {
    test('should logout when clicking logout button', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('button', { name: 'F3=Exit' }).click();
      
      // Should redirect to login page
      await expect(page).toHaveURL(/\/(login)?$/);
    });
  });

  test.describe('Admin Dashboard', () => {
    test('should show admin badge for admin users', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      // Look for ADMIN badge in header
      await expect(page.getByRole('banner').getByText('ADMIN', { exact: true })).toBeVisible();
    });
  });
});
