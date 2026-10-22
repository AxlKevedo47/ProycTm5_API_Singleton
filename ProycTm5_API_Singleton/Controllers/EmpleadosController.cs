using Microsoft.AspNetCore.Mvc;
using ProycTm5_API_Singleton.Models;
using ProycTm5_API_Singleton.Services;

namespace ProycTm5_API_Singleton.Controllers;   // JQ203: Se Plantea el CRUD

[ApiController]
[Route("api/[controller]")]
public class EmpleadosController : ControllerBase
{
    //MM19037: Referencia al servicio Singleton que contiene el estado y la lógica de negocio de los empleados
    private readonly IEmpleadoService _empleadoService;

    //MM19037: Recibe por inyección de dependencias la instancia Singleton de IEmpleadoService
    public EmpleadosController(IEmpleadoService empleadoService)
    {
        _empleadoService = empleadoService;
    }

    [HttpGet]
    //CC22070: método ObtenerTodos que devuelve todos los empleados de forma asíncrona facilitando la atención de otras solicitudes simultáneas
    public async Task<ActionResult<IEnumerable<Empleado>>> Get() => Ok(await _empleadoService.ObtenerTodosAsync());


    [HttpGet("{id:int}")]
    public ActionResult<Empleado> ObtenerPorId(int id)
    {
        //MM19037: Busca el empleado en el servicio; si no existe, responde 404
        var empleado = _empleadoService.ObtenerPorId(id);

        return empleado is null ? NotFound() : Ok(empleado);
    }

    [HttpPost]
    public ActionResult<Empleado> Crear(Empleado empleado)
    {
        //MM19037: El área debe ser una de las permitidas para poder crear el empleado
        if (!_empleadoService.AreaEsValida(empleado.Area))
        {
            return BadRequest("El área debe ser Ventas, Producción, Transporte o Marketing.");
        }

        //MM19037: El servicio asigna el Id de forma atómica, guarda el empleado y lo persiste en el archivo
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

        //MM19037: El servicio actualiza y persiste los datos del empleado; devuelve null si el Id no existe
        var empleado = _empleadoService.Actualizar(id, empleadoActualizado);

        return empleado is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Eliminar(int id)
    {
        //MM19037: El servicio elimina y persiste el cambio; devuelve false si el empleado no existe
        var eliminado = _empleadoService.Eliminar(id);

        return eliminado ? NoContent() : NotFound();
    }    
}