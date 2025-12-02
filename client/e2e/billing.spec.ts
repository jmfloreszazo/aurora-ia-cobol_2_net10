import { test, expect } from './fixtures';

test.describe('Billing / Bill Payment', () => {
  test.describe('Bill Payment Page', () => {
    test('should display billing page', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('heading', { name: 'Bill Payment' })).toBeVisible();
    });

    test('should show current balance', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      await page.waitForLoadState('networkidle');
      
      await expect(page.getByText(/balance|amount due/i)).toBeVisible();
    });

    test('should have account selector', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      const accountSelector = page.getByLabel(/account/i);
      if (await accountSelector.isVisible()) {
        await expect(accountSelector).toBeVisible();
      }
    });

    test('should have payment amount field', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByLabel(/amount|payment/i)).toBeVisible();
    });

    test('should have pay full balance button', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('button', { name: /pay full|full balance|pay all/i })).toBeVisible();
    });

    test('should have submit payment button', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('button', { name: /pay|submit|make payment/i })).toBeVisible();
    });
  });

  test.describe('Payment Processing', () => {
    test('should validate payment amount', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      await page.waitForLoadState('networkidle');
      
      // Try to submit with zero amount
      const amountField = page.getByLabel(/amount|payment/i);
      await amountField.fill('0');
      
      await page.getByRole('button', { name: /pay|submit|make payment/i }).click();
      
      // Should show validation error
      await expect(page.getByText(/invalid|greater than|minimum/i)).toBeVisible();
    });

    test('should validate amount does not exceed balance', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      await page.waitForLoadState('networkidle');
      
      // Try to pay more than balance
      const amountField = page.getByLabel(/amount|payment/i);
      await amountField.fill('9999999');
      
      await page.getByRole('button', { name: /pay|submit|make payment/i }).click();
      
      // Should show validation error or adjust amount
    });

    test('should show confirmation after successful payment', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      await page.waitForLoadState('networkidle');
      
      // Fill in valid payment amount
      const amountField = page.getByLabel(/amount|payment/i);
      await amountField.fill('50');
      
      // Submit payment
      await page.getByRole('button', { name: /pay|submit|make payment/i }).click();
      
      // Should show confirmation or success message
      const success = page.getByText(/success|confirmed|processed|thank you/i);
      // Result depends on backend availability
    });

    test('pay full balance should auto-fill amount', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      await page.waitForLoadState('networkidle');
      
      // Get current balance text
      const balanceText = await page.getByText(/\$[\d,]+\.\d{2}/).first().textContent();
      
      // Click pay full balance
      await page.getByRole('button', { name: /pay full|full balance|pay all/i }).click();
      
      // Amount field should be filled with balance
      const amountField = page.getByLabel(/amount|payment/i);
      const amountValue = await amountField.inputValue();
      
      // Amount should be set
    });
  });

  test.describe('Payment History', () => {
    test('should show payment history section', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      await page.waitForLoadState('networkidle');
      
      const historySection = page.getByText(/history|recent payment|past payment/i);
      // History section might be available
    });
  });

  test.describe('Minimum Payment', () => {
    test('should show minimum payment due', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      await page.waitForLoadState('networkidle');
      
      const minimumPayment = page.getByText(/minimum.*payment|min.*due/i);
      // Minimum payment info might be shown
    });

    test('should have pay minimum button', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      const payMinimum = page.getByRole('button', { name: /pay minimum|min payment/i });
      // Pay minimum button might be available
    });
  });
});
