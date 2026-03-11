import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService, TasksByStatusReport } from '../services/report.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  statusCounts: TasksByStatusReport[] = [];
  errorMessage = '';

  constructor(private reportService: ReportService) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.reportService.getTasksByStatus().subscribe({
      next: data => this.statusCounts = data,
      error: err => this.errorMessage = 'Failed to load dashboard metrics.'
    });
  }
}
