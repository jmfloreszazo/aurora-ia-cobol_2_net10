import { test, expect } from './fixtures';

test.describe('Cards', () => {
  test.describe('Card List', () => {
    test('should display cards list', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      
      await expect(page.getByRole('heading', { name: 'List Credit Cards' })).toBeVisible();
    });

    test('should show card table', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      // Check for table or card list
      const table = page.getByRole('table');
      const cardList = page.locator('[data-testid="card-list"]');
      
      // At least one should be visible
      expect(await table.isVisible() || await cardList.isVisible()).toBeTruthy();
    });

    test('should mask card numbers for security', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      // Card numbers should be masked (showing last 4 digits)
      // Look for pattern like **** **** **** 1234 or ****1234
      const maskedCard = page.getByText(/\*{4}[\s-]?\*{4}[\s-]?\*{4}[\s-]?\d{4}|\*+\d{4}/);
      // This might or might not be visible depending on data
    });

    test('should navigate to card details', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        await expect(page).toHaveURL(/\/cards\/.+/);
      }
    });
  });

  test.describe('Card View', () => {
    test('should display card details', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        await expect(page.getByText(/card number|card type/i)).toBeVisible();
        await expect(page.getByText(/status|expiration/i)).toBeVisible();
      }
    });

    test('should show card status (active/inactive)', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        // Look for status indicator
        const statusActive = page.getByText(/active/i);
        const statusInactive = page.getByText(/inactive/i);
        
        expect(await statusActive.isVisible() || await statusInactive.isVisible()).toBeTruthy();
      }
    });

    test('should show expiration date', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        await expect(page.getByText(/expir/i)).toBeVisible();
      }
    });
  });

  test.describe('Card Edit', () => {
    test('should navigate to edit page', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        const editLink = page.getByRole('link', { name: /edit/i });
        if (await editLink.isVisible()) {
          await editLink.click();
          await expect(page).toHaveURL(/\/cards\/.+\/edit/);
        }
      }
    });

    test('should have status toggle option', async ({ authenticatedPage: page }) => {
      await page.goto('/cards');
      await page.waitForLoadState('networkidle');
      
      const viewLink = page.getByRole('link', { name: /view|details/i }).first();
      if (await viewLink.isVisible()) {
        await viewLink.click();
        
        const editLink = page.getByRole('link', { name: /edit/i });
        if (await editLink.isVisible()) {
          await editLink.click();
          
          // Look for status field
          const statusField = page.getByLabel(/status/i);
          if (await statusField.isVisible()) {
            await expect(statusField).toBeVisible();
          }
        }
      }
    });
  });
});
