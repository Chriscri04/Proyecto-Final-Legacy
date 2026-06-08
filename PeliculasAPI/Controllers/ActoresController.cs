using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/actores")]
    public class ActoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        public ActoresController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get()
        {
            var entidades = await context.Actores.ToListAsync();
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
        public async Task<ActionResult> Post([FromBody] ActorCreacionDTO actorCreacionDTO)
        {
            var entidad = mapper.Map<Actor>(actorCreacionDTO);

            context.Add(entidad);
            await context.SaveChangesAsync();
            var dto = mapper.Map<ActorDTO>(entidad);
            return new CreatedAtRouteResult("ObtenerActor",
                                             new { id = entidad.Id },
                                             dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] ActorCreacionDTO actorCreacionDTO)
        {
            var entidad = mapper.Map<Actor>(actorCreacionDTO);
            entidad.Id = id;
            context.Entry(entidad).State = EntityState.Modified;
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
