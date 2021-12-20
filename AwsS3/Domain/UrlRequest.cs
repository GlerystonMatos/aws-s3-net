using System.ComponentModel.DataAnnotations;

namespace AwsS3.Domain
{
    public class UrlRequest
    {
        [Required]
        public string BucketName { get; set; }

        [Required]
        public string FileKey { get; set; }

        [Required]
        public double Duration { get; set; }
    }
}