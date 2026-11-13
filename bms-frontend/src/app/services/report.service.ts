import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DashboardSummary, ReportFilters, ReportsResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly apiUrl = `${environment.apiUrl}/reports`;

  constructor(private http: HttpClient) {}

  getDashboard(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.apiUrl}/dashboard`);
  }

  getReports(filters: ReportFilters): Observable<ReportsResponse> {
    return this.http.get<ReportsResponse>(this.apiUrl, { params: this.toParams(filters) });
  }

  export(filters: ReportFilters): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export`, {
      params: this.toParams(filters),
      responseType: 'blob'
    });
  }

  private toParams(filters: ReportFilters): HttpParams {
    return new HttpParams()
      .set('fromDate', filters.fromDate)
      .set('toDate', filters.toDate)
      .set('expiryWithinDays', filters.expiryWithinDays);
  }
}
