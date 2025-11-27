using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetMvc.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using VetMvc.Reports;


namespace VetMvc.Controllers
{
    [Authorize]
    public class DuenosController : Controller
    {
        private readonly VetDbContext _context;
        private readonly ILogger<DuenosController> _logger;

        public DuenosController(VetDbContext context, ILogger<DuenosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Duenos
        public async Task<IActionResult> Index()
        {
            try
            {
                var duenos = await _context.Duenos
                    .AsNoTracking()
                    .OrderBy(d => d.Apellidos).ThenBy(d => d.Nombres)
                    .ToListAsync();

                _logger.LogInformation("Se cargaron {Count} dueños", duenos.Count);
                return View(duenos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la lista de dueños");
                TempData["Error"] = "Error al cargar la lista de dueños.";
                return View(new List<Dueno>());
            }
        }

        // GET: /Duenos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest("ID no proporcionado");

            try
            {
                var dueno = await _context.Duenos
                    .Include(d => d.Mascota)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dueno == null)
                {
                    _logger.LogWarning("Dueño con ID {Id} no encontrado", id);
                    return NotFound();
                }

                return View(dueno);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dueño ID {Id}", id);
                TempData["Error"] = "Error al cargar los detalles del dueño.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Duenos/Create
        public IActionResult Create() => View(new Dueno());

        // POST: /Duenos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Dueno dueno)
        {
            if (!ModelState.IsValid) return View(dueno);

            try
            {
                // RUT único
                bool rutExiste = await _context.Duenos.AnyAsync(d => d.Rut == dueno.Rut);
                if (rutExiste)
                {
                    ModelState.AddModelError("Rut", "Ya existe un dueño con este RUT.");
                    return View(dueno);
                }

                // Email único (si viene)
                if (!string.IsNullOrWhiteSpace(dueno.Email))
                {
                    bool emailExiste = await _context.Duenos.AnyAsync(d => d.Email == dueno.Email);
                    if (emailExiste)
                    {
                        ModelState.AddModelError("Email", "Ya existe un dueño con este email.");
                        return View(dueno);
                    }
                }

                _context.Add(dueno);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Dueño creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear dueño {Rut}", dueno.Rut);
                ModelState.AddModelError("", "Ocurrió un error al guardar. Intente nuevamente.");
                return View(dueno);
            }
        }

        // GET: /Duenos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest("ID no proporcionado");
            var dueno = await _context.Duenos.FindAsync(id);
            if (dueno == null) return NotFound();
            return View(dueno);
        }

        // POST: /Duenos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Dueno dueno)
        {
            if (id != dueno.Id) return BadRequest("ID inconsistente");
            if (!ModelState.IsValid) return View(dueno);

            try
            {
                // RUT único (excluyéndose)
                bool rutExiste = await _context.Duenos.AnyAsync(d => d.Rut == dueno.Rut && d.Id != dueno.Id);
                if (rutExiste)
                {
                    ModelState.AddModelError("Rut", "Ya existe un dueño con este RUT.");
                    return View(dueno);
                }

                if (!string.IsNullOrWhiteSpace(dueno.Email))
                {
                    bool emailExiste = await _context.Duenos.AnyAsync(d => d.Email == dueno.Email && d.Id != dueno.Id);
                    if (emailExiste)
                    {
                        ModelState.AddModelError("Email", "Ya existe un dueño con este email.");
                        return View(dueno);
                    }
                }

                _context.Update(dueno);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Dueño actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await _context.Duenos.AnyAsync(d => d.Id == dueno.Id))
                    return NotFound();
                _logger.LogError(ex, "Concurrencia al editar dueño ID {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar dueño ID {Id}", id);
                ModelState.AddModelError("", "No se pudo guardar los cambios.");
                return View(dueno);
            }
        }

        // GET: /Duenos/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest("ID no proporcionado");
            var dueno = await _context.Duenos.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            if (dueno == null) return NotFound();
            return View(dueno);
        }

        // POST: /Duenos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dueno = await _context.Duenos.FindAsync(id);
            if (dueno == null) return NotFound();

            try
            {
                _context.Duenos.Remove(dueno);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Dueño eliminado.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Bloqueo al borrar dueño ID {Id}. Probables mascotas asociadas.", id);
                TempData["Error"] = "No se puede eliminar: tiene mascotas asociadas.";
                return RedirectToAction(nameof(Delete), new { id });
            }

        }
        public async Task<IActionResult> ExportExcel()
        {
            var data = await _context.Duenos
                .AsNoTracking()
                .OrderBy(d => d.Apellidos).ThenBy(d => d.Nombres)
                .Select(d => new
                {
                    Run = d.Rut,
                    Nombres = d.Nombres,
                    Apellidos = d.Apellidos,
                    Email = d.Email,
                    Telefono = d.Telefono
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Dueños");

            ws.Cell(1, 1).Value = "RUN";
            ws.Cell(1, 2).Value = "Nombres";
            ws.Cell(1, 3).Value = "Apellidos";
            ws.Cell(1, 4).Value = "Email";
            ws.Cell(1, 5).Value = "Teléfono";
            ws.Range(1, 1, 1, 5).Style.Font.Bold = true;

            var row = 2;
            foreach (var d in data)
            {
                ws.Cell(row, 1).Value = d.Run;
                ws.Cell(row, 2).Value = d.Nombres;
                ws.Cell(row, 3).Value = d.Apellidos;
                ws.Cell(row, 4).Value = d.Email;
                ws.Cell(row, 5).Value = d.Telefono;
                row++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"duenos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        public async Task<IActionResult> ExportPdf()
        {
            var rows = await _context.Duenos
                .AsNoTracking()
                .OrderBy(d => d.Apellidos).ThenBy(d => d.Nombres)
                .Select(d => new DuenosPdf.Row(
                    d.Rut ?? "",
                    d.Nombres ?? "",
                    d.Apellidos ?? "",
                    d.Email ?? "",
                    d.Telefono ?? ""))
                .ToListAsync();

            var doc = new Reports.DuenosPdf(rows, "Dueños - VetCare Clínica");
            var pdf = doc.GeneratePdf();
            return File(pdf, "application/pdf", $"duenos_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }
    }

}
