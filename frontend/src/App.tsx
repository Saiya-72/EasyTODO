import { Dashboard } from "./components/Dashboard";
import { LoginPage } from "./components/LoginPage";
import { useAuth } from "./context/AuthContext";

// Two screens, so no router needed: signed out -> LoginPage, signed in -> Dashboard.
// If the app grows (settings page, shareable URLs per tab), add react-router-dom.
export default function App() {
  const { user, initializing } = useAuth();

  if (initializing) return <div className="center muted">Loading…</div>;
  return user ? <Dashboard /> : <LoginPage />;
}
