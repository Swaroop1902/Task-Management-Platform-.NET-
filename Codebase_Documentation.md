# Codebase Documentation: Task Management Platform

This document is to help you understand how the code is structured, the major components, and how everything communicates. It's designed to help you explain the project if you need to!

---

## 🏗 High-Level Architecture
This application follows a **Microservices Architecture**. Instead of having one massive application doing everything (a monolith), we split the responsibilities into three small independent `.NET 8 Web APIs`, and one separate frontend application built in `Angular 18`.

**The 4 main pieces:**
1. **User Service:** Handles who people are and their login tokens.
2. **Task Service:** Handles creating, editing, and listing the actual tasks.
3. **Reporting Service:** A dashboard service that "talks to" the other two services to calculate statistics.
4. **Angular Frontend:** The website that the user sees in their browser.

---

## 🛠 1. User Service (`TaskManagement.UserService`)
**Port:** `5147`
**Responsibility:** Authentication and user data.

- **Models (`/Models/User.cs`):** Defines what a user looks like in the database (`Id`, `Username`, `PasswordHash`, `Role`).
- **Database (`/Data/UserDbContext.cs`):** Uses Entity Framework Core with an `InMemory` database. It automatically seeds three users (`admin`, `manager`, `engineer`) when it starts.
- **API Endpoints (`Program.cs`):** 

| Method | Endpoint | Description | Auth Required? | Payload | Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **POST** | `/auth/login` | Authenticates a user. | No | `{ "username": "admin", "password": "admin" }` | `{ "token": "ey...", "username": "admin", "role": "Admin" }` |
| **GET** | `/api/users` | Lists all users. | Yes (Bearer Token) | None | `[ { "id": 1, "username": "admin", "role": "Admin" } ]` |
| **POST** | `/api/users` | Creates a new user. | Yes (Admin Role Only) | `{ "username": "new", "password": "pwd", "role": "Engineer" }` | `201 Created` |
| **PUT/DELETE** | `/api/users/{id}` | Update/Delete user. | Yes (Admin Role Only) | *Depends* | `204 NoContent` |

*How it works:* When you execute `/auth/login`, the service verifies the plaintext password against the `PasswordHash` stored in the InMemory DB (Note: in production, this needs proper hashing!). Finally, it creates a JWT token signed with an `HmacSha256Signature` secret key, containing the user ID and Role as "Claims".

---

## 🛠 2. Task Service (`TaskManagement.TaskService`)
**Port:** `5045`
**Responsibility:** Managing actual work items.

- **Models (`/Models`):** 
  - `TaskItem.cs` - Defines a task (`Id`, `Title`, `Description`, `Status`, `AssigneeId`, `DueDate`).
  - `ActivityLog.cs` - A history record of what happened to a task (e.g., "Status changed from Open to In Progress").
- **Database:** Also uses an `InMemory` database and seeds dummy tasks.
- **API Endpoints (`Program.cs`):**

| Method | Endpoint | Description | Payload | 
| :--- | :--- | :--- | :--- | 
| **GET** | `/api/tasks` | Lists all tasks. Can filter using query params like `?status=Open` or `?assigneeId=1`. | None | 
| **POST** | `/api/tasks` | Creates a new task. | `{ "title": "...", "description": "...", "assigneeId": 1, "dueDate": "2024-01-01T00:00:00Z" }` |
| **PUT** | `/api/tasks/{id}` | Updates existing task title, description, assignee, status, or due date. | `{ "title": "...", "status": "In Progress" }` |
| **DELETE** | `/api/tasks/{id}` | Deletes a task. Only accessible by Admins or Managers. | None |

*How it works:* Every API call to this service is protected by `[Authorize]` attributes. The `app.UseJwtBearer(...)` middleware catches the incoming HTTP `Authorization` header, decrypts the token using the shared secret string, and validates that it hasn't expired. 
*Activity Logging:* In the `PUT` endpoint, the code manually checks `if (existingTask.Status != request.Status)`. If they differ, it inserts a new `ActivityLog` object describing the change before calling `db.SaveChangesAsync()`.

---

## 🛠 3. Reporting Service (`TaskManagement.ReportingService`)
**Port:** `5062`
**Responsibility:** Generating aggregate stats for the Dashboard/Reports page.

**This is the most complex backend service.** It acts as an Aggregator or "BFF" (Backend for Frontend).

| Method | Endpoint | Description | 
| :--- | :--- | :--- | 
| **GET** | `/api/reports/tasks-by-user` | Returns a list of users, each with a count of tasks currently assigned to them. |
| **GET** | `/api/reports/tasks-by-status` | Groups all tasks in the system by status (Open, Blocked, etc.) and returns the counts. | 
| **GET** | `/api/reports/sla-breaches` | Returns a list of tasks that have missed their `DueDate` but are not yet `Completed`, grouped by assigning user. |

*How it works under the hood:*
1. **Synchronous Communication:** It uses C#'s `HttpClientFactory` to make real REST API calls *to* the User Service (`/api/users`) and Task Service (`/api/tasks`) to gather the raw lists.
2. **Token Passing:** To be authorized by the other services, it intercepts the JWT token that the Angular app sent it, applies it to the `HttpClient.DefaultRequestHeaders.Authorization`, and forwards it to the User/Task services.
3. **Caching (`IMemoryCache`):** Because making network hits to other microservices is slow, the Reporting Service wraps its API returns in a `cache.GetOrCreateAsync("Key", ...)`. If you refresh the dashboard rapidly, it responds instantly from memory for 60 seconds without hitting the other microservices!
4. **SLA Breach Logic:** It calculates overdue tasks dynamically using LINQ: `tasks.Where(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed")` and calculates the days overdue mathematically.

---

## 💻 4. Angular Frontend (`TaskManagement.Frontend`)
**Port:** `4200`
**Responsibility:** The User Interface.

- **`src/environments/environment.ts`:** This is where the frontend knows how to find the backend microservices. We mapped the ports here.
- **Services (`/src/app/services`):**
  - `auth.service.ts`: Handles hitting the `/auth/login` endpoint and saving the JWT Token inside the browser's `localStorage`.
  - `task.service.ts` & `report.service.ts`: Makes HTTP calls to the backends to get data.
  - **`auth.interceptor.ts`:** A very important file! It intercepts *every* HTTP request Angular makes and automatically injects the `Authorization: Bearer <token>` header, so we don't have to write that code manually everywhere.
- **Components (`/src/app/...`):**
  - The UI is built using **Bootstrap 5** classes for layout and styling.
  - Examples: `dashboard.component.ts` (shows the cards), `task-list.component.ts` (shows the data grid).
- **Routing (`app.routes.ts`):** We use an `AuthGuard` setup in the component router logic so you cannot view the dashboard unless `auth.service.ts` says you have a token.

---

## 🐳 Docker (`Dockerfile` & `docker-compose.yml`)
While the app can run via `run.bat` using standard local dotnet processes, we fully containerized it so that it can run on any server without installing .NET.

- **`Dockerfile` for Backend:** Uses a "Multi-stage build". It uses the heavy `.NET SDK` image to build and compile the code, then copies the results into a tiny, fast `.NET ASPNET Runtime` image to run it.
- **`Dockerfile` for Frontend:** Uses `Node.js` to run `npm run build` to compile the Angular TypeScript into plain HTML/JS/CSS. Then it copies those static files into an **Nginx** web server container.
- **`docker-compose.yml`:** A script that tells Docker to boot all 4 containers at the exact same time and put them on the same internal "bridge network" so they can communicate together seamlessly.
