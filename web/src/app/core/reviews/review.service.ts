import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ReviewQueueItemDto } from '../models/review.models';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly http = inject(HttpClient);

  getQueue(): Observable<ReviewQueueItemDto[]> {
    return this.http.get<ReviewQueueItemDto[]>(
      `${environment.apiUrl}/api/reviews/queue`,
    );
  }

  approve(ticketId: string): Observable<void> {
    return this.http.post<void>(
      `${environment.apiUrl}/api/reviews/${encodeURIComponent(ticketId)}/approve`,
      {},
    );
  }
}