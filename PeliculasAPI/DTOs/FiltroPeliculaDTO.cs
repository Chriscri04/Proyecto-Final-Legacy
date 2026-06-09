namespace PeliculasAPI.DTOs
{
    // DTO para filtrar y paginar la lista de películas en consultas (incluye ordenación).
    public class FiltroPeliculaDTO
    {
        public int Pagina { get; set; }
        public int CantidadRegistrosPorPagina { get; set; } = 10;
        public PaginacionDTO Paginacion
        {
            get { return new PaginacionDTO() { Pagina = Pagina, CantidadRegistrosPorPagina = CantidadRegistrosPorPagina}; }
        }

        public string Titulo { get; set; }
        public int GeneroId { get; set; }
        public bool EnCines { get; set; }
        public bool ProximosEstrenos { get; set; }
        public string CampoOrdernar { get; set; }
        public bool OrdenAscendente { get; set; } = true;

    }
}
