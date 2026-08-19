using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace Infrastructure.Reports.Charts;

/// <summary>
/// Adds native Excel charts to an already-saved ClosedXML workbook by reopening the package with
/// the OpenXML SDK, since ClosedXML itself has no chart-authoring API (only chart-sheet passthrough
/// and sparklines). Series data is embedded as literal values rather than cell-reference formulas,
/// so the charts render correctly independent of exact column/row placement in the ClosedXML sheet.
/// </summary>
[SuppressMessage("SonarLint", "S3220:Method calls should not resolve ambiguously to overloads with params", Justification = "OpenXml Append/constructor params overload is intentional")]
internal static class ExcelChartBuilder
{
    private const uint BarCategoryAxisId = 111111111U;
    private const uint BarValueAxisId = 222222222U;
    private const uint RadarCategoryAxisId = 333333333U;
    private const uint RadarValueAxisId = 444444444U;

    public static byte[] AddSatisfactionCharts(
        byte[] workbookBytes,
        string sheetName,
        IReadOnlyList<string> categories,
        IReadOnlyList<decimal> values)
    {
        if (values.Count == 0)
        {
            return workbookBytes;
        }

        using var stream = new MemoryStream();
        stream.Write(workbookBytes, 0, workbookBytes.Length);
        stream.Position = 0;

        using (var document = SpreadsheetDocument.Open(stream, true))
        {
            WorksheetPart worksheetPart = FindWorksheetPart(document, sheetName);

            DrawingsPart drawingsPart = worksheetPart.AddNewPart<DrawingsPart>();

            ChartPart barChartPart = drawingsPart.AddNewPart<ChartPart>();
            WriteChartPart(barChartPart, categories, values, isRadar: false);

            ChartPart radarChartPart = drawingsPart.AddNewPart<ChartPart>();
            WriteChartPart(radarChartPart, categories, values, isRadar: true);

            var worksheetDrawing = new Xdr.WorksheetDrawing(
                BuildAnchor(drawingsPart.GetIdOfPart(barChartPart), 1, "SatisfactionBarChart", 8, 1, 17, 20),
                BuildAnchor(drawingsPart.GetIdOfPart(radarChartPart), 2, "SatisfactionRadarChart", 18, 1, 27, 20));

            drawingsPart.WorksheetDrawing = worksheetDrawing;

            // CT_Worksheet requires <drawing> before <tableParts>/<extLst>; ClosedXML always emits
            // tableParts (even empty), so a plain Append would put <drawing> after it and produce a
            // file Excel flags for repair.
            var drawing = new Drawing { Id = worksheetPart.GetIdOfPart(drawingsPart) };
            OpenXmlElement? tailAnchor = worksheetPart.Worksheet.ChildElements
                .FirstOrDefault(e => e.LocalName is "tableParts" or "extLst");

            if (tailAnchor is not null)
            {
                worksheetPart.Worksheet.InsertBefore(drawing, tailAnchor);
            }
            else
            {
                worksheetPart.Worksheet.Append(drawing);
            }

            worksheetPart.Worksheet.Save();
        }

        return stream.ToArray();
    }

    private static WorksheetPart FindWorksheetPart(SpreadsheetDocument document, string sheetName)
    {
        WorkbookPart workbookPart = document.WorkbookPart!;
        Sheet sheet = workbookPart.Workbook.Sheets!.Elements<Sheet>().First(s => s.Name == sheetName);
        return (WorksheetPart)workbookPart.GetPartById(sheet.Id!.Value!);
    }

    private static void WriteChartPart(
        ChartPart chartPart,
        IReadOnlyList<string> categories,
        IReadOnlyList<decimal> values,
        bool isRadar)
    {
        C.PlotArea plotArea = isRadar
            ? BuildRadarPlotArea(categories, values)
            : BuildBarPlotArea(categories, values);

        var chart = new C.Chart(
            new C.AutoTitleDeleted { Val = true },
            plotArea,
            new C.Legend(new C.LegendPosition { Val = C.LegendPositionValues.Bottom }),
            new C.PlotVisibleOnly { Val = true });

        chartPart.ChartSpace = new C.ChartSpace(
            new C.EditingLanguage { Val = "ru-RU" },
            chart);
    }

    private static C.PlotArea BuildBarPlotArea(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        var barChart = new C.BarChart(
            new C.BarDirection { Val = C.BarDirectionValues.Column },
            new C.BarGrouping { Val = C.BarGroupingValues.Clustered },
            BuildBarSeries(categories, values),
            new C.AxisId { Val = BarCategoryAxisId },
            new C.AxisId { Val = BarValueAxisId });

        return new C.PlotArea(
            new C.Layout(),
            barChart,
            BuildCategoryAxis(BarCategoryAxisId, BarValueAxisId),
            BuildValueAxis(BarValueAxisId, BarCategoryAxisId));
    }

