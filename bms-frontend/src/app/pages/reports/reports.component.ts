import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ReportService } from '../../services/report.service';
import { ReportFilters, ReportsResponse } from '../../models/models';
import { NotificationBellComponent } from '../../components/notification-bell.component';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, NotificationBellComponent],
  templateUrl: './reports.component.html'
})
export class ReportsComponent implements OnInit {
  filters: ReportFilters;
  reports: ReportsResponse | null = null;
  loading = false;
  exporting = false;
  error = '';

  constructor(private reportService: ReportService) {
    const today = new Date();
    const from = new Date(today.getFullYear(), today.getMonth() - 11, 1);
    this.filters = {
      fromDate: this.formatDate(from),
      toDate: this.formatDate(today),
      expiryWithinDays: 30
    };
  }

  ngOnInit(): void {
    this.loadReports();
  }

  loadReports(): void {
    this.loading = true;
    this.error = '';
    this.reportService.getReports(this.filters).subscribe({
      next: (reports) => { this.reports = reports; this.loading = false; },
      error: (error) => {
        this.error = error.error?.message || 'Could not load reports.';
        this.loading = false;
      }
    });
  }

  exportReports(): void {
    this.exporting = true;
    this.error = '';
    this.reportService.export(this.filters).subscribe({
      next: (file) => {
        const url = URL.createObjectURL(file);
        const link = document.createElement('a');
        link.href = url;
        link.download = `bms-reports-${this.filters.toDate}.xlsx`;
        link.click();
        URL.revokeObjectURL(url);
        this.exporting = false;
      },
      error: (error) => {
        this.error = error.error?.message || 'Could not export reports.';
        this.exporting = false;
      }
    });
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
