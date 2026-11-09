// =============================================================================
// http.ts — shared plumbing for every API call.
// =============================================================================
//
// Every file in src/api/ follows the same pattern:
//
//   export async function getThing(): Promise<Thing> {
//     if (USE_MOCKS) return mock.getThing();   // <- works today, no backend needed
//
//     // TODO(you): real fetch call goes here
//     throw notImplemented("getThing");
//   }
//
// Workflow:
//   1. Build the UI against mocks (VITE_USE_MOCKS=true, the default).
//   2. Implement the matching C# endpoint in the backend.
//   3. Replace the `throw notImplemented(...)` with your fetch code.
//   4. Set VITE_USE_MOCKS=false and test against the real backend.
//
// health.ts contains one fully written call you can copy from.
// =============================================================================

/** Toggle between mock data and the real backend. Set in .env.local or docker-compose.yml. */
export const USE_MOCKS: boolean = import.meta.env.VITE_USE_MOCKS !== "false";

/**
 * Base path for the backend. It's relative on purpose: the Vite dev server proxies
 * anything under /api to the C# backend (see vite.config.ts), so no CORS and no hardcoded host.
 */
export const API_BASE = "/api";

/** Thrown by API functions you haven't written yet, so the UI shows a clear message. */
export function notImplemented(fnName: string): Error {
  return new Error(
    `${fnName}() is not implemented yet. Write the fetch call in src/api/, ` +
      `or set VITE_USE_MOCKS=true to use mock data.`,
  );
}

// -----------------------------------------------------------------------------
// Auth token storage
// -----------------------------------------------------------------------------
// After login, the backend returns a token. Store it so every later request can
// send it in the Authorization header, and so a page refresh keeps you signed in.

const TOKEN_KEY = "todo_app_token";

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string | null): void {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

// -----------------------------------------------------------------------------
// TODO(you): write a reusable request helper.
// -----------------------------------------------------------------------------
// Every endpoint needs the same things: the base URL, JSON headers, the auth
// header, error handling, and JSON parsing. Rather than repeating that in
// every function, write it once here and call it from auth.ts / todos.ts /
// categories.ts.
//
// Suggested signature:
//
//   export async function request<T>(path: string, options: RequestInit = {}): Promise<T>
//
// What it should do, step by step:
//
//   1. Build the URL:              `${API_BASE}${path}`   e.g. "/api" + "/todos"
//
//   2. Build headers:
//        - "Content-Type": "application/json"  (only matters when there's a body,
//          but harmless otherwise)
//        - If getToken() returns something: "Authorization": `Bearer ${token}`
//        - Merge in any headers the caller passed in options.headers
//
//   3. Call fetch(url, { ...options, headers }).
//      Remember fetch() does NOT throw on 4xx/5xx — it only throws on network
//      failure. You have to check `response.ok` yourself.
//
//   4. If response.status === 401:
//        - The token is missing/expired. Call setToken(null) and throw an Error.
//        - Bonus: the AuthContext could listen for this and kick the user back
//          to the login screen (e.g. dispatch a window CustomEvent("auth:expired")).
//
//   5. If !response.ok:
//        - Try to read an error message from the body. ASP.NET Core returns
//          ProblemDetails JSON for errors: { title, status, detail, errors? }.
//          Wrap `await response.json()` in try/catch because some errors have no body.
//        - throw new Error(detail ?? title ?? `Request failed: ${response.status}`)
//
//   6. If response.status === 204 (No Content — typical for DELETE):
//        - return undefined as T   (calling .json() on an empty body throws)
//
//   7. Otherwise: return (await response.json()) as T
//
// Example usage once written:
//
//   const todos = await request<Todo[]>("/todos?completed=false");
//   const created = await request<Todo>("/todos", {
//     method: "POST",
//     body: JSON.stringify({ title: "Buy milk", categoryId: null }),
//   });
//   await request<void>(`/todos/${id}`, { method: "DELETE" });
// -----------------------------------------------------------------------------
