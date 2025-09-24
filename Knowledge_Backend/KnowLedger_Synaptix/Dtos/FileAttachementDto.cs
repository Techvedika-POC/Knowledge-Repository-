using System;
using Microsoft.AspNetCore.Http;

namespace KnowLedger_Synaptix.Dtos
{
    public class FileAttachmentDto
    {
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long FileSize { get; set; }
        public byte[] FileData { get; set; }

        public static FileAttachmentDto FromFormFile(IFormFile file)
        {
            if (file == null) return null;

            using var ms = new System.IO.MemoryStream();
            file.CopyTo(ms);
            return new FileAttachmentDto
            {
                FileName = file.FileName,
                MimeType = file.ContentType,
                FileSize = file.Length,
                FileData = ms.ToArray()
            };
        }
    }
}
