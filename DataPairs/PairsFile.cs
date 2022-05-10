using DataPairs.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataPairs
{
    internal class PairsFile : IPairs
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _jsonSerializerSettings;
        public PairsFile() : this(AppDomain.CurrentDomain.BaseDirectory)
        {
        }
        public PairsFile(JsonSerializerOptions jsonSerializerSettings) : this(AppDomain.CurrentDomain.BaseDirectory, jsonSerializerSettings)
        {

        }

        public PairsFile(string path)
        {
            _jsonSerializerSettings = new()
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = true
            };
            _path = Path.Combine(path, "Config");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
        }

        public PairsFile(string path, JsonSerializerOptions jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
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
                await WriteFileAsync(fileName, await SerializeObject(value));
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
            var newValue = await SerializeObject(value);
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
                await WriteFileAsync(fileName, await SerializeObject(value));
                return;
            }
            var newValue = SerializeObject(value);
            if (!newValue.Equals(ReadFile(fileName)))
            {
                await WriteFileAsync(fileName, await SerializeObject(value));
            }
        }

        public async Task<T?> TryGetValueAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            var fileName = Path.Combine(_path, key + ".json");
            if (!File.Exists(fileName))
                return default;
            using FileStream fs = File.OpenRead(fileName);
            return await JsonSerializer.DeserializeAsync<T>(fs, _jsonSerializerSettings);
        }

        public async Task TryRemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            var fileName = Path.Combine(_path, key + ".json");
            if (File.Exists(fileName))
                File.Delete(fileName);
            await Task.CompletedTask;
        }

        private async Task WriteFileAsync(string fileName, string text)
        {
            byte[] rs = Encoding.UTF8.GetBytes(text);
            using var fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.WriteThrough);
            await fs.WriteAsync(rs, 0, rs.Length);
            await fs.FlushAsync();
        }

        private string ReadFile(string fileName) => File.ReadAllText(fileName, Encoding.UTF8);
        private async Task<string> SerializeObject<T>(T value)
        {
            using MemoryStream ms = new();
            await JsonSerializer.SerializeAsync(ms, value, _jsonSerializerSettings);
            ms.Position = 0;
            using var reader = new StreamReader(ms);
            return await reader.ReadToEndAsync();
        }
    }
}
