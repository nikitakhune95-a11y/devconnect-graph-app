// ============================================================
// MULTI-HOP TRAVERSAL QUERIES (2+ hops)
// ============================================================

// ------------------------------------------------------------
// 1. Skill gaps through the network (2-hop traversal)
// Developer -> COLLABORATED_WITH -> Developer -> HAS_SKILL -> Skill
// "What skills do my collaborators have that I don't?"
// ------------------------------------------------------------
MATCH (me:Developer {id: $developerId})-[:COLLABORATED_WITH]-(peer:Developer)
      -[:HAS_SKILL]->(peerSkill:Skill)
WHERE NOT (me)-[:HAS_SKILL]->(peerSkill)
RETURN peerSkill.id AS skillId,
       peerSkill.name AS skillName,
       peerSkill.category AS category,
       collect(DISTINCT peer.name) AS knownBy
ORDER BY size(knownBy) DESC;


// ------------------------------------------------------------
// 2. Shortest collaboration path between two developers
// (variable-length traversal, 1 to 5 hops)
// "How is developer A connected to developer F through the team?"
// ------------------------------------------------------------
MATCH path = shortestPath(
    (a:Developer {id: $fromId})-[:COLLABORATED_WITH*1..5]-(b:Developer {id: $toId})
)
RETURN [n IN nodes(path) | n.name] AS developersInPath,
       length(path) AS hopCount;


// ------------------------------------------------------------
// 3. Three-hop reach: skills a project could tap into via its
// current team's collaborators (Project -> Developer -> COLLABORATED_WITH -> Developer -> Skill)
// Useful for "who could we pull in if we needed more hands"
// ------------------------------------------------------------
MATCH (p:Project {id: $projectId})<-[:WORKED_ON]-(teamMember:Developer)
      -[:COLLABORATED_WITH]-(extendedContact:Developer)-[:HAS_SKILL]->(s:Skill)
WHERE NOT (extendedContact)-[:WORKED_ON]->(p)
RETURN DISTINCT extendedContact.name AS candidateName,
       collect(DISTINCT s.name) AS skillsTheyBring,
       teamMember.name AS connectedThrough
ORDER BY candidateName;
