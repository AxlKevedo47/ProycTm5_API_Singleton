using System.ComponentModel.DataAnnotations;

namespace ProycTm5_API_Singleton.Models;

// MF22036:  Modelo de datos utilizado para representar a los empleados en la API.
public class Empleado
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del empleado es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo del empleado es obligatorio.")]
    public string Cargo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El área del empleado es obligatoria.")]
    public string Area { get; set; } = string.Empty;
}