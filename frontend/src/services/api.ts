import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class Api {
  /*
  private baseUrl =  'https://api.tuservidor.com';

  constructor(private http: HttpClient) {}

  // ejemplo GET: obtener lista de items
  getItems(): Observable<Item[]> {
    return this.http.get<Item[]>(`${this.baseUrl}/items`)
      .pipe(
        catchError(this.handleError)
      );
  }

  // ejemplo GET por id
  getItem(id: number): Observable<Item> {
    return this.http.get<Item>(`${this.baseUrl}/items/${id}`)
      .pipe(catchError(this.handleError));
  }

  // ejemplo POST: enviar JSON (login, crear recurso, etc.)
  createItem(payload: CreateItemDto): Observable<Item> {
    return this.http.post<Item>(`${this.baseUrl}/items`, payload)
      .pipe(catchError(this.handleError));
  }
  // ejemplo PUT
  updateItem(id: number, payload: UpdateItemDto): Observable<Item> {
    return this.http.put<Item>(`${this.baseUrl}/items/${id}`, payload)
      .pipe(catchError(this.handleError));
  }
  // ejemplo DELETE
  deleteItem(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/items/${id}`)
      .pipe(catchError(this.handleError));
  }
  // ejemplo con headers (Bearer token)
  login(credentials: LoginDto): Observable<LoginResponse> {
    const headers = new HttpHeaders({'Content-Type': 'application/json'});
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, credentials, { headers })
      .pipe(catchError(this.handleError));
  }
  // handler común de errores
  private handleError(error: any) {
    // puedes mejorar con lógica más específica
    const errMsg = error?.error?.message || error.statusText || 'Server error';
    return throwError(() => new Error(errMsg));
  }*/
}
