import { test, expect } from '@playwright/test';

test.describe('review flow @smoke', () => {
  test.skip(!process.env.TRIAGEIQ_E2E, 'Set TRIAGEIQ_E2E=1 with stack up (api + web + seeded admin).');

  test('login and open review queue', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/email/i).fill(process.env.SEED_ADMIN_EMAIL ?? 'admin@local.test');
    await page.getByLabel(/password/i).fill(process.env.SEED_ADMIN_PASSWORD ?? 'ChangeMe!123');
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page).toHaveURL(/review/);
    await expect(page.getByRole('heading', { name: /review queue/i })).toBeVisible();
  });
});