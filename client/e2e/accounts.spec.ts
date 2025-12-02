import { test, expect } from './fixtures';

test.describe('Accounts', () => {
  test.describe('Account List', () => {
    test('should display accounts page', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      
      await expect(page.getByRole('heading', { name: 'Account List' })).toBeVisible();
    });

    test('should show customer selector', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      
      await expect(page.getByText('Select Customer:')).toBeVisible();
      await expect(page.locator('select').first()).toBeVisible();
    });

    test('should show accounts table header', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      
      await expect(page.locator('.bg-gray-800').getByText('Accounts')).toBeVisible();
    });

    test('should show message when no customer selected', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      
      await expect(page.getByText('Please select a customer to view their accounts.')).toBeVisible();
    });
  });

  test.describe('Customer Selection', () => {
    test('should have customer options in dropdown', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      const customerSelect = page.locator('select').first();
      await expect(customerSelect).toBeVisible();
      
      // Should have placeholder option
      await expect(page.locator('option', { hasText: /Select a Customer/i })).toBeVisible();
    });

    test('should allow customer selection', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      const customerSelect = page.locator('select').first();
      
      // Try to select first customer
      const options = await customerSelect.locator('option').count();
      if (options > 1) {
        await customerSelect.selectOption({ index: 1 });
        // After selection, message should change or table should appear
        await page.waitForTimeout(500);
      }
    });
  });

  test.describe('Navigation', () => {
    test('should navigate to accounts from sidebar', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Accounts' }).click();
      
      await expect(page).toHaveURL('/accounts');
      await expect(page.getByRole('heading', { name: 'Account List' })).toBeVisible();
    });
  });
});
