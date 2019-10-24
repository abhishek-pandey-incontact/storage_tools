using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System;
using System.Configuration;
using System.Net;

namespace StorageGlacierRetrieval.Helper
{
    public class S3Helper
    {
        private AmazonS3Client s3Client;
        public S3Helper(string region)
        {
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException("Invalid region");
            }
 
            Console.WriteLine("in getS3Access");
     /*       AmazonSecurityTokenServiceClient stsClient = new AmazonSecurityTokenServiceClient();
            AssumeRoleResponse res = stsClient.AssumeRole(new AssumeRoleRequest()
            {
                RoleArn = ConfigurationManager.AppSettings["roleArn"],
                RoleSessionName = ConfigurationManager.AppSettings["roleSessionName"]
            });*/

            string tempAccessKeyId = "ASIA2XNQT5DDURHCA255";
            string tempSessionToken = "FQoGZXIvYXdzECIaDJQo8U/ByqxI/REfESLrAebHclvD5HZ1RhzIdve9U7et5fJeUS1qmsvZHI/nksNkCWFk7ftzP+rr/nrSYGykHIH0ao58L3M8V68GSLLaxbHy1xrcJcDlZkYixijeQ7kSpQCzcLTuIqYacBrKx8jgMgqAQ7LDYzuJk+2eUUJTUElDF6zI7e84l9VQlmoecaR+YB/wKCzs5H59yDytgmGVu5HAPO6a8tH3SO4gpp3eF1Orv/f40kV4gPYFI42FYmDwoFwO55DjDxkNmxFF8iCVc2Se96pPvWmD+Kry8EF15Nl65axE8r/JSRt0noNEgrAxHmMArFpoLH87OYUo+/jB7QU=";
            string tempSecretAccessKey = "eTmdHFDu4kbJPk+c1gVsX0YImhapXWptjFTKQOEV";
            Console.WriteLine("tempAccessKeyId::" + tempAccessKeyId);
            Console.WriteLine("tempSessionToken::" + tempSessionToken);
            Console.WriteLine("tempSecretAccessKey::" + tempSecretAccessKey);
            SessionAWSCredentials tempCredentials = new SessionAWSCredentials(tempAccessKeyId, tempSecretAccessKey, tempSessionToken);

            s3Client = new AmazonS3Client(tempAccessKeyId, tempSecretAccessKey, tempSessionToken, RegionEndpoint.GetBySystemName("us-west-2"));
            Console.WriteLine("S3 client is created:: ");
        }
        
        public HttpStatusCode RestoreObject(string bucket, string key, int days,int index,FileReader fileReader)
        {
            if (string.IsNullOrEmpty(bucket))
            {
                throw new ArgumentNullException("Invalid bucket");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("Invalid key");
            }

            if (days <= 0)
            {
                throw new ArgumentNullException(string.Format("Invalid days: {0}", days));
            }

            Console.WriteLine(
                string.Format("S3Helper -> RestoreObject -> Restoring file: Bucket: {0}, Key: {1}, Days: {2}",
                    bucket, key, days));

            HttpStatusCode response = HttpStatusCode.NotFound;
            try
            {
                RestoreObjectResponse response1 = s3Client.RestoreObject(bucket, key, days);
                response = response1.HttpStatusCode;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Restore is not allowed, as object's storage class is not GLACIER"))
                {
                    return HttpStatusCode.OK;
                }
                if (e.Message.Contains("The XML you provided was not well-formed or did not validate against our published schema"))
                {
                    return HttpStatusCode.Conflict;
                }
                Console.WriteLine("Exception occured:: {0}", e.ToString());
                fileReader.LogExeceptionForFailure(index, e.ToString());
                return response;
            }
            Console.WriteLine("File restored successfully");
            return response;
        }
    }
}
