import { useState, type KeyboardEvent } from "react";
import type { Category, Todo, UpdateTodoRequest } from "../types";

interface Props {
  todo: Todo;
  categories: Category[];
  onUpdate: (id: string, req: UpdateTodoRequest) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
}

export function TodoItem({ todo, categories, onUpdate, onDelete }: Props) {
  const [editing, setEditing] = useState(false);
  const [draft, setDraft] = useState(todo.title);

  async function saveTitle() {
    setEditing(false);
    const title = draft.trim();
    if (title && title !== todo.title) await onUpdate(todo.id, { title });
    else setDraft(todo.title);
  }

  function onKeyDown(e: KeyboardEvent<HTMLInputElement>) {
    if (e.key === "Enter") void saveTitle();
    if (e.key === "Escape") {
      setDraft(todo.title);
      setEditing(false);
    }
  }

  return (
    <li className={todo.isCompleted ? "todo done" : "todo"}>
      <input
        type="checkbox"
        checked={todo.isCompleted}
        onChange={(e) => onUpdate(todo.id, { isCompleted: e.target.checked })}
        aria-label={todo.isCompleted ? "Mark as active" : "Mark as completed"}
      />

      {editing ? (
        <input
          className="edit-title"
          value={draft}
          autoFocus
          onChange={(e) => setDraft(e.target.value)}
          onBlur={saveTitle}
          onKeyDown={onKeyDown}
        />
      ) : (
        <span className="title" onDoubleClick={() => !todo.isCompleted && setEditing(true)} title="Double-click to edit">
          {todo.title}
        </span>
      )}

      {todo.completedAt && <span className="muted small">{new Date(todo.completedAt).toLocaleDateString()}</span>}

      <select
        className="move"
        value={todo.categoryId ?? ""}
        onChange={(e) => onUpdate(todo.id, { categoryId: e.target.value || null })}
        aria-label="Category"
      >
        <option value="">Uncategorized</option>
        {categories.map((c) => (
          <option key={c.id} value={c.id}>
            {c.name}
          </option>
        ))}
      </select>

      <button className="icon danger" onClick={() => onDelete(todo.id)} aria-label="Delete">
        ✕
      </button>
    </li>
  );
}
