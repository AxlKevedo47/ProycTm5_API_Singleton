# API RESTful de Gestión de Empleados con ASP.NET Core y Patrón Singleton

**Universidad de El Salvador**\
**Facultad Multidisciplinaria de Occidente**\
**Departamento de Ingeniería y Arquitectura**\
**Carrera:** Ingeniería en Desarrollo de Software\
**Asignatura:** Desarrollo y Reutilización de Software\
**Ciclo:** II/2026\
**Actividad Evaluada 2: Proyecto — Tema 5**

---

## Estructura principal del proyecto

```text
ProycTm5_API_Singleton/
├── Controllers/
│   └── EmpleadosController.cs
├── Models/
│   └── Empleado.cs
├── Services/
│   ├── IEmpleadoService.cs
│   └── EmpleadoSingletonService.cs
├── empleados.json
└── Program.cs
```

---

## 1. Implementación de los componentes

El proyecto implementa una API REST con ASP.NET Core Web API sobre .NET 10 para gestionar empleados de las áreas de Ventas, Producción, Transporte y Marketing.

### `Models/Empleado.cs`

Contiene la clase `Empleado`, que representa los datos de cada empleado mediante las propiedades `Id`, `Nombre`, `Cargo` y `Area`.

### `Controllers/EmpleadosController.cs`

Expone los endpoints HTTP para consultar, crear, actualizar y eliminar empleados. Recibe `IEmpleadoService` mediante inyección de dependencias y delega en el servicio las operaciones de gestión. Según el resultado de cada solicitud, devuelve el código de estado HTTP correspondiente.

### `Services/IEmpleadoService.cs`

Define el contrato del servicio de empleados y declara las operaciones para consultar, crear, actualizar y eliminar registros.

### `Services/EmpleadoSingletonService.cs`

Implementa `IEmpleadoService` y se encarga de gestionar los empleados y persistir sus datos. Utiliza `ConcurrentDictionary<int, Empleado>` para mantener los registros en memoria, `Interlocked.Increment` para generar identificadores y `SemaphoreSlim` para coordinar el acceso a las operaciones que requieren sincronización.

Al iniciar, carga los datos desde `empleados.json`. Cuando se crea, actualiza o elimina un empleado, guarda los cambios en el archivo. Para ello, escribe primero un archivo temporal y después reemplaza el archivo JSON principal.

### `Program.cs`

Configura la aplicación ASP.NET Core, registra los controladores y Swagger, y establece la configuración necesaria para iniciar el servidor y utilizar el servicio de empleados.

### `empleados.json`

Es el archivo JSON que conserva los datos de los empleados entre ejecuciones de la aplicación. Si no existe al iniciar el servicio, se crea con una colección vacía.

---

## 2. Patrón de diseño utilizado

### Singleton

El patrón Singleton se implementa mediante el contenedor de inyección de dependencias de ASP.NET Core. En `Program.cs`, se configura el servicio mediante la siguiente instrucción:

```csharp
builder.Services.AddSingleton<IEmpleadoService, EmpleadoSingletonService>();
```

El método `AddSingleton` hace que el contenedor proporcione una única instancia de `EmpleadoSingletonService` durante la ejecución de la aplicación. De este modo, las solicitudes comparten el estado de los empleados que mantiene el servicio en memoria.

El controlador depende de la interfaz `IEmpleadoService` y no directamente de `EmpleadoSingletonService`. Esta separación permite cambiar la implementación del servicio sin modificar el controlador, siempre que se mantenga el contrato definido por la interfaz.

---

## 3. Ejecución del proyecto

### Requisitos previos

- Tener instalado el SDK de .NET 10.
- Para ejecutar el proyecto desde Visual Studio, disponer de una versión compatible con .NET 10 y el desarrollo de aplicaciones ASP.NET Core.

### Desde Visual Studio

1. Abra la solución `ProycTm5_API_Singleton.slnx`.
2. Seleccione el perfil de ejecución `http` o `https` junto al botón de inicio.
3. Presione el botón de inicio para ejecutar el proyecto.
4. Abra manualmente en el navegador la dirección de Swagger correspondiente al perfil seleccionado.

Las direcciones configuradas para las pruebas locales son:

- **HTTP:** `http://localhost:5035/swagger`
- **HTTPS:** `https://localhost:7048/swagger`

Estas direcciones están definidas en `Properties/launchSettings.json` y pueden variar si se modifica la configuración de ejecución.

### Desde la terminal

Abra una terminal y acceda a la carpeta que contiene el archivo `ProycTm5_API_Singleton.csproj`.

Ejecute los siguientes comandos, uno por uno:

```bash
dotnet restore
dotnet build
dotnet run --launch-profile http
```

Estos comandos restauran las dependencias, compilan el proyecto e inician la aplicación mediante el perfil HTTP definido en `Properties/launchSettings.json`.

Cuando la terminal indique que la aplicación está escuchando en `http://localhost:5035`, abra la siguiente dirección en el navegador:

`http://localhost:5035/swagger`

Swagger permite ejecutar las solicitudes disponibles y consultar los códigos de estado HTTP y los datos devueltos en formato JSON.

---

## Integrantes

1. Josué Edenilson Martínez Flores — MF22036
2. José Arnoldo Landaverde Gómez — LG22018
3. Rodrigo Vladimir Cruz Cortez — CC22070
4. Nelson Josué Morán Mata — MM19037
5. Axel Rodrigo Juárez Quevedo — JQ22003