using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace VetMvc.Reports;

public class AtencionesPdf : IDocument
{
    public record Row(DateTime Fecha, string Mascota, string Dueno, string RunDueno,
                      string Motivo, string Diagnostico, string Tratamiento, decimal? Costo);

    private readonly IEnumerable<Row> _data;
    private readonly string _title;

    public AtencionesPdf(IEnumerable<Row> data, string title)
    { _data = data; _title = title; }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer doc)
    {
        doc.Page(p =>
        {
            p.Margin(30);
            p.Header().Text(_title).SemiBold().FontSize(16);
            p.Content().Element(Render);
            p.Footer().AlignRight().Text($"Generado: {DateTime.Now:dd-MM-yyyy HH:mm}").FontSize(9);
        });
    }

    void Render(IContainer c)
    {
        c.Table(t =>
        {
            t.ColumnsDefinition(c =>
            {
                c.ConstantColumn(105); // Fecha
                c.RelativeColumn(2);   // Mascota
                c.RelativeColumn(3);   // Dueño
                c.ConstantColumn(90);  // RUN
                c.RelativeColumn(2);   // Motivo
                c.RelativeColumn(2);   // Diagnóstico
                c.RelativeColumn(2);   // Tratamiento
                c.ConstantColumn(70);  // Costo
            });

            static IContainer H(IContainer x) => x.Background(Colors.Grey.Lighten3).Padding(4).BorderBottom(1);
            static IContainer C(IContainer x) => x.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

            t.Header(h =>
            {
                h.Cell().Element(H).Text("Fecha");
                h.Cell().Element(H).Text("Mascota");
                h.Cell().Element(H).Text("Dueño");
                h.Cell().Element(H).Text("RUN Dueño");
                h.Cell().Element(H).Text("Motivo");
                h.Cell().Element(H).Text("Diagnóstico");
                h.Cell().Element(H).Text("Tratamiento");
                h.Cell().Element(H).Text("Costo");
            });

            foreach (var r in _data)
            {
                t.Cell().Element(C).Text(r.Fecha.ToString("dd-MM-yyyy HH:mm"));
                t.Cell().Element(C).Text(r.Mascota);
                t.Cell().Element(C).Text(r.Dueno);
                t.Cell().Element(C).Text(r.RunDueno);
                t.Cell().Element(C).Text(r.Motivo);
                t.Cell().Element(C).Text(r.Diagnostico);
                t.Cell().Element(C).Text(r.Tratamiento);
                t.Cell().Element(C).Text(r.Costo.HasValue ? r.Costo.Value.ToString("$ #,##0") : "");
            }
        });
    }
}
