import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { ReviewQueueItemDto } from '../../core/models/review.models';
import { AuthService } from '../../core/auth/auth.service';
import { ReviewService } from '../../core/reviews/review.service';

@Component({
  selector: 'app-review-queue',
  standalone: true,
  imports: [DatePipe, DecimalPipe],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark mb-4">
      <div class="container-fluid">
        <span class="navbar-brand mb-0 h1">Review queue</span>
        <button type="button" class="btn btn-outline-light btn-sm" (click)="signOut()">
          Sign out
        </button>
      </div>
    </nav>

    <div class="container pb-5">
      @if (loadError) {
        <div class="alert alert-danger" role="alert">{{ loadError }}</div>
      }

      @if (loading) {
        <p class="text-muted">Loading queue…</p>
      } @else if (!items.length) {
        <p class="text-muted">No tickets awaiting review.</p>
      } @else {
        <div class="table-responsive">
          <table class="table table-striped table-hover align-middle bg-white shadow-sm">
            <thead class="table-light">
              <tr>
                <th>Subject</th>
                <th>Status</th>
                <th>Category</th>
                <th>Priority</th>
                <th>Confidence</th>
                <th>Reason</th>
                <th>Created</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (item of items; track item.ticketId) {
                <tr>
                  <td>
                    <div class="fw-semibold">{{ item.subject }}</div>
                    <div class="small text-muted text-truncate" style="max-width: 18rem">
                      {{ item.summary }}
                    </div>
                  </td>
                  <td><span class="badge text-bg-secondary">{{ item.status }}</span></td>
                  <td>{{ item.category }}</td>
                  <td>{{ item.priority }}</td>
                  <td>{{ item.confidence | number: '1.2-2' }}</td>
                  <td><code class="small">{{ item.reviewReason }}</code></td>
                  <td class="text-nowrap">{{ item.createdAt | date: 'short' }}</td>
                  <td class="text-end">
                    <button
                      type="button"
                      class="btn btn-sm btn-success"
                      [disabled]="approvingId === item.ticketId"
                      (click)="approve(item)"
                    >
                      {{ approvingId === item.ticketId ? 'Approving…' : 'Approve' }}
                    </button>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      }
    </div>
  `,
})
export class ReviewQueueComponent implements OnInit {
  private readonly reviews = inject(ReviewService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  items: ReviewQueueItemDto[] = [];
  loading = true;
  loadError = '';
  approvingId: string | null = null;

  ngOnInit(): void {
    this.refreshQueue();
  }

  approve(item: ReviewQueueItemDto): void {
    this.approvingId = item.ticketId;
    this.reviews.approve(item.ticketId).subscribe({
      next: () => {
        this.approvingId = null;
        this.items = this.items.filter((row) => row.ticketId !== item.ticketId);
      },
      error: () => {
        this.approvingId = null;
        this.loadError = 'Approve failed. Try again or refresh the queue.';
      },
    });
  }

  signOut(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }

  private refreshQueue(): void {
    this.loading = true;
    this.loadError = '';

    this.reviews.getQueue().subscribe({
      next: (queue) => {
        this.items = queue;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.loadError = 'Could not load the review queue.';
      },
    });
  }
}