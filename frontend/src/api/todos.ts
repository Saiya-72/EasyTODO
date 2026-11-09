// =============================================================================
// todos.ts — CRUD for todo items
// =============================================================================
// Backend counterpart: backend/TodoApi/Controllers/TodosController.cs
//
// All endpoints require the Authorization header. The backend figures out
// WHICH user from the token — never send a userId from the frontend, or any
// user could read anyone's todos by changing it.
//
// REST layout:
//   GET    /api/todos?completed=false   list (filtered)
//   POST   /api/todos                   create
//   PATCH  /api/todos/{id}              partial update (title, isCompleted, categoryId)
//   DELETE /api/todos/{id}              delete
// =============================================================================

import * as mock from "../mocks/mockStore";
import type { CreateTodoRequest, Todo, UpdateTodoRequest } from "../types";
import { USE_MOCKS, notImplemented } from "./http";

/**
 * GET /api/todos?completed={true|false}
 * Returns: 200 Todo[]
 *
 * The Active tab calls this with completed=false, the Completed tab with true.
 * Filtering on the server (rather than fetching everything and filtering in
 * the browser) keeps payloads small once a user has hundreds of finished todos.
 */
export async function getTodos(completed: boolean): Promise<Todo[]> {
  if (USE_MOCKS) return mock.getTodos(completed);

  // TODO(you):
  //   return request<Todo[]>(`/todos?completed=${completed}`);
  //
  //   If you add more filters later (categoryId, search text), build the query
  //   string with URLSearchParams instead of string concatenation:
  //     const params = new URLSearchParams({ completed: String(completed) });
  //     return request<Todo[]>(`/todos?${params}`);
  throw notImplemented("getTodos");
}

/**
 * POST /api/todos
 * Body:    { title, categoryId }   (CreateTodoRequest)
 * Returns: 201 Todo  — the created item, including the server-generated id and createdAt
 *          400 if title is empty
 */
export async function createTodo(req: CreateTodoRequest): Promise<Todo> {
  if (USE_MOCKS) return mock.createTodo(req);

  // TODO(you):
  //   return request<Todo>("/todos", { method: "POST", body: JSON.stringify(req) });
  throw notImplemented("createTodo");
}

/**
 * PATCH /api/todos/{id}
 * Body:    any subset of { title, isCompleted, categoryId }   (UpdateTodoRequest)
 * Returns: 200 Todo  — the updated item
 *          404 if it doesn't exist OR belongs to another user
 *              (return 404 rather than 403 so you don't leak which ids exist)
 *
 * Used for: checking/unchecking (moves it between tabs), renaming, re-categorizing.
 */
export async function updateTodo(id: string, req: UpdateTodoRequest): Promise<Todo> {
  if (USE_MOCKS) return mock.updateTodo(id, req);

  // TODO(you):
  //   return request<Todo>(`/todos/${id}`, { method: "PATCH", body: JSON.stringify(req) });
  //
  //   Why PATCH and not PUT? PUT means "replace the whole object", so you'd have
  //   to send every field. PATCH means "change just these fields", which matches
  //   how the UI works (toggle one checkbox -> send { isCompleted: true }).
  throw notImplemented("updateTodo");
}

/**
 * DELETE /api/todos/{id}
 * Returns: 204 No Content
 *          404 if not found / not yours
 */
export async function deleteTodo(id: string): Promise<void> {
  if (USE_MOCKS) return mock.deleteTodo(id);

  // TODO(you):
  //   await request<void>(`/todos/${id}`, { method: "DELETE" });
  //   (Make sure your request() helper handles 204 without calling .json().)
  throw notImplemented("deleteTodo");
}
