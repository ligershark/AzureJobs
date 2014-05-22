using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;

namespace AzureJobs.Common
{
    public class FileHashStore
    {
        private string _filePath;
        private Dictionary<string, string> _store = new Dictionary<string, string>();
        private XmlSerializer serializer = new XmlSerializer(typeof(Item[]), new XmlRootAttribute() { ElementName = "items" });
        private Logger _log;

        public FileHashStore(string fileName, Logger log)
        {
            _filePath = fileName;
            _log = log;

            Load();
        }

        private void Load()
        {
            try
            {
                // If the file hasn't been created yet, just ignore it.
                if (!File.Exists(_filePath))
                    return;

                using (var stream = File.OpenRead(_filePath))
                {
                    _store = ((Item[])serializer.Deserialize(stream)).ToDictionary(i => i.File, i => i.Hash);
                }
            }
            catch
            {
                // Do nothing. The file format has changed and will be overwritten next time Save() is called.
            }
        }

        public void Save(string file)
        {
            _store[file] = GetHash(file);

            var dir = Path.GetDirectoryName(_filePath);

            try
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var stream = File.OpenWrite(_filePath))
                {
                    serializer.Serialize(stream, _store.Select(kv => new Item() { File = kv.Key, Hash = kv.Value }).ToArray());
                }
            }
            catch (Exception ex)
            {
                _log.Write(ex.Message);
            }
        }

        public bool HasChangedOrIsNew(string file)
        {
            if (!_store.ContainsKey(file))
                return true;

            string currentHash = GetHash(file);

            if (string.IsNullOrEmpty(currentHash))
                return true;

            return currentHash != _store[file];
        }

        private string GetHash(string file)
        {
            if (!File.Exists(file))
                return null;

            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(file))
            {
                byte[] hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash);
            }
        }
    }

    [XmlType(TypeName = "item")]
    public class Item
    {
        [XmlAttribute(AttributeName = "file")]
        public string File;

        [XmlAttribute(AttributeName = "hash")]
        public string Hash;
    }
}