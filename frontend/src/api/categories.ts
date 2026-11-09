// =============================================================================
// categories.ts — user-defined groups for todos
// =============================================================================
// Backend counterpart: backend/TodoApi/Controllers/CategoriesController.cs
//
// Categories belong to a user (each user has their own list).
//
//   GET    /api/categories          list
//   POST   /api/categories          create
//   DELETE /api/categories/{id}     delete (todos in it become uncategorized)
//
// Stretch goal: PATCH /api/categories/{id} to rename. The UI doesn't need it yet.
// =============================================================================

import * as mock from "../mocks/mockStore";
import type { Category, CreateCategoryRequest } from "../types";
import { USE_MOCKS, notImplemented } from "./http";

/**
 * GET /api/categories
 * Returns: 200 Category[]   (sorted by name is a nice touch — do it in the SQL/LINQ query)
 */
export async function getCategories(): Promise<Category[]> {
  if (USE_MOCKS) return mock.getCategories();

  // TODO(you): return request<Category[]>("/categories");
  throw notImplemented("getCategories");
}

/**
 * POST /api/categories
 * Body:    { name }
 * Returns: 201 Category
 *          400 if name is empty
 *          409 Conflict if this user already has a category with that name
 *              (enforce with a unique index on (UserId, Name) in the database)
 */
export async function createCategory(req: CreateCategoryRequest): Promise<Category> {
  if (USE_MOCKS) return mock.createCategory(req);

  // TODO(you):
  //   return request<Category>("/categories", { method: "POST", body: JSON.stringify(req) });
  throw notImplemented("createCategory");
}

/**
 * DELETE /api/categories/{id}
 * Returns: 204 No Content
 *          404 if not found / not yours
 *
 * Side effect on the server: todos in this category get categoryId = null.
 * The UI refetches todos afterwards so they show up under "Uncategorized".
 */
export async function deleteCategory(id: string): Promise<void> {
  if (USE_MOCKS) return mock.deleteCategory(id);

  // TODO(you): await request<void>(`/categories/${id}`, { method: "DELETE" });
  throw notImplemented("deleteCategory");
}
