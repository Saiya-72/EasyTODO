// Shared shapes between frontend and backend.
// Keep these in sync with the C# DTOs in backend/TodoApi/Dtos/.
// ASP.NET Core serializes PascalCase C# properties to camelCase JSON by default,
// so `public string Title` in C# becomes `title` here.

export interface User {
  id: string;
  username: string;
}

export interface AuthResponse {
  token: string; // JWT (or whatever token you decide on) — sent back as "Authorization: Bearer <token>"
  user: User;
}

export interface Category {
  id: string;
  name: string;
}

export interface Todo {
  id: string;
  title: string;
  isCompleted: boolean;
  categoryId: string | null; // null = "Uncategorized"
  createdAt: string; // ISO 8601 string. JSON has no Date type, so parse with new Date(...) when needed.
  completedAt: string | null;
}

// Request bodies -----------------------------------------------------------

export interface LoginRequest {
  username: string;
  password: string;
}

export interface CreateTodoRequest {
  title: string;
  categoryId: string | null;
}

// Partial update: only send the fields that changed.
export interface UpdateTodoRequest {
  title?: string;
  isCompleted?: boolean;
  categoryId?: string | null;
}

export interface CreateCategoryRequest {
  name: string;
}
