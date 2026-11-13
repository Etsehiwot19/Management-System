import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Tenant, CreateTenant } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class TenantService {
  private apiUrl = `${environment.apiUrl}/tenants`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(this.apiUrl);
  }

  getById(id: number): Observable<Tenant> {
    return this.http.get<Tenant>(`${this.apiUrl}/${id}`);
  }

  search(term: string): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(`${this.apiUrl}/search?term=${term}`);
  }

  create(tenant: CreateTenant): Observable<Tenant> {
    return this.http.post<Tenant>(this.apiUrl, tenant);
  }

  update(id: number, tenant: any): Observable<Tenant> {
    return this.http.put<Tenant>(`${this.apiUrl}/${id}`, tenant);
  }

  deactivate(id: number): Observable<any> {
    return this.http.patch(`${this.apiUrl}/${id}/deactivate`, {});
  }
}
