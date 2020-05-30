using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using YesSql.Sql.Schema;

namespace YesSql.Sql
{
    public abstract class BaseCommandInterpreter : ICommandInterpreter
    {
        private const char Space = ' ';
        
        protected readonly ISqlDialect _dialect;
        protected readonly NamingPolicy _namingPolicy;

        public BaseCommandInterpreter(ISqlDialect dialect, NamingPolicy namingPolicy)
        {
            _dialect = dialect;
            _namingPolicy = namingPolicy;
        }

        public IEnumerable<string> CreateSql(IEnumerable<ISchemaCommand> commands)
        {

            var sqlCommands = new List<string>();

            foreach (var command in commands)
            {
                var schemaCommand = command as SchemaCommand;
                if (schemaCommand == null)
                {
                    continue;
                }

                switch (schemaCommand.Type)
                {
                    case SchemaCommandType.CreateTable:
                        sqlCommands.AddRange(Run((ICreateTableCommand)schemaCommand));
                        break;
                    case SchemaCommandType.AlterTable:
                        sqlCommands.AddRange(Run((IAlterTableCommand)schemaCommand));
                        break;
                    case SchemaCommandType.DropTable:
                        sqlCommands.AddRange(Run((IDropTableCommand)schemaCommand));
                        break;
                    case SchemaCommandType.SqlStatement:
                        sqlCommands.AddRange(Run((ISqlStatementCommand)schemaCommand));
                        break;
                    case SchemaCommandType.CreateForeignKey:
                        sqlCommands.AddRange(Run((ICreateForeignKeyCommand)schemaCommand));
                        break;
                    case SchemaCommandType.DropForeignKey:
                        sqlCommands.AddRange(Run((IDropForeignKeyCommand)schemaCommand));
                        break;
                }
            }

            return sqlCommands;
        }

        public virtual IEnumerable<string> Run(ICreateTableCommand command)
        {
            // TODO: Support CreateForeignKeyCommand in a CREATE TABLE (in sqlite they can only be created with the table)

            var builder = new StringBuilder();

            builder.Append(_dialect.CreateTableString)
                .Append(' ')
                .Append(_dialect.QuoteForTableName(_namingPolicy.ConvertName(command.Name)))
                .Append(" (");

            var appendComma = false;
            foreach (var createColumn in command.TableCommands.OfType<CreateColumnCommand>())
            {
                if (appendComma)
                {
                    builder.Append(", ");
                }
                appendComma = true;

                Run(builder, createColumn);
            }

            var primaryKeys = command.TableCommands.OfType<CreateColumnCommand>().Where(ccc => ccc.IsPrimaryKey && !ccc.IsIdentity).Select(ccc => _dialect.QuoteForColumnName(_namingPolicy.ConvertName(ccc.ColumnName))).ToArray();
            if (primaryKeys.Any())
            {
                if (appendComma)
                {
                    builder.Append(", ");
                }

                builder.Append(_dialect.PrimaryKeyString)
                    .Append(" ( ")
                    .Append(String.Join(", ", primaryKeys.ToArray()))
                    .Append(" )");
            }

            builder.Append(" )");
            yield return builder.ToString();
        }

        public virtual IEnumerable<string> Run(IDropTableCommand command)
        {
            var builder = new StringBuilder();

            builder.Append(_dialect.GetDropTableString(_namingPolicy.ConvertName(command.Name)));
            yield return builder.ToString();
        }

        public virtual IEnumerable<string> Run(IAlterTableCommand command)
        {
            if (command.TableCommands.Count == 0)
            {
                yield break;
            }

            // drop columns
            foreach (var dropColumn in command.TableCommands.OfType<DropColumnCommand>())
            {
                var builder = new StringBuilder();
                Run(builder, dropColumn);
                yield return builder.ToString();
            }

            // add columns
            foreach (var addColumn in command.TableCommands.OfType<AddColumnCommand>())
            {
                var builder = new StringBuilder();
                Run(builder, addColumn);
                yield return builder.ToString();
            }

            // alter columns
            foreach (var alterColumn in command.TableCommands.OfType<AlterColumnCommand>())
            {
                var builder = new StringBuilder();
                Run(builder, alterColumn);
                yield return builder.ToString();
            }

            // rename columns
            foreach (var renameColumn in command.TableCommands.OfType<RenameColumnCommand>())
            {
                var builder = new StringBuilder();
                Run(builder, renameColumn);
                yield return builder.ToString();
            }

            // add index
            foreach (var addIndex in command.TableCommands.OfType<AddIndexCommand>())
            {
                var builder = new StringBuilder();
                Run(builder, addIndex);
                yield return builder.ToString();
            }

            // drop index
            foreach (var dropIndex in command.TableCommands.OfType<DropIndexCommand>())
            {
                var builder = new StringBuilder();
                Run(builder, dropIndex);
                yield return builder.ToString();
            }
        }

        public virtual void Run(StringBuilder builder, IAddColumnCommand command)
        {
            builder.AppendFormat("alter table {0} add ", _dialect.QuoteForTableName(_namingPolicy.ConvertName(command.Name)));
            Run(builder, (CreateColumnCommand)command);
        }

