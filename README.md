# EasyTodo

EasyTodo is a simple to-do list website. Create an account, add the tasks you need to get done,
sort them into categories, and check them off as you go. Your list is saved to your account, so
it's there the next time you sign in.

I built this as a clean way to handle my to-do list since I was just keeping it in a notepad on my phone before,
while also teaching myself more about SQL, APIs, and C# in general.

<!-- TODO: add the live site link, e.g. **Live site:** https://your-url-here -->

<!-- TODO: add a screenshot, e.g. ![EasyTodo screenshot](docs/screenshot.png) -->

---

### Features

- Account sign-up and login (JWT authentication)
- Add, edit, complete, and delete tasks
- Organize tasks into custom categories
- Everything is saved to your account, so it syncs across devices

---

### Dev Notes

- **Frontend:** TypeScript, React, HTML, CSS (Vite)
- **Backend:** C# / ASP.NET Core
- **Database:** PostgreSQL (mostly raw SQL, some LINQ)

#### API Endpoints

| Method | Endpoint               | Description                |
| ------ | ---------------------- | -------------------------- |
| POST   | `/api/auth/register`   | Create an account          |
| POST   | `/api/auth/login`      | Log in and receive a token |
| GET    | `/api/auth/me`         | Get the signed-in user     |
| GET    | `/api/todos`           | List your tasks            |
| POST   | `/api/todos`           | Create a task              |
| PATCH  | `/api/todos/{id}`      | Update a task              |
| DELETE | `/api/todos/{id}`      | Delete a task              |
| GET    | `/api/categories`      | List your categories       |
| POST   | `/api/categories`      | Create a category          |
| DELETE | `/api/categories/{id}` | Delete a category          |
| GET    | `/api/health`          | Health check               |

---

### Code Usage

Feel free to use the code any way you'd like without attribution :) See [LICENSE](LICENSE).
