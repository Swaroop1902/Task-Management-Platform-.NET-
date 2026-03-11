import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface User {
    username: string;
    role: string;
}

export interface LoginResponse {
    token: string;
    username: string;
    role: string;
}

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private apiUrl = environment.userServiceUrl;
    private currentUserSubject = new BehaviorSubject<User | null>(null);
    public currentUser$ = this.currentUserSubject.asObservable();
    private readonly TOKEN_KEY = 'stub_jwt_token';

    constructor(private http: HttpClient) {
        this.checkToken();
    }

    login(username: string, password: string): Observable<LoginResponse> {
        return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, { username, password })
            .pipe(
                tap(response => {
                    localStorage.setItem(this.TOKEN_KEY, response.token);
                    this.currentUserSubject.next({ username: response.username, role: response.role });
                })
            );
    }

    logout(): void {
        localStorage.removeItem(this.TOKEN_KEY);
        this.currentUserSubject.next(null);
    }

    getToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    // Simple mock check to keep state on refresh
    private checkToken() {
        const token = this.getToken();
        if (token) {
            // Decode JWT payload (naive approach for stub token)
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                const username = payload.unique_name || payload.name || 'User';
                const role = payload.role || 'Guest';
                this.currentUserSubject.next({ username, role });
            } catch (e) {
                this.logout();
            }
        }
    }
}
