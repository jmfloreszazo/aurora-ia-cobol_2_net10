import { test as base, expect, Page } from '@playwright/test';

// Test user credentials
export const TEST_USERS = {
  admin: {
    userId: 'ADMIN',
    password: 'Admin@123',
    role: 'ADMIN'
  },
  user: {
    userId: 'USER01',
    password: 'User@123',
    role: 'USER'
  }
};

// Extended test fixture with authentication helpers
export const test = base.extend<{
  authenticatedPage: Page;
  adminPage: Page;
}>({
  authenticatedPage: async ({ page }, use) => {
    await loginAs(page, TEST_USERS.user);
    await use(page);
  },
  adminPage: async ({ page }, use) => {
    await loginAs(page, TEST_USERS.admin);
    await use(page);
  }
});

// Login helper function
export async function loginAs(page: Page, user: { userId: string; password: string }) {
  await page.goto('/login');
  await page.getByLabel('User ID').fill(user.userId);
  await page.getByLabel('Password').fill(user.password);
  await page.getByRole('button', { name: 'Sign in' }).click();
  await page.waitForURL('/dashboard');
}

// Logout helper function
export async function logout(page: Page) {
  await page.getByRole('button', { name: /logout|sign out/i }).click();
  await page.waitForURL('/login');
}

// Wait for loading to complete
export async function waitForLoadingComplete(page: Page) {
  await page.waitForLoadState('networkidle');
  // Wait for any loading spinners to disappear
  const spinner = page.locator('[data-testid="loading-spinner"], .animate-spin');
  if (await spinner.count() > 0) {
    await spinner.first().waitFor({ state: 'hidden', timeout: 10000 });
  }
}

// Format currency for assertions
export function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD'
  }).format(amount);
}

// Generate random test data
export function generateTestData() {
  const timestamp = Date.now();
  return {
    uniqueId: `TEST${timestamp}`,
    amount: Math.floor(Math.random() * 1000) + 1,
    description: `E2E Test ${timestamp}`
  };
}

export { expect };
