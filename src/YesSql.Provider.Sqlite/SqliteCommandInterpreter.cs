using System.Collections.Generic;
using YesSql.Sql;
using YesSql.Sql.Schema;

namespace YesSql.Provider.Sqlite
{
    public class SqliteCommandInterpreter : BaseCommandInterpreter
    {
        public SqliteCommandInterpreter(ISqlDialect dialect, NamingPolicy namingPolicy) : base(dialect, namingPolicy)
        {
        }

        public override IEnumerable<string> Run(ICreateForeignKeyCommand command)
        {
            yield break;
        }

        public override IEnumerable<string> Run(IDropForeignKeyCommand command)
        {
            yield break;
        }
    }
}
