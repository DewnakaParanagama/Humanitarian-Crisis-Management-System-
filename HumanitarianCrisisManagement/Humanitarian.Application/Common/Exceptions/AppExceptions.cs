namespace Humanitarian.Application.Common.Exceptions;

/// <summary>The requested record does not exist (HTTP 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.")
    {
    }
}

/// <summary>The request clashes with existing data, e.g. a duplicate (HTTP 409).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

/// <summary>The user is logged in but not allowed to do this (HTTP 403).</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(
        string message = "You do not have permission to perform this action.")
        : base(message)
    {
    }
}

/// <summary>The request is valid but breaks a business rule (HTTP 422).</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}

/// <summary>One or more input fields are invalid (HTTP 400).</summary>
public class AppValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public AppValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] })
    {
    }
}