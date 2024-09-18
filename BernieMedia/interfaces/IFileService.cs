namespace BernieMedia
{
    public interface IFileService
    {
        string SelectMediaFile();
        string GetFileName(string filePath);
        bool IsAudioFile(string filePath);
        bool IsVideoFile(string filePath);
    }
}
