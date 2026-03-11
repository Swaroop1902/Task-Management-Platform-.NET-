import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface TasksByUserReport {
    userId: number;
    username: string;
    taskCount: number;
}

export interface TasksByStatusReport {
    status: string;
    taskCount: number;
}

export interface SLABreachReport {
    taskId: number;
    taskTitle: string;
    assigneeUsername: string;
    daysOverdue: number;
}

@Injectable({
    providedIn: 'root'
})
export class ReportService {
    private apiUrl = `${environment.reportingServiceUrl}/api/reports`;

    constructor(private http: HttpClient) { }

    getTasksByUser(): Observable<TasksByUserReport[]> {
        return this.http.get<TasksByUserReport[]>(`${this.apiUrl}/tasks-by-user`);
    }

    getTasksByStatus(): Observable<TasksByStatusReport[]> {
        return this.http.get<TasksByStatusReport[]>(`${this.apiUrl}/tasks-by-status`);
    }

    getSLABreaches(): Observable<SLABreachReport[]> {
        return this.http.get<SLABreachReport[]>(`${this.apiUrl}/sla-breaches`);
    }
}
