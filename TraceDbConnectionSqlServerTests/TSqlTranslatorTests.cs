using System;
using System.Data.SqlClient;
using System.Globalization;
using TraceDbConnection.SqlServer.Translation;
using Xunit;

namespace TraceDbConnectionSqlServerTests
{
    public class TSqlTranslatorTests
    {
        private static readonly TSqlCommandToTextTranslator _sut = new TSqlCommandToTextTranslator();

        [Fact]
        public void Should_ThrowNullArgExc_When_NullInsteadCmdIsPassed()
        {
            // Act, Assert
            Assert.Throws<ArgumentNullException>("command", () => _sut.Translate(null));
        }

        [Fact]
        public void Should_TranslateCommand_When_OneParameterIsUsed()
        {
            // Arrange
            const string pname = "id";
            const int pvalue = 1;
            const string query = "SELECT * FROM [dbo].[tUsers] WHEN Id=@id";
            var expectedTSql = $"DECLARE @{pname} INT = {pvalue.ToString(CultureInfo.InvariantCulture)};" +
                               Environment.NewLine +
                               Environment.NewLine +
                               query;
            using (var cmd = new SqlCommand("SELECT * FROM [dbo].[tUsers] WHEN Id=@id"))
            {
                cmd.Parameters.Add(new SqlParameter("id", 1));

                // Act
                var tsql =_sut.Translate(cmd);

                // Assert
                Assert.Equal(tsql, expectedTSql);
            }
        }
    }
}
