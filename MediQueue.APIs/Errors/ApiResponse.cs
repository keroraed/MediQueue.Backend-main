namespace MediQueue.APIs.Errors;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public ApiResponse(int statusCode, string message = null)
    {
        StatusCode = statusCode;
        Message = message ?? GetDefaultMessageForStatusCode(statusCode);
    }

    private string GetDefaultMessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            200 => "تمت العملية بنجاح",
            201 => "تم الإنشاء بنجاح",
            204 => "لا يوجد محتوى",
            400 => "طلب غير صحيح",
            401 => "غير مصرح بالوصول",
            403 => "محظور",
            404 => "المورد غير موجود",
            500 => "خطأ داخلي في الخادم",
            _ => "حدث خطأ غير معروف"
        };
    }
}
