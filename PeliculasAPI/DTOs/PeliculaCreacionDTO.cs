using PeliculasAPI.Validaciones;

namespace PeliculasAPI.DTOs
{
    public class PeliculaCreacionDTO: PeliculaPatchDTO
    {
        
        [PesoImagenValidacion(pesoMaximoEnMegas: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile Poster { get; set; }
    }
}
