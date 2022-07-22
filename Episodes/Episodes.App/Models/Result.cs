namespace Episodes.App.Models;

public class Result
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public static Result Ok()
    {
        return new Result()
        {
            Success = true
        };
    }

    public static Result Error(string errorMessage)
    {
        return new Result()
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
