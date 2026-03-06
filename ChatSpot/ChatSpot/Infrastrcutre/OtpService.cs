using ChatSpot.Contracts.Infrastructure;

namespace ChatSpot.Infrastrcutre;

public class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@!$#_";
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}