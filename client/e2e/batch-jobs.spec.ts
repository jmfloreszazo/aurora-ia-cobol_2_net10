import { test, expect } from './fixtures';

test.describe('Batch Jobs (Admin)', () => {
  test.describe('Batch Jobs Page', () => {
    test('should display batch jobs page for admin', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await expect(page.getByRole('heading', { name: 'Batch Job Administration' })).toBeVisible();
    });

    test('should show batch job sections', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      // Check for batch job sections
      await expect(page.getByRole('heading', { name: 'Transaction Posting' })).toBeVisible();
      await expect(page.getByRole('heading', { name: 'Interest Calculation' })).toBeVisible();
    });

    test('should show export options', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await expect(page.getByRole('heading', { name: 'Export Accounts' })).toBeVisible();
    });
  });

  test.describe('Access Control', () => {
    test('should restrict batch jobs page for non-admin users', async ({ authenticatedPage: page }) => {
      // authenticatedPage logs in as regular user
      await page.goto('/admin/batch-jobs');
      
      // Should show access denied message
      await expect(page.getByText(/Access denied|Admin privileges required/i)).toBeVisible();
    });
  });
});
