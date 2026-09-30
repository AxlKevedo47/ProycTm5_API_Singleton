namespace ProycTm5_API_Singleton.Models;

// JQ203: Se Definio el Modelo JSON con Id, Nombre, Cargo y Area.

public class Empleado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
}
