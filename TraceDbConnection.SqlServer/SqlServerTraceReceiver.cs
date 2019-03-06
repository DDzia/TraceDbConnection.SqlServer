using System;
using System.Data.Common;
using System.Data.SqlClient;
using TraceDbConnection.SqlServer.Translation;
using TraceDbConnection.TraceReporting.Command;

namespace TraceDbConnection.SqlServer
{
    public abstract class SqlServerTraceReceiver: ITraceReceiver
    {
        public void Save(DbCommand command, ICommandTraceEntry traceEntry)
        {
            if(traceEntry is null)
                throw new ArgumentNullException(nameof(traceEntry));

            if (command is null)
                throw new ArgumentNullException(nameof(command));

            if (!(command is SqlCommand))
                throw new InvalidOperationException("Command is no SqlCommand.");

            var tsql = new TSqlCommandToTextTranslator()
                .Translate((SqlCommand)command);
            Save(tsql, command, traceEntry);
        }

        protected abstract void Save(string tsql, DbCommand command, ICommandTraceEntry traceEntry);
    }
}
