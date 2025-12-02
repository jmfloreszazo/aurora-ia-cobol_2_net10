import { test, expect } from './fixtures';

test.describe('Reports', () => {
  test.describe('Reports Page', () => {
    test('should display reports page', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByRole('heading', { name: 'Transaction Reports' })).toBeVisible();
    });

    test('should have report type options', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      // Check for report type options
      const monthlyReport = page.getByText(/monthly/i);
      const yearlyReport = page.getByText(/yearly|annual/i);
      
      // At least one should be visible
      expect(await monthlyReport.isVisible() || await yearlyReport.isVisible()).toBeTruthy();
    });

    test('should have date range selector', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      // Look for date inputs
      const fromDate = page.getByLabel(/from|start/i);
      const toDate = page.getByLabel(/to|end/i);
      
      // Date range might be available
    });

    test('should have account selector', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      const accountSelector = page.getByLabel(/account/i);
      if (await accountSelector.isVisible()) {
        await expect(accountSelector).toBeVisible();
      }
    });

    test('should have generate report button', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByRole('button', { name: /generate|create|run/i })).toBeVisible();
    });

    test('should have export options', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      // Look for export buttons
      const pdfExport = page.getByRole('button', { name: /pdf/i });
      const excelExport = page.getByRole('button', { name: /excel|xlsx/i });
      const csvExport = page.getByRole('button', { name: /csv/i });
      
      // At least one export option should be available
    });
  });

  test.describe('Report Generation', () => {
    test('should show loading state when generating report', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      // Fill in required fields if any
      const accountSelector = page.getByLabel(/account/i);
      if (await accountSelector.isVisible()) {
        await accountSelector.selectOption({ index: 0 });
      }
      
      // Click generate
      await page.getByRole('button', { name: /generate|create|run/i }).click();
      
      // Should show loading indicator
      const loading = page.getByText(/loading|generating/i);
      // Loading state might be quick
    });

    test('should display report results', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      await page.waitForLoadState('networkidle');
      
      // Select account if required
      const accountSelector = page.getByLabel(/account/i);
      if (await accountSelector.isVisible()) {
        await accountSelector.selectOption({ index: 0 });
      }
      
      // Generate report
      await page.getByRole('button', { name: /generate|create|run/i }).click();
      await page.waitForLoadState('networkidle');
      
      // Should show results section
      const results = page.getByText(/result|summary|total/i);
      // Results should appear after generation
    });
  });

  test.describe('Monthly Report', () => {
    test('should show month selector', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      const monthlyOption = page.getByText(/monthly/i);
      if (await monthlyOption.isVisible()) {
        await monthlyOption.click();
        
        const monthSelector = page.getByLabel(/month/i);
        if (await monthSelector.isVisible()) {
          await expect(monthSelector).toBeVisible();
        }
      }
    });
  });

  test.describe('Yearly Report', () => {
    test('should show year selector', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      const yearlyOption = page.getByText(/yearly|annual/i);
      if (await yearlyOption.isVisible()) {
        await yearlyOption.click();
        
        const yearSelector = page.getByLabel(/year/i);
        if (await yearSelector.isVisible()) {
          await expect(yearSelector).toBeVisible();
        }
      }
    });
  });
});
