import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Lease, CreateLease } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class LeaseService {
  private apiUrl = `${environment.apiUrl}/leases`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Lease[]> {
    return this.http.get<Lease[]>(this.apiUrl);
  }

  getById(id: number): Observable<Lease> {
    return this.http.get<Lease>(`${this.apiUrl}/${id}`);
  }

  getByTenant(tenantId: number): Observable<Lease[]> {
    return this.http.get<Lease[]>(`${this.apiUrl}/tenant/${tenantId}`);
  }

  getByUnit(unitId: number): Observable<Lease[]> {
    return this.http.get<Lease[]>(`${this.apiUrl}/unit/${unitId}`);
  }

  create(lease: CreateLease): Observable<Lease> {
    return this.http.post<Lease>(this.apiUrl, lease);
  }

  renew(id: number, data: { newEndDate: string; newMonthlyRent: number }): Observable<Lease> {
    return this.http.patch<Lease>(`${this.apiUrl}/${id}/renew`, data);
  }

  terminate(id: number, data: { terminationReason: string }): Observable<Lease> {
    return this.http.patch<Lease>(`${this.apiUrl}/${id}/terminate`, data);
  }
}
