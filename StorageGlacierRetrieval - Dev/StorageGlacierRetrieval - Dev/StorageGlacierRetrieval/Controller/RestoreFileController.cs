using StorageGlacierRetrieval.Helper;
using StorageGlacierRetrieval.Model;
using System;
using System.Configuration;
using System.Net;

namespace StorageGlacierRetrieval.Controller
{
    public class RestoreFileController
    {
        private ApiHelper _apiHelper;
        private FileReader _fileReader;
        private S3Helper _s3Helper;

        private int _fileCount;

        private ApiHelper Api
        {
            get
            {
                if (_apiHelper == null)
                {
                    Console.WriteLine("RestoreFileController -> Creating ApiHelper");
                    _apiHelper = new ApiHelper(
                        ConfigurationManager.AppSettings["apiUser"],
                        ConfigurationManager.AppSettings["apiPassword"],
                        ConfigurationManager.AppSettings["apiEndpoint"]
                        );
                }
                return _apiHelper;
            }
        }
        private S3Helper S3
        {
            get
            {
                if (_s3Helper == null)
                {
                    Console.WriteLine("RestoreFileController -> Creating S3Helper");
                    _s3Helper = new S3Helper(ConfigurationManager.AppSettings["s3Region"]);
                }
                return _s3Helper;
            }
        }
        private FileReader CsvFileReader
        {
            get
            {
                if (_fileReader == null)
                {
                    Console.WriteLine("RestoreFileController -> Creating FileReader");
                    _fileReader = new FileReader(ConfigurationManager.AppSettings["fileLocation"]);
                }
                return _fileReader;
            }
        }

        public RestoreFileController()
        {
            ProcessFiles();
        }

        private void ProcessFiles()
        {
            _fileCount = 0;
            CloudFileEntry fileEntry;
            try
            {
                while (_fileCount < CsvFileReader.Count)
                {
                    fileEntry = CsvFileReader.GetAt(_fileCount);
                    if (string.IsNullOrEmpty(fileEntry.FileRestored))
                    {
                        if (TryToRestoreFile(fileEntry) != HttpStatusCode.Conflict)
                        {
                            Console.WriteLine("RestoreFileController ProcessFiles " + fileEntry);
                            _fileCount++;
                        }
                    }
                    else
                    {
                        _fileCount++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("RestoreFileController -> ProcessFiles -> exception: " + e);
                CsvFileReader.LogExeceptionForFailure(_fileCount, e.ToString());
            }

            CsvFileReader.SaveFile();
        }

        private HttpStatusCode TryToRestoreFile(CloudFileEntry fileEntry)
        {
            HttpStatusCode response = HttpStatusCode.NotFound;
            string filePath = Api.GetFilePath(fileEntry.Bus_No, Guid.Parse(fileEntry.CloudLocation));
            Console.WriteLine("TryToRestoreFile::" + filePath);
            if (!string.IsNullOrEmpty(filePath))
            {
                response = S3.RestoreObject(
                    ConfigurationManager.AppSettings["bucketName"],
                    filePath,
                    int.Parse(ConfigurationManager.AppSettings["restoreNumberOfDays"]),
                     _fileCount,
                     _fileReader
                );
                //Restore object returns accepted when object is restored for the first time after that it returns ok.
                if (response == HttpStatusCode.Accepted || response == HttpStatusCode.OK)
                {
                    CsvFileReader.MarkFileAsRestored(_fileCount);
                }
            }

            return response;
        }
    }
}
