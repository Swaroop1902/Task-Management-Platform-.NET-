import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TaskService, TaskItem, CreateTaskReq, UpdateTaskReq } from '../../services/task.service';
import { UserService, UserDto } from '../../services/user.service';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.css'
})
export class TaskListComponent implements OnInit {
  tasks: TaskItem[] = [];
  users: UserDto[] = [];
  filterStatus = '';
  filterAssigneeId: number | undefined;
  filterStartDate = '';
  filterEndDate = '';
  loading = false;

  // Modal State
  showModal = false;
  isEditing = false;
  currentTask: any = { title: '', description: '', priority: 'Medium', status: 'Open' };

  constructor(private taskService: TaskService, private userService: UserService) { }

  ngOnInit(): void {
    this.loadUsers();
    this.loadTasks();
  }

  loadUsers() {
    this.userService.getUsers().subscribe(data => {
      this.users = data;
    });
  }

  loadTasks() {
    this.loading = true;
    this.taskService.getTasks(
      this.filterStatus || undefined,
      this.filterAssigneeId,
      this.filterStartDate || undefined,
      this.filterEndDate || undefined
    ).subscribe({
      next: data => {
        this.tasks = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
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
