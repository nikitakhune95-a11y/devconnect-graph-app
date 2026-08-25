using Neo4j.Driver;
using Newtonsoft.Json;
using DotNetEnv;

// Load .env from this folder (data/Seeder/.env) if present.
Env.Load();

string uri = Environment.GetEnvironmentVariable("COGNODB_URI")
    ?? throw new Exception("COGNODB_URI not set");
string user = Environment.GetEnvironmentVariable("COGNODB_USER")
    ?? throw new Exception("COGNODB_USER not set");
string password = Environment.GetEnvironmentVariable("COGNODB_PASSWORD")
    ?? throw new Exception("COGNODB_PASSWORD not set");

Console.WriteLine("Connecting to CognoDB...");
var driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));

try
{
    await driver.VerifyConnectivityAsync();
    Console.WriteLine("Connected successfully.\n");
}
catch (Exception ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
    return;
}

await using var session = driver.AsyncSession();

// -------------------- JSON models --------------------
var skillsJsonPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "skills.json");
var devsJsonPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "developers.json");
var projectsJsonPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "projects.json");

var skills = JsonConvert.DeserializeObject<List<SkillJson>>(await File.ReadAllTextAsync(skillsJsonPath))!;
var developers = JsonConvert.DeserializeObject<List<DeveloperJson>>(await File.ReadAllTextAsync(devsJsonPath))!;
var projects = JsonConvert.DeserializeObject<List<ProjectJson>>(await File.ReadAllTextAsync(projectsJsonPath))!;

Console.WriteLine($"Loaded {skills.Count} skills, {developers.Count} developers, {projects.Count} projects.\n");

Console.WriteLine("Clearing existing graph data...");
await session.RunAsync("MATCH (n) DETACH DELETE n");

Console.WriteLine("Creating constraints...");
await session.RunAsync("CREATE CONSTRAINT dev_id IF NOT EXISTS FOR (d:Developer) REQUIRE d.id IS UNIQUE");
await session.RunAsync("CREATE CONSTRAINT skill_id IF NOT EXISTS FOR (s:Skill) REQUIRE s.id IS UNIQUE");
await session.RunAsync("CREATE CONSTRAINT project_id IF NOT EXISTS FOR (p:Project) REQUIRE p.id IS UNIQUE");

Console.WriteLine("Loading skills...");
foreach (var skill in skills)
{
    await session.RunAsync(
        "MERGE (s:Skill {id: $id}) SET s.name = $name, s.category = $category",
        new { id = skill.id, name = skill.name, category = skill.category });
}
Console.WriteLine($"  -> {skills.Count} skills created.");

Console.WriteLine("Loading developers and their skills...");
foreach (var dev in developers)
{
    await session.RunAsync(
        @"MERGE (d:Developer {id: $id})
          SET d.name = $name, d.email = $email, d.experienceYears = $experienceYears,
              d.location = $location, d.bio = $bio",
        new
        {
            id = dev.id,
            name = dev.name,
            email = dev.email,
            experienceYears = dev.experienceYears,
            location = dev.location,
            bio = dev.bio
        });

    foreach (var devSkill in dev.skills)
    {
        await session.RunAsync(
            @"MATCH (d:Developer {id: $devId}), (s:Skill {id: $skillId})
              MERGE (d)-[r:HAS_SKILL]->(s)
              SET r.proficiency = $proficiency",
            new { devId = dev.id, skillId = devSkill.skillId, proficiency = devSkill.proficiency });
    }
}
Console.WriteLine($"  -> {developers.Count} developers created with skill relationships.");

Console.WriteLine("Loading projects and required skills...");
foreach (var proj in projects)
{
    await session.RunAsync(
        @"MERGE (p:Project {id: $id})
          SET p.name = $name, p.description = $description,
              p.status = $status, p.startDate = $startDate",
        new
        {
            id = proj.id,
            name = proj.name,
            description = proj.description,
            status = proj.status,
            startDate = proj.startDate
        });

    foreach (var skillId in proj.requiredSkills)
    {
        await session.RunAsync(
            @"MATCH (p:Project {id: $projId}), (s:Skill {id: $skillId})
              MERGE (p)-[:REQUIRES]->(s)",
            new { projId = proj.id, skillId = skillId });
    }
}
Console.WriteLine($"  -> {projects.Count} projects created with required-skill relationships.");

Console.WriteLine("Loading developer-project (WORKED_ON) relationships...");
int workedOnCount = 0;
foreach (var proj in projects)
{
    foreach (var member in proj.team)
    {
        await session.RunAsync(
            @"MATCH (d:Developer {id: $devId}), (p:Project {id: $projId})
              MERGE (d)-[r:WORKED_ON]->(p)
              SET r.role = $role",
            new { devId = member.developerId, projId = proj.id, role = member.role });
        workedOnCount++;
    }
}
Console.WriteLine($"  -> {workedOnCount} WORKED_ON relationships created.");

Console.WriteLine("Deriving COLLABORATED_WITH relationships from shared projects...");
await session.RunAsync(
    @"MATCH (d1:Developer)-[:WORKED_ON]->(p:Project)<-[:WORKED_ON]-(d2:Developer)
      WHERE d1.id < d2.id
      MERGE (d1)-[r:COLLABORATED_WITH]-(d2)
      ON CREATE SET r.sharedProjects = 1
      ON MATCH SET r.sharedProjects = r.sharedProjects + 1");
Console.WriteLine("  -> COLLABORATED_WITH relationships derived.");

Console.WriteLine("\nSeeding complete!");
await driver.CloseAsync();

// -------------------- Data models --------------------
public class SkillJson
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public string category { get; set; } = "";
}

public class DevSkillJson
{
    public string skillId { get; set; } = "";
    public string proficiency { get; set; } = "";
}

public class DeveloperJson
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public string email { get; set; } = "";
    public int experienceYears { get; set; }
    public string location { get; set; } = "";
    public string bio { get; set; } = "";
    public List<DevSkillJson> skills { get; set; } = new();
}

public class TeamMemberJson
{
    public string developerId { get; set; } = "";
    public string role { get; set; } = "";
}

public class ProjectJson
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public string description { get; set; } = "";
    public string status { get; set; } = "";
    public string startDate { get; set; } = "";
    public List<string> requiredSkills { get; set; } = new();
    public List<TeamMemberJson> team { get; set; } = new();
}
