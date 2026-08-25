import { NavLink } from "react-router-dom";

const links = [
  { to: "/", label: "Developers", end: true },
  { to: "/projects", label: "Projects" },
  { to: "/path-finder", label: "Path Finder" },
];

export default function NavBar({ dbStatus }) {
  return (
    <nav className="nav">
      <div className="nav-inner">
        <NavLink to="/" className="brand">
          {/* signature mark: a tiny literal graph — two nodes, one edge */}
          <svg width="26" height="18" viewBox="0 0 26 18" aria-hidden="true">
            <line x1="5" y1="13" x2="21" y2="5" stroke="var(--signal)" strokeWidth="1.6" />
            <circle cx="5" cy="13" r="4" fill="var(--graph-teal)" />
            <circle cx="21" cy="5" r="4" fill="var(--signal)" />
          </svg>
          DevConnect
        </NavLink>
        <div className="nav-links">
          {links.map((l) => (
            <NavLink
              key={l.to}
              to={l.to}
              end={l.end}
              className={({ isActive }) => "nav-link" + (isActive ? " active" : "")}
            >
              {l.label}
            </NavLink>
          ))}
          <span
            className="nav-link"
            title={dbStatus === "ok" ? "CognoDB connected" : dbStatus === "down" ? "CognoDB unreachable" : "Checking…"}
            style={{ display: "flex", alignItems: "center", gap: 6, cursor: "default" }}
          >
            <span className={"status-dot " + (dbStatus === "ok" ? "ok" : dbStatus === "down" ? "down" : "")} />
            {dbStatus === "ok" ? "Connected" : dbStatus === "down" ? "DB unreachable" : "Checking…"}
          </span>
        </div>
      </div>
    </nav>
  );
}
