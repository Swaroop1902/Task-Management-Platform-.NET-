import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TaskService, TaskItem } from '../../services/task.service';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './task-detail.component.html',
  styleUrl: './task-detail.component.css'
})
export class TaskDetailComponent implements OnInit {
  task: TaskItem | null = null;
  errorMessage = '';

  constructor(
    private route: ActivatedRoute,
    private taskService: TaskService
  ) { }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.taskService.getTask(+idParam).subscribe({
        next: data => this.task = data,
        error: err => this.errorMessage = 'Could not load task details.'
      });
    }
  }
}
