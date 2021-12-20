using System.ComponentModel.DataAnnotations;

namespace AwsS3.Domain
{
    public class UploadRequest
    {
        [Required]
        public string BucketName { get; set; }

        [Required]
        public string LocalFilePath { get; set; }

        [Required]
        public string BucketPath { get; set; }

        [Required]
        public bool RenameFile { get; set; }
    }
}