import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Unit, CreateUnit } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class UnitService {
  private apiUrl = `${environment.apiUrl}/units`;
  private cachedUnits: Unit[] = [];

  constructor(private http: HttpClient) {}

  getAll(): Observable<Unit[]> {
    return this.http.get<Unit[]>(this.apiUrl).pipe(
      tap(units => this.cachedUnits = units)
    );
  }

  getCached(): Unit[] {
    return this.cachedUnits;
  }

  getById(id: number): Observable<Unit> {
    return this.http.get<Unit>(`${this.apiUrl}/${id}`);
  }

  getByFloor(floor: number): Observable<Unit[]> {
    return this.http.get<Unit[]>(`${this.apiUrl}/floor/${floor}`);
  }

  create(unit: CreateUnit): Observable<Unit> {
    return this.http.post<Unit>(this.apiUrl, unit);
  }

  update(id: number, unit: any): Observable<Unit> {
    return this.http.put<Unit>(`${this.apiUrl}/${id}`, unit);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
