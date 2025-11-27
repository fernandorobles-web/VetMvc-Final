using ClosedXML.Excel;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Linq;
using VetMvc.Models;
using VetMvc.Reports;



namespace VetMvc.Controllers
{
    [Authorize]
    public class MascotasController : Controller
    {
        private readonly VetDbContext _context;
        private readonly ILogger<MascotasController> _logger;

        public MascotasController(VetDbContext context, ILogger<MascotasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Utilidad: combos de Dueños y Especies
        private async Task PrepararCombosAsync(Mascota? m = null)
        {
            var duenos = await _context.Duenos
                .AsNoTracking()
                .Select(d => new { d.Id, Nombre = d.Nombres + " " + d.Apellidos + " (" + d.Rut + ")" })
                .OrderBy(x => x.Nombre)
                .ToListAsync();

            var especies = await _context.Especies
                .AsNoTracking()
                .OrderBy(e => e.Nombre)
                .ToListAsync();

            ViewData["DuenoId"] = new SelectList(duenos, "Id", "Nombre", m?.DuenoId);
            ViewData["EspecieId"] = new SelectList(especies, "Id", "Nombre", m?.EspecieId);
        }

        // GET: /Mascotas
        public async Task<IActionResult> Index()
        {
            try
            {
                var mascotas = await _context.Mascotas
                    .Include(m => m.Dueno)
                    .Include(m => m.Especie)
                    .AsNoTracking()
                    .OrderBy(m => m.Nombre)
                    .ToListAsync();

                return View(mascotas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar mascotas");
                TempData["Error"] = "Error al cargar la lista de mascotas.";
                return View(new List<Mascota>());
            }
        }

        // GET: /Mascotas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest("ID no proporcionado");

            var mascota = await _context.Mascotas
                .Include(m => m.Dueno)
                .Include(m => m.Especie)
                .Include(m => m.Atenciones)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            return mascota == null ? NotFound() : View(mascota);
        }

        // GET: /Mascotas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var m = await _context.Mascotas.FindAsync(id);
            if (m == null) return NotFound();
            await PrepararCombosAsync(m);
            return View(m);
        }

        // GET: /Mascotas/Create
        public async Task<IActionResult> Create()
        {
            await PrepararCombosAsync();
            return View(new Mascota
            {
                // Si tu propiedad es DateOnly:
                FechaNacimiento = DateOnly.FromDateTime(DateTime.Today).AddYears(-1),
                Sexo = "M",
                PesoKg = 1 // evita rango inválido
            });
        }

        // POST: /Mascotas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("DuenoId,EspecieId,Nombre,Sexo,FechaNacimiento,PesoKg,Esterilizado,Chip,Color,Observaciones")]
            Mascota mascota)
        {
            // Evitar que ModelState exija las navegaciones (Dueno/Especie)
            ModelState.Remove("Dueno");
            ModelState.Remove("Especie");

            // CHIP único si viene
            if (!string.IsNullOrWhiteSpace(mascota.Chip) &&
                await _context.Mascotas.AnyAsync(m => m.Chip == mascota.Chip))
                ModelState.AddModelError("Chip", "Ya existe una mascota con este chip/registro.");

            // Validaciones simples
            if (mascota.DuenoId <= 0) ModelState.AddModelError("DuenoId", "Seleccione un dueño.");
            if (mascota.EspecieId <= 0) ModelState.AddModelError("EspecieId", "Seleccione una especie.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los campos marcados en rojo.";
                await PrepararCombosAsync(mascota);
                return View(mascota);
            }

            _context.Add(mascota);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Mascota '{mascota.Nombre}' creada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Mascotas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
          [Bind("Id,DuenoId,EspecieId,Nombre,Sexo,FechaNacimiento,PesoKg,Esterilizado,Chip,Color,Observaciones")]
          Mascota mascota)
        {
            if (id != mascota.Id) return BadRequest();
            ModelState.Remove("Dueno");
            ModelState.Remove("Especie");

            if (!string.IsNullOrWhiteSpace(mascota.Chip) &&
                await _context.Mascotas.AnyAsync(x => x.Chip == mascota.Chip && x.Id != mascota.Id))
                ModelState.AddModelError("Chip", "Ya existe una mascota con este chip/registro.");
            if (mascota.PesoKg <= 0 || mascota.PesoKg > 120)
                ModelState.AddModelError("PesoKg", "El peso debe estar entre 0,1 y 120 kg.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los campos marcados en rojo.";
                await PrepararCombosAsync(mascota);
                return View(mascota);
            }

            try
            {
                _context.Entry(mascota).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Mascota actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se pudo actualizar en la base de datos.";
                await PrepararCombosAsync(mascota);
                return View(mascota);
            }
        }

