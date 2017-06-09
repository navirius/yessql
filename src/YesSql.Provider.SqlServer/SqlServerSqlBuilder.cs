using System.Text;
using YesSql.Sql;

namespace YesSql.Provider.SqlServer
{
    public class SqlServerSqlBuilder : SqlBuilder
    {
        public SqlServerSqlBuilder(string tablePrefix, ISqlDialect dialect) : base(tablePrefix, dialect)
        {
        }

        public override string ToSqlString(ISqlDialect dialect, bool ignoreOrderBy = false)
        {
            if (_clause == "select")
            {
                if ((_skip != 0 || _count != 0))
                {
                    dialect.Page(this, _skip, _count);
                }

                var tableName = _dialect.QuoteForTableName(_tablePrefix + _table);
                var sb = new StringBuilder();
                sb.Append(_clause).Append(" ").Append(_selector).Append(" from ");

                if (dialect.Name == "SqlServer 2008 R2" && _count != 0)
                {
                    sb.Append($"(select *, row_number() over(order by Id) AS RowNum from {tableName}) As {tableName}");
                }
                else
                {
                    sb.Append(tableName);
                }

                if (_join != null)
                {
                    sb.Append(" ").Append(_join.ToString());
                }

                if (_where != null)
                {
                    sb.Append(" where ").Append(_where);
                }

                if (_order != null && !ignoreOrderBy)
                {
                    sb.Append(" order by ").Append(_order);
                }

                if (!string.IsNullOrEmpty(Trail))
                {
                    sb.Append(" ").Append(Trail);
                }
                
                return sb.ToString();
            }

            return "";
        }
    }
}
