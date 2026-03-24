import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ActivityLog {
    id: number;
    taskId: number;
    statusChangedTo: string;
    changedByUserId: number;
    timestamp: Date;
}

export interface TaskItem {
    id: number;
    title: string;
    description?: string;
    priority: string;
    status: string;
    assigneeId?: number;
    createdAt: Date;
    updatedAt: Date;
    dueDate?: Date;
    activityLogs: ActivityLog[];
}

export interface CreateTaskReq {
    title: string;
    description?: string;
    priority: string;
    status: string;
    assigneeId?: number;
    dueDate?: Date | string;
}

export interface UpdateTaskReq {
    title?: string;
    description?: string;
    priority?: string;
    status?: string;
    assigneeId?: number;
    dueDate?: Date | string;
}

@Injectable({
    providedIn: 'root'
})
export class TaskService {
    private apiUrl = `${environment.taskServiceUrl}/api/tasks`;

    constructor(private http: HttpClient) { }

    getTasks(status?: string, assigneeId?: number, startDate?: string, endDate?: string): Observable<TaskItem[]> {
        let params = new HttpParams();
        if (status) params = params.set('status', status);
        if (assigneeId) params = params.set('assigneeId', assigneeId.toString());
        if (startDate) params = params.set('startDate', startDate);
        if (endDate) params = params.set('endDate', endDate);

        return this.http.get<TaskItem[]>(this.apiUrl, { params });
    }

    getTask(id: number): Observable<TaskItem> {
        return this.http.get<TaskItem>(`${this.apiUrl}/${id}`);
    }

    createTask(task: CreateTaskReq): Observable<number> {
        return this.http.post<number>(this.apiUrl, task);
    }

    updateTask(id: number, task: UpdateTaskReq): Observable<void> {
        return this.http.put<void>(`${this.apiUrl}/${id}`, task);
    }
}