        // GET: /Mascotas/Delete/5

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest("ID no proporcionado");

            var mascota = await _context.Mascotas
                .Include(m => m.Dueno).Include(m => m.Especie)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            return mascota == null ? NotFound() : View(mascota);
        }

        // POST: /Mascotas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null) return NotFound();

            try
            {
                _context.Mascotas.Remove(mascota);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Mascota eliminada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "No se puede eliminar mascota ID {Id}. Tiene atenciones.", id);
                TempData["Error"] = "No se puede eliminar: existen atenciones asociadas.";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        public async Task<IActionResult> ExportExcel()
        {
            var data = await _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Especie)
                .Include(m => m.Dueno)
                .OrderBy(m => m.Nombre)
                .Select(m => new
                {
                    Nombre = m.Nombre,
                    Especie = m.Especie != null ? m.Especie.Nombre : "",
                    Dueno = m.Dueno != null ? $"{m.Dueno.Nombres} {m.Dueno.Apellidos}" : "",
                    RunDueno = m.Dueno != null ? m.Dueno.Rut : "",
                    Sexo = m.Sexo,                         // si no existe, quita esta línea
                    FechaNac = m.FechaNacimiento          // si no existe, quita esta línea
                })
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Mascotas");

            ws.Cell(1, 1).Value = "Nombre";
            ws.Cell(1, 2).Value = "Especie";
            ws.Cell(1, 3).Value = "Dueño";
            ws.Cell(1, 4).Value = "RUN Dueño";
            ws.Cell(1, 5).Value = "Sexo";
            ws.Cell(1, 6).Value = "Fecha Nac.";
            ws.Range(1, 1, 1, 6).Style.Font.Bold = true;

            var row = 2;
            foreach (var m in data)
            {
                ws.Cell(row, 1).Value = m.Nombre;
                ws.Cell(row, 2).Value = m.Especie;
                ws.Cell(row, 3).Value = m.Dueno;
                ws.Cell(row, 4).Value = m.RunDueno;
                ws.Cell(row, 5).Value = m.Sexo ?? "";
                ws.Cell(row, 6).Style.DateFormat.Format = "dd-MM-yyyy";
                row++;
            }

            ws.Columns().AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);

            return File(ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"mascotas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        public async Task<IActionResult> ExportPdf()
        {
            var rows = await _context.Mascotas
                .AsNoTracking()
                .Include(m => m.Especie)
                .Include(m => m.Dueno)
                .OrderBy(m => m.Nombre)
                .Select(m => new Reports.MascotasPdf.Row(
                     m.Nombre ?? "",
                     m.Especie != null ? (m.Especie.Nombre ?? "") : "",
                     m.Dueno != null ? $"{m.Dueno.Nombres} {m.Dueno.Apellidos}" : "",
                     m.Dueno != null ? (m.Dueno.Rut ?? "") : "",
                     m.Sexo ?? "",
                     m.FechaNacimiento.ToDateTime(TimeOnly.MinValue)
                 ))
                  .ToListAsync(); // <--- AGREGA ESTO

            var doc = new Reports.MascotasPdf(rows, "Mascotas - VetCare Clínica");
            var pdf = doc.GeneratePdf();
            return File(pdf, "application/pdf", $"mascotas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
        }

    }


}
