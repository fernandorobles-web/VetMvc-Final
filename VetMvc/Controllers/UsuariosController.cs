using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetMvc.Models;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using VetMvc.Reports;

namespace VetMvc.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly VetDbContext _context;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(VetDbContext context, ILogger<UsuariosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Usuarios
        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _context.Usuarios
                    .OrderBy(u => u.NombreCompleto)
                    .ToListAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar usuarios");
                TempData["Error"] = ex.GetBaseException().Message;
                return View(new List<Usuario>());
            }
        }

        // GET: /Usuarios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return NotFound();
            return View(u);
        }

        // GET: /Usuarios/Create
        public IActionResult Create()
        {
            return View(new UsuarioFormVM { Activo = true });
        }

        public async Task<IActionResult> ExportExcel()
        {
            var data = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Usuarios");

            // Encabezados
            ws.Cell(1, 1).Value = "Nombre";
            ws.Cell(1, 2).Value = "Usuario";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Rol";
            ws.Cell(1, 5).Value = "Activo";
            ws.Cell(1, 6).Value = "Fecha Registro";
            ws.Range(1, 1, 1, 6).Style.Font.Bold = true;

            // Filas
            var row = 2;
            foreach (var u in data)
            {
                ws.Cell(row, 1).Value = u.NombreCompleto;
                ws.Cell(row, 2).Value = u.NombreUsuario;
                ws.Cell(row, 3).Value = u.Email;
                ws.Cell(row, 4).Value = u.Rol;
                ws.Cell(row, 5).Value = u.Activo ? "Sí" : "No";
                ws.Cell(row, 6).Value = u.FechaCreacion;
                ws.Cell(row, 6).Style.DateFormat.Format = "dd-MM-yyyy HH:mm";
                row++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"usuarios_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            const string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(stream.ToArray(), mime, fileName);
        }

        public async Task<IActionResult> ExportPdf()
        {
            var data = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();

            var doc = new UsuariosPdf(data, "Usuarios - VetCare Clínica");
            var pdfBytes = doc.GeneratePdf();

            var fileName = $"usuarios_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // POST: /Usuarios/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioFormVM vm)
        {
            // En crear, password es obligatoria
            if (string.IsNullOrWhiteSpace(vm.Password))
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");

            // Unicidad
            if (await _context.Usuarios.AnyAsync(x => x.NombreUsuario == vm.NombreUsuario))
                ModelState.AddModelError("NombreUsuario", "Nombre de usuario ya existe.");

            if (await _context.Usuarios.AnyAsync(x => x.Email == vm.Email))
                ModelState.AddModelError("Email", "Email ya registrado.");

            if (!ModelState.IsValid) return View(vm);

            try
            {
                var usuario = new Usuario
                {
                    NombreCompleto = vm.NombreCompleto.Trim(),
                    NombreUsuario = vm.NombreUsuario.Trim(),
                    Email = vm.Email.Trim().ToLower(),
                    Rol = vm.Rol,
                    Activo = vm.Activo,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password!)
                };

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                TempData["Error"] = "Ocurrió un error al crear el usuario.";
                return View(vm);
            }
        }

        // GET: /Usuarios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return NotFound();

            var vm = new UsuarioFormVM
            {
                Id = u.Id,
                NombreCompleto = u.NombreCompleto,
                NombreUsuario = u.NombreUsuario,
                Email = u.Email,
                Rol = u.Rol,
                Activo = u.Activo
            };
            return View(vm);
        }

        // POST: /Usuarios/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioFormVM vm)
        {
            if (id != vm.Id) return BadRequest();

            // Validar unicidad excluyendo el propio Id
            if (await _context.Usuarios.AnyAsync(x => x.Id != id && x.NombreUsuario == vm.NombreUsuario))
                ModelState.AddModelError("NombreUsuario", "Nombre de usuario ya existe.");

            if (await _context.Usuarios.AnyAsync(x => x.Id != id && x.Email == vm.Email))
                ModelState.AddModelError("Email", "Email ya registrado.");

            // Si el usuario escribió contraseña, la validación [Compare] ya aplica; si la dejó vacía, no es obligatorio.
            if (!string.IsNullOrEmpty(vm.Password) && vm.Password.Length < 8)
                ModelState.AddModelError("Password", "La contraseña debe tener al menos 8 caracteres.");

            if (!ModelState.IsValid) return View(vm);

            try
            {
                var u = await _context.Usuarios.FindAsync(id);
                if (u == null) return NotFound();

                u.NombreCompleto = vm.NombreCompleto.Trim();
                u.NombreUsuario = vm.NombreUsuario.Trim();
                u.Email = vm.Email.Trim().ToLower();
                u.Rol = vm.Rol;
                u.Activo = vm.Activo;

                if (!string.IsNullOrWhiteSpace(vm.Password))
                {
                    u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar usuario");
                TempData["Error"] = "Ocurrió un error al guardar los cambios.";
                return View(vm);
            }
        }

        // GET: /Usuarios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Usuarios.FindAsync(id);
            if (u == null) return NotFound();
            return View(u);
        }

        // POST: /Usuarios/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var u = await _context.Usuarios.FindAsync(id);
                if (u == null) return NotFound();
                _context.Usuarios.Remove(u);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuario eliminado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                TempData["Error"] = "No se pudo eliminar el usuario.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
