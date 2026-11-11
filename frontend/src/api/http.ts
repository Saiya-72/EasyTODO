// =============================================================================
// http.ts — shared plumbing for every API call.
// =============================================================================

/** Toggle between mock data and the real backend. Set in .env.local or docker-compose.yml. */
export const USE_MOCKS: boolean = import.meta.env.VITE_USE_MOCKS !== "false";

/**
 * Base path for the backend. It's relative on purpose: the Vite dev server proxies
 * anything under /api to the C# backend (see vite.config.ts), so no CORS and no hardcoded host.
 */
export const API_BASE = import.meta.env.VITE_API_URL ?? "/api";

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

//Reusable helper
export async function request<T>(path: string, options: RequestInit = {}): Promise<T>{
  // 1. Base URL
  const url_base = `${API_BASE}${path}`;

  // 2. Headers
  const headers: Record<string, string> = {
    "Content-Type": "application/json", 
    ...(options.headers as Record<string, string> | undefined),
  };
  let token = getToken();
  if (token != null) { headers["authorization"] = `Bearer ${token}`; }

  // 3. fetch
  let response = await fetch(url_base, { ...options, headers });
  
  // 4. If response.status === 401
  if(response.status == 401) {
      setToken(null);
      throw new Error("Unauthorized (not logged in)");
  }

  // 5. If !response.ok
  if(!response.ok){
    let message = `Request failed: ${response.status}`;
    try{
      const problem = await response.json();
      message = problem.error ?? problem.detail ?? problem.title ?? message;
    } catch (e) {
      // Ignore JSON parse errors, use default message
    }
    throw new Error(message);
  }

  // 6. If response.status === 204 (No Content — typical for DELETE):
  if(response.status == 204){
    return undefined as T;
  }

  // 7. All good!
  return (await response.json()) as T

}