namespace PeliculasAPI.DTOs
{
    // DTO que representa la relación actor-personaje dentro de los detalles de una película.
    public class ActorPeliculaDetalleDTO
    {
        public int ActorId { get; set; }
        public string Personaje { get; set; }
        public string NombrePersona { get; set; }
    }
}
