import { Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TaskListComponent } from './tasks/task-list/task-list.component';
import { TaskDetailComponent } from './tasks/task-detail/task-detail.component';
import { ReportsComponent } from './reports/reports.component';

export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: 'login', component: LoginComponent },
    { path: 'dashboard', component: DashboardComponent },
    { path: 'tasks', component: TaskListComponent },
    { path: 'tasks/:id', component: TaskDetailComponent },
    { path: 'reports', component: ReportsComponent },
    { path: '**', redirectTo: 'dashboard' }
];
