import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'reviews',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/review-queue/review-queue.component').then(
        (m) => m.ReviewQueueComponent,
      ),
  },
  { path: '', pathMatch: 'full', redirectTo: 'reviews' },
  { path: '**', redirectTo: 'reviews' },
];