import { useEffect, useState } from "react";
import { Routes, Route } from "react-router-dom";
import NavBar from "./components/NavBar";
import DevelopersPage from "./pages/DevelopersPage";
import DeveloperDetailPage from "./pages/DeveloperDetailPage";
import ProjectsPage from "./pages/ProjectsPage";
import ProjectDetailPage from "./pages/ProjectDetailPage";
import PathFinderPage from "./pages/PathFinderPage";
import { api } from "./api";

export default function App() {
  const [dbStatus, setDbStatus] = useState("checking");

  useEffect(() => {
    let cancelled = false;

    function check() {
      api
        .health()
        .then(() => !cancelled && setDbStatus("ok"))
        .catch(() => !cancelled && setDbStatus("down"));
    }

    check();
    const interval = setInterval(check, 30000);
    return () => {
      cancelled = true;
      clearInterval(interval);
    };
  }, []);

  return (
    <>
      <NavBar dbStatus={dbStatus} />
      {dbStatus === "down" && (
        <div className="status-banner">
          Can't reach CognoDB right now. Pages below may fail to load until the connection is restored.
        </div>
      )}
      <main>
        <Routes>
          <Route path="/" element={<DevelopersPage />} />
          <Route path="/developers/:id" element={<DeveloperDetailPage />} />
          <Route path="/projects" element={<ProjectsPage />} />
          <Route path="/projects/:id" element={<ProjectDetailPage />} />
          <Route path="/path-finder" element={<PathFinderPage />} />
        </Routes>
      </main>
      <footer>DevConnect — a CognoDB (graph database) demo application</footer>
    </>
  );
}
