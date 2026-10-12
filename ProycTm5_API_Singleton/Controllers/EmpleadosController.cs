using Microsoft.AspNetCore.Mvc;
using ProycTm5_API_Singleton.Models;
using ProycTm5_API_Singleton.Services;

namespace ProycTm5_API_Singleton.Controllers;   // JQ203: Se Plantea el CRUD

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    //MM19037: Se elimina el almacenamiento estático propio del controlador (List<Empleado> y AreasValidas) para delegar el funcionamiento al servicio
    private readonly IEmpleadoService _empleadoService;

    //MM19037: Se agrega constructor con inyección de IEmpleadoService para que el controlador use la instancia Singleton
    public EmpleadosController(IEmpleadoService empleadoService)
    {
        _empleadoService = empleadoService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Empleado>> ObtenerTodos()
    {
        //MM19037: Se delega la obtención de empleados al servicio
        return Ok(_empleadoService.ObtenerTodos());
    }

    [HttpGet("{id:int}")]
    public ActionResult<Empleado> ObtenerPorId(int id)
    {
        //MM19037: Se delega la búsqueda por ID al servicio
        var empleado = _empleadoService.ObtenerPorId(id);

        return empleado is null ? NotFound() : Ok(empleado);
    }

    [HttpPost]
    public ActionResult<Empleado> Crear(Empleado empleado)
    {
        //MM19037: Se usa la validación de área del servicio
        if (!_empleadoService.AreaEsValida(empleado.Area))
        {
            return BadRequest("El área debe ser Ventas, Producción, Transporte o Marketing.");
        }

        //MM19037: La asignación del ID y creación del usuario se delega al servicio
        var empleadoCreado = _empleadoService.Crear(empleado);

        return CreatedAtAction(nameof(ObtenerPorId), new { id = empleadoCreado.Id }, empleadoCreado);
    }

    [HttpPut("{id:int}")]
    public IActionResult Actualizar(int id, Empleado empleadoActualizado)
    {
        if (!_empleadoService.AreaEsValida(empleadoActualizado.Area))
        {
            return BadRequest("El área debe ser Ventas, Producción, Transporte o Marketing.");
        }

        //MM19037: Se delega la actualización al servicio; si devuelve null significa que el empleado no existe
        var empleado = _empleadoService.Actualizar(id, empleadoActualizado);

        return empleado is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        //MM19037: Se delega la eliminación al servicio, que devuelve false si el empleado no existe
        var eliminado = _empleadoService.Eliminar(id);

        return eliminado ? NoContent() : NotFound();
    }
}