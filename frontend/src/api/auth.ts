// =============================================================================
// auth.ts — sign in / sign up / who am I
// =============================================================================
// Backend counterpart: backend/TodoApi/Controllers/AuthController.cs
//
// Flow:
//   1. User submits the login form -> login() -> POST /api/auth/login
//   2. Backend verifies the password and returns { token, user }
//   3. We store the token (setToken) so every later request can send it
//   4. On page refresh, AuthContext calls getCurrentUser() -> GET /api/auth/me
//      to check the stored token is still valid
// =============================================================================

import * as mock from "../mocks/mockStore";
import type { AuthResponse, LoginRequest, User } from "../types";
import { USE_MOCKS, request, setToken } from "./http";

/**
 * POST /api/auth/login
 * Body:     { username, password }            (LoginRequest)
 * Returns:  200 { token, user }               (AuthResponse)
 *           401 if the credentials are wrong
 */
export async function login(credentials: LoginRequest): Promise<User> {
  if (USE_MOCKS) {
    const res = await mock.login(credentials);
    setToken(res.token);
    return res.user;
  }

  try{
    
    const res = await request<AuthResponse>("/auth/login", {
      method: "POST", 
      body: JSON.stringify(credentials),
    });
    setToken(res.token);
    return res.user;

  } catch {
    throw new Error("Invalid username or password");
  }
}

/**
 * POST /api/auth/register
 * Body:     { username, password }
 * Returns:  201 { token, user }   — log the user straight in after signing up
 *           409 if the username is taken
 *           400 if validation fails (e.g. password too short)
 */
export async function register(credentials: LoginRequest): Promise<User> {
  if (USE_MOCKS) {
    const res = await mock.register(credentials);
    setToken(res.token);
    return res.user;
  }

  try{
    
    const res = await request<AuthResponse>("/auth/register", {
      method: "POST", 
      body: JSON.stringify(credentials),
    });
    setToken(res.token);
    return res.user;

  } catch {
    throw new Error("Invalid username or password");
  }
}

/**
 * GET /api/auth/me      (requires Authorization header)
 * Returns:  200 { id, username }   (User)
 *           401 if the token is missing/invalid/expired
 *
 * Called once on app load to restore the session from a stored token.
 */
export async function getCurrentUser(): Promise<User> {
  if (USE_MOCKS) return mock.getCurrentUser();

  return request<User>("/auth/me", {method: "GET"});
}

/**
 * Logging out with JWTs is purely client-side: forget the token.
 * (If you ever switch to server sessions/refresh tokens, call
 *  POST /api/auth/logout here first so the server can revoke it.)
 */
export function logout(): void {
  setToken(null);
}

// Re-exported so components can import the type from one place if they like.
export type { AuthResponse };
