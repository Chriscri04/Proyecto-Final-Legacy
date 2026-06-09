namespace PeliculasAPI.DTOs
{
    // DTO para los detalles completos de una película, incluye géneros y lista de actores con sus personajes.
    public class PeliculasDetallesDTO: PeliculasDTO
    {
        public List<GeneroDTO> Generos { get; set; }
        public List<ActorPeliculaDetalleDTO> Actores { get; set; }
    }
}
