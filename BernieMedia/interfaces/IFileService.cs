namespace BernieMedia
{
    public interface IFileService
    {
        string SelectMediaFile();
        bool IsAudioFile(string filePath);
        bool IsVideoFile(string filePath);
    }
}
