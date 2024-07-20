using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Project_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {      
        private readonly string _imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedImages");

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                // Tạo thư mục lưu trữ nếu chưa tồn tại
                if (!Directory.Exists(_imageFolder))
                {
                    Directory.CreateDirectory(_imageFolder);
                }

                // Tạo tên tệp duy nhất để tránh trùng lặp
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(_imageFolder, uniqueFileName);

                // Lưu tệp vào thư mục
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Lưu đường dẫn tệp vào cơ sở dữ liệu
                SaveImagePathToDatabase(filePath);

                return Ok(filePath);
            }

            return BadRequest("No file uploaded.");
        }

        private void SaveImagePathToDatabase(string filePath)
        {
            // Viết mã để lưu đường dẫn ảnh vào cơ sở dữ liệu tại đây
            // Ví dụ sử dụng Entity Framework:
            // using (var context = new YourDbContext())
            // {
            //     var img = new ImageModel { FilePath = filePath };
            //     context.Images.Add(img);
            //     context.SaveChanges();
            // }
        }
    }
}

