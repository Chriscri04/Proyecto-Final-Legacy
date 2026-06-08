using Microsoft.AspNetCore.Http;
using PeliculasAPI.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTOs
{
    public class ActorCreacionDTO
    {
        [Required]
        [StringLength(120)]
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        //recibir la foto como tal
        [PesoImagenValidacion(pesoMaximoEnMegas: 4)]
        public IFormFile Foto { get; set; }
    }
}
