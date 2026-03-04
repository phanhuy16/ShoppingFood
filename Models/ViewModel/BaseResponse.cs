namespace ShoppingFood.Models.ViewModel
{
    public class BaseResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static BaseResponse<T> Ok(T data, string message = "Success") => new() { Success = true, Data = data, Message = message };
        public static BaseResponse<T> Fail(string message) => new() { Success = false, Message = message };
    }
}
