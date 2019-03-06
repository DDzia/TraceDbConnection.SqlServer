using System;
using System.Data.Common;
using System.Data.SqlClient;
using Moq;
using TraceDbConnection.SqlServer;
using TraceDbConnection.TraceReporting.Command;
using Xunit;

namespace TraceDbConnectionSqlServerTests
{
    public class ReceiverTests
    {
        private class ReceiverUnderClass : SqlServerTraceReceiver
        {
            protected override void Save(string tsql, DbCommand command, ICommandTraceEntry traceEntry) {}
        }

        private static readonly SqlServerTraceReceiver _sut = new ReceiverUnderClass();

        [Fact]
        public void Should_ThrowInvOpEx_When_IsNoSqlCommandIsUsed()
        {
            // Arrange
            var cmdMock = new Mock<DbCommand>();
            var tEntryMock = new Mock<ICommandTraceEntry>();

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => _sut.Save(cmdMock.Object, tEntryMock.Object));

            // Assert
            Assert.Equal("Command is no SqlCommand.", ex.Message);
        }

        [Fact]
        public void Should_ThrowNullArgEx_When_NullInsteadCmdIsUsed()
        {
            // Arrange
            var tEntryMock = new Mock<ICommandTraceEntry>();

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => _sut.Save(null, tEntryMock.Object));

            // Assert
            Assert.Equal($"Value cannot be null.{Environment.NewLine}Parameter name: command", ex.Message);
        }

        [Fact]
        public void Should_ThrowNullArgEx_When_NullInsteadTraceEntryIsUsed()
        {
            // Arrange
            using (var cmd = new SqlCommand())
            {
                // Act
                var ex = Assert.Throws<ArgumentNullException>(() => _sut.Save(cmd, null));

                // Assert
                Assert.Equal($"Value cannot be null.{Environment.NewLine}Parameter name: traceEntry", ex.Message);
            }
        }
    }
}
