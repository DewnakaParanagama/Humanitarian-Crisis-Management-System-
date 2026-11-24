
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Humanitarian.API.Middleware;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>)
                .MakeGenericType(argument.GetType());

            var validator = _serviceProvider.GetService(validatorType)
                as IValidator;

            if (validator is null)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(
                argument);

            var validationResult = await validator.ValidateAsync(
                validationContext,
                context.HttpContext.RequestAborted);

            if (!validationResult.IsValid)
            {
                var errorDetails = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.ErrorMessage)
                            .ToArray());

                context.Result = new BadRequestObjectResult(
                    new ValidationProblemDetails(errorDetails));

                return;
            }
        }

        await next();
    }
}
