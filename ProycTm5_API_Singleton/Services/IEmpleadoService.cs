using ProycTm5_API_Singleton.Models;

namespace ProycTm5_API_Singleton.Services;

public interface IEmpleadoService
{
    IEnumerable<Empleado> ObtenerTodos();

    Empleado? ObtenerPorId(int id);

    Empleado Crear(Empleado empleado);

    Empleado? Actualizar(int id, Empleado empleadoActualizado);

    bool Eliminar(int id);

    bool AreaEsValida(string area);
}