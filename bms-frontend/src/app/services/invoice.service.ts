import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Invoice, OutstandingBalance } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private apiUrl = `${environment.apiUrl}/invoices`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.apiUrl);
  }

  getById(id: number): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.apiUrl}/${id}`);
  }

  getByLease(leaseId: number): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(`${this.apiUrl}/lease/${leaseId}`);
  }

  getOutstandingBalances(): Observable<OutstandingBalance[]> {
    return this.http.get<OutstandingBalance[]>(`${this.apiUrl}/outstanding-balances`);
  }

  generateMonthly(month: number, year: number): Observable<Invoice[]> {
    return this.http.post<Invoice[]>(`${this.apiUrl}/generate`, { month, year });
  }

  markOverdue(): Observable<any> {
    return this.http.post(`${this.apiUrl}/mark-overdue`, {});
  }
}
