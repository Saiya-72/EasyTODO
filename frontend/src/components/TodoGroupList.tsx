import { useMemo } from "react";
import type { Category, Todo, UpdateTodoRequest } from "../types";
import { TodoItem } from "./TodoItem";

interface Props {
  todos: Todo[];
  categories: Category[];
  emptyMessage: string;
  onUpdate: (id: string, req: UpdateTodoRequest) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
}

interface Group {
  key: string;
  name: string;
  todos: Todo[];
}

/** Renders todos bucketed under their category headings, with "Uncategorized" last. */
export function TodoGroupList({ todos, categories, emptyMessage, onUpdate, onDelete }: Props) {
  const groups = useMemo<Group[]>(() => {
    const byCategory = new Map<string, Todo[]>();
    for (const todo of todos) {
      const key = todo.categoryId ?? "__none__";
      byCategory.set(key, [...(byCategory.get(key) ?? []), todo]);
    }

    const result: Group[] = categories
      .filter((c) => byCategory.has(c.id))
      .map((c) => ({ key: c.id, name: c.name, todos: byCategory.get(c.id)! }));

    // Todos pointing at a category id we don't know about (e.g. just deleted) fall into Uncategorized.
    const known = new Set(categories.map((c) => c.id));
    const orphans = todos.filter((t) => t.categoryId === null || !known.has(t.categoryId));
    if (orphans.length) result.push({ key: "__none__", name: "Uncategorized", todos: orphans });

    return result;
  }, [todos, categories]);

  if (todos.length === 0) return <p className="empty">{emptyMessage}</p>;

  return (
    <div className="groups">
      {groups.map((group) => (
        <section key={group.key} className="group">
          <h3>
            {group.name} <span className="count">{group.todos.length}</span>
          </h3>
          <ul>
            {group.todos.map((todo) => (
              <TodoItem key={todo.id} todo={todo} categories={categories} onUpdate={onUpdate} onDelete={onDelete} />
            ))}
          </ul>
        </section>
      ))}
    </div>
  );
}
