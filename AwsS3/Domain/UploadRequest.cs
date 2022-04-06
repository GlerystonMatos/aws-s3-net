using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AwsS3.Domain
{
    public class UploadRequest
    {
        [Required]
        public string BucketName { get; set; }

        [Required]
        public IFormFile File { get; set; }

        [Required]
        public string BucketPath { get; set; }

        [Required]
        public bool RenameFile { get; set; }
    }
}