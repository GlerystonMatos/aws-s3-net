using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using AwsS3.Domain;
using AwsS3.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AwsS3.Service
{
    public class AwsS3Service : IAwsS3Service
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly ILogger<AwsS3Service> _logger;

        public AwsS3Service(ILogger<AwsS3Service> logger, IAmazonS3 amazonS3)
        {
            _logger = logger;
            _amazonS3 = amazonS3;
            _logger.LogInformation("Criando serviço Amazon S3...");
        }

        // Criar um bucket se ele não existir
        public async void CreateBucketAsync(string bucketName)
        {
            try
            {
                if (!await IsBucketExist(bucketName))
                {
                    _logger.LogInformation("Criando o bucket no S3 da Amazon...");
                    PutBucketRequest bucketRequest = new PutBucketRequest();
                    bucketRequest.BucketName = bucketName;

                    PutBucketResponse response = await _amazonS3.PutBucketAsync(bucketRequest);

                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        _logger.LogError("Algo deu errado ao criar o bucket no S3 da Amazon", response);
                    }
                    else
                    {
                        _logger.LogInformation("Bucket no S3 da Amazon criado com sucesso");
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError("Algo deu errado", ex);
                throw;
            }
        }

        // Exclui um bucket se ele existir
        public async void DeleteBucketAsync(string bucketName)
        {
            try
            {
                if (await IsBucketExist(bucketName))
                {
                    _logger.LogInformation("Excluindo o bucket no S3 da Amazon...");
                    DeleteBucketResponse response = await _amazonS3.DeleteBucketAsync(bucketName, CancellationToken.None);

                    if (response.HttpStatusCode != HttpStatusCode.NoContent)
                    {
                        _logger.LogError("Algo deu errado ao excluir o bucket no S3 da Amazon", response);
                    }
                    else
                    {
                        _logger.LogInformation("Bucket no S3 da Amazon excluido com sucesso");
                    }
                }
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError("Algo deu errado", ex);
                throw;
            }
        }

        // Uploads para um bucket S3 com nome especificado
        public async void UploadToS3BucketAsync(UploadRequest uploadRequest)
        {
            try
            {
                _logger.LogInformation("Iniciando upload para o bucket no S3 da Amazon...");
                string originalFileName = WebUtility.HtmlEncode(Path.GetFileName(uploadRequest.LocalFilePath));
                string fileExtension = Path.GetExtension(uploadRequest.LocalFilePath).ToLowerInvariant();
                string bucketName = uploadRequest.BucketName;
                string fileName = originalFileName;

                if (!IsValidFile(uploadRequest.LocalFilePath))
                {
                    _logger.LogInformation("Arquivo inválido");
                }
                else
                {
                    if (uploadRequest.RenameFile)
                    {
                        // Renomear o arquivo para uma string aleatória para evitar injeção e ameaças de segurança semelhantes
                        fileName = Path.GetRandomFileName() + fileExtension;
                    }

                    fileName = uploadRequest.BucketPath + fileName;

                    // Criar o objeto de imagem a ser carregado na memória
                    PutObjectRequest putObjectRequest = new PutObjectRequest();
                    putObjectRequest.FilePath = uploadRequest.LocalFilePath; // O caminho completo do arquivo local
                    putObjectRequest.Key = fileName; // O nome de armazenamento do arquivo.
                    putObjectRequest.BucketName = bucketName; // Especifica o bucket de destino para upload
                    putObjectRequest.CannedACL = S3CannedACL.PublicRead; // Certifique-se de que o arquivo seja somente leitura para permitir que os usuários vejam suas fotos
                    putObjectRequest.Metadata.Add("originalFileName", originalFileName); // Nome original do arquivo

                    await _amazonS3.PutObjectAsync(putObjectRequest);

                    _logger.LogInformation("Arquivo carregado para o bucket no S3 da Amazon com sucesso");
                }
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError("Os dados do arquivo não estão contidos no formulário", ex);
                throw;
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError("Algo deu errado durante o upload do arquivo", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Algo deu errado durante o upload do arquivo", ex);
                throw;
            }
        }

        // Excluir arquivo de um bucket S3 com o nome especificado
        public async void DeleteToS3BucketAsync(DeleteRequest deleteRequest)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão do arquivo no bucket no S3 da Amazon...");
                string bucketName = deleteRequest.BucketName;

                // Criar o objeto de imagem a ser carregado na memória
                DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest();
                deleteObjectRequest.BucketName = bucketName; // Especifica o bucket de origem do arquivo
                deleteObjectRequest.Key = deleteRequest.FileKey; // O nome de armazenamento do arquivo.

                await _amazonS3.DeleteObjectAsync(deleteObjectRequest);

                _logger.LogInformation("Arquivo excluído do bucket no S3 da Amazon com sucesso");
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError("Algo deu errado durante a exclusão do arquivo", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Algo deu errado durante a exclusão do arquivo", ex);
                throw;
            }
        }

        // Gerar URL temporária para acessar o arquivo
        public string GetPreSignedURL(UrlRequest urlRequest)
        {
            try
            {
                GetPreSignedUrlRequest getPreSignedUrlRequest = new GetPreSignedUrlRequest();
                getPreSignedUrlRequest.Expires = DateTime.Now.AddMinutes(urlRequest.Duration);
                getPreSignedUrlRequest.BucketName = urlRequest.BucketName;
                getPreSignedUrlRequest.Key = urlRequest.FileKey;

                _logger.LogInformation("Duração: " + getPreSignedUrlRequest.Expires);

                return _amazonS3.GetPreSignedURL(getPreSignedUrlRequest);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError("Algo deu errado durante a geração da URL", ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("Algo deu errado durante a geração da URL", ex);
                throw;
            }
        }

        // Verifica se o bucket informado existe
        private async Task<bool> IsBucketExist(string bucketName)
        {
            _logger.LogInformation("Verificando se o bucket no S3 da Amazon existe...");
            if (await AmazonS3Util.DoesS3BucketExistV2Async(_amazonS3, bucketName))
            {
                _logger.LogInformation($"O bucket no S3 da Amazon com o nome '{bucketName}' já existe");
                return true;
            }
            else
            {
                _logger.LogInformation($"O bucket no S3 da Amazon com o nome '{bucketName}' não existe");
                return false;
            }
        }

        // Verifica se um arquivo enviado corresponde às restrições aceitas
        private bool IsValidFile(string localFilePath)
        {
            FileStream file = File.OpenRead(localFilePath);

            // Verifique o tamanho do arquivo
            if (file.Length < 0)
            {
                return false;
            }

            // Verifique a extensão do arquivo para evitar ameaças de segurança associadas a tipos de arquivo desconhecidos
            string[] permittedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".pdf", ".rar", ".zip", ".xml", ".json" };
            string ext = Path.GetExtension(localFilePath).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains<string>(ext))
            {
                return false;
            }

            // Verifique se o tamanho do arquivo é maior do que o limite permitido
            if (file.Length > 10485760) // 10 MB (10485760 B)
            {
                return false;
            }

            return true;
        }
    }
}