using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTOs;
using PeliculasAPI.Entidades;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/generos")]
    public class GenerosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public GenerosController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        //CRUD de generos

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get()
        {
            var entidades = await context.Generos.ToListAsync();
            var dtos = mapper.Map<List<GeneroDTO>>(entidades);
            return dtos;
        }

        [HttpGet("{id:int}", Name = "ObtenerGenero")]
        public async Task<ActionResult<GeneroDTO>> Get(int id)
        {
            //Obtener la entidad desde la base de datos
            var entidad = await context.Generos.FirstOrDefaultAsync(x => x.Id == id);
            //Si no existe retornar un 404
            if (entidad == null)
            {
                return NotFound();
            }
            //Mapear la entidad a un DTO
            var dto = mapper.Map<GeneroDTO>(entidad);

            return dto;
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            //Realizar un mapeo
            var entidad = mapper.Map<Genero>(generoCreacionDTO);

            //se agrega la entidad 
            context.Add(entidad);

            //Cambios a la base de datos
            await context.SaveChangesAsync();

            //mappear de nuevo a un DTO para retornar al cliente 
            var generoDTO = mapper.Map<GeneroDTO>(entidad);

            //Retornar un 201 con la ruta para obtener el nuevo recurso creado
            return new CreatedAtRouteResult("ObtenerGenero", new { id = generoDTO.Id }, generoDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            //Validar que el id exista en la base de datos
            var entidad = mapper.Map<Genero>(generoCreacionDTO);
            //Si no existe retornar un 404
            //Si existe se actualiza la entidad
            //Se asigna el id a la entidad para que EF Core sepa que es una actualización
            entidad.Id = id;
            //Se marca la entidad como modificada
            context.Entry(entidad).State = EntityState.Modified;
            //Cambios a la base de datos
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            //Validar que el id exista en la base de datos
            var existe = await context.Generos.AnyAsync(x => x.Id == id);
            //Si no existe retornar un 404
            if (!existe)
            {
                return NotFound();
            }

            //Si existe se elimina la entidad
            context.Remove(new Genero() { Id = id });
            //Cambios a la base de datos
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
