using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.Servicios;

namespace PeliculasAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Configuración de servicios, como controladores, bases de datos, etc.

            services.AddAutoMapper(typeof(Startup)); // Configura AutoMapper

            services.AddTransient<IAlmacenadorArchivo, AlmacenadorArchivosLocal>(); // Configura el servicio de almacenamiento de archivos
            services.AddHttpContextAccessor(); // Configura el acceso al contexto HTTP para obtener información de la solicitud actual

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers()
                .AddNewtonsoftJson();
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(); // Habilita el uso de archivos estáticos, como imágenes

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
