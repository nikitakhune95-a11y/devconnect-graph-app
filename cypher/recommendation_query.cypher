// ============================================================
// RECOMMENDATION QUERY — the one a relational DB finds awkward
// ============================================================
// Goal: for a given project, rank developers NOT currently on the
// team by how many of the project's required skills they already
// have, and show exactly which skills match and which are missing.
//
// In SQL this needs: a developer_skills join table, a
// project_required_skills join table, a self-join / EXISTS
// subquery to exclude current team members, GROUP BY + COUNT to
// rank matches, and a set-difference (NOT IN) for missing skills —
// typically 3-4 joins plus a correlated subquery.
//
// In Cypher, the graph shape *is* the query.
// ============================================================

MATCH (p:Project {id: $projectId})-[:REQUIRES]->(required:Skill)
WITH p, collect(required) AS requiredSkills

MATCH (d:Developer)-[:HAS_SKILL]->(s:Skill)
WHERE s IN requiredSkills
WITH p, d, requiredSkills, collect(DISTINCT s) AS matchingSkills

OPTIONAL MATCH (d)-[:WORKED_ON]->(p)
WITH d, requiredSkills, matchingSkills, count(*) > 0 AS alreadyOnProject
WHERE NOT alreadyOnProject

RETURN d.id AS developerId,
       d.name AS developerName,
       d.experienceYears AS experienceYears,
       size(matchingSkills) AS matchingSkillsCount,
       [s IN matchingSkills | s.name] AS matchingSkillNames,
       [s IN requiredSkills WHERE NOT s IN matchingSkills | s.name] AS missingSkillNames,
       alreadyOnProject
ORDER BY matchingSkillsCount DESC, experienceYears DESC
LIMIT 10;


// ------------------------------------------------------------
// Bonus: skill coverage summary for a project — which required
// skills are already covered by the current team, and which are gaps.
// ------------------------------------------------------------
MATCH (p:Project {id: $projectId})-[:REQUIRES]->(s:Skill)
OPTIONAL MATCH (p)<-[:WORKED_ON]-(:Developer)-[:HAS_SKILL]->(s)
RETURN s.name AS skillName,
       s.category AS category,
       count(*) > 0 AS isCoveredByTeam
ORDER BY isCoveredByTeam, s.name;
