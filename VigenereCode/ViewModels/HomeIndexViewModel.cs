
namespace VigenereCode.ViewModels
{
    public class HomeIndexViewModel
    {
        public string DownloadFileName { get; set; }
        public string FileName { get; set; }
        public string SourceText { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Warning { get; set; } = string.Empty;
    }
}
