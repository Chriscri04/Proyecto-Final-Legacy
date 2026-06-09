using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Helpers;
using PeliculasAPI.Servicios;
using System.Linq.Dynamic.Core;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/peliculas")]
    public class PeliculasController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivo almacenadorArchivo;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivo almacenadorArchivo)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivo = almacenadorArchivo;
        }

        [HttpGet]
        public async Task<ActionResult<PeliculaIndexDTO>> Get()
        {
            var top = 5;
            var hoy = DateTime.Today;

            var proximosEstrenos = await context.Peliculas
                .Where(x => x.FechaEstreno > hoy)
                .OrderBy(x => x.FechaEstreno)
                .ToListAsync();

            var enCines = await context.Peliculas
                .Where(x => x.EnCines)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculaIndexDTO();
            resultado.FuturosEstrenos = mapper.Map<List<PeliculasDTO>>(proximosEstrenos);
            resultado.EnCines = mapper.Map<List<PeliculasDTO>>(enCines);
            return resultado;

        }
        [HttpGet("filtro")]
        public async Task<ActionResult<List<PeliculasDTO>>> Filtrar([FromQuery] FiltroPeliculaDTO filtroPeliculaDTO)
        {
            var peliculasQueryable = context.Peliculas.AsQueryable();

            if (!string.IsNullOrEmpty(filtroPeliculaDTO.Titulo))
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.Titulo.Contains(filtroPeliculaDTO.Titulo));
            }

            if (filtroPeliculaDTO.EnCines)
            {
                peliculasQueryable = peliculasQueryable.Where(x => x.FechaEstreno > hoy);
            }

            if (filtroPeliculaDTO.GeneroId != 0)
            {
                peliculasQueryable = peliculasQueryable
                    .Where(x => x.PeliculasGeneros.Select(y => y.GeneroId)
                    .Contains(filtroPeliculaDTO.GeneroId));
            }

            if (!string.IsNullOrEmpty(filtroPeliculaDTO.CampoOrdernar))
            {
                var tipoOrder = filtroPeliculaDTO.OrdenAscendente ? "ascending" : "descending";
                peliculasQueryable = peliculasQueryable.OrderBy($"{filtroPeliculaDTO.CampoOrdernar} {tipoOrder}");
            }

            await HttpContext.InsertarParametrosPaginacion(peliculasQueryable, filtroPeliculaDTO.CantidadRegistrosPorPagina);

            var peliculas = await peliculasQueryable.Paginar(filtroPeliculaDTO.Paginacion).ToListAsync();

            return mapper.Map<List<PeliculasDTO>>(peliculas);
        }

        [HttpGet("{id}", Name = "ObtenerPelicula")]
        public async Task<ActionResult<PeliculasDetallesDTO>> Get(int id)
        {
            var pelicula = await context.Peliculas
                .Include(x => x.PeliculasActores).ThenInclude(x => x.Actor)
                .Include(x => x.PeliculasGeneros).ThenInclude(x => x.Genero)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            pelicula.PeliculasActores = pelicula.PeliculasActores.OrderBy(x => x.Orden).ToList();

            return mapper.Map<PeliculasDetallesDTO>(pelicula);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] PeliculaCreacionDTO peliculaCreacion)
        {
            var pelicula = mapper.Map<Pelicula>(peliculaCreacion);

            if (peliculaCreacion.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await peliculaCreacion.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculaCreacion.Poster.FileName);
                    pelicula.Poster = await almacenadorArchivo.GuardarArchivo(contenido, extension, contenedor, peliculaCreacion.Poster.ContentType);
                }
            }
            AsignarOrdenActores(pelicula);
            context.Add(pelicula);
            await context.SaveChangesAsync();
            var peliculaDTO = mapper.Map<PeliculasDTO>(pelicula);
            return new CreatedAtRouteResult("ObtenerPelicula", new { id = pelicula.Id }, peliculaDTO);
        }

        private void AsignarOrdenActores(Pelicula pelicula)
        {
            if (pelicula.PeliculasActores != null)
            {
                for (int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] PeliculaCreacionDTO peliculaCreacion)
        {
            var peliculaDB = await context.Peliculas
                .Include(x => x.PeliculasActores)
                .Include(x =>x.PeliculasGeneros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (peliculaDB == null) { return NotFound(); }

            peliculaDB = mapper.Map(peliculaCreacion, peliculaDB);

            if (peliculaCreacion.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await peliculaCreacion.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculaCreacion.Poster.FileName);
                    peliculaDB.Poster = await almacenadorArchivo.EditarArchivo(contenido, extension, contenedor, peliculaDB.Poster, peliculaCreacion.Poster.ContentType);
                }
            }

            AsignarOrdenActores(peliculaDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PeliculaPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var entidadDB = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);

            if (entidadDB == null)
            {
                return NotFound();
            }

            var entidadDTO = mapper.Map<PeliculaPatchDTO>(entidadDB);
            patchDocument.ApplyTo(entidadDTO, ModelState);

            var esValido = TryValidateModel(entidadDTO);

            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(entidadDTO, entidadDB);

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            //Validar que el id exista en la base de datos
            var existe = await context.Peliculas.AnyAsync(x => x.Id == id);
            //Si no existe retornar un 404
            if (!existe)
            {
                return NotFound();
            }

            //Si existe se elimina la entidad
            context.Remove(new Pelicula() { Id = id });
            //Cambios a la base de datos
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
