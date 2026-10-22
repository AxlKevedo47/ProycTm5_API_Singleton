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

        //MM19037: Diccionario en memoria con los empleados indexados por Id; es el estado compartido por la única instancia Singleton del servicio
        private  ConcurrentDictionary<int, Empleado> _empleados = new();
        //MM19037: Guarda el último Id asignado, usado como base para generar el siguiente de forma atómica
        private int _ultimoId = 0;


        //MM19037: Busca un empleado por Id bajo el semáforo y devuelve una copia; si no existe, devuelve null
        public Empleado? ObtenerPorId(int id)
        {
            _bloqueo.Wait();
            try
            {
                return _empleados.TryGetValue(id, out var empleado)
                    ? ClonarEmpleado(empleado)
                    : null;
            }
            finally
            {
                _bloqueo.Release();
            }
        }

        public Empleado Crear(Empleado empleado)//CC22070: Se crea un nuevo empleado, se le asigna un ID único y se agrega a la lista de empleados
        {
            _bloqueo.Wait();
            try
            {
                //MM19037: Genera el nuevo Id de forma atómica a partir del último asignado
                int nuevoId = Interlocked.Increment(ref _ultimoId);
                var empleadoCreado = ClonarEmpleado(empleado);
                empleadoCreado.Id = nuevoId;
                _empleados[nuevoId] = empleadoCreado;

                try
                {
                    GuardarEmpleados();
                    return ClonarEmpleado(empleadoCreado);
                }
                catch
                {
                    _empleados.TryRemove(nuevoId, out _);
                    throw;
                }
            }
            finally
            {
                _bloqueo.Release();
            }
        }

        //MM19037: Actualiza los campos de un empleado existente y persiste el cambio; devuelve null si el Id no existe
        public Empleado? Actualizar(int id, Empleado empleadoActualizado)
        {
            _bloqueo.Wait();
            try
            {
                if (!_empleados.TryGetValue(id, out var empleado))
                {
                    return null;
                }

                var empleadoAnterior = ClonarEmpleado(empleado);
                var empleadoNuevo = ClonarEmpleado(empleadoActualizado);
                empleadoNuevo.Id = id;
                _empleados[id] = empleadoNuevo;

                try
                {
                    GuardarEmpleados();
                    return ClonarEmpleado(empleadoNuevo);
                }
                catch
                {
                    _empleados[id] = empleadoAnterior;
                    throw;
                }
            }
            finally
            {
                _bloqueo.Release();
            }
        }

        
        public bool Eliminar(int id)
        {
            _bloqueo.Wait();
            try
            {
                if (!_empleados.TryRemove(id, out var eliminado))
                {
                    return false;
                }

                try
                {
                    GuardarEmpleados();
                    return true;
                }
                catch
                {
                    _empleados[id] = eliminado;
                    throw;
                }
            }
            finally
            {
                _bloqueo.Release();
            }
        }

        public bool AreaEsValida(string area)//CC22070: Se valida si el área del empleado es válida, se compara con una lista de áreas válidas y se devuelve true si es válida o false si no lo es
        {
            string[] AreasValidas = new[] { "Ventas", "Producción", "Transporte", "Marketing" };

            return Array.Exists(AreasValidas, a => a.Equals(area, StringComparison.OrdinalIgnoreCase));
        }

        private readonly string _rutaArchivo; //CC22070: Se define la ruta del archivo donde se guardarán los empleados
        private readonly ILogger _logger; //CC22070: Se define un logger para registrar información y errores es decir control de errores
        private readonly SemaphoreSlim _bloqueo = new(1, 1); //CC22070: Se define un semáforo para controlar el acceso concurrente a la lectura y escritura del archivo
        private bool _estadoInicializado;

        //CC22070: Se define un JsonSerializerOptions para configurar la  deserialización del json, en este caso se ignora el mayusculas y minusculas del texto
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
            CargarEmpleados().GetAwaiter().GetResult();
        }
        //CC22070: Se define un método que devuelve todos los empleados cargados desde el archivo json, si no se han cargado aún, se llama a CargarEmpleados
        public Task<IEnumerable<Empleado>> ObtenerTodosAsync()
        {
            _bloqueo.Wait();
            try
            {
                return Task.FromResult<IEnumerable<Empleado>>(
                    _empleados.Values.Select(ClonarEmpleado).ToList());
            }
            finally
            {
                _bloqueo.Release();
            }
        }
        //CC22070: Se define un método que carga los empleados desde el archivo json de forma asíncrona, usando un semáforo para evitar accesos concurrentes
        public async Task CargarEmpleados()
        {
            //CC22070: Se espera a que el semáforo esté disponible para evitar que varios hilos accedan al archivo al mismo tiempo
            await _bloqueo.WaitAsync();

            try
            {                
                if (_estadoInicializado)
                {
                    return;
                }

                if (File.Exists(_rutaArchivo))
                {
                    //CC22070: Se lee el contenido del archivo empleados.json de forma asíncrona 
                    var contenido = await File.ReadAllTextAsync(_rutaArchivo);

                    //CC22070: Se convierten los datos del contenido del archivo json en objetos para poder utilizarlos y se asigna al campo _empleados
                    var empleadoLista = JsonSerializer.Deserialize<List<Empleado>>(contenido, _opcionesJson) ?? new List<Empleado>();

                    // Crear un ConcurrentDictionary a partir de la lista
                    _empleados = new ConcurrentDictionary<int, Empleado>(empleadoLista.ToDictionary(e => e.Id, e => e));

                    // CC22070: actualizar _ultimoId al máximo Id cargado, evita que Interlocked.Increment cree IDs duplicados
                    _ultimoId = _empleados.Keys.DefaultIfEmpty(0).Max();


                    //CC22070: Se registra un mensaje de información en el logger indicando que los empleados se cargaron correctamente desde el archivo
                    _logger.LogInformation("Empleados cargados desde el archivo: {RutaArchivo}", _rutaArchivo);
                }
                else
                {
                    await File.WriteAllTextAsync(_rutaArchivo, "[]");
                    _logger.LogInformation("Se creó el archivo JSON inicial en: {Ruta}", _rutaArchivo);
                }

                _estadoInicializado = true;
            }//CC22070: Se captura cualquier excepción que ocurra durante la carga de empleados y se registra un error en el logger
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los empleados desde el archivo.");
                throw new InvalidOperationException(
                    $"No se pudieron cargar los empleados desde '{_rutaArchivo}'.", ex);
            }
            finally
            {
                _bloqueo.Release();
            }
        }

        private void GuardarEmpleados()
        {
            var contenido = JsonSerializer.Serialize(_empleados.Values.ToList(), _opcionesJson);
            var archivoTemporal = _rutaArchivo + ".tmp";

            try
            {
                File.WriteAllText(archivoTemporal, contenido);
                File.Move(archivoTemporal, _rutaArchivo, true);
                _logger.LogInformation("Empleados guardados en el archivo: {RutaArchivo}", _rutaArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar los empleados en el archivo.");
                try
                {
                    if (File.Exists(archivoTemporal))
                    {
                        File.Delete(archivoTemporal);
                    }
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(cleanupException, "Error al limpiar el archivo temporal de empleados.");
                }

                throw;
            }
        }

        private static Empleado ClonarEmpleado(Empleado empleado)
        {
            return new Empleado
            {
                Id = empleado.Id,
                Nombre = empleado.Nombre,
                Cargo = empleado.Cargo,
                Area = empleado.Area
            };
        }
     }
}
