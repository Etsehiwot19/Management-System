import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Payment, RecordPayment } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Payment[]> {
    return this.http.get<Payment[]>(this.apiUrl);
  }

  getByInvoice(invoiceId: number): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.apiUrl}/invoice/${invoiceId}`);
  }

  getByLease(leaseId: number): Observable<Payment[]> {
    return this.http.get<Payment[]>(`${this.apiUrl}/lease/${leaseId}`);
  }

  record(payment: RecordPayment): Observable<Payment> {
    return this.http.post<Payment>(this.apiUrl, payment);
  }
}
