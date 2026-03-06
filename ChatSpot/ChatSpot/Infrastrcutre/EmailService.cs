using ChatSpot.Contracts.Infrastructure;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ChatSpot.Infrastrcutre;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailConfirmationOtpAsync(string email, string firstName, string otp)
    {
        var subject = "Confirm Your Email - ChatSpot";
        var body = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
                    <h1 style='color: white; margin: 0;'>Welcome to ChatSpot! 🎉</h1>
                </div>
                
                <div style='background-color: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
                    <h2 style='color: #333;'>Hello {firstName},</h2>
                    <p>Thank you for registering with ChatSpot! To complete your registration, please verify your email address.</p>
                    
                    <div style='background-color: white; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                        <p style='margin: 0 0 10px 0; color: #666;'>Your verification code is:</p>
                        <h1 style='color: #667eea; font-size: 48px; letter-spacing: 8px; margin: 0; font-weight: bold;'>{otp}</h1>
                    </div>
                    
                    <p>This code will expire in <strong>10 minutes</strong>.</p>
                    
                    <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 12px; margin: 20px 0; border-radius: 4px;'>
                        <p style='margin: 0; color: #856404;'>⚠️ If you didn't create an account with ChatSpot, please ignore this email.</p>
                    </div>
                    
                    <p style='margin-top: 30px; color: #666; font-size: 14px;'>
                        Best regards,<br>
                        <strong>The ChatSpot Team</strong>
                    </p>
                </div>
                
                <div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>
                    <p>© 2026 ChatSpot. All rights reserved.</p>
                    <p>Your Premier Event Ticketing Platform</p>
                </div>
            </body>
            </html>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var otpMatch = System.Text.RegularExpressions.Regex.Match(
            body,
            @"letter-spacing: 8px;[^>]*>(\d{6})</h1>");
        var otp = otpMatch.Success ? otpMatch.Groups[1].Value : "N/A";
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            emailSettings["SenderName"],
            emailSettings["SenderEmail"]));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(
            emailSettings["SmtpServer"],
            int.Parse(emailSettings["SmtpPort"]!),
            SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(
            emailSettings["SmtpUsername"],
            emailSettings["SmtpPassword"]);
        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}