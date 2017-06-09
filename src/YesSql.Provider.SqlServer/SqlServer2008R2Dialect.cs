namespace YesSql.Provider.SqlServer
{
    public class SqlServer2008Dialect : SqlServerDialect
    {
        public override string Name => "SqlServer 2008 R2";

        public override void Page(ISqlBuilder sqlBuilder, int offset, int limit)
        {
            if (limit != 0)
            {
                var selector = sqlBuilder.GetSelector();
                selector = " top " + limit + " " + selector;
                sqlBuilder.Selector(selector);
            }

            if (offset != 0)
            {
                sqlBuilder.WhereAlso("RowNum >" + offset);
            }
        }
    }
}
