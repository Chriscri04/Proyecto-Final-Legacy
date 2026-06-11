using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.Entidades;
using System.Security.Claims;

namespace PeliculasAPI
{
    public class ApplicationDbContext: IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PeliculasActores>()
                .HasKey(x => new { x.ActorId, x.PeliculaId });

            modelBuilder.Entity<PeliculasGeneros>()
                .HasKey(x => new { x.GeneroId , x.PeliculaId });
            modelBuilder.Entity<PeliculasSalasDeCine>()
                .HasKey(x => new { x.PeliculaId, x.SaladeCineId });

            base.OnModelCreating(modelBuilder);

           // SeedData(modelBuilder);
        }

        //private void seeddata(modelbuilder modelbuilder)
        //{
        //    var roladminid = "9aae0b6d-d50c-4d0a-9b90-2a6873e3845d";
        //    var usuarioadminid = "5673b8cf-12de-44f6-92ad-fae4a77932ad";

        //    var roladmin = new identityrole()
        //    {
        //        id = roladminid,
        //        name = "admin",
        //        normalizedname = "admin"
        //    };

        //    var passwordhasher = new passwordhasher<identityuser>();
        //    var username = "chrispaz@hotmail.com";
        //    var usuarioadmin = new identityuser()
        //    {
        //        id = usuarioadminid,
        //        username = username,
        //        normalizedusername = username,
        //        email = username,
        //        normalizedemail = username,
        //        passwordhash = passwordhasher.hashpassword(null, "aa123456!")
        //    };

            //modelbuilder.entity<identityuser>()
            //    .hasdata(usuarioadmin);
            //modelbuilder.entity<identityrole>()
            //    .hasdata(roladmin);
            //modelbuilder.entity<identityuserclaim<string>>()
            //    .hasdata(new identityuserclaim<string>()
            //    {
            //        id = 1,
            //        claimtype = claimtypes.role,
            //        userid = usuarioadminid,
            //        claimvalue = "admin"
            //    });
        //}


        public DbSet<Genero> Generos { get; set; }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<PeliculasActores> PeliculasActores { get; set; }
        public DbSet<PeliculasGeneros> PeliculasGeneros { get; set; }
        public DbSet<SalaDeCine> SalasDeCine { get; set; }
        public DbSet<PeliculasSalasDeCine> PeliculasSalasDeCines { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
