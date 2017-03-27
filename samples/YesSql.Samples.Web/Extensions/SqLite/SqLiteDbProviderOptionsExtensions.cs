using Microsoft.Data.Sqlite;
using System;
using System.Data;
using YesSql.Core.Services;
using YesSql.Data;
using YesSql.Storage.Sql;

namespace YesSql.Extensions.SqLite
{
    public static class SqLiteDbOptionsExtensions
    {
        public static void UseSqLite(
            this IDbProviderOptions options, string connectionString)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var configuration = new Configuration
            {
                ConnectionFactory = new DbConnectionFactory<SqliteConnection>(connectionString, true),
                DocumentStorageFactory = new SqlDocumentStorageFactory(),
                IsolationLevel = IsolationLevel.Serializable
            };

            options.ProviderName = "SQLite";
            options.Configuration = configuration;
        }
    }
}
