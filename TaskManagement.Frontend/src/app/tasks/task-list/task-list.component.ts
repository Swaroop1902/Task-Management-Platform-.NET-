import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TaskService, TaskItem, CreateTaskReq, UpdateTaskReq } from '../../services/task.service';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.css'
})
export class TaskListComponent implements OnInit {
  tasks: TaskItem[] = [];
  filterStatus = '';

  // Modal State
  showModal = false;
  isEditing = false;
  currentTask: any = { title: '', description: '', priority: 'Medium', status: 'Open' };

  constructor(private taskService: TaskService) { }

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks() {
    this.taskService.getTasks(this.filterStatus || undefined).subscribe(data => {
      this.tasks = data;
    });
  }

  applyFilter() {
    this.loadTasks();
  }

  openCreateModal() {
    this.isEditing = false;
    this.currentTask = { title: '', description: '', priority: 'Medium', status: 'Open' };
    this.showModal = true;
  }

  openEditModal(task: TaskItem) {
    this.isEditing = true;
    this.currentTask = { ...task };
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  saveTask() {
    if (this.isEditing) {
      const updateReq: UpdateTaskReq = {
        title: this.currentTask.title,
        description: this.currentTask.description,
        priority: this.currentTask.priority,
        status: this.currentTask.status,
        dueDate: this.currentTask.dueDate
      };
      this.taskService.updateTask(this.currentTask.id, updateReq).subscribe(() => {
        this.closeModal();
        this.loadTasks();
      });
    } else {
      const createReq: CreateTaskReq = {
        title: this.currentTask.title,
        description: this.currentTask.description,
        priority: this.currentTask.priority,
        status: this.currentTask.status,
        dueDate: this.currentTask.dueDate
      };
      this.taskService.createTask(createReq).subscribe(() => {
        this.closeModal();
        this.loadTasks();
      });
    }
  }
}
