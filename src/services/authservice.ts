
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface LoginResponse {
  status: string;   // ejemplo: 'ok' | 'error'
  token?: string;   // futura implementación JWT JWT token 
}

@Injectable({
  providedIn: 'root'
})
export class Authservice {
  private baseUrl = 'https://localhost:5001/api'; //revisar esto , la api cambia de direccion 
  constructor(private http: HttpClient) {}
  login(credentials: LoginCredentials): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, credentials);
  }
}