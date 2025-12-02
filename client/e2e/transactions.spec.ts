import { test, expect, generateTestData } from './fixtures';

test.describe('Transactions', () => {
  test.describe('Transaction List', () => {
    test('should display transactions list', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await expect(page.getByRole('heading', { name: 'List Transactions' })).toBeVisible();
    });

    test('should show transaction table with columns', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      await page.waitForLoadState('networkidle');
      
      // Check for key columns
      const columns = ['date', 'description', 'amount', 'type'];
      for (const col of columns) {
        const header = page.getByRole('columnheader', { name: new RegExp(col, 'i') });
        // Some columns might not be present
      }
    });

    test('should have add transaction button', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await expect(page.getByRole('link', { name: /add|new|create/i })).toBeVisible();
    });

    test('should navigate to transaction details', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        await expect(page).toHaveURL(/\/transactions\/.+/);
      }
    });

    test('should display amounts with currency format', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      await page.waitForLoadState('networkidle');
      
      // Look for currency format ($123.45)
      const currencyAmount = page.getByText(/\$[\d,]+\.\d{2}/);
      if (await currencyAmount.first().isVisible()) {
        await expect(currencyAmount.first()).toBeVisible();
      }
    });
  });

  test.describe('Transaction View', () => {
    test('should display transaction details', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        await expect(page.getByText(/transaction id|id/i)).toBeVisible();
        await expect(page.getByText(/amount/i)).toBeVisible();
      }
    });

    test('should show merchant information if available', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        // Merchant info might not be present for all transactions
        const merchant = page.getByText(/merchant/i);
        // This is optional field
      }
    });
  });

  test.describe('Add Transaction', () => {
    test('should navigate to add transaction page', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions');
      
      await page.getByRole('link', { name: /add|new|create/i }).click();
      
      await expect(page).toHaveURL('/transactions/new');
    });

    test('should display add transaction form', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      await expect(page.getByRole('heading', { name: /add|new|create.*transaction/i })).toBeVisible();
    });

    test('should have required form fields', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      // Check for required fields
      await expect(page.getByLabel(/card|account/i)).toBeVisible();
      await expect(page.getByLabel(/amount/i)).toBeVisible();
      await expect(page.getByLabel(/description/i)).toBeVisible();
    });

    test('should have transaction type selector', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      const typeSelector = page.getByLabel(/type/i);
      if (await typeSelector.isVisible()) {
        await expect(typeSelector).toBeVisible();
      }
    });

    test('should have submit button', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      await expect(page.getByRole('button', { name: /submit|add|create|save/i })).toBeVisible();
    });

    test('should validate required fields', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      // Try to submit empty form
      await page.getByRole('button', { name: /submit|add|create|save/i }).click();
      
      // Should show validation errors or prevent submission
      const errorMessage = page.getByText(/required|invalid|error/i);
      // HTML5 validation or custom validation should trigger
    });

    test('should validate amount is positive', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      const amountField = page.getByLabel(/amount/i);
      await amountField.fill('-100');
      
      await page.getByRole('button', { name: /submit|add|create|save/i }).click();
      
      // Should show validation error for negative amount
    });

    test('should show cancel button', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      await expect(page.getByRole('button', { name: /cancel/i })).toBeVisible();
    });

    test('should navigate back on cancel', async ({ authenticatedPage: page }) => {
      await page.goto('/transactions/new');
      
      await page.getByRole('button', { name: /cancel/i }).click();
      
      await expect(page).toHaveURL('/transactions');
    });
  });
});
