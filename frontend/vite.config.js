import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Port is pinned to 3000 because the backend's default CORS policy
// (see backend/DevConnect.Api/appsettings.json -> Cors:AllowedOrigins)
// only allows http://localhost:3000 by default.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
  },
});
