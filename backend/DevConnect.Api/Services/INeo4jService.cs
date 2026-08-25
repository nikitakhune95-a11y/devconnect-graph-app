using Neo4j.Driver;

namespace DevConnect.Api.Services
{
    /// <summary>
    /// Thin abstraction over the Neo4j driver so controllers/query repositories
    /// never touch IDriver directly. Makes it easy to mock in tests and keeps
    /// session lifecycle management in one place.
    /// </summary>
    public interface INeo4jService
    {
        /// <summary>
        /// Runs a read (or write) Cypher query with parameters and maps each
        /// result record using the provided mapper function.
        /// </summary>
        Task<List<T>> RunQueryAsync<T>(
            string cypher,
            IDictionary<string, object> parameters,
            Func<IRecord, T> mapper);

        /// <summary>
        /// Runs a write Cypher query with no return value (e.g. MERGE/CREATE only).
        /// </summary>
        Task RunWriteAsync(string cypher, IDictionary<string, object> parameters);

        /// <summary>
        /// Verifies the driver can reach the database — used for health checks.
        /// </summary>
        Task<bool> VerifyConnectivityAsync();
    }
}
