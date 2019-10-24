using Storage.ClientSDK;
using Storage.ClientSDK.Contracts;
using Storage.ClientSDK.Contracts.Request;
using System;
using System.Text.RegularExpressions;

namespace StorageGlacierRetrieval.Helper
{
    public class ApiHelper
    {
        private ApiGatewayStorageClient _storageClient;
        public ApiHelper(string apiUser, string apiPass, string endpoint)
        {
            if(string.IsNullOrEmpty(apiUser))
            {
                throw new ArgumentNullException("Invalid API User name");
            }

            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException("Invalid API Endpoint");
            }

            if (string.IsNullOrEmpty(apiPass))
            {
                throw new ArgumentNullException("Invalid API Password");
            }

            _storageClient = new ApiGatewayStorageClient(new StorageEndpoint {
                Credentials = new StorageCredentials
                {
                    UserName = apiUser,
                    Password = apiPass
                },
                Host = endpoint
            }, AuthType.Basic);
        }

        public string GetFilePath(int bu, Guid id)
        {
            Console.WriteLine(string.Format("ApiHelper -> Get File Path -> Bu: {0}, Id: {1}", bu, id));
            var response = _storageClient.GetFileInformation(new GetFileRequest
            {
                BusinessUnit = bu,
                Id = id
            });
            Console.WriteLine(string.Format("ApiHelper -> Get File Path -> Bu: {0}, Id: {1}, SignedUrl{2}", bu, id, response.Result.SignedUrl));

            string filePath = null;
            if(!string.IsNullOrEmpty(response.Result.SignedUrl))
            {
                filePath = Regex.Match(
                    response.Result.SignedUrl,
                    string.Format("{0}(.*){1}", "amazonaws.com/", "\\?X-Amz"))
                .Groups[1].Value;
            }

            return filePath;
        }
    }
}