        public virtual void Run(StringBuilder builder, IDropColumnCommand command)
        {
            builder.AppendFormat("alter table {0} drop column {1}",
                _dialect.QuoteForTableName(_namingPolicy.ConvertName(command.Name)),
                _dialect.QuoteForColumnName(_namingPolicy.ConvertName(command.ColumnName)));
        }

        public virtual void Run(StringBuilder builder, IAlterColumnCommand command)
        {
            builder.AppendFormat("alter table {0} alter column {1} ",
                _dialect.QuoteForTableName(_namingPolicy.ConvertName(command.Name)),
                _dialect.QuoteForColumnName(_namingPolicy.ConvertName(command.ColumnName)));

            // type
            if (command.DbType != DbType.Object)
            {
                builder.Append(_dialect.GetTypeName(command.DbType, command.Length, command.Precision, command.Scale));
            }
            else
            {
                if (command.Length > 0 || command.Precision > 0 || command.Scale > 0)
                {
                    throw new Exception("Error while executing data migration: you need to specify the field's type in order to change its properties");
                }
            }

            // [default value]
            if (command.Default != null)
            {
                builder.Append(" set default ").Append(_dialect.GetSqlValue(command.Default)).Append(Space);
            }
        }

        public virtual void Run(StringBuilder builder, IRenameColumnCommand command)
        {
            builder.AppendFormat("alter table {0} rename column {1} to {2}",
                _dialect.QuoteForTableName(_namingPolicy.ConvertName(command.Name)),
                _dialect.QuoteForColumnName(_namingPolicy.ConvertName(command.ColumnName)),
                _dialect.QuoteForColumnName(_namingPolicy.ConvertName(command.NewColumnName))
                );
        }

        public virtual void Run(StringBuilder builder, IAddIndexCommand command)
        {
            builder.AppendFormat("create index {1} on {0} ({2}) ",
                _dialect.QuoteForTableName(_namingPolicy.ConvertName(command.Name)),
                _dialect.QuoteForColumnName(_namingPolicy.ConvertName(command.IndexName)),
                String.Join(", ", command.ColumnNames.Select(x => _dialect.QuoteForColumnName(_namingPolicy.ConvertName(x))).ToArray()));
        }

        public virtual void Run(StringBuilder builder, IDropIndexCommand command)
        {
            builder.Append(_dialect.GetDropIndexString(_namingPolicy.ConvertName(command.IndexName), _namingPolicy.ConvertName(command.Name)));
        }

        public virtual IEnumerable<string> Run(ISqlStatementCommand command)
        {
            if (command.Providers.Count != 0)
            {
                yield break;
            }

            yield return command.Sql;
        }

        public virtual IEnumerable<string> Run(ICreateForeignKeyCommand command)
        {
            var builder = new StringBuilder();

            builder.Append("alter table ")
                .Append(_dialect.QuoteForTableName(_namingPolicy.ConvertName(command.SrcTable)));

            builder.Append(_dialect.GetAddForeignKeyConstraintString(command.Name,
                command.SrcColumns.Select(x => _dialect.QuoteForColumnName(_namingPolicy.ConvertName(x))).ToArray(),
                _dialect.QuoteForTableName(_namingPolicy.ConvertName(command.DestTable)),
                command.DestColumns.Select(x => _dialect.QuoteForColumnName(_namingPolicy.ConvertName(x))).ToArray(),
                false));

            yield return builder.ToString();
        }

        public virtual IEnumerable<string> Run(IDropForeignKeyCommand command)
        {
            var builder = new StringBuilder();

            builder.Append("alter table ")
                .Append(_dialect.QuoteForTableName(_namingPolicy.ConvertName(command.SrcTable)))
                .Append(_dialect.GetDropForeignKeyConstraintString(command.Name));
            yield return builder.ToString();
        }

        private void Run(StringBuilder builder, ICreateColumnCommand command)
        {
            // name
            builder.Append(_dialect.QuoteForColumnName(_namingPolicy.ConvertName(command.ColumnName))).Append(Space);

            if (!command.IsIdentity || _dialect.HasDataTypeInIdentityColumn)
            {
                builder.Append(_dialect.GetTypeName(command.DbType, command.Length, command.Precision, command.Scale));
            }

            // append identity if handled
            if (command.IsIdentity && _dialect.SupportsIdentityColumns)
            {
                builder.Append(Space).Append(_dialect.IdentityColumnString);
            }

            // [default value]
            if (command.Default != null)
            {
                builder.Append(" default ").Append(_dialect.GetSqlValue(command.Default)).Append(Space);
            }

            // nullable
            builder.Append(command.IsNotNull
                               ? " not null"
                               : !command.IsPrimaryKey && !command.IsUnique
                                     ? _dialect.NullColumnString
                                     : string.Empty);

            // append unique if handled, otherwise at the end of the satement
            if (command.IsUnique && _dialect.SupportsUnique)
            {
                builder.Append(" unique");
            }
        }
    }
}
