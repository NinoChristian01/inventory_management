using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ServiceDesk.Web.Helpers;

/// <summary>
/// TempData keys read by the _Toasts partial after a POST-redirect-GET. Messages are full
/// sentences that say what happened, for example "Ticket MIS-26K013 resolved."
/// </summary>
public static class Toasts
{
    public const string Success = "Success";
    public const string Error = "Error";
    public const string Info = "Info";
    public const string Warning = "Warning";

    public static void ToastSuccess(this ITempDataDictionary tempData, string message) => Set(tempData, Success, message);

    public static void ToastError(this ITempDataDictionary tempData, string message) => Set(tempData, Error, message);

    public static void ToastInfo(this ITempDataDictionary tempData, string message) => Set(tempData, Info, message);

    public static void ToastWarning(this ITempDataDictionary tempData, string message) => Set(tempData, Warning, message);

    private static void Set(ITempDataDictionary tempData, string key, string message)
    {
        ArgumentNullException.ThrowIfNull(tempData);
        tempData[key] = message;
    }
}
