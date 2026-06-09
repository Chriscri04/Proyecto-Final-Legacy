using PeliculasAPI.DTOs;

namespace PeliculasAPI.Helpers
{
    public static class QueryableExtensions
    {
        // Aplica Skip/Take al IQueryable usando los valores del DTO de paginación
        // para devolver sólo la página solicitada.
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDTO paginacionDTO)
        {
            return queryable
                .Skip((paginacionDTO.Pagina - 1) * paginacionDTO.CantidadRegistrosPorPaguina)
                .Take(paginacionDTO.CantidadRegistrosPorPaguina);
        }
    }
}
