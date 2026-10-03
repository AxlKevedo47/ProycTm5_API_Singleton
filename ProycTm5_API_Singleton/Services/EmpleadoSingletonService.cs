using System;
using System.Collections.Generic;
using ProycTm5_API_Singleton.Models;

namespace ProycTm5_API_Singleton.Services
{
    public class EmpleadoSingletonService : IEmpleadoService
    {

        private static readonly List<Empleado> Empleados = new List<Empleado>();//CC22070: Se inicializa la lista de empleados como una lista vacía
        private int nextId = 1;//CC22070: Se inicializa el siguiente ID como 1

        public IEnumerable<Empleado> ObtenerTodos()//CC22070: Se devuelve la lista de empleados
        {
            return Empleados;
        }

        public Empleado ObtenerPorId(int id)//CC22070: Se busca un empleado por su ID, si no se encuentra se lanza una excepción
        {
            return Empleados.Find(e => e.Id == id) ?? throw new InvalidOperationException("Empleado no encontrado");
        }

        public Empleado Crear(Empleado empleado)//CC22070: Se crea un nuevo empleado, se le asigna un ID único y se agrega a la lista de empleados
        {
            empleado.Id = nextId++;
            Empleados.Add(empleado);
            return empleado;
        }

        public Empleado Actualizar(int id, Empleado empleadoActualizado)//CC22070: Se actualiza un empleado existente, se buscan los datos del empleado por su ID y se actualizan los campos correspondientes
        {
            var empleado = ObtenerPorId(id);            

            empleado.Nombre = empleadoActualizado.Nombre;
            empleado.Cargo = empleadoActualizado.Cargo;
            empleado.Area = empleadoActualizado.Area;

            return empleado;
        }

        public bool Eliminar(int id)//CC22070: Se elimina un empleado existente, se busca el empleado por su ID y se elimina de la lista de empleados
        {
            var empleado = ObtenerPorId(id);
           
            Empleados.Remove(empleado);
            return true;
        }

        public bool AreaEsValida(string area)//CC22070: Se valida si el área del empleado es válida, se compara con una lista de áreas válidas y se devuelve true si es válida o false si no lo es
        {
            string[] AreasValidas = new[] { "Ventas", "Producción", "Transporte", "Marketing" };

            return Array.Exists(AreasValidas, a => a.Equals(area, StringComparison.OrdinalIgnoreCase));
        }
    }
}
