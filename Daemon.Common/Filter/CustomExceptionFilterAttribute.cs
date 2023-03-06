using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using Daemon.Common.Exceptions;

namespace Daemon.Common.Filter
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<CustomExceptionFilterAttribute> _logger;

        public CustomExceptionFilterAttribute(ILogger<CustomExceptionFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled)
            {
                HandleAPIException(context);
            }
            
            context.ExceptionHandled = true;
        }

        private void HandleAPIException(ExceptionContext actionExecutedContext)
        {
            HttpStatusCode statusCode;
            if (actionExecutedContext.Exception is DownloadKeyExpiredException
                || actionExecutedContext.Exception is NonexistentFieldException
                || actionExecutedContext.Exception is NonexistentEntityException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (actionExecutedContext.Exception is ValueDuplicateException)
            {
                statusCode = HttpStatusCode.Conflict;
            }
            else if (actionExecutedContext.Exception is UnprocessableEntityException ||
                actionExecutedContext.Exception is BadRequestException ||
                actionExecutedContext.Exception is GridFilterException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            else if (actionExecutedContext.Exception is PreconditionFailedException)
            {
                statusCode = HttpStatusCode.PreconditionFailed;
            }
            else
            {
                statusCode = HttpStatusCode.NotFound;
            }

            actionExecutedContext.Result = new ResultModel(statusCode);
        }
    }
}
