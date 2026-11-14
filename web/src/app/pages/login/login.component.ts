import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="container py-5">
      <div class="row justify-content-center">
        <div class="col-md-5">
          <div class="card shadow-sm">
            <div class="card-body p-4">
              <h1 class="h4 mb-3">TriageIQ — Sign in</h1>
              @if (errorMessage) {
                <div class="alert alert-danger" role="alert">{{ errorMessage }}</div>
              }
              <form [formGroup]="form" (ngSubmit)="onSubmit()">
                <div class="mb-3">
                  <label class="form-label" for="email">Email</label>
                  <input
                    id="email"
                    type="email"
                    class="form-control"
                    formControlName="email"
                    autocomplete="username"
                  />
                </div>
                <div class="mb-3">
                  <label class="form-label" for="password">Password</label>
                  <input
                    id="password"
                    type="password"
                    class="form-control"
                    formControlName="password"
                    autocomplete="current-password"
                  />
                </div>
                <button
                  type="submit"
                  class="btn btn-primary w-100"
                  [disabled]="form.invalid || submitting"
                >
                  {{ submitting ? 'Signing in…' : 'Sign in' }}
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  errorMessage = '';
  submitting = false;

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    this.errorMessage = '';
    this.submitting = true;

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting = false;
        void this.router.navigate(['/reviews']);
      },
      error: () => {
        this.submitting = false;
        this.errorMessage = 'Invalid email or password.';
      },
    });
  }
}