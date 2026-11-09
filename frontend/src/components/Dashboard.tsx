// The main signed-in screen. Owns the data (todos + categories) and passes
// callbacks down to the presentational components.
//
// You shouldn't need to change this file to hook up the backend: it only talks
// to src/api/*. Once those functions make real fetch calls, this just works.

import { useCallback, useEffect, useState } from "react";
import * as categoriesApi from "../api/categories";
import * as todosApi from "../api/todos";
import type { Category, CreateTodoRequest, Todo, UpdateTodoRequest } from "../types";
import { AddTodoForm } from "./AddTodoForm";
import { CategoryPanel } from "./CategoryPanel";
import { Header } from "./Header";
import { Tabs, type TabKey } from "./Tabs";
import { TodoGroupList } from "./TodoGroupList";

export function Dashboard() {
  const [tab, setTab] = useState<TabKey>("active");
  const [todos, setTodos] = useState<Todo[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const showError = (err: unknown) => setError(err instanceof Error ? err.message : String(err));

  // Runs an API call and surfaces any thrown error in the banner instead of crashing.
  const run = useCallback(async (fn: () => Promise<void>) => {
    setError(null);
    try {
      await fn();
    } catch (err) {
      showError(err);
    }
  }, []);

  const loadTodos = useCallback(async () => {
    setLoading(true);
    await run(async () => setTodos(await todosApi.getTodos(tab === "completed")));
    setLoading(false);
  }, [tab, run]);

  // Categories load once; todos reload whenever the tab changes.
  useEffect(() => {
    void run(async () => setCategories(await categoriesApi.getCategories()));
  }, [run]);

  useEffect(() => {
    void loadTodos();
  }, [loadTodos]);

  // ---- Todo handlers ---------------------------------------------------------

  const addTodo = (req: CreateTodoRequest) =>
    run(async () => {
      const created = await todosApi.createTodo(req);
      setTodos((prev) => [...prev, created]);
    });

  const updateTodo = (id: string, req: UpdateTodoRequest) =>
    run(async () => {
      const updated = await todosApi.updateTodo(id, req);
      const stillBelongsOnThisTab = updated.isCompleted === (tab === "completed");
      setTodos((prev) =>
        stillBelongsOnThisTab ? prev.map((t) => (t.id === id ? updated : t)) : prev.filter((t) => t.id !== id),
      );
    });

  const deleteTodo = (id: string) =>
    run(async () => {
      await todosApi.deleteTodo(id);
      setTodos((prev) => prev.filter((t) => t.id !== id));
    });

  // ---- Category handlers -----------------------------------------------------

  const createCategory = (name: string) =>
    run(async () => {
      const created = await categoriesApi.createCategory({ name });
      setCategories((prev) => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)));
    });

  const deleteCategory = (id: string) =>
    run(async () => {
      await categoriesApi.deleteCategory(id);
      setCategories((prev) => prev.filter((c) => c.id !== id));
      await loadTodos(); // server moved its todos to Uncategorized; refetch to reflect that
    });

  return (
    <div className="app">
      <Header />
      <div className="layout">
        <CategoryPanel categories={categories} onCreate={createCategory} onDelete={deleteCategory} />

        <main className="main">
          <Tabs current={tab} onChange={setTab} />

          {error && (
            <div className="error banner">
              {error}
              <button className="icon" onClick={() => setError(null)} aria-label="Dismiss">
                ✕
              </button>
            </div>
          )}

          {tab === "active" && <AddTodoForm categories={categories} onAdd={addTodo} />}

          {loading ? (
            <p className="muted">Loading…</p>
          ) : (
            <TodoGroupList
              todos={todos}
              categories={categories}
              emptyMessage={tab === "active" ? "Nothing to do. Nice." : "No completed todos yet."}
              onUpdate={updateTodo}
              onDelete={deleteTodo}
            />
          )}
        </main>
      </div>
    </div>
  );
}
