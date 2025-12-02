import { test, expect } from './fixtures';

test.describe('Cards', () => {
  test.describe('Card List', () => {
    test('should display cards page', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      
      await expect(page.getByRole('heading', { name: 'List Credit Cards' })).toBeVisible();
    });

    test('should show customer selector', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      
      await expect(page.getByText('Select Customer:')).toBeVisible();
    });

    test('should show account number selector', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      
      await expect(page.getByText('Account Number:')).toBeVisible();
    });

    test('should show cards table header', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      
      await expect(page.locator('.bg-gray-800').getByText('Credit Cards')).toBeVisible();
    });

    test('should show message when no account selected', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      
      await expect(page.getByText('Please select a customer and account to view cards.')).toBeVisible();
    });

    test('should have customer dropdown with options', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      const customerSelect = page.locator('select').first();
      await expect(customerSelect).toBeVisible();
      
      // Should have placeholder option
      await expect(page.locator('option', { hasText: /Select Customer/i })).toBeVisible();
    });
  });

  test.describe('Customer Selection', () => {
    test('should enable account selector after customer selection', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      // Account selector should be disabled initially
      const accountSelect = page.locator('select').nth(1);
      await expect(accountSelect).toBeDisabled();
      
      // Select customer if available
      const customerSelect = page.locator('select').first();
      const options = await customerSelect.locator('option').count();
      if (options > 1) {
        await customerSelect.selectOption({ index: 1 });
        await page.waitForLoadState('networkidle');
        
        // Account selector should be enabled after customer selection
        await expect(accountSelect).not.toBeDisabled();
      }
    });
  });

  test.describe('Navigation', () => {
    test('should navigate to cards from sidebar', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Cards' }).click();
      
      await expect(page).toHaveURL('/cards');
      await expect(page.getByRole('heading', { name: 'List Credit Cards' })).toBeVisible();
    });
  });
});
