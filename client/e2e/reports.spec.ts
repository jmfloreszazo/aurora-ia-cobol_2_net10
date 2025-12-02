import { test, expect } from './fixtures';

test.describe('Reports', () => {
  test.describe('Reports Page', () => {
    test('should display reports page', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByRole('heading', { name: /Transaction Reports/i }).first()).toBeVisible();
    });

    test('should have report type options', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByText('Monthly Report')).toBeVisible();
      await expect(page.getByText('Yearly Report')).toBeVisible();
      await expect(page.getByText('Custom Date Range')).toBeVisible();
    });

    test('should have output options', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByText('Display on Screen')).toBeVisible();
      await expect(page.getByText('Print Report')).toBeVisible();
    });

    test('should have generate report button', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByRole('button', { name: /Generate Report/i })).toBeVisible();
    });

    test('should have exit button', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByRole('main').getByRole('button', { name: 'F3=Exit' })).toBeVisible();
    });

    test('should have clear button', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await expect(page.getByRole('button', { name: 'F4=Clear' })).toBeVisible();
    });
  });

  test.describe('Report Type Selection', () => {
    test('should show month selector for monthly report', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      // Monthly report is default
      await expect(page.getByText('Month', { exact: true })).toBeVisible();
    });

    test('should show year selector for yearly report', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await page.getByText('Yearly Report').click();
      
      await expect(page.getByText('Year', { exact: true })).toBeVisible();
    });

    test('should show date range for custom report', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await page.getByText('Custom Date Range').click();
      
      await expect(page.getByText('Start Date')).toBeVisible();
      await expect(page.getByText('End Date')).toBeVisible();
    });
  });

  test.describe('Report Generation', () => {
    test('should generate and display report', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await page.getByRole('button', { name: /Generate Report/i }).click();
      
      // Should show summary section
      await expect(page.getByText('Total Transactions')).toBeVisible();
    });

    test('should show report table after generation', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await page.getByRole('button', { name: /Generate Report/i }).click();
      
      // Should show table with month column
      await expect(page.getByRole('columnheader', { name: 'Month' })).toBeVisible();
    });

    test('should show export options after generation', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await page.getByRole('button', { name: /Generate Report/i }).click();
      
      await expect(page.getByRole('button', { name: 'Export CSV' })).toBeVisible();
      await expect(page.getByRole('button', { name: 'Export PDF' })).toBeVisible();
    });
  });

  test.describe('Navigation', () => {
    test('should navigate to reports from sidebar', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await page.getByRole('link', { name: 'Reports' }).click();
      
      await expect(page).toHaveURL('/reports');
    });

    test('should exit to dashboard', async ({ authenticatedPage: page }) => {
      await page.goto('/reports');
      
      await page.getByRole('main').getByRole('button', { name: 'F3=Exit' }).click();
      
      await expect(page).toHaveURL('/dashboard');
    });
  });
});
