using System.Data.SqlClient;
using System.Text;

namespace TraceDbConnection.SqlServer.Translation
{
    public class TSqlCommandToTextTranslator
    {
        public string Translate(SqlCommand command)
        {
            var sb = new StringBuilder();

            var declareBuilder = new TSqlDeclareBuilder();
            var declareTsql = declareBuilder.Build(command);

            if (declareTsql != string.Empty)
            {
                sb.Append(declareTsql)
                    .AppendLine()
                    .AppendLine(); // append free line after declarations
            }

            sb.Append(command.CommandText);

            return sb.ToString();
        }
    }
}
