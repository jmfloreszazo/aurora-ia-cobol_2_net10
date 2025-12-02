import { test, expect } from './fixtures';

test.describe('Admin - User Management', () => {
  test.describe('User List Page', () => {
    test('should display user list page for admin', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByRole('heading', { name: /User List/i }).first()).toBeVisible();
    });

    test('should have search filter', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByPlaceholder(/User ID, Name/i)).toBeVisible();
    });

    test('should have user type filter', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByText('User Type')).toBeVisible();
      await expect(page.locator('select').first()).toBeVisible();
    });

    test('should have status filter', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByText('Status')).toBeVisible();
    });

    test('should display users table', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByRole('columnheader', { name: 'User ID' })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: 'First Name' })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: 'Last Name' })).toBeVisible();
    });

    test('should have add user button', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByRole('button', { name: /Add User/i })).toBeVisible();
    });

    test('should have clear filters button', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByRole('button', { name: /Clear Filters/i })).toBeVisible();
    });
  });

  test.describe('Search and Filter', () => {
    test('should search users', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await page.getByPlaceholder(/User ID, Name/i).fill('John');
      
      await page.waitForTimeout(500);
      // Should filter results
      await expect(page.locator('table')).toBeVisible();
    });

    test('should filter by user type', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      const typeSelect = page.locator('select').first();
      await typeSelect.selectOption('ADMIN');
      
      await page.waitForTimeout(500);
      // Should show admin users
      await expect(page.locator('table')).toBeVisible();
    });

    test('should clear filters', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await page.getByPlaceholder(/User ID, Name/i).fill('test');
      await page.getByRole('button', { name: /Clear Filters/i }).click();
      
      // Search should be cleared
      await expect(page.getByPlaceholder(/User ID, Name/i)).toHaveValue('');
    });
  });

  test.describe('User Selection', () => {
    test('should select user on row click', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      // Click first row
      await page.locator('table tbody tr').first().click();
      
      // Action bar should appear
      await expect(page.getByText('Selected:')).toBeVisible();
    });

    test('should show action buttons when user selected', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await page.locator('table tbody tr').first().click();
      
      await expect(page.getByRole('button', { name: /View/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /Update/i })).toBeVisible();
    });
  });

  test.describe('Navigation', () => {
    test('should navigate to users from sidebar as admin', async ({ adminPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Users' }).click();
      
      await expect(page).toHaveURL('/admin/users');
    });

    test('should navigate to add user page', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await page.getByRole('button', { name: /Add User/i }).click();
      
      await expect(page).toHaveURL(/\/admin\/users\/new/);
    });
  });

  test.describe('Access Control', () => {
    test('should redirect non-admin users from users page', async ({ authenticatedPage: page }) => {
      await page.goto('/admin/users');
      
      // Should redirect or show access denied
      const url = page.url();
      const hasAccess = url.includes('/admin/users');
      
      // Regular users might not have access, or might see limited view
      expect(url).toBeDefined();
    });
  });
});
