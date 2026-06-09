using AutoMapper;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;
using PeliculasAPI.Helpers;
using PeliculasAPI.Servicios;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class ActoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivo almacenadorArchivo;
        private readonly string contenedor = "actores";

        public ActoresController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivo almacenadorArchivo)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivo = almacenadorArchivo;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Actores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacion(queryable, paginacionDTO.CantidadRegistrosPorPaguina);
            var entidades = await queryable.Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<ActorDTO>>(entidades);
        }

        [HttpGet("{id}", Name = "ObtenerActor")]
        public async Task<ActionResult<ActorDTO>> Get(int id)
        {
            var entidad = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
            {
                return NotFound();
            }
            var dto = mapper.Map<ActorDTO>(entidad);

            return dto;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var entidad = mapper.Map<Actor>(actorCreacionDTO);
            if (actorCreacionDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
                    entidad.Foto = await almacenadorArchivo.GuardarArchivo(contenido, extension, contenedor, actorCreacionDTO.Foto.ContentType);
                }
            }
            context.Add(entidad);
            await context.SaveChangesAsync();
            var dto = mapper.Map<ActorDTO>(entidad);
            dto.Foto = entidad.Foto;
            return new CreatedAtRouteResult("ObtenerActor",new { id = entidad.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var actorDB = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (actorDB == null) { return NotFound(); }

            actorDB = mapper.Map(actorCreacionDTO, actorDB);

            if (actorCreacionDTO.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDTO.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreacionDTO.Foto.FileName);
                    actorDB.Foto = await almacenadorArchivo.EditarArchivo(contenido, extension, contenedor,actorDB.Foto, actorCreacionDTO.Foto.ContentType);
                }
            }

            await context.SaveChangesAsync();
            return NoContent();

        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<ActorPatchDTO> patchDocument)
        {
           if (patchDocument == null)
            {
                return BadRequest();
            }

           var entidadDB = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (entidadDB == null)
            {
                return NotFound();
            }

            var entidadDTO = mapper.Map<ActorPatchDTO>(entidadDB);
            patchDocument.ApplyTo(entidadDTO, ModelState);

            var esValido = TryValidateModel(entidadDTO);

            if(!esValido)
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
            var existe = await context.Actores.AnyAsync(x => x.Id == id);
            //Si no existe retornar un 404
            if (!existe)
            {
                return NotFound();
            }

            //Si existe se elimina la entidad
            context.Remove(new Actor() { Id = id });
            //Cambios a la base de datos
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
