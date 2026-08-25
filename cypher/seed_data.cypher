// ============================================================
// Manual seed script (alternative to running data/Seeder).
// Paste this directly into CognoDB's query console if you prefer
// not to run the C# seeder project.
// ============================================================

// ---------- Skills ----------
UNWIND [
  {id: 'skill001', name: 'C#', category: 'Backend'},
  {id: 'skill002', name: 'ASP.NET Core', category: 'Backend'},
  {id: 'skill003', name: 'Entity Framework', category: 'Backend'},
  {id: 'skill004', name: 'SQL Server', category: 'Database'},
  {id: 'skill005', name: 'React', category: 'Frontend'},
  {id: 'skill006', name: 'JavaScript', category: 'Frontend'},
  {id: 'skill007', name: 'Python', category: 'Backend'},
  {id: 'skill008', name: 'Node.js', category: 'Backend'},
  {id: 'skill009', name: 'Docker', category: 'DevOps'},
  {id: 'skill010', name: 'Azure', category: 'DevOps'},
  {id: 'skill011', name: 'Neo4j', category: 'Database'},
  {id: 'skill012', name: 'REST APIs', category: 'Backend'},
  {id: 'skill013', name: 'MongoDB', category: 'Database'},
  {id: 'skill014', name: 'TypeScript', category: 'Frontend'},
  {id: 'skill015', name: 'CI/CD', category: 'DevOps'}
] AS skill
MERGE (s:Skill {id: skill.id})
SET s.name = skill.name, s.category = skill.category;

// ---------- Developers ----------
UNWIND [
  {id: 'dev001', name: 'Nikita Khune', email: 'nikita@devconnect.io', experienceYears: 4, location: 'Pune, India', bio: '.NET Full Stack Developer focused on backend systems and REST APIs.'},
  {id: 'dev002', name: 'Aarav Sharma', email: 'aarav@devconnect.io', experienceYears: 6, location: 'Bengaluru, India', bio: 'Backend engineer specializing in distributed systems and databases.'},
  {id: 'dev003', name: 'Priya Deshmukh', email: 'priya@devconnect.io', experienceYears: 3, location: 'Mumbai, India', bio: 'Frontend developer with a passion for design systems.'},
  {id: 'dev004', name: 'Rohan Mehta', email: 'rohan@devconnect.io', experienceYears: 8, location: 'Hyderabad, India', bio: 'DevOps lead with cloud infrastructure expertise.'},
  {id: 'dev005', name: 'Sneha Kulkarni', email: 'sneha@devconnect.io', experienceYears: 5, location: 'Pune, India', bio: 'Full stack developer bridging backend APIs and React frontends.'},
  {id: 'dev006', name: 'Karan Patil', email: 'karan@devconnect.io', experienceYears: 2, location: 'Nagpur, India', bio: 'Junior backend developer, growing in database design.'}
] AS dev
MERGE (d:Developer {id: dev.id})
SET d.name = dev.name, d.email = dev.email, d.experienceYears = dev.experienceYears,
    d.location = dev.location, d.bio = dev.bio;

// ---------- Projects ----------
UNWIND [
  {id: 'proj001', name: 'FinPay Gateway', description: 'Payment and transaction management platform with role-based access control.', status: 'Completed', startDate: '2023-01-15'},
  {id: 'proj002', name: 'DevConnect Graph Explorer', description: 'Internal tool to visualize developer-skill-project relationships using CognoDB.', status: 'Active', startDate: '2026-06-01'},
  {id: 'proj003', name: 'Cloud Migration Suite', description: 'Migrate legacy on-prem services to Azure with CI/CD pipelines.', status: 'Active', startDate: '2025-11-10'},
  {id: 'proj004', name: 'Analytics Dashboard', description: 'Real-time analytics dashboard for business metrics.', status: 'OnHold', startDate: '2024-08-20'},
  {id: 'proj005', name: 'Recommendation Engine', description: 'Graph-based recommendation system for matching developers to projects.', status: 'Active', startDate: '2026-03-05'}
] AS proj
MERGE (p:Project {id: proj.id})
SET p.name = proj.name, p.description = proj.description, p.status = proj.status, p.startDate = proj.startDate;

