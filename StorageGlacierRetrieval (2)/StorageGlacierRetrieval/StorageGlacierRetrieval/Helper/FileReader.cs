using CsvHelper;
using StorageGlacierRetrieval.Model;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace StorageGlacierRetrieval.Helper
{
    public class FileReader
    {
        private CsvReader _csvReader;
        private CsvWriter _csvWriter;
        private string _filePath;
        private List<CloudFileEntry> _cloudFileEntryList;

        public int Count { get; set; }
        public FileReader(string filePath)
        {
            _filePath = filePath;
            ReadFile();
        }

        private void ReadFile()
        {
            _cloudFileEntryList = new List<CloudFileEntry>();
            using (TextReader reader = File.OpenText(_filePath))
            {
                Console.WriteLine(string.Format("FileReader -> ReadFile -> Reading file: {0}", _filePath));
                using (_csvReader = new CsvReader(reader))
                {
                    _csvReader.Configuration.HeaderValidated = null;
                    _csvReader.Configuration.MissingFieldFound = null;
                    _cloudFileEntryList = _csvReader.GetRecords<CloudFileEntry>().ToList();
                    Count = _cloudFileEntryList.Count;
                    Console.WriteLine(string.Format("FileReader -> ReadFile -> Number of loaded entries: {0}", Count));
                }
            }
        }

        public CloudFileEntry GetAt(int index)
        {
            Console.WriteLine(string.Format("FileReader -> GetAt -> Getting file at: {0}", index));
            return _cloudFileEntryList[index];
        }

        public void MarkFileAsRestored(int index)
        {
            Console.WriteLine(string.Format("FileReader -> MarkFileAsRestored -> Marking file as restored at: {0}", index));
            _cloudFileEntryList[index].FileRestored = "Restored";
        }


        public void LogExeceptionForFailure(int index,string ex)
        {
           _cloudFileEntryList[index].FileRestored = ex;
        }

        public void MarkFileAsFailed(int index)
        {
            Console.WriteLine(string.Format("FileReader -> MarkFileAsRestored -> Marking file as restored at: {0}", index));
            _cloudFileEntryList[index].FileRestored = "Failed ";
        }

        public void SaveFile()
        {
            Console.WriteLine(string.Format("FileReader -> SaveFile -> Saving CSV file"));
            using (TextWriter writer = File.CreateText(_filePath))
            {
                using (_csvWriter = new CsvWriter(writer))
                {
                    _csvWriter.WriteRecords(_cloudFileEntryList);
                }
            }
        }
    }
}
