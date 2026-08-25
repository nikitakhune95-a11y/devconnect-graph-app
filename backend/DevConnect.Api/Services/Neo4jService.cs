using DevConnect.Api.Config;
using Neo4j.Driver;

namespace DevConnect.Api.Services
{
    public class Neo4jService : INeo4jService, IAsyncDisposable
    {
        private readonly IDriver _driver;
        private readonly ILogger<Neo4jService> _logger;

        public Neo4jService(CognoDbSettings settings, ILogger<Neo4jService> logger)
        {
            _logger = logger;

            // bolt+s:// already implies encrypted transport, so no extra config needed.
            _driver = GraphDatabase.Driver(
                settings.Uri,
                AuthTokens.Basic(settings.User, settings.Password));
        }

        public async Task<List<T>> RunQueryAsync<T>(
            string cypher,
            IDictionary<string, object> parameters,
            Func<IRecord, T> mapper)
        {
            await using var session = _driver.AsyncSession();
            try
            {
                return await session.ExecuteReadAsync(async tx =>
                {
                    var cursor = await tx.RunAsync(cypher, parameters);
                    var records = await cursor.ToListAsync();
                    return records.Select(mapper).ToList();
                });
            }
            catch (Neo4jException ex)
            {
                _logger.LogError(ex, "CognoDB query failed: {Query}", cypher);
                throw new InvalidOperationException("A database error occurred while running the query.", ex);
            }
        }

        public async Task RunWriteAsync(string cypher, IDictionary<string, object> parameters)
        {
            await using var session = _driver.AsyncSession();
            try
            {
                await session.ExecuteWriteAsync(async tx =>
                {
                    await tx.RunAsync(cypher, parameters);
                });
            }
            catch (Neo4jException ex)
            {
                _logger.LogError(ex, "CognoDB write failed: {Query}", cypher);
                throw new InvalidOperationException("A database error occurred while writing data.", ex);
            }
        }

        public async Task<bool> VerifyConnectivityAsync()
        {
            try
            {
                await _driver.VerifyConnectivityAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CognoDB connectivity check failed.");
                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _driver.DisposeAsync();
        }
    }
}
