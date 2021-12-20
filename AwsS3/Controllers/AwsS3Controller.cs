using AwsS3.Domain;
using AwsS3.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AwsS3.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AwsS3Controller : ControllerBase
    {
        private readonly IAwsS3Service _awsS3;

        public AwsS3Controller(IAwsS3Service awsS3)
            => _awsS3 = awsS3;

        [HttpPost("CreateBucket")]
        public IActionResult CreateBucketAsync(string bucketName)
        {
            _awsS3.CreateBucketAsync(bucketName);
            return Ok();
        }

        [HttpDelete("DeleteBucket")]
        public IActionResult DeleteBucketAsync(string bucketName)
        {
            _awsS3.DeleteBucketAsync(bucketName);
            return Ok();
        }

        [HttpPost("UploadFile")]
        public IActionResult UploadToS3BucketAsync([FromForm] UploadRequest uploadRequest)
        {
            _awsS3.UploadToS3BucketAsync(uploadRequest);
            return Ok();
        }

        [HttpDelete("DeleteFile")]
        public IActionResult DeleteToS3BucketAsync([FromForm] DeleteRequest deleteRequest)
        {
            _awsS3.DeleteToS3BucketAsync(deleteRequest);
            return Ok();
        }

        [HttpGet("PreSignedURL")]
        public IActionResult GetPreSignedURL([FromQuery] UrlRequest urlRequest)
            => Ok(_awsS3.GetPreSignedURL(urlRequest));
    }
}