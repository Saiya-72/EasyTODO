import { useState, type FormEvent } from "react";
import { useAuth } from "../context/AuthContext";
import { USE_MOCKS } from "../api/http";

export function LoginPage() {
  const { login, register } = useAuth();
  const [mode, setMode] = useState<"login" | "register">("login");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const credentials = { username, password };
      if (mode === "login") await login(credentials);
      else await register(credentials);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Something went wrong.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="login-page">
      <form className="login-card" onSubmit={handleSubmit}>
        <h1>Todos</h1>
        <p className="muted">{mode === "login" ? "Sign in to your list" : "Create an account"}</p>

        <label>
          Username
          <input value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" autoFocus />
        </label>

        <label>
          Password
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete={mode === "login" ? "current-password" : "new-password"}
          />
        </label>

        {error && <div className="error">{error}</div>}

        <button type="submit" className="primary" disabled={submitting}>
          {submitting ? "Please wait…" : mode === "login" ? "Sign in" : "Sign up"}
        </button>

        <button type="button" className="link" onClick={() => setMode(mode === "login" ? "register" : "login")}>
          {mode === "login" ? "No account? Sign up" : "Already have an account? Sign in"}
        </button>

        {USE_MOCKS && <p className="hint">Mock mode: any username and password will work.</p>}
      </form>
    </div>
  );
}
