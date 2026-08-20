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
        IReadOnlyList<decimal> values,
        int fromRow)
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

            int toRow = fromRow + 19;

            // Both charts sit below the data table (which starts at column A), side by side with
            // each other, rather than to the right of the table as before.
            var worksheetDrawing = new Xdr.WorksheetDrawing(
                BuildAnchor(drawingsPart.GetIdOfPart(barChartPart), 1, "SatisfactionBarChart", 0, fromRow, 9, toRow),
                BuildAnchor(drawingsPart.GetIdOfPart(radarChartPart), 2, "SatisfactionRadarChart", 11, fromRow, 20, toRow));

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
        C.Chart chart = isRadar
            ? BuildRadarChart(categories, values)
            : BuildBar3DChart(categories, values);

        chartPart.ChartSpace = new C.ChartSpace(
            new C.EditingLanguage { Val = "ru-RU" },
            chart);
    }

    // Mirrors the college-specs reference workbook's per-discipline bar chart exactly (see
    // xl/charts/chart3.xml in обработка_удовл_обучающихся...xlsx): a 3-D clustered column chart
    // with value data labels on the bars, no gridlines/legend. The flat 2-D column chart this
    // replaced didn't match what the manual spreadsheet actually produced.
    private static C.Chart BuildBar3DChart(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        var bar3DChart = new C.Bar3DChart(
            new C.BarDirection { Val = C.BarDirectionValues.Column },
            new C.BarGrouping { Val = C.BarGroupingValues.Clustered },
            new C.VaryColors { Val = false },
            BuildBarSeries(categories, values),
            new C.Shape { Val = C.ShapeValues.Box },
            new C.AxisId { Val = BarCategoryAxisId },
            new C.AxisId { Val = BarValueAxisId },
            new C.AxisId { Val = 0U });

        var plotArea = new C.PlotArea(
            new C.Layout(),
            bar3DChart,
            BuildCategoryAxis(BarCategoryAxisId, BarValueAxisId, withGridlines: false, tickMark: C.TickMarkValues.None),
            BuildValueAxis(BarValueAxisId, BarCategoryAxisId, withGridlines: false, tickMark: C.TickMarkValues.None));

        return new C.Chart(
            new C.AutoTitleDeleted { Val = true },
            new C.View3D(
                new C.RotateX { Val = (sbyte)15 },
                new C.RotateY { Val = (ushort)20 },
                new C.RightAngleAxes { Val = true }),
            new C.Floor(new C.Thickness { Val = (byte)0 }),
            new C.SideWall(new C.Thickness { Val = (byte)0 }),
            new C.BackWall(new C.Thickness { Val = (byte)0 }),
            plotArea,
            new C.PlotVisibleOnly { Val = true },
            new C.DisplayBlanksAs { Val = C.DisplayBlanksAsValues.Gap });
    }

    // Mirrors the reference workbook's radar chart (xl/charts/chart2.xml): a marker-style radar
    // with no data labels, gridlines on both axes. The third chart present in the original file
    // per sheet (another radar series) is a dangling #REF! left over from editing the source
    // spreadsheet by hand - a genuine authoring artifact, not something worth reproducing here.
    private static C.Chart BuildRadarChart(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        var radarChart = new C.RadarChart(
            new C.RadarStyle { Val = C.RadarStyleValues.Marker },
            new C.VaryColors { Val = false },
            BuildRadarSeries(categories, values),
            new C.AxisId { Val = RadarCategoryAxisId },
            new C.AxisId { Val = RadarValueAxisId });

        var plotArea = new C.PlotArea(
            new C.Layout(),
            radarChart,
            BuildCategoryAxis(RadarCategoryAxisId, RadarValueAxisId, withGridlines: true, tickMark: C.TickMarkValues.Outside),
            BuildValueAxis(RadarValueAxisId, RadarCategoryAxisId, withGridlines: true, tickMark: C.TickMarkValues.Cross));

        return new C.Chart(
            new C.AutoTitleDeleted { Val = true },
            plotArea,
            new C.PlotVisibleOnly { Val = true },
            new C.DisplayBlanksAs { Val = C.DisplayBlanksAsValues.Gap });
    }

    private static C.CategoryAxis BuildCategoryAxis(uint axisId, uint crossingAxisId, bool withGridlines, C.TickMarkValues tickMark)
    {
        var axis = new C.CategoryAxis(
            new C.AxisId { Val = axisId },
            new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
            new C.Delete { Val = false },
            new C.AxisPosition { Val = C.AxisPositionValues.Bottom });

        if (withGridlines)
        {
            axis.Append(new C.MajorGridlines());
        }

        axis.Append(new C.MajorTickMark { Val = tickMark });
        axis.Append(new C.CrossingAxis { Val = crossingAxisId });

        return axis;
    }

    private static C.ValueAxis BuildValueAxis(uint axisId, uint crossingAxisId, bool withGridlines, C.TickMarkValues tickMark)
    {
        var axis = new C.ValueAxis(
            new C.AxisId { Val = axisId },
            new C.Scaling(new C.Orientation { Val = C.OrientationValues.MinMax }),
            new C.Delete { Val = false },
            new C.AxisPosition { Val = C.AxisPositionValues.Left });

        if (withGridlines)
        {
            axis.Append(new C.MajorGridlines());
        }

        axis.Append(new C.NumberingFormat { FormatCode = "0.0", SourceLinked = false });
        axis.Append(new C.MajorTickMark { Val = tickMark });
        axis.Append(new C.CrossingAxis { Val = crossingAxisId });

        return axis;
    }

    private static C.BarChartSeries BuildBarSeries(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        return new C.BarChartSeries(
            new C.Index { Val = 0U },
            new C.Order { Val = 0U },
            BuildCategoryAxisData(categories),
            BuildValues(values),
            BuildValueDataLabels());
    }

    private static C.RadarChartSeries BuildRadarSeries(IReadOnlyList<string> categories, IReadOnlyList<decimal> values)
    {
        return new C.RadarChartSeries(
            new C.Index { Val = 0U },
            new C.Order { Val = 0U },
            BuildCategoryAxisData(categories),
            BuildValues(values),
            new C.Marker(new C.Symbol { Val = C.MarkerStyleValues.None }));
    }

    private static C.DataLabels BuildValueDataLabels()
    {
        return new C.DataLabels(
            new C.ShowLegendKey { Val = false },
            new C.ShowValue { Val = true },
            new C.ShowCategoryName { Val = false },
            new C.ShowSeriesName { Val = false },
            new C.ShowPercent { Val = false },
            new C.ShowBubbleSize { Val = false });
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
