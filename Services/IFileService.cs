namespace ShoppingFood.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        void DeleteFile(string fileName, string folder);
    }
}
