using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MicroBlog.Server.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;

namespace MicroBlog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Writer")]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private static readonly string[] ALLOWED_TYPES =
            { ".png", ".jpg", ".gif", ".jpeg", ".mp3", ".mp4" };

        public UploadController(ILogger<UploadController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get(string name)
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", name);
            var provider = new FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(path, out contentType))
            {
                contentType = "application/octet-stream";
            }

            if (!System.IO.File.Exists(path))
                return NotFound(name + "Not Found!");
            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new FileStreamResult(fs, contentType);
            //return PhysicalFile(path, contentType, true);
        }

        [HttpPost]
        [SwaggerResponse(System.Net.HttpStatusCode.Created, typeof(Response<string>))]
        [SwaggerResponse(System.Net.HttpStatusCode.BadRequest, typeof(Response<string>))]
        [SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(Response<string>))]
        public async Task<ActionResult<Response<string>>> Post(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string fileExtesnsion = Path.GetExtension(file.FileName);
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                _logger.LogInformation("upload file info, FileName: {filename}, Name: {name}, type {type}", file.FileName, file.Name, file.ContentType);

                if (!ALLOWED_TYPES.Any(x => x.Equals(fileExtesnsion)))
                {
                    _logger.LogWarning("trying to send invalid file {ext}", fileExtesnsion);
                    return BadRequest($"Invalid file type, only jpg, jpeg and png are allowed.");
                }
                try
                {
                    string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName + fileExtesnsion);
                    using var stream = new FileStream(uploadPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    _logger.LogInformation("new file stored into the disk, file path is: {path}", uploadPath);

                    string filePath = Path.Combine("uploads", fileName + fileExtesnsion);
                    return Ok(new Response<string>(data: filePath));
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
