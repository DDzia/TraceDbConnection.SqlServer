using System;
using System.Data.SqlClient;
using System.Globalization;
using Xunit;
using TraceDbConnection.SqlServer.Translation;

namespace TraceDbConnectionSqlServerTests
{
    public class DeclareBuilderTests
    {
        private static readonly TSqlDeclareBuilder _sut = new TSqlDeclareBuilder();

        [Fact]
        public void Should_ThrowNullArgException_When_NullInsteadCommandIsPassed()
        {
            // Act, Assert
            Assert.Throws<ArgumentNullException>("command", () => _sut.Build(null));
        }

        [Fact]
        public void Should_ReturnEmptyLine_When_CommandHasNoParameters()
        {
            // Arrange
            using (var cmd = new SqlCommand())
            {
                // Act
                var declareTsql = _sut.Build(cmd);

                // Assert
                Assert.Equal(declareTsql, string.Empty);
            }
        }

        [Fact]
        public void Should_BuildCorrectTSql_When_StringNetTypeAsParameterIsUsed()
        {
            // Arrange
            const string pName = "param0";
            const string pValue = "param0value";
            var expectedTSql = $"DECLARE @{pName} NVARCHAR({pValue.Length}) = '{pValue}';";
            using (var cmd = new SqlCommand())
            {
                cmd.Parameters.Add(new SqlParameter(pName, pValue));

                // Act
                var declareTsql = _sut.Build(cmd);

                // Assert
                Assert.Equal(declareTsql, expectedTSql);
            }
        }

        [Fact]
        public void Should_BuildCorrectTSql_When_DecimalNetTypeAsParameterIsUsed()
        {
            // Arrange
            const string pName = "param0";
            const decimal pValue = 1.5M;
            var expectedTSql = $"DECLARE @{pName} DECIMAL = {pValue.ToString(CultureInfo.InvariantCulture)};";
            using (var cmd = new SqlCommand())
            {
                cmd.Parameters.Add(new SqlParameter(pName, pValue));

                // Act
                var declareTsql = _sut.Build(cmd);

                // Assert
                Assert.Equal(declareTsql, expectedTSql);
            }
        }

        [Fact]
        public void Should_BuildCorrectTSql_When_IntNetTypeAsParameterIsUsed()
        {
            // Arrange
            const string pName = "param0";
            const int pValue = 1;
            var expectedTSql = $"DECLARE @{pName} INT = {pValue.ToString(CultureInfo.InvariantCulture)};";
            using (var cmd = new SqlCommand())
            {
                cmd.Parameters.Add(new SqlParameter(pName, pValue));

                // Act
                var declareTsql = _sut.Build(cmd);

                // Assert
                Assert.Equal(declareTsql, expectedTSql);
            }
        }

        [Fact]
        public void Should_BuildCorrectTSql_When_DateTimeNetTypeAsParameterIsUsed()
        {
            // Arrange
            const string pName = "param0";
            var pValue = DateTime.Now;
            var expectedTSql = $"DECLARE @{pName} DATETIME = '{pValue.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture)}';";
            using (var cmd = new SqlCommand())
            {
                cmd.Parameters.Add(new SqlParameter(pName, pValue));

                // Act
                var declareTsql = _sut.Build(cmd);

                // Assert
                Assert.Equal(declareTsql, expectedTSql);
            }
        }

        [Fact]
        public void Should_BuildCorrectTSql_When_BooleanNetTypeAsParameterIsUsed()
        {
            // Arrange
            const string pName = "param0";
            bool pValue = true;
            var expectedTSql = $"DECLARE @{pName} BIT = {Convert.ToInt16(pValue)};";
            using (var cmd = new SqlCommand())
            {
                cmd.Parameters.Add(new SqlParameter(pName, pValue));

                // Act
                var declareTsql = _sut.Build(cmd);

                // Assert
                Assert.Equal(declareTsql, expectedTSql);
            }
        }

        [Fact]
        public void Should_BuildEachDeclarationAtNewLine_When_MoreThenOneParameterIsDefined()
        {
            // Arrange
            const string pName0 = "param0";
            bool pValue0 = true;
            const string pName1 = "param1";
            bool pValue1 = false;
            var expectedTSql = $"DECLARE @{pName0} BIT = {Convert.ToInt16(pValue0)};" +
                               Environment.NewLine +
                               $"DECLARE @{pName1} BIT = {Convert.ToInt16(pValue1)};";
            using (var cmd = new SqlCommand())
            {
                cmd.Parameters.Add(new SqlParameter(pName0, pValue0));
                cmd.Parameters.Add(new SqlParameter(pName1, pValue1));

                // Act
                var declareTsql = _sut.Build(cmd);

                // Assert
                Assert.Equal(declareTsql, expectedTSql);
            }
        }
    }
}