    private static C.PlotArea BuildRadarPlotArea(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        var radarChart = new C.RadarChart(
            new C.RadarStyle { Val = C.RadarStyleValues.Marker },
            BuildRadarSeries(categories, values),
            new C.AxisId { Val = RadarCategoryAxisId },
            new C.AxisId { Val = RadarValueAxisId });

        return new C.PlotArea(
            new C.Layout(),
            radarChart,
            BuildCategoryAxis(RadarCategoryAxisId, RadarValueAxisId),
            BuildValueAxis(RadarValueAxisId, RadarCategoryAxisId));
    }

    private static C.CategoryAxis BuildCategoryAxis(uint axisId, uint crossingAxisId)
    {
        return new C.CategoryAxis(
            new C.AxisId { Val = axisId },
            new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
            new C.Delete { Val = false },
            new C.AxisPosition { Val = C.AxisPositionValues.Bottom },
            new C.CrossingAxis { Val = crossingAxisId });
    }

    private static C.ValueAxis BuildValueAxis(uint axisId, uint crossingAxisId)
    {
        return new C.ValueAxis(
            new C.AxisId { Val = axisId },
            new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
            new C.Delete { Val = false },
            new C.AxisPosition { Val = C.AxisPositionValues.Left },
            new C.CrossingAxis { Val = crossingAxisId });
    }

    private static C.BarChartSeries BuildBarSeries(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        return new C.BarChartSeries(
            new C.Index { Val = 0U },
            new C.Order { Val = 0U },
            BuildCategoryAxisData(categories),
            BuildValues(values));
    }

    private static C.RadarChartSeries BuildRadarSeries(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        return new C.RadarChartSeries(
            new C.Index { Val = 0U },
            new C.Order { Val = 0U },
            BuildCategoryAxisData(categories),
            BuildValues(values));
    }

    private static C.CategoryAxisData BuildCategoryAxisData(IReadOnlyList<string> categories)
    {
        var literal = new C.StringLiteral(new C.PointCount { Val = (uint)categories.Count });
        for (int i = 0; i < categories.Count; i++)
        {
            literal.Append(new C.StringPoint(new C.NumericValue(categories[i])) { Index = (uint)i });
        }

        return new C.CategoryAxisData(literal);
    }

    private static C.Values BuildValues(IReadOnlyList<decimal> values)
    {
        var literal = new C.NumberLiteral(new C.FormatCode("General"), new C.PointCount { Val = (uint)values.Count });
        for (int i = 0; i < values.Count; i++)
        {
            literal.Append(new C.NumericPoint(new C.NumericValue(values[i].ToString(CultureInfo.InvariantCulture))) { Index = (uint)i });
        }

        return new C.Values(literal);
    }

    private static Xdr.TwoCellAnchor BuildAnchor(
        string chartRelationshipId,
        uint frameId,
        string frameName,
        int fromColumn,
        int fromRow,
        int toColumn,
        int toRow)
    {
        return new Xdr.TwoCellAnchor(
            new Xdr.FromMarker(
                new Xdr.ColumnId(fromColumn.ToString(CultureInfo.InvariantCulture)),
                new Xdr.ColumnOffset("0"),
                new Xdr.RowId(fromRow.ToString(CultureInfo.InvariantCulture)),
                new Xdr.RowOffset("0")),
            new Xdr.ToMarker(
                new Xdr.ColumnId(toColumn.ToString(CultureInfo.InvariantCulture)),
                new Xdr.ColumnOffset("0"),
                new Xdr.RowId(toRow.ToString(CultureInfo.InvariantCulture)),
                new Xdr.RowOffset("0")),
            new Xdr.GraphicFrame(
                new Xdr.NonVisualGraphicFrameProperties(
                    new Xdr.NonVisualDrawingProperties { Id = frameId, Name = frameName },
                    new Xdr.NonVisualGraphicFrameDrawingProperties()),
                new Xdr.Transform(
                    new A.Offset { X = 0, Y = 0 },
                    new A.Extents { Cx = 0, Cy = 0 }),
                new A.Graphic(
                    new A.GraphicData(
                        new C.ChartReference { Id = chartRelationshipId })
                    { Uri = "http://schemas.openxmlformats.org/drawingml/2006/chart" }))
            { Macro = string.Empty },
            new Xdr.ClientData());
    }
}
