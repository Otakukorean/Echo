using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Shared.Contracts.Pdf;

namespace Shared.Pdf;

public class QuestPdfService : IPdfService
{
    public byte[] Generate(Action<IPdfBuilder> buildAction)
    {
        var builder = new QuestPdfBuilder();
        buildAction(builder);
        return builder.Build();
    }
}

public class QuestPdfBuilder : IPdfBuilder
{
    private readonly List<Action<ColumnDescriptor>> _contentActions = new();
    private string? _footerText;

    public void AddTitle(string title)
    {
        _contentActions.Add(col =>
            col.Item().Text(title).FontSize(24).Bold());
    }

    public void AddSubtitle(string subtitle)
    {
        _contentActions.Add(col =>
            col.Item().Text(subtitle).FontSize(12).FontColor(Colors.Grey.Medium));
    }

    public void AddText(string text)
    {
        _contentActions.Add(col =>
            col.Item().Text(text).FontSize(11));
    }

    public void AddSpacing(float points = 10)
    {
        _contentActions.Add(col =>
            col.Item().PaddingBottom(points));
    }

    public void AddTable(string[] headers, List<string[]> rows)
    {
        _contentActions.Add(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    foreach (var _ in headers)
                        columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    foreach (var h in headers)
                    {
                        header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten1)
                            .Padding(5).Text(h).Bold().FontSize(10);
                    }
                });

                foreach (var row in rows)
                {
                    foreach (var cell in row)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                            .Padding(5).Text(cell).FontSize(10);
                    }
                }
            });
        });
    }

    public void AddTotalLine(string label, string value)
    {
        _contentActions.Add(col =>
            col.Item().PaddingTop(10).AlignRight()
                .Text($"{label}: {value}").FontSize(14).Bold());
    }

    public void AddFooter(string text)
    {
        _footerText = text;
    }

    public byte[] Build()
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Content().Column(column =>
                {
                    foreach (var action in _contentActions)
                        action(column);
                });

                if (_footerText is not null)
                {
                    page.Footer().AlignCenter().Text(_footerText).FontSize(9).FontColor(Colors.Grey.Medium);
                }
            });
        }).GeneratePdf();
    }
}
