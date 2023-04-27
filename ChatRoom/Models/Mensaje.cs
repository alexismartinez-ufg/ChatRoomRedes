using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Mensaje
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdMensaje { get; set; }
    public string Contenido { get; set; }
    public DateTime FechaHora { get; set; }
    public string NombreRemitente { get; set; }
}
