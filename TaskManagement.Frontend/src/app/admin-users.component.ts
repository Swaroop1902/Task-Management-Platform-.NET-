import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserService, UserDto } from './services/user.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="mt-4">
        <h2>Admin - Manage Users</h2>
        <hr>
        <div class="table-responsive">
            <table class="table table-striped">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Username</th>
                        <th>Role</th>
                    </tr>
                </thead>
                <tbody>
                    <tr *ngFor="let user of users">
                        <td>{{ user.id }}</td>
                        <td>{{ user.username }}</td>
                        <td>{{ user.role }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
  `
})
export class AdminUsersComponent implements OnInit {
  users: UserDto[] = [];

  constructor(private userService: UserService) { }

  ngOnInit(): void {
    this.userService.getUsers().subscribe((data: UserDto[]) => {
      this.users = data;
    });
  }
}
