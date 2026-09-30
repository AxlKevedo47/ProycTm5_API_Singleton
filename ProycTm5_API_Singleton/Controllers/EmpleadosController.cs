using Microsoft.AspNetCore.Mvc;
using ProycTm5_API_Singleton.Models;

namespace ProycTm5_API_Singleton.Controllers;   // JQ203: Se Plantea el CRUD 

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    private static readonly List<Empleado> Empleados = [];
    private static readonly string[] AreasValidas = ["Ventas", "Producción", "Transporte", "Marketing"];

    // JQ203: Validación de las áreas permitidas para los empleados

    [HttpGet]
    public ActionResult<IEnumerable<Empleado>> ObtenerTodos()
    {
        return Ok(Empleados);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Empleado> ObtenerPorId(int id)
    {
        var empleado = Empleados.FirstOrDefault(e => e.Id == id);

        return empleado is null ? NotFound() : Ok(empleado);
    }

    [HttpPost]
    public ActionResult<Empleado> Crear(Empleado empleado)
    {
        if (!AreaEsValida(empleado.Area))
        {
            return BadRequest("El área debe ser Ventas, Producción, Transporte o Marketing.");
        }

        empleado.Id = Empleados.Count == 0 ? 1 : Empleados.Max(e => e.Id) + 1;
        Empleados.Add(empleado);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = empleado.Id }, empleado);
    }

    [HttpPut("{id:int}")]
    public IActionResult Actualizar(int id, Empleado empleadoActualizado)
    {
        var empleado = Empleados.FirstOrDefault(e => e.Id == id);

        if (empleado is null)
        {
            return NotFound();
        }

        if (!AreaEsValida(empleadoActualizado.Area))
        {
            return BadRequest("El área debe ser Ventas, Producción, Transporte o Marketing.");
        }

        empleado.Nombre = empleadoActualizado.Nombre;
        empleado.Cargo = empleadoActualizado.Cargo;
        empleado.Area = empleadoActualizado.Area;

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        var empleado = Empleados.FirstOrDefault(e => e.Id == id);

        if (empleado is null)
        {
            return NotFound();
        }

        Empleados.Remove(empleado);

        return NoContent();
    }

    private static bool AreaEsValida(string area)
    {
        return AreasValidas.Contains(area, StringComparer.OrdinalIgnoreCase);
    }
}
