import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../api";
import { Loading, EmptyState, ErrorState } from "../components/States";
import SkillBadge from "../components/SkillBadge";

export default function ProjectDetailPage() {
  const { id } = useParams();
  const [project, setProject] = useState({ status: "loading", data: null, error: null });
  const [skills, setSkills] = useState({ status: "loading", data: null, error: null });
  const [team, setTeam] = useState({ status: "loading", data: null, error: null });
  const [recs, setRecs] = useState({ status: "loading", data: null, error: null });

  useEffect(() => {
    setProject({ status: "loading", data: null, error: null });
    setSkills({ status: "loading", data: null, error: null });
    setTeam({ status: "loading", data: null, error: null });
    setRecs({ status: "loading", data: null, error: null });

    api
      .project(id)
      .then((data) => setProject({ status: "ok", data, error: null }))
      .catch((error) => setProject({ status: "error", data: null, error }));

    api
      .projectSkills(id)
      .then((data) => setSkills({ status: "ok", data, error: null }))
      .catch((error) => setSkills({ status: "error", data: null, error }));

    api
      .projectTeam(id)
      .then((data) => setTeam({ status: "ok", data, error: null }))
      .catch((error) => setTeam({ status: "error", data: null, error }));

    api
      .recommendations(id)
      .then((data) => setRecs({ status: "ok", data: data.filter((r) => !r.alreadyOnProject), error: null }))
      .catch((error) => setRecs({ status: "error", data: null, error }));
  }, [id]);

  if (project.status === "loading") {
    return (
      <div className="container">
        <Loading label="Loading project" />
      </div>
    );
  }

  if (project.status === "error") {
    return (
      <div className="container">
        <ErrorState error={project.error} />
      </div>
    );
  }

  const p = project.data;

  return (
    <div className="container">
      <Link to="/projects" className="back-link">
        ← All projects
      </Link>

      <div className="detail-header">
        <div>
          <span className="eyebrow">Project · {p.status}</span>
          <h1>{p.name}</h1>
          <p style={{ color: "var(--ink-soft)", marginTop: 6, maxWidth: "60ch" }}>{p.description}</p>
        </div>
      </div>

      <div className="section">
        <h2>Required skills</h2>
        <p className="section-sub">REQUIRES relationships — flagged by whether the current team already covers them.</p>
        {skills.status === "loading" && <Loading label="Checking coverage" />}
        {skills.status === "error" && <ErrorState error={skills.error} />}
        {skills.status === "ok" && skills.data.length === 0 && <EmptyState title="No required skills set" />}
        {skills.status === "ok" && skills.data.length > 0 && (
          <div className="badge-row">
            {skills.data.map((s) => (
              <SkillBadge key={s.skillId} name={s.name} variant={s.isCovered ? "covered" : "uncovered"} />
            ))}
          </div>
        )}
      </div>

      <div className="section">
        <h2>Current team</h2>
        <p className="section-sub">Developers already WORKED_ON this project.</p>
        {team.status === "loading" && <Loading label="Loading team" />}
        {team.status === "error" && <ErrorState error={team.error} />}
        {team.status === "ok" && team.data.length === 0 && <EmptyState title="No one staffed yet" />}
        {team.status === "ok" && team.data.length > 0 && (
          <div className="grid">
            {team.data.map((t) => (
              <Link key={t.developerId} to={`/developers/${t.developerId}`} className="card">
                <h3>{t.name}</h3>
                <div className="meta">{t.role}</div>
              </Link>
            ))}
          </div>
        )}
      </div>

      <div className="section">
        <h2>Recommended developers</h2>
        <p className="section-sub">
          Not yet on the team, ranked by how many required skills they already match — the query a relational join
          finds awkward.
        </p>
        {recs.status === "loading" && <Loading label="Ranking candidates" />}
        {recs.status === "error" && <ErrorState error={recs.error} />}
        {recs.status === "ok" && recs.data.length === 0 && (
          <EmptyState title="No candidates found" detail="Everyone with a matching skill is already on this project." />
        )}
        {recs.status === "ok" && recs.data.length > 0 && (
          <div className="grid">
            {recs.data.map((r) => (
              <Link key={r.developerId} to={`/developers/${r.developerId}`} className="card">
                <h3>{r.developerName}</h3>
                <div className="meta">
                  {r.matchingSkillsCount} matching skills · {r.experienceYears} yrs exp.
                </div>
                <div className="badge-row">
                  {r.matchingSkills.map((s) => (
                    <SkillBadge key={s} name={s} variant="covered" />
                  ))}
                  {r.missingSkills.slice(0, 2).map((s) => (
                    <SkillBadge key={s} name={s} variant="gap" />
                  ))}
                </div>
              </Link>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
