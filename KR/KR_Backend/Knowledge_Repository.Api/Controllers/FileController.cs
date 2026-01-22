using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Knowledge_Repository.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly string _uploadRoot;

        public FileController()
        {
            _uploadRoot = Path.Combine(AppContext.BaseDirectory, "uploads");

            if (!Directory.Exists(_uploadRoot))
                Directory.CreateDirectory(_uploadRoot);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file received.");

            var originalName = Path.GetFileName(file.FileName);
            var safeName = originalName.Replace(" ", "_").Replace("(", "").Replace(")", "");
            var newFileName = $"{Guid.NewGuid()}_{safeName}";

            var filePath = Path.Combine(_uploadRoot, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var publicUrl = $"{baseUrl}/uploads/{newFileName}";

            return Ok(new
            {
                url = publicUrl,                
                relativePath = $"/uploads/{newFileName}",
                fileName = originalName,
                storedName = newFileName
            });
        }
    }
}
