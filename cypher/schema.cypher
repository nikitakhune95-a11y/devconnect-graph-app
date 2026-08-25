// ============================================================
// DevConnect — Graph Schema (constraints & indexes)
// Run once against a fresh CognoDB instance before seeding.
// ============================================================

// Uniqueness constraints (also create backing indexes automatically)
CREATE CONSTRAINT dev_id IF NOT EXISTS
FOR (d:Developer) REQUIRE d.id IS UNIQUE;

CREATE CONSTRAINT skill_id IF NOT EXISTS
FOR (s:Skill) REQUIRE s.id IS UNIQUE;

CREATE CONSTRAINT project_id IF NOT EXISTS
FOR (p:Project) REQUIRE p.id IS UNIQUE;

// ============================================================
// Data model summary
// ============================================================
// (:Developer {id, name, email, experienceYears, location, bio})
// (:Skill     {id, name, category})
// (:Project   {id, name, description, status, startDate})
//
// (:Developer)-[:HAS_SKILL {proficiency}]->(:Skill)
// (:Developer)-[:WORKED_ON {role}]->(:Project)
// (:Project)-[:REQUIRES]->(:Skill)
// (:Developer)-[:COLLABORATED_WITH {sharedProjects}]-(:Developer)   // derived, undirected
// ============================================================
