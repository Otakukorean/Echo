namespace Shared.Contracts.Pdf;

public interface IPdfService
{
    byte[] Generate(Action<IPdfBuilder> buildAction);
}

public interface IPdfBuilder
{
    void AddTitle(string title);
    void AddSubtitle(string subtitle);
    void AddText(string text);
    void AddSpacing(float points = 10);
    void AddTable(string[] headers, List<string[]> rows);
    void AddTotalLine(string label, string value);
    void AddFooter(string text);
}
