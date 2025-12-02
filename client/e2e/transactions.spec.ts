import { test, expect } from './fixtures';

test.describe('Transactions', () => {
  test.describe('Transaction List Page', () => {
    test('should display transactions page', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await expect(page.getByRole('heading', { name: /List Transactions/i }).first()).toBeVisible();
    });

    test('should have customer selector', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await expect(page.getByText('Select Customer:')).toBeVisible();
      await expect(page.locator('select').first()).toBeVisible();
    });

    test('should have account selector', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await expect(page.getByText('Account Number:')).toBeVisible();
    });

    test('should show message before selection', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await expect(page.getByText(/select a customer and account/i)).toBeVisible();
    });

    test('should have add transaction button', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await expect(page.getByRole('button', { name: /Add Transaction/i })).toBeVisible();
    });
  });

  test.describe('Customer Selection', () => {
    test('should have customer dropdown', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      await page.waitForLoadState('networkidle');
      
      const customerSelect = page.locator('select').first();
      await expect(customerSelect).toBeVisible();
    });
  });

  test.describe('Navigation', () => {
    test('should navigate to transactions from sidebar', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Transactions' }).click();
      
      await expect(page).toHaveURL('/transactions');
    });

    test('should navigate to add transaction page', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await page.getByRole('button', { name: /Add Transaction/i }).click();
      
      await expect(page).toHaveURL('/transactions/add');
    });
  });
});
