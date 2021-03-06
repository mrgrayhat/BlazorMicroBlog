using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MicroBlog.Server.Wrappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MicroBlog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly string[] ALLOWED_TYPES =
            { "png", "jpg", "gif", "jpeg", "mp3", "mp4" };

        public UploadController(ILogger<UploadController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(string path)
        {
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(fs, "");
        }

        [HttpPost]
        public async Task<ActionResult<Response<string>>> Post(IFormFile file)
        {
            if (file?.Length > 0)
            {
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                string fileExtesnsion = Path.GetExtension(file.FileName);
                string fileName = Path.GetFileNameWithoutExtension(file.Name);
                if (!ALLOWED_TYPES.Contains(fileExtesnsion))
                {
                    _logger.LogWarning("trying to send invalid file {ext}", fileExtesnsion);
                    return BadRequest($"Invalid file type, only {ALLOWED_TYPES} are allowed.");
                }
                string fullPath = Path.Combine(uploadPath, fileName, fileExtesnsion);
                try
                {
                    using var stream = new FileStream(fullPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    _logger.LogInformation("new file stored into the disk, file path is: {path}", fullPath);

                    return Ok(new Response<string>(data: fullPath));
                }
                catch (Exception ex)
                {
                    _logger.LogError("an error occured while saving the file {ex}", ex);
                    return StatusCode(500, ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("trying to send empty file");
                return BadRequest(new Response<string>(message: "File is empty!"));
            }
        }

    }
}
