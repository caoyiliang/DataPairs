using DataPairs.Entities;
using DataPairs.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.IO;
using System.Text;

namespace DataPairs
{
    internal class PairsFile : IPairs
    {
        private readonly string _path;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private Formatting _formatting = Formatting.Indented;
        public PairsFile(bool includeTypeName = true) : this(AppDomain.CurrentDomain.BaseDirectory, includeTypeName)
        {
        }
        public PairsFile(JsonSerializerSettings jsonSerializerSettings) : this(AppDomain.CurrentDomain.BaseDirectory, jsonSerializerSettings, Formatting.Indented)
        {

        }
        public PairsFile(JsonSerializerSettings jsonSerializerSettings, Formatting formatting) : this(AppDomain.CurrentDomain.BaseDirectory, jsonSerializerSettings, formatting)
        {

        }
        public PairsFile(string path, bool includeTypeName = true)
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = includeTypeName ? TypeNameHandling.All : TypeNameHandling.None,
                ContractResolver = new MyContractResolver()
            };
            _path = Path.Combine(path, "Config");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }
        public PairsFile(string path)
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new MyContractResolver()
            };
            _path = Path.Combine(path, "Config");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        public PairsFile(string path, JsonSerializerSettings jsonSerializerSettings, Formatting formatting)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _formatting = formatting;
            _path = Path.Combine(path, "Config");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }
        public async Task<bool> TryAddAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            var fileName = Path.Combine(_path, key + ".json");
            if (!File.Exists(fileName))
            {
                await WriteFileAsync(fileName, SerializeObject(value));
                return true;
            }
            return false;
        }

        public async Task<bool> TryUpdateAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            var fileName = Path.Combine(_path, key + ".json");
            if (!File.Exists(fileName))
                return false;
            var newValue = SerializeObject(value);
            var text = File.ReadAllText(fileName, Encoding.UTF8);
            if (!text.Equals(newValue))
            {
                await WriteFileAsync(fileName, newValue);
            }
            return true;
        }
        public async Task TryAddOrUpdateAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            var fileName = Path.Combine(_path, key + ".json");
            if (!File.Exists(fileName))
            {
                await WriteFileAsync(fileName, SerializeObject(value));
                return;
            }
            var newValue = SerializeObject(value);
            if (!newValue.Equals(ReadFile(fileName)))
            {
                await WriteFileAsync(fileName, SerializeObject(value));
            }
        }

        public async Task<T> TryGetValueAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            var fileName = Path.Combine(_path, key + ".json");
            if (!File.Exists(fileName))
                return default;
            return JsonConvert.DeserializeObject<T>(ReadFile(fileName), _jsonSerializerSettings);
        }

        public async Task TryRemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            var fileName = Path.Combine(_path, key + ".json");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        private async Task WriteFileAsync(string fileName, string text)
        {
            byte[] rs = Encoding.UTF8.GetBytes(text);
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.WriteThrough))
            {
                await fs.WriteAsync(rs, 0, rs.Length);
                await fs.FlushAsync();
            }
        }

        private string ReadFile(string fileName) => File.ReadAllText(fileName, Encoding.UTF8);
        private string SerializeObject<T>(T value) => JsonConvert.SerializeObject(value, Formatting.Indented, _jsonSerializerSettings);
    }
}
