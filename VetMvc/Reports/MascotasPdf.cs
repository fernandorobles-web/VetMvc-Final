using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace VetMvc.Reports;

public class MascotasPdf : IDocument
{
    public record Row(string Nombre, string Especie, string Dueno, string RunDueno, string Sexo, DateTime? FechaNac);

    private readonly IEnumerable<Row> _data;
    private readonly string _title;
    public MascotasPdf(IEnumerable<Row> data, string title) { _data = data; _title = title; }

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
                c.RelativeColumn(2);  // Nombre
                c.RelativeColumn(2);  // Especie
                c.RelativeColumn(3);  // Dueño
                c.ConstantColumn(90); // RUN
                c.ConstantColumn(60); // Sexo
                c.ConstantColumn(90); // Fecha Nac
            });

            static IContainer H(IContainer x) => x.Background(Colors.Grey.Lighten3).Padding(4).BorderBottom(1);
            static IContainer C(IContainer x) => x.PaddingVertical(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

            t.Header(h =>
            {
                h.Cell().Element(H).Text("Nombre");
                h.Cell().Element(H).Text("Especie");
                h.Cell().Element(H).Text("Dueño");
                h.Cell().Element(H).Text("RUN Dueño");
                h.Cell().Element(H).Text("Sexo");
                h.Cell().Element(H).Text("Fecha Nac.");
            });

            foreach (var r in _data)
            {
                t.Cell().Element(C).Text(r.Nombre);
                t.Cell().Element(C).Text(r.Especie);
                t.Cell().Element(C).Text(r.Dueno);
                t.Cell().Element(C).Text(r.RunDueno);
                t.Cell().Element(C).Text(r.Sexo);
                t.Cell().Element(C).Text(r.FechaNac?.ToString("dd-MM-yyyy") ?? "");
            }
        });
    }
}
