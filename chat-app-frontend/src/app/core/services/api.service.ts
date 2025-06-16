import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Auth endpoints
  login(username: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login`, { username, password });
  }

  register(username: string, email: string, password: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register`, { username, email, password });
  }

  refreshToken(refreshToken: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/refresh-token`, { refreshToken });
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/logout`, {});
  }

  // Message endpoints
  getRecentMessages(count: number = 50): Observable<any> {
    return this.http.get(`${this.apiUrl}/message?count=${count}`);
  }

  getMessagesByUser(username: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/message/user/${username}`);
  }

  getMessageCount(): Observable<any> {
    return this.http.get(`${this.apiUrl}/message/count`);
  }

  updateMessage(id: number, message: string): Observable<any> {
    return this.http.put(`${this.apiUrl}/message/${id}`, { message });
  }

  deleteMessage(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/message/${id}`);
  }
} 