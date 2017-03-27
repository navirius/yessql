using System;
using System.Data;
using System.Data.SqlClient;
using YesSql.Core.Services;
using YesSql.Data;
using YesSql.Storage.Sql;

namespace YesSql.Extensions.SqlServer
{
    public static class SqlServerDbOptionsExtensions
    {
        public static void UseSqlServer(
            this IDbProviderOptions options, string connectionString)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var configuration = new Configuration
            {
                ConnectionFactory = new DbConnectionFactory<SqlConnection>(connectionString, true),
                DocumentStorageFactory = new SqlDocumentStorageFactory(),
                IsolationLevel = IsolationLevel.ReadUncommitted
            };

            options.ProviderName = "SQL Server";
            options.Configuration = configuration;
        }
    }
}
