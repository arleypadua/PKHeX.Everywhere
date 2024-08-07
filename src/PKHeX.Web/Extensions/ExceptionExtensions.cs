namespace PKHeX.Web.Extensions;

public static class ExceptionExtensions
{
    public static string GetExceptionTrackingId(this Exception ex)
    {
        if (!ex.Data.Contains("ExceptionId"))
            ex.Data["ExceptionId"] = Guid.NewGuid().ToString();
        
        return ex.Data["ExceptionId"]!.ToString()!;
    }
}