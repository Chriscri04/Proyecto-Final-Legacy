namespace PeliculasAPI.DTOs
{
    // DTO para agrupar películas destacadas en la página principal: próximos estrenos y en cines.
    public class PeliculaIndexDTO
    {
        public List<PeliculasDTO> FuturosEstrenos { get; set; }
        public List<PeliculasDTO> EnCines { get; set; }
    }
}
