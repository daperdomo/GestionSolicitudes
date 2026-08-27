using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SB.Solicitudes.Application.Common;

namespace SB.Solicitudes.Infrastructure.Persistence;

internal sealed class SqlSolicitudCodeGenerator(ApplicationDbContext dbContext) : ISolicitudCodeGenerator
{
    public async Task<string> NextAsync(DateTimeOffset currentDate, CancellationToken cancellationToken)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        bool shouldClose = connection.State != ConnectionState.Open;

        if (shouldClose)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR [dbo].[SolicitudCodigoSequence]";
            object? value = await command.ExecuteScalarAsync(cancellationToken);
            long sequence = Convert.ToInt64(value, CultureInfo.InvariantCulture);

            return FormattableString.Invariant($"SOL-{currentDate.UtcDateTime.Year}-{sequence:D4}");
        }
        finally
        {
            if (shouldClose)
            {
                await connection.CloseAsync();
            }
        }
    }
}
