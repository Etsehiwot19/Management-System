import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { NotificationItem } from '../models/models';
import { NotificationService } from '../services/notification.service';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notification-bell.component.html'
})
export class NotificationBellComponent implements OnInit {
  notifications: NotificationItem[] = [];
  unreadCount = 0;
  open = false;
  error = '';

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadUnreadCount();
  }

  toggle(): void {
    this.open = !this.open;
    if (this.open) {
      this.error = '';
      this.notificationService.getAll().subscribe({
        next: (notifications) => { this.notifications = notifications; },
        error: () => { this.error = 'Could not load notifications.'; }
      });
    }
  }

  markAsRead(notification: NotificationItem): void {
    if (notification.isRead) return;
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
        this.unreadCount = Math.max(0, this.unreadCount - 1);
      },
      error: () => { this.error = 'Could not update notification.'; }
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.notifications = this.notifications.map(notification => ({ ...notification, isRead: true }));
        this.unreadCount = 0;
      },
      error: () => { this.error = 'Could not update notifications.'; }
    });
  }

  private loadUnreadCount(): void {
    this.notificationService.getUnreadCount().subscribe({
      next: (count) => { this.unreadCount = count; },
      error: () => { this.error = 'Could not load notifications.'; }
    });
  }
}
