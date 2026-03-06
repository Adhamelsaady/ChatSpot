namespace ChatSpot.Contracts.Infrastructure;

public interface IEmailService
{
    Task SendEmailConfirmationOtpAsync(string email, string firstName, string otp);
}