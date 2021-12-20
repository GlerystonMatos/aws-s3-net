using System.ComponentModel.DataAnnotations;

namespace AwsS3.Domain
{
    public class DeleteRequest
    {
        [Required]
        public string BucketName { get; set; }

        [Required]
        public string FileKey { get; set; }
    }
}