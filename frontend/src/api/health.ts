// =============================================================================
// health.ts — the ONE finished API call, as a reference example.
// =============================================================================
// GET /api/health is implemented in the backend (Program.cs) and needs no auth.
// The header shows a green/red dot based on this, so you can confirm the
// frontend -> Vite proxy -> C# backend chain works before writing anything else.
//
// Notice this calls fetch() directly. Once you've written request<T>() in
// http.ts, the other endpoints should use that instead.
// =============================================================================

import { API_BASE } from "./http";

export interface HealthResponse {
  status: string;
  time: string;
}

export async function checkHealth(): Promise<HealthResponse> {
  const response = await fetch(`${API_BASE}/health`);

  // fetch only rejects on network errors; HTTP errors must be checked manually.
  if (!response.ok) {
    throw new Error(`Health check failed: ${response.status}`);
  }

  return (await response.json()) as HealthResponse;
}
