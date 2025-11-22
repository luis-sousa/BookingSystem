using FluentValidation;
using System.Net;
using System.Text.Json;

namespace UserService.Middleware
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ve)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

                var errors = ve.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                var response = JsonSerializer.Serialize(new { errors }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                await context.Response.WriteAsync(response);
            }
            catch (BusinessException be)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = be.HttpCode;

                var response = JsonSerializer.Serialize(new { code = be.Code, message = be.Message },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                await context.Response.WriteAsync(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO REAL → " + ex);  // 👈 ADICIONA AQUI

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = JsonSerializer.Serialize(new { message = "Ocorreu um erro interno no servidor." },
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

                await context.Response.WriteAsync(response);

                // IMPORTANTE TEMPORARIAMENTE:
                throw; // 👈 ADICIONA ISTO PARA VER NO TESTE A EXCEÇÃO REAL
            }
        }
    }   
}