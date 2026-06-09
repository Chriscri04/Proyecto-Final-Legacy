using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace PeliculasAPI.Helpers
{
    public static class HttpContextExtension
    {
        // Calcula el número de páginas y añade el header 'cantidadPaginas' a la respuesta
        // para que el cliente sepa cuántas páginas hay en la paginación.
        public async static Task InsertarParametrosPaginacion<T>(this HttpContext httpContext, IQueryable<T> queryable, int cantidadRegistrosPorPagina)
        {
            double cantidad = await queryable.CountAsync();
            double cantidadPaginas = Math.Ceiling(cantidad / cantidadRegistrosPorPagina);
            httpContext.Response.Headers.Add("cantidadPaginas", cantidadPaginas.ToString());
        }
    }
}
