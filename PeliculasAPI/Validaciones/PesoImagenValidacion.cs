using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Validaciones
{
    public class PesoImagenValidacion : ValidationAttribute
    {
        private readonly int pesoMaximoEnMegas;
        public PesoImagenValidacion(int pesoMaximoEnMegas)
        {
            this.pesoMaximoEnMegas = pesoMaximoEnMegas;
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            IFormFile formFile = value as IFormFile;

            if (formFile == null)
            {
                return ValidationResult.Success;
            }

            if (formFile.Length > pesoMaximoEnMegas * 1024 * 1024)
            {
                return new ValidationResult($"El peso máximo permitido es de {pesoMaximoEnMegas} MB.");
            }

            return ValidationResult.Success;
        }
    }
}
