using System.Security.Authentication;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace OnlineQueueAPI.Services
{
    public static class ExceptionHelper
    {
        public static IActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                ArgumentNullException => new BadRequestObjectResult(ex.Message),
                InvalidDataException => new BadRequestObjectResult(ex.Message),
                ArgumentException => new NotFoundObjectResult(ex.Message),
                InvalidOperationException => new ConflictObjectResult(ex.Message),
                AutoMapperMappingException => new ConflictObjectResult(ex.Message),
                AuthenticationException => new UnauthorizedObjectResult(ex.Message),
                _ => new ObjectResult($"Internal server error: {ex.Message}") { StatusCode = 500 }
            };
        }
    }
}