// ---------- HAS_SKILL relationships ----------
UNWIND [
  {devId: 'dev001', skillId: 'skill001', proficiency: 'Expert'},
  {devId: 'dev001', skillId: 'skill002', proficiency: 'Expert'},
  {devId: 'dev001', skillId: 'skill003', proficiency: 'Advanced'},
  {devId: 'dev001', skillId: 'skill004', proficiency: 'Advanced'},
  {devId: 'dev001', skillId: 'skill012', proficiency: 'Expert'},
  {devId: 'dev001', skillId: 'skill005', proficiency: 'Intermediate'},
  {devId: 'dev002', skillId: 'skill007', proficiency: 'Expert'},
  {devId: 'dev002', skillId: 'skill008', proficiency: 'Advanced'},
  {devId: 'dev002', skillId: 'skill011', proficiency: 'Advanced'},
  {devId: 'dev002', skillId: 'skill013', proficiency: 'Intermediate'},
  {devId: 'dev002', skillId: 'skill009', proficiency: 'Advanced'},
  {devId: 'dev003', skillId: 'skill005', proficiency: 'Expert'},
  {devId: 'dev003', skillId: 'skill006', proficiency: 'Expert'},
  {devId: 'dev003', skillId: 'skill014', proficiency: 'Advanced'},
  {devId: 'dev003', skillId: 'skill012', proficiency: 'Intermediate'},
  {devId: 'dev004', skillId: 'skill009', proficiency: 'Expert'},
  {devId: 'dev004', skillId: 'skill010', proficiency: 'Expert'},
  {devId: 'dev004', skillId: 'skill015', proficiency: 'Expert'},
  {devId: 'dev004', skillId: 'skill002', proficiency: 'Intermediate'},
  {devId: 'dev005', skillId: 'skill001', proficiency: 'Advanced'},
  {devId: 'dev005', skillId: 'skill002', proficiency: 'Advanced'},
  {devId: 'dev005', skillId: 'skill005', proficiency: 'Advanced'},
  {devId: 'dev005', skillId: 'skill006', proficiency: 'Expert'},
  {devId: 'dev005', skillId: 'skill004', proficiency: 'Intermediate'},
  {devId: 'dev006', skillId: 'skill007', proficiency: 'Intermediate'},
  {devId: 'dev006', skillId: 'skill013', proficiency: 'Beginner'},
  {devId: 'dev006', skillId: 'skill011', proficiency: 'Beginner'}
] AS rel
MATCH (d:Developer {id: rel.devId}), (s:Skill {id: rel.skillId})
MERGE (d)-[r:HAS_SKILL]->(s)
SET r.proficiency = rel.proficiency;

// ---------- REQUIRES relationships ----------
UNWIND [
  {projId: 'proj001', skillId: 'skill001'}, {projId: 'proj001', skillId: 'skill002'},
  {projId: 'proj001', skillId: 'skill003'}, {projId: 'proj001', skillId: 'skill004'},
  {projId: 'proj001', skillId: 'skill012'},
  {projId: 'proj002', skillId: 'skill001'}, {projId: 'proj002', skillId: 'skill002'},
  {projId: 'proj002', skillId: 'skill011'}, {projId: 'proj002', skillId: 'skill005'},
  {projId: 'proj002', skillId: 'skill012'},
  {projId: 'proj003', skillId: 'skill009'}, {projId: 'proj003', skillId: 'skill010'},
  {projId: 'proj003', skillId: 'skill015'}, {projId: 'proj003', skillId: 'skill002'},
  {projId: 'proj004', skillId: 'skill007'}, {projId: 'proj004', skillId: 'skill013'},
  {projId: 'proj004', skillId: 'skill005'}, {projId: 'proj004', skillId: 'skill006'},
  {projId: 'proj005', skillId: 'skill011'}, {projId: 'proj005', skillId: 'skill007'},
  {projId: 'proj005', skillId: 'skill001'}, {projId: 'proj005', skillId: 'skill012'}
] AS rel
MATCH (p:Project {id: rel.projId}), (s:Skill {id: rel.skillId})
MERGE (p)-[:REQUIRES]->(s);

// ---------- WORKED_ON relationships ----------
UNWIND [
  {devId: 'dev001', projId: 'proj001', role: 'Backend Developer'},
  {devId: 'dev005', projId: 'proj001', role: 'Full Stack Developer'},
  {devId: 'dev004', projId: 'proj001', role: 'DevOps Engineer'},
  {devId: 'dev001', projId: 'proj002', role: 'Backend Lead'},
  {devId: 'dev002', projId: 'proj002', role: 'Graph DB Specialist'},
  {devId: 'dev003', projId: 'proj002', role: 'Frontend Developer'},
  {devId: 'dev004', projId: 'proj003', role: 'DevOps Lead'},
  {devId: 'dev002', projId: 'proj003', role: 'Backend Engineer'},
  {devId: 'dev003', projId: 'proj004', role: 'Frontend Developer'},
  {devId: 'dev006', projId: 'proj004', role: 'Backend Developer'},
  {devId: 'dev001', projId: 'proj005', role: 'Backend Developer'},
  {devId: 'dev002', projId: 'proj005', role: 'Graph DB Specialist'},
  {devId: 'dev006', projId: 'proj005', role: 'Backend Developer'}
] AS rel
MATCH (d:Developer {id: rel.devId}), (p:Project {id: rel.projId})
MERGE (d)-[r:WORKED_ON]->(p)
SET r.role = rel.role;

// ---------- Derive COLLABORATED_WITH from shared projects ----------
MATCH (d1:Developer)-[:WORKED_ON]->(p:Project)<-[:WORKED_ON]-(d2:Developer)
WHERE d1.id < d2.id
MERGE (d1)-[r:COLLABORATED_WITH]-(d2)
ON CREATE SET r.sharedProjects = 1
ON MATCH SET r.sharedProjects = r.sharedProjects + 1;
