using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Servicios;

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
        public async Task<ActionResult<List<PeliculasDTO>>> Get()
        {
            var peliculas = await context.Peliculas.ToListAsync();
            return mapper.Map<List<PeliculasDTO>>(peliculas);
        }

        [HttpGet("{id}", Name = "ObtenerPelicula")]
        public async Task<ActionResult<PeliculasDTO>> Get(int id)
        {
            var pelicula = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);

            if (pelicula == null)
            {
                return NotFound();
            }

            return mapper.Map<PeliculasDTO>(pelicula);
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

            context.Add(pelicula);
            await context.SaveChangesAsync();
            var peliculaDTO = mapper.Map<PeliculasDTO>(pelicula);
            return new CreatedAtRouteResult("ObtenerPelicula", new { id = pelicula.Id }, peliculaDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] PeliculaCreacionDTO peliculaCreacion)
        {
            var peliculaDB = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);

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
