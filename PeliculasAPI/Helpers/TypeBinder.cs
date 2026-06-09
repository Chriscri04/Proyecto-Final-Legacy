using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace PeliculasAPI.Helpers
{
    public class TypeBinder<T>: IModelBinder

    //Comentario General sobre la clase: TypeBinder sireve para Deserializa un valor JSON de la petición a T para que el parámetro del controlador llegue ya convertido.
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var nombrePropiedad = bindingContext.ModelName;
            var proveedoresDeValores = bindingContext.ValueProvider.GetValue(nombrePropiedad);

            if (proveedoresDeValores == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }
            try
            {
                var valorDeserializado = JsonConvert.DeserializeObject<T>(proveedoresDeValores.FirstValue);
                bindingContext.Result = ModelBindingResult.Success(valorDeserializado);
            }
            catch
            {
                bindingContext.ModelState.TryAddModelError(nombrePropiedad, "Valor invalido para tipo List<int>");
            }
            return Task.CompletedTask;
        }
    }
}
