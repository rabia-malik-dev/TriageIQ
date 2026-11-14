export interface ReviewQueueItemDto {
  ticketId: string;
  subject: string;
  status: string;
  category: string;
  priority: string;
  summary: string;
  confidence: number;
  reviewReason: string;
  createdAt: string;
}