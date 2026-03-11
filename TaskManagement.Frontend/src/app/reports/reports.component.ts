import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReportService, TasksByUserReport, SLABreachReport } from '../services/report.service';

@Component({
    selector: 'app-reports',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './reports.component.html',
    styleUrl: './reports.component.css'
})
export class ReportsComponent implements OnInit {
    userReports: TasksByUserReport[] = [];
    slaReports: SLABreachReport[] = [];
    errorMessage = '';

    constructor(private reportService: ReportService) { }

    ngOnInit(): void {
        this.loadReports();
    }

    loadReports() {
        this.reportService.getTasksByUser().subscribe({
            next: data => this.userReports = data,
            error: err => this.errorMessage = 'Failed to load user reports.'
        });

        this.reportService.getSLABreaches().subscribe({
            next: data => this.slaReports = data,
            error: err => this.errorMessage = 'Failed to load SLA reports.'
        });
    }
}
