namespace Humanitarian.Application.Interfaces;

public interface IEmailService
{
    Task SendOtpEmailAsync(string email, string otp);
}