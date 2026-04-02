namespace ChatSpot.Dtos;

public class BaseResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string? Data { get; set; }
}