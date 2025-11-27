using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using VetMvc.Models;

namespace VetMvc.Reports;

public class UsuariosPdf : IDocument
{
    private readonly List<Usuario> _data;
    private readonly string _titulo;

    public UsuariosPdf(IEnumerable<Usuario> usuarios, string titulo = "Listado de Usuarios")
    {
        _data = usuarios.ToList();
        _titulo = titulo;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Row(row =>
            {
                row.RelativeItem().Text(_titulo).SemiBold().FontSize(16);
                row.ConstantItem(120).AlignRight().Text(DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
            });

            page.Content().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);  // #
                    columns.RelativeColumn(2);   // Nombre
                    columns.RelativeColumn(1.4f);// Usuario
                    columns.RelativeColumn(2);   // Email
                    columns.RelativeColumn(1);   // Rol
                    columns.ConstantColumn(60);  // Activo
                    columns.RelativeColumn(1.5f);// Fecha
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(CellHeader).Text("#");
                    header.Cell().Element(CellHeader).Text("Nombre");
                    header.Cell().Element(CellHeader).Text("Usuario");
                    header.Cell().Element(CellHeader).Text("Email");
                    header.Cell().Element(CellHeader).Text("Rol");
                    header.Cell().Element(CellHeader).Text("Activo");
                    header.Cell().Element(CellHeader).Text("Registro");
                    static IContainer CellHeader(IContainer c) =>
                        c.Background(Colors.Grey.Lighten3).Padding(4).Border(1).BorderColor(Colors.Grey.Medium).DefaultTextStyle(x => x.SemiBold());
                });

                // Rows
                var i = 1;
                foreach (var u in _data)
                {
                    table.Cell().Element(Cell).Text(i++);
                    table.Cell().Element(Cell).Text(u.NombreCompleto);
                    table.Cell().Element(Cell).Text(u.NombreUsuario);
                    table.Cell().Element(Cell).Text(u.Email);
                    table.Cell().Element(Cell).Text(u.Rol);
                    table.Cell().Element(Cell).Text(u.Activo ? "Sí" : "No");
                    table.Cell().Element(Cell).Text(u.FechaCreacion.ToString("dd-MM-yyyy HH:mm"));
                }

                static IContainer Cell(IContainer c) =>
                    c.Padding(4).Border(1).BorderColor(Colors.Grey.Medium);
            });

            page.Footer().AlignRight().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" / ");
                x.TotalPages();
            });
        });
    }
}
