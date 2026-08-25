# DevConnect — Developer, Skill & Project Graph

A small application backed by **CognoDB** (a managed graph database speaking openCypher over Bolt)
for exploring how developers, skills, and projects connect to each other — and for answering
questions like *"who should I put on this project?"* and *"how is developer A connected to developer F?"*

---

## Why a graph database?

DevConnect's core questions are all about **relationships**, not rows:

- *"Which developers already have most of the skills a new project needs, and aren't already on it?"*
  In a relational schema this means joining `developers`, `developer_skills`, `project_required_skills`,
  and `project_team` tables, then running a `GROUP BY` + `HAVING` + a `NOT IN` subquery to exclude
  people already on the team. It works, but it's a query nobody enjoys writing or reading.
- *"What skills do my collaborators have that I don't?"* — a **2-hop traversal**
  (`Developer → COLLABORATED_WITH → Developer → HAS_SKILL → Skill`). In SQL this is a self-join on a
  team-membership table plus another join into skills, with a `NOT EXISTS` to exclude skills you
  already have.
- *"How is developer A connected to developer F?"* — a **variable-length shortest path** query.
  This is the case relational databases genuinely struggle with: you don't know in advance how many
  hops it takes, so SQL needs a recursive CTE that gets slower and uglier the deeper the chain goes.
  In Cypher it's `shortestPath((a)-[:COLLABORATED_WITH*1..5]-(b))` — one line.

As the network of developers, skills, and shared projects grows, these traversal queries stay roughly
constant-time in a graph (each hop follows direct pointers), while the equivalent SQL joins get more
expensive as the tables grow. The data **is** a graph — modeling it as one, and querying it with pattern
matching instead of joins, is a better fit than forcing it into normalized tables.

---

## Data model

```
(:Developer {id, name, email, experienceYears, location, bio})
(:Skill     {id, name, category})
(:Project   {id, name, description, status, startDate})

(:Developer)-[:HAS_SKILL {proficiency}]->(:Skill)
(:Developer)-[:WORKED_ON {role}]->(:Project)
(:Project)-[:REQUIRES]->(:Skill)
(:Developer)-[:COLLABORATED_WITH {sharedProjects}]-(:Developer)   -- derived, undirected
```

```
   (Developer)──HAS_SKILL──▶(Skill)◀──REQUIRES──(Project)
        │                                          ▲
        └──────────────WORKED_ON──────────────────┘
        │
        └──COLLABORATED_WITH──(Developer)   (derived from shared projects)
```

*(See `docs/data-model-diagram.png` for the visual version.)*

---

## Project structure

```
DevConnect-GraphApp/
├── backend/          ASP.NET Core Web API (Controllers, Services, Neo4j driver)
├── frontend/          React app (developer/project explorer UI)
├── data/              Seed JSON + C# seeder console app
├── cypher/            Standalone, documented Cypher queries
└── docs/              Data model diagram + UI screenshots
```

---

## Setup & run

### 1. Create your CognoDB instance
1. Sign up at https://console.cognodb.com/signup (free, no credit card).
2. Create a free (`c0`) instance, pick a region.
3. Copy the `bolt+s://<instance-id>.databases.cognodb.cloud` URI and the generated
   password for user `cognodb` — **the password is shown once**, save it immediately.

### 2. Seed the database
```bash
cd data/Seeder
cp .env.example .env      # then fill in COGNODB_URI / COGNODB_USER / COGNODB_PASSWORD
dotnet restore
dotnet run
```
This clears any existing data, creates constraints, and loads all developers, skills, projects,
and derived relationships from the JSON files in `data/`.

*(Alternative: paste `cypher/seed_data.cypher` directly into CognoDB's query console.)*

### 3. Run the backend API
```bash
cd backend/DevConnect.Api
cp ../.env.example .env    # or export the three COGNODB_* vars directly
dotnet restore
dotnet run
```
API runs at `http://localhost:5000` (Swagger UI at `/swagger`).

### 4. Run the frontend
```bash
cd frontend
cp .env.example .env       # set VITE_API_URL=http://localhost:5000
npm install
npm run dev
```
Opens at `http://localhost:3000` (pinned in `vite.config.js` to match the backend's default
CORS origin — see `Cors:AllowedOrigins` in `backend/DevConnect.Api/appsettings.json`).

The frontend is a small React (Vite) single-page app with four views:
- **Developers** — searchable list + profile pages (skills, projects, direct collaborators,
  and a 2-hop "skill gaps through the network" panel).
- **Projects** — required-skill coverage, current team, and ranked developer recommendations.
- **Path Finder** — pick any two developers and visualize the shortest COLLABORATED_WITH
  chain between them (variable-length traversal).

A connection-status indicator in the nav bar calls `/api/graph/health` every 30s and shows a
banner if CognoDB becomes unreachable, so the graceful-error-handling requirement is visible
in the UI, not just the API.

---

## Main queries (see `cypher/` for full documented versions)

| Query | File | What it does |
|---|---|---|
| Skill gaps through network | `multi_hop_traversal.cypher` | 2-hop: skills your collaborators have that you don't |
| Shortest collaboration path | `multi_hop_traversal.cypher` | Variable-length path between two developers |
| Project recommendations | `recommendation_query.cypher` | Ranks developers not on a project by matching skills |

## API endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/developers` | List all developers |
| GET | `/api/developers/{id}` | Single developer profile |
| GET | `/api/developers/{id}/collaborators` | Direct collaborators |
| GET | `/api/projects` | List all projects |
| GET | `/api/projects/{id}/skills` | Required skills + coverage |
| GET | `/api/projects/{id}/team` | Current team |
| GET | `/api/graph/recommendations/{projectId}` | Recommended developers for a project |
| GET | `/api/graph/skill-gaps/{developerId}` | 2-hop skill-gap traversal |
| GET | `/api/graph/path?fromDevId=&toDevId=` | Shortest collaboration path |
| GET | `/api/graph/health` | DB connectivity check |

---

## Screenshots

*(see `docs/screenshots/`)*

## Demo

- Hosted demo: _add link here_
- Screen recording: _add link here_
