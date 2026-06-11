using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using PeliculasAPI.Helpers;
using PeliculasAPI.Servicios;
using System.Text;

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

            services.AddAutoMapper(typeof(Startup)); // Configura AutoMapper, Registrar cada mapper para cada profile  (reflexion [caro en recursos]) 

            services.AddTransient<IAlmacenadorArchivo, AlmacenadorArchivosLocal>(); // Configura el servicio de almacenamiento de archivos
            services.AddHttpContextAccessor(); // Configura el acceso al contexto HTTP para obtener información de la solicitud actual

            services.AddSingleton<GeometryFactory>(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));

            services.AddScoped<PeliculaExisteAttribute>();

            services.AddSingleton(provider =>

                new MapperConfiguration(config =>
                {
                    var geometryFactory = provider.GetRequiredService<GeometryFactory>();
                    config.AddProfile(new AutoMapperProfiles(geometryFactory));
                }).CreateMapper()
            );

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sqlServerOptions => sqlServerOptions.UseNetTopologySuite()));

            services.AddControllers()
                .AddNewtonsoftJson();

            // Configura Identity (usuarios/roles) y los proveedores de tokens.
            // Los tokens JWT se configuran más abajo usando una clave simétrica definida en appsettings.
            services.AddIdentity<IdentityUser,  IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Configura autenticación JWT (validación de token).
            // La clave simétrica se lee desde appsettings.Development.json (jwt:key).
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => 
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(Configuration["jwt:key"])),
                        ClockSkew = TimeSpan.Zero
                    }
                );
            
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

            // Habilita primero el middleware de autenticación para que procese el token
            // y luego el middleware de autorización que comprueba permisos/roles.
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
