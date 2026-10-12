using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using ProycTm5_API_Singleton.Models;

namespace ProycTm5_API_Singleton.Services
{
    public class EmpleadoSingletonService : IEmpleadoService
    {

        //MM19037: Se cambia de static List<Empleado> a ConcurrentDictionary<int, Empleado> como campo de instancia para permitir acceso seguro desde múltiples hilos en el Singleton
        private readonly ConcurrentDictionary<int, Empleado> _empleados = new();
        //MM19037: Se usa un campo de instancia junto con Interlocked para incrementar el ID de forma atómica y evitar condiciones de carrera
        private int _ultimoId = 0;

        public IEnumerable<Empleado> ObtenerTodos()//CC22070: Se devuelve la lista de empleados
        {
            return _empleados.Values;
        }

        //MM19037: Se cambia el retorno a Empleado? para cumplir con el contrato de la interfaz y evitar lanzar excepciones cuando no se encuentra el empleado
        public Empleado? ObtenerPorId(int id)
        {
            _empleados.TryGetValue(id, out var empleado);
            return empleado;
        }

        public Empleado Crear(Empleado empleado)//CC22070: Se crea un nuevo empleado, se le asigna un ID único y se agrega a la lista de empleados
        {
            //MM19037: Interlocked.Increment garantiza que la asignación del ID sea atómica aunque varios hilos creen empleados al mismo tiempo
            int nuevoId = Interlocked.Increment(ref _ultimoId);
            empleado.Id = nuevoId;
            _empleados[nuevoId] = empleado;
            return empleado;
        }

        //MM19037: Se maneja el caso en que ObtenerPorId devuelva null, devolviendo null en vez de dejar que una excepción se propague
        public Empleado? Actualizar(int id, Empleado empleadoActualizado)
        {
            var empleado = ObtenerPorId(id);
            if (empleado is null)
            {
                return null;
            }

            empleado.Nombre = empleadoActualizado.Nombre;
            empleado.Cargo = empleadoActualizado.Cargo;
            empleado.Area = empleadoActualizado.Area;

            return empleado;
        }

        //MM19037: Se usa TryRemove del ConcurrentDictionary para eliminar de forma segura y se devuelve false si el empleado no existe
        public bool Eliminar(int id)
        {
            return _empleados.TryRemove(id, out _);
        }

        public bool AreaEsValida(string area)//CC22070: Se valida si el área del empleado es válida, se compara con una lista de áreas válidas y se devuelve true si es válida o false si no lo es
        {
            string[] AreasValidas = new[] { "Ventas", "Producción", "Transporte", "Marketing" };

            return Array.Exists(AreasValidas, a => a.Equals(area, StringComparison.OrdinalIgnoreCase));
        }
    }
}
