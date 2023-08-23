using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace NewsAPI.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public static string LocalizationKey => "LocalizationKey";

        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "error during executing {Context}", context.Request.Path.Value);
                var response = context.Response;

                var (status, message) = GetResponse(exception);
                response.StatusCode = (int)status;
                await response.WriteAsync(message);
            }
        }
        public (HttpStatusCode code, string message) GetResponse(Exception exception)
        {
            if (exception is KeyNotFoundException)
            {
                return (HttpStatusCode.NotFound, "Key not exist");
            }
            else if (exception is InvalidOperationException)
            {
                return (HttpStatusCode.BadRequest, "Bad request");
            }

            return (HttpStatusCode.InternalServerError, "Something went wrong");
        }

    }
}
