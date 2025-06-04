using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuizAppBackend.DTOs; // Ensure this is present
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuizAppBackend.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message); // Log the exception
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Return different details based on environment
                var response = _env.IsDevelopment()
                    ? new ErrorDto { Message = "Ett oväntat fel uppstod.", Details = ex.ToString(), Code = "UNHANDLED_EXCEPTION" } // Include stack trace in dev
                    : new ErrorDto { Message = "Ett oväntat fel uppstod.", Code = "UNHANDLED_EXCEPTION" }; // Generic message in production

                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
            }
        }
    }
}