using AwsS3.Domain;

namespace AwsS3.Interfaces
{
    public interface IAwsS3Service
    {
        void CreateBucketAsync(string bucketName);

        void DeleteBucketAsync(string bucketName);

        void UploadToS3BucketAsync(UploadRequest uploadRequest);

        void DeleteToS3BucketAsync(DeleteRequest deleteRequest);

        string GetPreSignedURL(UrlRequest urlRequest);
    }
}