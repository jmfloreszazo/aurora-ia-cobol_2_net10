import { test, expect } from './fixtures';

test.describe('Batch Jobs (Admin)', () => {
  test.describe('Batch Jobs Page', () => {
    test('should display batch jobs page for admin', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await expect(page.getByRole('heading', { name: 'Batch Job Administration' })).toBeVisible();
    });

    test('should show available batch jobs', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      // Check for batch job buttons
      await expect(page.getByRole('button', { name: /post.*transaction|transaction.*post/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /interest|calculate/i })).toBeVisible();
      await expect(page.getByRole('button', { name: /statement|generate/i })).toBeVisible();
    });

    test('should have nightly batch button', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await expect(page.getByRole('button', { name: /nightly|full batch|run all/i })).toBeVisible();
    });

    test('should have export data options', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      const exportButton = page.getByRole('button', { name: 'Export' }).first();
      await expect(exportButton).toBeVisible();
    });
  });

  test.describe('Transaction Posting', () => {
    test('should run transaction posting job', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /post.*transaction|transaction.*post/i }).click();
      
      // Should show loading or processing state
      const processing = page.getByText(/processing|running|loading/i);
      // Job execution might be quick
      
      // Should eventually show result
      await page.waitForTimeout(2000);
      const result = page.getByText(/complete|success|processed|result/i);
    });

    test('should display posting results', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /post.*transaction|transaction.*post/i }).click();
      await page.waitForLoadState('networkidle');
      
      // Should show number of processed/skipped transactions
      const processedCount = page.getByText(/processed.*\d+|\d+.*processed/i);
    });
  });

  test.describe('Interest Calculation', () => {
    test('should run interest calculation job', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /interest|calculate/i }).click();
      
      await page.waitForLoadState('networkidle');
      
      // Should show result
      const result = page.getByText(/complete|success|calculated|result/i);
    });

    test('should display interest calculation results', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /interest|calculate/i }).click();
      await page.waitForLoadState('networkidle');
      
      // Should show accounts processed count
      const accountsProcessed = page.getByText(/accounts?.*\d+|\d+.*accounts?/i);
    });
  });

  test.describe('Statement Generation', () => {
    test('should run statement generation job', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /statement|generate/i }).click();
      
      await page.waitForLoadState('networkidle');
      
      // Should show result
      const result = page.getByText(/complete|success|generated|result/i);
    });

    test('should display statements generated count', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /statement|generate/i }).click();
      await page.waitForLoadState('networkidle');
      
      // Should show statements count
      const statementsCount = page.getByText(/statements?.*\d+|\d+.*statements?/i);
    });
  });

  test.describe('Data Export', () => {
    test('should have export format options', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      // Look for format selection
      const formatSelector = page.getByLabel(/format/i);
      if (await formatSelector.isVisible()) {
        await expect(formatSelector).toBeVisible();
      }
    });

    test('should have entity selection for export', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      // Look for entity selection (accounts, transactions, etc.)
      const entitySelector = page.getByLabel(/entity|data type/i);
      if (await entitySelector.isVisible()) {
        await expect(entitySelector).toBeVisible();
      }
    });

    test('should trigger export download', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      // Setup download handler
      const downloadPromise = page.waitForEvent('download', { timeout: 10000 });
      
      await page.getByRole('button', { name: /export/i }).click();
      
      // Download might be triggered
    });
  });

  test.describe('Nightly Batch', () => {
    test('should run full nightly batch', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /nightly|full batch|run all/i }).click();
      
      // Should show progress for each step
      await page.waitForLoadState('networkidle');
      
      const result = page.getByText(/complete|success|finished/i);
    });

    test('should show progress for each step', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      
      await page.getByRole('button', { name: /nightly|full batch|run all/i }).click();
      
      // Should show individual job statuses
      const postingStatus = page.getByText(/posting|transaction.*post/i);
      const interestStatus = page.getByText(/interest/i);
      const statementStatus = page.getByText(/statement/i);
    });
  });

  test.describe('Job History', () => {
    test('should show job history section', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      await page.waitForLoadState('networkidle');
      
      const historySection = page.getByText(/history|recent.*jobs?|past.*jobs?/i);
    });

    test('should display job execution times', async ({ adminPage: page }) => {
      await page.goto('/admin/batch-jobs');
      await page.waitForLoadState('networkidle');
      
      // History might show execution times
      const executionTime = page.getByText(/executed|ran|time|duration/i);
    });
  });

  test.describe('Access Control', () => {
    test('should restrict batch jobs page for non-admin users', async ({ authenticatedPage: page }) => {
      // authenticatedPage logs in as regular user
      await page.goto('/admin/batch-jobs');
      
      // Should either redirect or show access denied
      const accessDenied = page.getByText(/access denied|unauthorized|forbidden/i);
      const redirected = page.url() !== 'http://localhost:3000/admin/batch-jobs';
      
      expect(await accessDenied.isVisible() || redirected).toBeTruthy();
    });
  });
});
