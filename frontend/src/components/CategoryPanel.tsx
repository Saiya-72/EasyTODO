import { useState, type FormEvent } from "react";
import type { Category } from "../types";

interface Props {
  categories: Category[];
  onCreate: (name: string) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
}

export function CategoryPanel({ categories, onCreate, onDelete }: Props) {
  const [name, setName] = useState("");

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!name.trim()) return;
    await onCreate(name.trim());
    setName("");
  }

  function confirmDelete(category: Category) {
    // Todos in the category aren't deleted — they move to Uncategorized.
    if (window.confirm(`Delete "${category.name}"? Its todos will move to Uncategorized.`)) {
      void onDelete(category.id);
    }
  }

  return (
    <aside className="category-panel">
      <h2>Categories</h2>
      <ul>
        {categories.map((c) => (
          <li key={c.id}>
            <span>{c.name}</span>
            <button className="icon danger" onClick={() => confirmDelete(c)} aria-label={`Delete ${c.name}`}>
              ✕
            </button>
          </li>
        ))}
        {categories.length === 0 && <li className="muted">No categories yet</li>}
      </ul>
      <form onSubmit={handleSubmit} className="add-category">
        <input placeholder="New category" value={name} onChange={(e) => setName(e.target.value)} />
        <button type="submit" disabled={!name.trim()}>
          +
        </button>
      </form>
    </aside>
  );
}
