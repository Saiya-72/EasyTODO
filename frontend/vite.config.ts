import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

// The dev server proxies every request that starts with /api to the C# backend.
// This means the frontend code can call fetch("/api/todos") with a relative URL
// and you never have to deal with CORS during development.
//
//   - Running locally (npm run dev on your machine): backend is at http://localhost:5080
//   - Running in Docker Compose: docker-compose.yml sets API_PROXY_TARGET=http://backend:8080
//     ("backend" is the service name; Docker's internal DNS resolves it).
export default defineConfig(({ mode }) => {
  // loadEnv reads .env, .env.local, etc. The "" prefix means "load all vars, not just VITE_*".
  // Real environment variables (like the ones docker-compose sets) take priority.
  const env = { ...loadEnv(mode, process.cwd(), ""), ...process.env };
  const apiTarget = env.API_PROXY_TARGET ?? "http://localhost:5080";

  return {
    plugins: [react()],
    server: {
      host: true, // listen on 0.0.0.0 so the container port is reachable from your browser
      port: 5173,
      // File watching through Docker volume mounts (especially Windows/macOS) can miss changes.
      // Polling is slower but reliable, so it's only switched on inside the container.
      watch: env.CHOKIDAR_USEPOLLING === "true" ? { usePolling: true } : undefined,
      proxy: {
        "/api": {
          target: apiTarget,
          changeOrigin: true,
        },
      },
    },
  };
});
