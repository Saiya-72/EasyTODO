// =============================================================================
// mockStore.ts — in-memory fake backend used when VITE_USE_MOCKS=true.
// =============================================================================
// It behaves the way the real API should (same inputs, same outputs, same
// errors), so the UI can be built and tested before the C# side exists.
// Treat it as a spec: when you write the C# controllers, they should behave
// like these functions.
//
// Data resets on page reload. That's intentional — it's just for UI testing.
// =============================================================================

import type {
  AuthResponse,
  Category,
  CreateCategoryRequest,
  CreateTodoRequest,
  LoginRequest,
  Todo,
  UpdateTodoRequest,
} from "../types";

const delay = (ms = 200) => new Promise((r) => setTimeout(r, ms)); // simulate network latency
const newId = () => crypto.randomUUID();
const now = () => new Date().toISOString();

let categories: Category[] = [
  { id: "cat-work", name: "Work" },
  { id: "cat-school", name: "School" },
  { id: "cat-gym", name: "Gym" },
];

let todos: Todo[] = [
  { id: newId(), title: "Finish API design doc", isCompleted: false, categoryId: "cat-work", createdAt: now(), completedAt: null },
  { id: newId(), title: "Review pull requests", isCompleted: false, categoryId: "cat-work", createdAt: now(), completedAt: null },
  { id: newId(), title: "Study for algorithms exam", isCompleted: false, categoryId: "cat-school", createdAt: now(), completedAt: null },
  { id: newId(), title: "Leg day", isCompleted: false, categoryId: "cat-gym", createdAt: now(), completedAt: null },
  { id: newId(), title: "Call the bank", isCompleted: false, categoryId: null, createdAt: now(), completedAt: null },
  { id: newId(), title: "Submit lab report", isCompleted: true, categoryId: "cat-school", createdAt: now(), completedAt: now() },
];

// ---- Auth --------------------------------------------------------------------

export async function login(req: LoginRequest): Promise<AuthResponse> {
  await delay();
  // Any non-empty username/password works in mock mode.
  if (!req.username.trim() || !req.password) throw new Error("Username and password are required.");
  return { token: "mock-token", user: { id: "user-1", username: req.username.trim() } };
}

export async function register(req: LoginRequest): Promise<AuthResponse> {
  return login(req);
}

export async function getCurrentUser(): Promise<AuthResponse["user"]> {
  await delay(50);
  return { id: "user-1", username: "demo" };
}

// ---- Todos -------------------------------------------------------------------

export async function getTodos(completed: boolean): Promise<Todo[]> {
  await delay();
  return todos.filter((t) => t.isCompleted === completed).map((t) => ({ ...t }));
}

export async function createTodo(req: CreateTodoRequest): Promise<Todo> {
  await delay();
  if (!req.title.trim()) throw new Error("Title is required.");
  const todo: Todo = {
    id: newId(),
    title: req.title.trim(),
    isCompleted: false,
    categoryId: req.categoryId,
    createdAt: now(),
    completedAt: null,
  };
  todos = [...todos, todo];
  return { ...todo };
}

export async function updateTodo(id: string, req: UpdateTodoRequest): Promise<Todo> {
  await delay();
  const existing = todos.find((t) => t.id === id);
  if (!existing) throw new Error("Todo not found.");
  const updated: Todo = { ...existing, ...req };
  if (req.isCompleted !== undefined) {
    updated.completedAt = req.isCompleted ? now() : null;
  }
  todos = todos.map((t) => (t.id === id ? updated : t));
  return { ...updated };
}

export async function deleteTodo(id: string): Promise<void> {
  await delay();
  todos = todos.filter((t) => t.id !== id);
}

// ---- Categories --------------------------------------------------------------

export async function getCategories(): Promise<Category[]> {
  await delay();
  return categories.map((c) => ({ ...c }));
}

export async function createCategory(req: CreateCategoryRequest): Promise<Category> {
  await delay();
  const name = req.name.trim();
  if (!name) throw new Error("Category name is required.");
  if (categories.some((c) => c.name.toLowerCase() === name.toLowerCase())) {
    throw new Error("A category with that name already exists."); // real API: 409 Conflict
  }
  const category = { id: newId(), name };
  categories = [...categories, category];
  return { ...category };
}

export async function deleteCategory(id: string): Promise<void> {
  await delay();
  categories = categories.filter((c) => c.id !== id);
  // Decision baked in here: deleting a category moves its todos to "Uncategorized"
  // rather than deleting them. Mirror this in the backend (SET NULL on the FK).
  todos = todos.map((t) => (t.categoryId === id ? { ...t, categoryId: null } : t));
}
