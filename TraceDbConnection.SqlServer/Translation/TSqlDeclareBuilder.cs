using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TraceDbConnection.SqlServer.Translation
{
    public class TSqlDeclareBuilder
    {
        private static readonly Func<object, string> BinaryContentToStringSql =_ => "<binary..content>";

        private static readonly Func<object, string> DateTimeToStringSql =
            x => "'" + ((DateTime)x).ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture) + "'";

        private static readonly Func<object, string> CharsContentToStringSql =
            x => "'" + (x.GetType().IsArray ? new StringBuilder().Append((char[])x).ToString() : (string)x) + "'";

        // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings
        private static readonly Dictionary<SqlDbType, (string, Func<object, string>)> _netToSqlTypes =
            new Dictionary<SqlDbType, (string, Func<object, string>)>()
            {

                {
                    SqlDbType.BigInt,
                    (
                        "BIGINT",
                        x => ((long)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.Binary,
                    (
                        "BINARY",
                        binaryContentToStringSql: BinaryContentToStringSql
                    )
                },
                {
                    SqlDbType.Bit,
                    (
                        "BIT",
                        x => ((bool)x) ? "1" : "0"
                    )
                },
                {
                    SqlDbType.Char,
                    (
                        "CHAR",
                        CharsContentToStringSql
                    )
                },
                {
                    SqlDbType.Date,
                    (
                        "DATE",
                        x => "'" + ((DateTime)x).ToString("MM/dd/yyyy") + "'"
                    )
                },
                {
                    SqlDbType.DateTime,
                    (
                        "DATETIME",
                        DateTimeToStringSql
                    )
                },
                {
                    SqlDbType.DateTime2,
                    (
                        "DATETIME2",
                        DateTimeToStringSql
                    )
                },
                {
                    SqlDbType.DateTimeOffset,
                    (
                        "DATETIMEOFFSET",
                        x => "'" + ((DateTimeOffset)x).UtcDateTime.ToString("yyyy-MM-dd hh:mm:ss")
                                 + (((DateTimeOffset)x).Offset.Ticks > 0 ? "+" : "-")
                                 + ((DateTimeOffset)x).Offset.ToString("hh:mm") + "'"
                    )
                },
                {
                    SqlDbType.Decimal,
                    (
                        "DECIMAL",
                        x => ((decimal)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.Float,
                    (
                        "FLOAT",
                        x => ((double)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.Image,
                    (
                        "IMAGE",
                        binaryContentToStringSql: BinaryContentToStringSql
                    )
                },
                {
                    SqlDbType.Int,
                    (
                        "INT",
                        x => ((int)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.Money,
                    (
                        "MONEY",
                        x => ((decimal)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.NChar,
                    (
                        "NCHAR",
                        CharsContentToStringSql
                    )
                },
                {
                    SqlDbType.NText,
                    (
                        "NTEXT",
                        CharsContentToStringSql
                    )
                },
                {
                    SqlDbType.NVarChar,
                    (
                        "NVARCHAR",
                        CharsContentToStringSql
                    )
                },
                {
                    SqlDbType.Real,
                    (
                        "REAL",
                        x => ((float)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.SmallDateTime,
                    (
                        "SMALLDATETIME",
                        DateTimeToStringSql
                    )
                },
                {
                    SqlDbType.SmallInt,
                    (
                        "SMALLINT",
                        x => ((short)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.SmallMoney,
                    (
                        "SMALLMONEY",
                        x => ((decimal)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.Text,
                    (
                        "TEXT",
                        CharsContentToStringSql
                    )
                },
                {
                    SqlDbType.Time,
                    (
                        "TIME",
                        x => ((TimeSpan)x).ToString("hh:mm:ss", CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.Timestamp,
                    (
                        "TIMESTAMP",
                        binaryContentToStringSql: BinaryContentToStringSql
                    )
                },
                {
                    SqlDbType.TinyInt,
                    (
                        "TINYINT",
                        x => ((byte)x).ToString(CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.UniqueIdentifier,
                    (
                        "UNIQUEIDENTIFIER",
                        x => ((Guid)x).ToString("D", CultureInfo.InvariantCulture)
                    )
                },
                {
                    SqlDbType.VarBinary,
                    (
                        "VARBINARY",
                        binaryContentToStringSql: BinaryContentToStringSql
                    )
                },
                {
                    SqlDbType.VarChar,
                    (
                        "VARCHAR",
                        CharsContentToStringSql
                    )
                },
                {
                    SqlDbType.Xml,
                    (
                        "XML",
                        CharsContentToStringSql
                    )
                }
            };

        private static readonly List<SqlDbType> _sqlTextTypes =
            new List<SqlDbType>()
            {
                SqlDbType.Char,
                SqlDbType.NChar,
                SqlDbType.NText,
                SqlDbType.NVarChar,
                SqlDbType.Text,
                SqlDbType.VarChar
            };

        public string Build(SqlCommand command)
        {
            if(command is null)
                throw new ArgumentNullException(nameof(command));

            var sb = new StringBuilder();

            var parameters = command.Parameters.Cast<SqlParameter>().ToArray();
            var currentIndex = 0;
            foreach (var p in parameters)
            {
                // append new line before each declaration(exception is the case for the first declaration)
                if (currentIndex != 0)
                {
                    sb.AppendLine();
                }

                sb.Append("DECLARE ")
                    .Append("@")
                    .Append(p.ParameterName)
                    .Append(" ")
                    .Append(GetParameterTSqlType(p))
                    .Append(" = ")
                    .Append(GetParameterTSqlValuePresentation(p))
                    .Append(";");

                currentIndex++;
            }

            return sb.ToString();
        }

        private string GetParameterTSqlType(SqlParameter p)
        {
            var (sqlName, castToStringFunc) = _netToSqlTypes[p.SqlDbType];
            if (_sqlTextTypes.Contains(p.SqlDbType))
            {
                return $"{sqlName}({p.Size})";
            }

            return sqlName;
        }

        private string GetParameterTSqlValuePresentation(SqlParameter p)
        {
            var (sqlName, castToStringFunc) = _netToSqlTypes[p.SqlDbType];
            return castToStringFunc(p.Value);
        }
    }
}