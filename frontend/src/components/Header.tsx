import { useEffect, useState } from "react";
import { checkHealth } from "../api/health";
import { USE_MOCKS } from "../api/http";
import { useAuth } from "../context/AuthContext";

type Health = "checking" | "up" | "down";

export function Header() {
  const { user, logout } = useAuth();
  const [health, setHealth] = useState<Health>("checking");

  // Pings GET /api/health once on load so you can see at a glance whether the backend is reachable.
  useEffect(() => {
    checkHealth()
      .then(() => setHealth("up"))
      .catch(() => setHealth("down"));
  }, []);

  const healthLabel = { checking: "Checking backend…", up: "Backend connected", down: "Backend unreachable" }[health];

  return (
    <header className="header">
      <div className="brand">Todos</div>
      <div className="header-right">
        {USE_MOCKS && <span className="badge">mock data</span>}
        <span className={`health health-${health}`} title={healthLabel}>
          <span className="dot" /> {healthLabel}
        </span>
        <span className="muted">{user?.username}</span>
        <button onClick={logout}>Sign out</button>
      </div>
    </header>
  );
}
