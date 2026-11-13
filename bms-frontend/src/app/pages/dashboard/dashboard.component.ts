import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { ReportService } from '../../services/report.service';
import { AuthResponse, DashboardSummary } from '../../models/models';
import { NotificationBellComponent } from '../../components/notification-bell.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationBellComponent],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  currentUser: AuthResponse | null = null;
  summary: DashboardSummary | null = null;
  error = '';

  constructor(
    private authService: AuthService,
    private reportService: ReportService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadStats();
  }

  loadStats(): void {
    this.reportService.getDashboard().subscribe({
      next: (summary) => { this.summary = summary; this.error = ''; },
      error: () => { this.error = 'Could not load dashboard data.'; }
    });
  }

  barHeight(amount: number): number {
    const maximum = Math.max(...(this.summary?.revenue.map(item => item.amount) ?? [0]), 1);
    return amount === 0 ? 0 : Math.max(5, amount / maximum * 100);
  }

  monthLabel(year: number, month: number): string {
    return new Date(year, month - 1).toLocaleString('en', { month: 'short' });
  }

  occupancyStyle(): string {
    const occupancy = this.summary && this.summary.totalUnits > 0
      ? this.summary.occupiedUnits / this.summary.totalUnits * 100
      : 0;
    return `conic-gradient(#1a3c5e ${occupancy}%, #e8edf2 0)`;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
