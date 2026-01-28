using System.Globalization;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Import;

internal sealed class ImportStudentsCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher)
    : ICommandHandler<ImportStudentsCommand, int>
{
    public async Task<Result<int>> Handle(ImportStudentsCommand command, CancellationToken cancellationToken)
    {
        int count = 0;
        string defaultPasswordHash = passwordHasher.Hash("12345678");

        try
        {
            using var document = SpreadsheetDocument.Open(command.FileStream, false);
            WorkbookPart? wbPart = document.WorkbookPart;
            if (wbPart == null)
            {
                return Result.Failure<int>(Error.Failure("Excel.Error", "Invalid file"));
            }

            Sheet? sheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault();
            if (sheet == null)
            {
                return Result.Failure<int>(Error.Failure("Excel.Error", "No sheets found"));
            }

            if (wbPart.GetPartById(sheet.Id!) is not WorksheetPart wsPart)
            {
                return Result.Failure<int>(Error.Failure("Excel.Error", "Worksheet part not found"));
            }

            SheetData sheetData = wsPart.Worksheet.Elements<SheetData>().First();

            foreach (Row? r in sheetData.Elements<Row>().Skip(1))
            {
                var cells = r.Elements<Cell>().ToList();

                if (cells.Count < 1)
                {
                    continue;
                }

                string groupNameStr = GetCellValue(document, cells[0]);
                if (string.IsNullOrWhiteSpace(groupNameStr))
                {
                    continue;
                }

                Result<GroupName> groupResult = GroupName.Create(groupNameStr);
                if (groupResult.IsFailure)
                {
                    continue;
                }

                bool exists = await context.Users.AnyAsync(u => u.Login.Value == groupResult.Value.Value, cancellationToken);
                if (exists)
                {
                    continue;
                }

                Result<User> userResult = User.CreateGroupUser(
                                    groupResult.Value,
                                    Guid.NewGuid(),
                                    defaultPasswordHash);

                if (userResult.IsSuccess)
                {
                    context.Users.Add(userResult.Value);
                    count++;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            return count;
        }
        catch (Exception ex)
        {
            return Result.Failure<int>(Error.Failure("Import.Failed", ex.Message));
        }
    }

    private static string GetCellValue(SpreadsheetDocument doc, Cell cell)
    {
        string value = cell.CellValue?.InnerText ?? string.Empty;

        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
        {
            return doc.WorkbookPart!.SharedStringTablePart!.SharedStringTable
                .Elements<SharedStringItem>().ElementAt(id).InnerText;
        }

        return value;
    }
}
