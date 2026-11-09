// Holds "who is signed in" for the whole app.
// Any component can call useAuth() to get the user and the login/logout functions.

import { createContext, useCallback, useContext, useEffect, useState, type ReactNode } from "react";
import * as authApi from "../api/auth";
import { getToken } from "../api/http";
import type { LoginRequest, User } from "../types";

interface AuthContextValue {
  user: User | null;
  initializing: boolean; // true while we check a stored token on first load
  login: (credentials: LoginRequest) => Promise<void>;
  register: (credentials: LoginRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [initializing, setInitializing] = useState(true);

  // On first load: if a token is saved, ask the backend who it belongs to.
  // If that fails (expired token, backend down, not implemented yet), fall back to the login screen.
  useEffect(() => {
    if (!getToken()) {
      setInitializing(false);
      return;
    }
    authApi
      .getCurrentUser()
      .then(setUser)
      .catch(() => authApi.logout())
      .finally(() => setInitializing(false));
  }, []);

  // TODO(you, optional): if your request() helper dispatches an "auth:expired" event on 401,
  // listen for it here so an expired token mid-session sends the user back to login:
  //
  //   useEffect(() => {
  //     const onExpired = () => setUser(null);
  //     window.addEventListener("auth:expired", onExpired);
  //     return () => window.removeEventListener("auth:expired", onExpired);
  //   }, []);

  const login = useCallback(async (credentials: LoginRequest) => {
    setUser(await authApi.login(credentials));
  }, []);

  const register = useCallback(async (credentials: LoginRequest) => {
    setUser(await authApi.register(credentials));
  }, []);

  const logout = useCallback(() => {
    authApi.logout();
    setUser(null);
  }, []);

  return (
    <AuthContext.Provider value={{ user, initializing, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside <AuthProvider>");
  return ctx;
}
