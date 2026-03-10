namespace EducAIte.Application.Services.Implementation;

using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using EducAIte.Application.Services.Interface;

public class AWSService(
    IAmazonS3 s3Client,
    string bucketName,
    string cloudFrontDomain,
    string cloudFrontKeyPairId,
    string cloudFrontPrivateKey) : IAWSService
{
    private readonly IAmazonS3 _s3Client = s3Client;
    private readonly string _bucketName = bucketName;
    private readonly string _cloudFrontDomain = cloudFrontDomain;
    private readonly string _cloudFrontKeyPairId = cloudFrontKeyPairId;
    private readonly string _cloudFrontPrivateKey = cloudFrontPrivateKey;

    public async Task<string> UploadFileAsync(IFormFile file, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = $"{Guid.NewGuid()}_{file.FileName}";

        using (var stream = file.OpenReadStream())
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);
        }

        return key;
    }

    public string GenerateSignedUrl(string key, TimeSpan validFor, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var expiry = (long)(DateTime.UtcNow.Add(validFor) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        var policy = $"{{\"Statement\":[{{\"Resource\":\"https://{_cloudFrontDomain}/{key}\",\"Condition\":{{\"DateLessThan\":{{\"AWS:EpochTime\":{expiry}}}}}}}]}}";

        using var rsa = RSA.Create();
        rsa.ImportFromPem(_cloudFrontPrivateKey);
        var signature = rsa.SignData(Encoding.UTF8.GetBytes(policy), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

        var encodedSignature = Convert.ToBase64String(signature)
            .Replace('+', '-')
            .Replace('=', '_')
            .Replace('/', '~');

        return $"https://{_cloudFrontDomain}/{key}?Expires={expiry}&Signature={encodedSignature}&Key-Pair-Id={_cloudFrontKeyPairId}";
    }
}
