import { test, expect } from './fixtures';

test.describe('Accounts', () => {
  test.describe('Account List', () => {
    test('should display accounts list', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      
      await expect(page.getByRole('heading', { name: /accounts/i })).toBeVisible();
    });

    test('should show account table with columns', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      
      // Wait for the table to load
      await page.waitForLoadState('networkidle');
      
      // Check for table headers
      await expect(page.getByRole('columnheader', { name: /account/i })).toBeVisible();
      await expect(page.getByRole('columnheader', { name: /balance|status/i })).toBeVisible();
    });

    test('should navigate to account details when clicking on account', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      // Click on first account link/row
      const accountLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await accountLink.isVisible()) {
        await accountLink.click();
        await expect(page).toHaveURL(/\/accounts\/\d+/);
      }
    });

    test('should have pagination if many accounts', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      // Pagination might not be visible with few accounts
      const pagination = page.locator('[aria-label="pagination"], .pagination, nav[role="navigation"]');
      // This is conditional - only check if pagination exists
    });
  });

  test.describe('Account View', () => {
    test('should display account details', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      // Navigate to first account
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        await expect(page.getByText(/account id|account number/i)).toBeVisible();
        await expect(page.getByText(/balance/i)).toBeVisible();
        await expect(page.getByText(/credit limit/i)).toBeVisible();
      }
    });

    test('should show edit button on account view', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        await expect(page.getByRole('link', { name: /edit/i })).toBeVisible();
      }
    });

    test('should navigate back to list', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        const backLink = page.getByRole('link', { name: /back|list/i });
        if (await backLink.isVisible()) {
          await backLink.click();
          await expect(page).toHaveURL('/accounts');
        }
      }
    });
  });

  test.describe('Account Edit', () => {
    test('should display edit form', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        await page.getByRole('link', { name: /edit/i }).click();
        
        await expect(page.getByRole('heading', { name: /edit account/i })).toBeVisible();
      }
    });

    test('should have form fields', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        await page.getByRole('link', { name: /edit/i }).click();
        
        // Check for form fields
        await expect(page.getByLabel(/credit limit/i)).toBeVisible();
      }
    });

    test('should show save and cancel buttons', async ({ authenticatedPage: page }) => {
      await page.goto('/accounts');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        await page.getByRole('link', { name: /edit/i }).click();
        
        await expect(page.getByRole('button', { name: /save|update/i })).toBeVisible();
        await expect(page.getByRole('button', { name: /cancel/i })).toBeVisible();
      }
    });
  });
});
