import { useState, type FormEvent } from "react";
import type { Category, CreateTodoRequest } from "../types";

interface Props {
  categories: Category[];
  onAdd: (req: CreateTodoRequest) => Promise<void>;
}

export function AddTodoForm({ categories, onAdd }: Props) {
  const [title, setTitle] = useState("");
  const [categoryId, setCategoryId] = useState<string>(""); // "" = uncategorized
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!title.trim()) return;
    setSubmitting(true);
    try {
      await onAdd({ title: title.trim(), categoryId: categoryId || null });
      setTitle(""); // keep the selected category so adding several to one group is quick
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className="add-todo" onSubmit={handleSubmit}>
      <input placeholder="What needs doing?" value={title} onChange={(e) => setTitle(e.target.value)} />
      <select value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
        <option value="">Uncategorized</option>
        {categories.map((c) => (
          <option key={c.id} value={c.id}>
            {c.name}
          </option>
        ))}
      </select>
      <button type="submit" className="primary" disabled={submitting || !title.trim()}>
        Add
      </button>
    </form>
  );
}
