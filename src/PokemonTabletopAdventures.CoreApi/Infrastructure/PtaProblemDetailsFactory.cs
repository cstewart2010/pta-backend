using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PokemonTabletopAdventures.CoreApi.Exceptions;

namespace PokemonTabletopAdventures.CoreApi.Infrastructure;

internal class PtaProblemDetailsFactory : ProblemDetailsFactory
{
    public override ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int? statusCode = null,
        string? title = null,
        string? type = null,
        string? detail = null,
        string? instance = null)
    {
        var exceptionHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
        if (exceptionHandlerFeature?.Error is PtaException exception)
        {
            return new ProblemDetails
            {
                Title = exception.Title,
                Detail = exception.Message,
                Status = (int)exception.StatusCode
            };
        }
        else
        {
            return new ProblemDetails
            {
                Title = title,
                Detail = detail,
                Status = statusCode,
                Instance = instance,
                Type = type
            };
        }
    }

    public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null)
    {
        ArgumentNullException.ThrowIfNull(modelStateDictionary);

        statusCode ??= StatusCodes.Status400BadRequest;
        var validationProblemDetails = new ValidationProblemDetails(modelStateDictionary)
        {
            Status = statusCode,
            Type = type,
            Detail = detail,
            Instance = instance
        };
        if (title is not null)
        {
            // For validation problem details, don't overwrite the default title with null.
            validationProblemDetails.Title = title;
        }

        return validationProblemDetails;
    }
}
