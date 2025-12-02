import { test, expect } from './fixtures';

test.describe('Billing / Bill Payment', () => {
  test.describe('Bill Payment Page', () => {
    test('should display billing page', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('heading', { name: 'Bill Payment' }).first()).toBeVisible();
    });

    test('should have account selector', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('heading', { name: 'Select Account' })).toBeVisible();
      await expect(page.locator('select').first()).toBeVisible();
    });

    test('should have payment amount field', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByText('Payment Amount')).toBeVisible();
    });

    test('should have payment date field', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByText('Payment Date')).toBeVisible();
    });

    test('should have proceed to payment button', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('button', { name: /Proceed to Payment/i })).toBeVisible();
    });

    test('should have exit button', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('main').getByRole('button', { name: 'F3=Exit' })).toBeVisible();
    });

    test('should have clear button', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await expect(page.getByRole('button', { name: 'F4=Clear' })).toBeVisible();
    });
  });

  test.describe('Account Selection', () => {
    test('should display account dropdown', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      const accountSelect = page.locator('select').first();
      await expect(accountSelect).toBeVisible();
      
      // Should have default option
      await expect(page.getByText('Select Account')).toBeVisible();
    });
  });

  test.describe('Payment Validation', () => {
    test('should require account selection', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      // Fill amount without selecting account
      await page.locator('input[type="number"]').fill('100.00');
      await page.getByRole('button', { name: /Proceed to Payment/i }).click();
      
      // Should show validation error
      await expect(page.getByText(/select an account/i)).toBeVisible();
    });
  });

  test.describe('Navigation', () => {
    test('should navigate to billing from sidebar', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Bill Payment' }).click();
      
      await expect(page).toHaveURL('/billing');
    });

    test('should exit to accounts', async ({ authenticatedPage: page }) => {
      await page.goto('/billing');
      
      await page.getByRole('main').getByRole('button', { name: 'F3=Exit' }).click();
      
      await expect(page).toHaveURL('/accounts');
    });
  });
});
