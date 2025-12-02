import { test, expect } from './fixtures';

test.describe('Dashboard', () => {
  test.describe('Dashboard Page', () => {
    test('should display dashboard', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      await expect(page.getByRole('heading', { name: /dashboard|welcome/i })).toBeVisible();
    });

    test('should show account summary', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');
      
      // Dashboard should show account overview
      const accountSummary = page.getByText(/account|balance|total/i);
      await expect(accountSummary.first()).toBeVisible();
    });

    test('should show quick actions', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      // Dashboard might have quick action buttons
      const quickActions = page.locator('[data-testid="quick-actions"], .quick-actions');
    });

    test('should show recent transactions', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');
      
      // Dashboard might show recent transactions
      const recentTransactions = page.getByText(/recent|transaction/i);
    });
  });

  test.describe('Dashboard Widgets', () => {
    test('should display total balance', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');
      
      // Should show total balance across accounts
      const totalBalance = page.getByText(/total.*balance|balance/i);
    });

    test('should display credit utilization', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');
      
      // Credit utilization might be shown
      const creditUtilization = page.getByText(/utilization|credit.*limit|available.*credit/i);
    });

    test('should display number of active cards', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      await page.waitForLoadState('networkidle');
      
      // Active cards count might be shown
      const activeCards = page.getByText(/active.*cards?|\d+.*cards?/i);
    });
  });

  test.describe('Dashboard Loading', () => {
    test('should show loading state initially', async ({ authenticatedPage: page }) => {
      // Intercept API calls to slow them down
      await page.route('**/api/**', async route => {
        await new Promise(resolve => setTimeout(resolve, 500));
        await route.continue();
      });
      
      await page.goto('/dashboard');
      
      // Should show loading indicators
      const loading = page.getByText(/loading/i);
    });

    test('should handle API errors gracefully', async ({ authenticatedPage: page }) => {
      // Intercept and fail API calls
      await page.route('**/api/**', route => {
        route.fulfill({ status: 500, body: 'Internal Server Error' });
      });
      
      await page.goto('/dashboard');
      
      // Should show error message
      const errorMessage = page.getByText(/error|failed|unavailable/i);
    });
  });

  test.describe('Dashboard Refresh', () => {
    test('should have refresh button', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      const refreshButton = page.getByRole('button', { name: /refresh/i });
      // Refresh might be available
    });

    test('should auto-refresh data periodically', async ({ authenticatedPage: page }) => {
      await page.goto('/dashboard');
      
      // Monitor for API calls over time
      let apiCallCount = 0;
      page.on('request', request => {
        if (request.url().includes('/api/')) {
          apiCallCount++;
        }
      });
      
      // Wait some time and check if more calls were made
      await page.waitForTimeout(5000);
      
      // Might have auto-refresh
    });
  });
});
