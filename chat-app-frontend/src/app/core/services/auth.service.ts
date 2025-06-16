import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user';

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(
    private apiService: ApiService,
    private router: Router
  ) {}

  login(username: string, password: string): Observable<any> {
    return this.apiService.login(username, password).pipe(
      tap(response => {
        this.setTokens(response.token, response.refreshToken);
        this.setUser(response.username);
        this.isAuthenticatedSubject.next(true);
      })
    );
  }

  register(username: string, email: string, password: string): Observable<any> {
    return this.apiService.register(username, email, password).pipe(
      tap(response => {
        this.setTokens(response.token, response.refreshToken);
        this.setUser(response.username);
        this.isAuthenticatedSubject.next(true);
      })
    );
  }

  logout(): void {
    this.apiService.logout().subscribe({
      complete: () => {
        this.clearAuth();
        this.router.navigate(['/auth/login']);
      }
    });
  }

  refreshToken(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      this.clearAuth();
      return new Observable();
    }

    return this.apiService.refreshToken(refreshToken).pipe(
      tap(response => {
        this.setTokens(response.token, response.refreshToken);
      })
    );
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getUser(): string | null {
    return localStorage.getItem(this.USER_KEY);
  }

  private setTokens(token: string, refreshToken: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  private setUser(username: string): void {
    localStorage.setItem(this.USER_KEY, username);
  }

  private clearAuth(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.isAuthenticatedSubject.next(false);
  }

  private hasToken(): boolean {
    return !!this.getToken();
  }
} 