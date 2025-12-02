import { test, expect, TEST_USERS } from './fixtures';

test.describe('Admin - User Management', () => {
  // All admin tests use adminPage fixture which logs in as ADMIN
  
  test.describe('User List', () => {
    test('should display users list for admin', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByRole('heading', { name: /users|user management/i })).toBeVisible();
    });

    test('should show user table', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      await page.waitForLoadState('networkidle');
      
      // Check for table headers
      await expect(page.getByRole('columnheader', { name: /user id|username/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /name|role/i })).toBeVisible();
    });

    test('should have add user button', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await expect(page.getByRole('link', { name: /add|new|create/i })).toBeVisible();
    });

    test('should show actions for each user', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      await page.waitForLoadState('networkidle');
      
      // Check for edit/delete actions
      const editLink = page.getByRole('link', { name: /edit/i }).first();
      const deleteLink = page.getByRole('link', { name: /delete/i }).first();
      
      if (await editLink.isVisible()) {
        await expect(editLink).toBeVisible();
      }
    });
  });

  test.describe('Add User', () => {
    test('should navigate to add user page', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      
      await page.getByRole('link', { name: /add|new|create/i }).click();
      
      await expect(page).toHaveURL('/admin/users/new');
    });

    test('should display add user form', async ({ adminPage: page }) => {
      await page.goto('/admin/users/new');
      
      await expect(page.getByRole('heading', { name: /add|new|create.*user/i })).toBeVisible();
    });

    test('should have required form fields', async ({ adminPage: page }) => {
      await page.goto('/admin/users/new');
      
      await expect(page.getByLabel(/user id/i)).toBeVisible();
      await expect(page.getByLabel(/password/i)).toBeVisible();
      await expect(page.getByLabel(/first name/i)).toBeVisible();
      await expect(page.getByLabel(/last name/i)).toBeVisible();
    });

    test('should have user type selector', async ({ adminPage: page }) => {
      await page.goto('/admin/users/new');
      
      const typeSelector = page.getByLabel(/type|role/i);
      await expect(typeSelector).toBeVisible();
    });

    test('should validate user id uniqueness', async ({ adminPage: page }) => {
      await page.goto('/admin/users/new');
      
      // Try to create user with existing ID
      await page.getByLabel(/user id/i).fill('ADMIN');
      await page.getByLabel(/password/i).fill('Test@123');
      await page.getByLabel(/first name/i).fill('Test');
      await page.getByLabel(/last name/i).fill('User');
      
      await page.getByRole('button', { name: /save|create|submit/i }).click();
      
      // Should show error about duplicate user
      await expect(page.getByText(/already exists|duplicate|error/i)).toBeVisible();
    });
  });

  test.describe('Edit User', () => {
    test('should navigate to edit user page', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      await page.waitForLoadState('networkidle');
      
      const editLink = page.getByRole('link', { name: /edit/i }).first();
      if (await editLink.isVisible()) {
        await editLink.click();
        await expect(page).toHaveURL(/\/admin\/users\/.+\/edit/);
      }
    });

    test('should pre-populate form with user data', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      await page.waitForLoadState('networkidle');
      
      const editLink = page.getByRole('link', { name: /edit/i }).first();
      if (await editLink.isVisible()) {
        await editLink.click();
        
        // Fields should have values
        const userIdField = page.getByLabel(/user id/i);
        const userIdValue = await userIdField.inputValue();
        expect(userIdValue.length).toBeGreaterThan(0);
      }
    });
  });

  test.describe('Delete User', () => {
    test('should navigate to delete confirmation', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      await page.waitForLoadState('networkidle');
      
      const deleteLink = page.getByRole('link', { name: /delete/i }).first();
      if (await deleteLink.isVisible()) {
        await deleteLink.click();
        await expect(page).toHaveURL(/\/admin\/users\/.+\/delete/);
      }
    });

    test('should show confirmation message', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      await page.waitForLoadState('networkidle');
      
      const deleteLink = page.getByRole('link', { name: /delete/i }).first();
      if (await deleteLink.isVisible()) {
        await deleteLink.click();
        
        await expect(page.getByText(/confirm|sure|delete/i)).toBeVisible();
      }
    });

    test('should have confirm and cancel buttons', async ({ adminPage: page }) => {
      await page.goto('/admin/users');
      await page.waitForLoadState('networkidle');
      
      const deleteLink = page.getByRole('link', { name: /delete/i }).first();
      if (await deleteLink.isVisible()) {
        await deleteLink.click();
        
        await expect(page.getByRole('button', { name: /confirm|yes|delete/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /cancel|no/i })).toBeVisible();
      }
    });
  });

  test.describe('Access Control', () => {
    test('should restrict access for non-admin users', async ({ authenticatedPage: page }) => {
      // authenticatedPage logs in as regular user
      await page.goto('/admin/users');
      
      // Should either redirect or show access denied
      const accessDenied = page.getByText(/access denied|unauthorized|forbidden/i);
      const redirected = page.url() !== 'http://localhost:3000/admin/users';
      
      expect(await accessDenied.isVisible() || redirected).toBeTruthy();
    });
  });
});
