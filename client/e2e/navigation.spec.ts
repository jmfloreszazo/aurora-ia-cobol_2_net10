import { test, expect } from './fixtures';

test.describe('Navigation', () => {
  test.describe('Main Navigation', () => {
    test('should have sidebar navigation', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.locator('nav').first()).toBeVisible();
    });

    test('should navigate to accounts', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Accounts' }).click();
      
      await expect(page).toHaveURL('/accounts');
    });

    test('should navigate to cards', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Cards' }).click();
      
      await expect(page).toHaveURL('/cards');
    });

    test('should navigate to transactions', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Transactions' }).click();
      
      await expect(page).toHaveURL('/transactions');
    });

    test('should navigate to billing', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Bill Payment' }).click();
      
      await expect(page).toHaveURL('/billing');
    });

    test('should navigate to reports', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Reports' }).click();
      
      await expect(page).toHaveURL('/reports');
    });
  });

  test.describe('Admin Navigation', () => {
    test('should show users link for admin users', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      // Admin users should see Users link
      await expect(page.getByRole('link', { name: 'Users' })).toBeVisible();
    });

    test('should show batch jobs link for admin', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('link', { name: 'Batch Jobs' })).toBeVisible();
    });

    test('should navigate to users page', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Users' }).click();
      
      await expect(page).toHaveURL('/admin/users');
    });

    test('should navigate to batch jobs page', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Batch Jobs' }).click();
      
      await expect(page).toHaveURL('/admin/batch-jobs');
    });
  });

  test.describe('Header', () => {
    test('should display header with system title', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('banner')).toBeVisible();
      await expect(page.getByText('CardDemo - Credit Card Management System')).toBeVisible();
    });

    test('should display user info in header', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByText('User:')).toBeVisible();
    });

    test('should have exit button in header', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('button', { name: 'F3=Exit' })).toBeVisible();
    });
  });

  test.describe('Footer', () => {
    test('should display footer with function keys', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('contentinfo')).toBeVisible();
    });
  });

  test.describe('Protected Routes', () => {
    test('should redirect unauthenticated users to login', async ({ page }) => {
      await page.goto('/dashboard');
      
      // Should redirect to login
      await expect(page).toHaveURL(/\/(login)?$/);
    });

    test('should redirect unauthenticated users from accounts', async ({ page }) => {
      await page.goto('/accounts');
      
      await expect(page).toHaveURL(/\/(login)?$/);
    });
  });
});
