using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace VetMvc.Reports;

public class DuenosPdf : IDocument
{
    public record Row(string Run, string Nombres, string Apellidos, string Email, string Telefono);

    private readonly IEnumerable<Row> _data;
    private readonly string _title;

    public DuenosPdf(IEnumerable<Row> data, string title)
    {
        _data = data;
        _title = title;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer doc)
    {
        doc.Page(page =>
        {
            page.Margin(30);
            page.Header().Text(_title).SemiBold().FontSize(16).FontColor(Colors.Black);
            page.Content().Element(RenderTable);
            page.Footer().AlignRight().Text($"Generado: {DateTime.Now:dd-MM-yyyy HH:mm}").FontSize(9);
        });
    }

    void RenderTable(IContainer c)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(90);    // RUN
                cols.RelativeColumn(2);     // Nombres
                cols.RelativeColumn(2);     // Apellidos
                cols.RelativeColumn(3);     // Email
                cols.ConstantColumn(90);    // Teléfono
            });

            static IContainer Header(IContainer x) =>
                x.Background(Colors.Grey.Lighten3).Padding(4).BorderBottom(1);

            static IContainer Cell(IContainer x) =>
                x.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

            table.Header(h =>
            {
                h.Cell().Element(Header).Text("RUN");
                h.Cell().Element(Header).Text("Nombres");
                h.Cell().Element(Header).Text("Apellidos");
                h.Cell().Element(Header).Text("Email");
                h.Cell().Element(Header).Text("Teléfono");
            });

            foreach (var r in _data)
            {
                table.Cell().Element(Cell).Text(r.Run);
                table.Cell().Element(Cell).Text(r.Nombres);
                table.Cell().Element(Cell).Text(r.Apellidos);
                table.Cell().Element(Cell).Text(r.Email);
                table.Cell().Element(Cell).Text(r.Telefono);
            }
        });
    }
}
