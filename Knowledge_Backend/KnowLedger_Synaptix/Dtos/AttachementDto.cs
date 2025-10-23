namespace KnowLedger_Synaptix.Dtos
{
    public class AttachmentDto
    {
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public string? FileUrl { get; set; }
        public long? FileSize { get; set; }
        public string? FilePath {  get; set; }
    }
}
