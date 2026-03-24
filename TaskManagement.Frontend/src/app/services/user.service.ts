import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserDto {
    id: number;
    username: string;
    role: string;
}

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private apiUrl = `${environment.userServiceUrl}/api/users`;

    constructor(private http: HttpClient) { }

    getUsers(): Observable<UserDto[]> {
        return this.http.get<UserDto[]>(this.apiUrl);
    }
}
