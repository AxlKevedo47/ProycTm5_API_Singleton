using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using ProycTm5_API_Singleton.Models;

namespace ProycTm5_API_Singleton.Services
{
    public class EmpleadoSingletonService : IEmpleadoService
    {

        //MM19037: Se cambia de static List<Empleado> a ConcurrentDictionary<int, Empleado> como campo de instancia para permitir acceso seguro desde múltiples hilos en el Singleton
        private  ConcurrentDictionary<int, Empleado> _empleados = new();
        //MM19037: Se usa un campo de instancia junto con Interlocked para incrementar el ID de forma atómica y evitar condiciones de carrera
        private int _ultimoId = 0;


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

        
        public bool Eliminar(int id)
        {
            return _empleados.TryRemove(id, out _);
        }

        public bool AreaEsValida(string area)//CC22070: Se valida si el área del empleado es válida, se compara con una lista de áreas válidas y se devuelve true si es válida o false si no lo es
        {
            string[] AreasValidas = new[] { "Ventas", "Producción", "Transporte", "Marketing" };

            return Array.Exists(AreasValidas, a => a.Equals(area, StringComparison.OrdinalIgnoreCase));
        }

        private readonly string _rutaArchivo; //CC22070: Se define la ruta del archivo donde se guardarán los empleados
        private readonly ILogger _logger; //CC22070: Se define un logger para registrar información y errores es decir control de errores
        private readonly SemaphoreSlim _bloqueo = new(1, 1); //CC22070: Se define un semáforo para controlar el acceso concurrente a la lectura y escritura del archivo

        //CC22070: Se define un JsonSerializerOptions para configurar la serialización y deserialización del json, en este caso se ignora el mayusculas y minusculas del texto
        private static readonly JsonSerializerOptions _opcionesJson = new()
        {
            PropertyNameCaseInsensitive = true
        };
        //CC22070: Se define un constructor que recibe el entorno de la aplicación y el logger, y se inicializan los campos correspondientes
        public EmpleadoSingletonService( IWebHostEnvironment env,ILogger<EmpleadoSingletonService> logger)
        {
            _logger = logger;
            //CC22070: Se construye la ruta del archivo empleados.json a partir del directorio raíz de la aplicación y se asigna al campo _rutaArchivo
            _rutaArchivo = Path.Combine(env.ContentRootPath, "empleados.json");

            _logger.LogWarning("EmpleadoSingletonService se ha inicializado. Ruta: {Ruta}",_rutaArchivo);
        }

        public async Task<IEnumerable<Empleado>> ObtenerTodosAsync()
        {
            await CargarEmpleados();
            return _empleados.Values.ToList();
        }
        public async Task CargarEmpleados()
        {
            //CC22070: Se espera a que el semáforo esté disponible para evitar que varios hilos accedan al archivo al mismo tiempo
            await _bloqueo.WaitAsync();

            try
            {                
                if (File.Exists(_rutaArchivo))
                {
                    //CC22070: Se lee el contenido del archivo empleados.json de forma asíncrona 
                    var contenido = await File.ReadAllTextAsync(_rutaArchivo);

                    //CC22070: Se deserializa el contenido del archivo json a una lista de empleados y se asigna al campo _empleados, si es null se asigna una lista vacía
                    var empleadoLista = JsonSerializer.Deserialize<List<Empleado>>(contenido, _opcionesJson);

                    // Crear un ConcurrentDictionary a partir de la lista
                    _empleados = new ConcurrentDictionary<int, Empleado>(empleadoLista.ToDictionary(e => e.Id, e => e));

                    // Opcional: actualizar _ultimoId al máximo Id cargado (evit que Interlocked.Increment cree IDs duplicados)
                    _ultimoId = _empleados.Keys.DefaultIfEmpty(0).Max();


                    //CC22070: Se registra un mensaje de información en el logger indicando que los empleados se cargaron correctamente desde el archivo
                    _logger.LogInformation("Empleados cargados desde el archivo: {RutaArchivo}", _rutaArchivo);
                }
                _logger.LogWarning("El archivo JSON no existe en: {Ruta}", _rutaArchivo);
            }//CC22070: Se captura cualquier excepción que ocurra durante la carga de empleados y se registra un error en el logger
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los empleados desde el archivo.");
            }
            finally
            {
                _bloqueo.Release();
            }
        }
     }
}
