using Microsoft.Data.Sqlite;
using System;
using System.Data;
using YesSql.Core.Services;
using YesSql.Data;
using YesSql.Storage.InMemory;

namespace YesSql.Extensions.InMemory
{
    public static class InMemoryDbProviderOptionsExtensions
    {
        public static void UseInMemory(
            this IDbProviderOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var connectionString = "Data Source=:memory:";

            var configuration = new Configuration
            {
                ConnectionFactory = new DbConnectionFactory<SqliteConnection>(connectionString, true),
                DocumentStorageFactory = new InMemoryDocumentStorageFactory(),
                IsolationLevel = IsolationLevel.Serializable
            };

            options.ProviderName = "InMemory";
            options.Configuration = configuration;
        }
    }
}
