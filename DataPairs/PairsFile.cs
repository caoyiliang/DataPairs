﻿using Ceras;
using DataPairs.Interfaces;
using System.Text;

namespace DataPairs
{
    internal class PairsFile : IPairs
    {
        private readonly string _path;
        CerasSerializer _ceras;

        public PairsFile() : this(AppDomain.CurrentDomain.BaseDirectory)
        {

        }

        public PairsFile(string path)
        {
            _path = Path.Combine(path, "Config");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            _ceras = new();
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

        public async Task<T?> TryGetValueAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            var fileName = Path.Combine(_path, key + ".json");
            if (!File.Exists(fileName))
            {
                await Task.CompletedTask;
                return default;
            }
            return _ceras.Deserialize<T>(ReadFile(fileName));
        }

        public async Task TryRemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            var fileName = Path.Combine(_path, key + ".json");
            if (File.Exists(fileName))
                File.Delete(fileName);
            await Task.CompletedTask;
        }

        private async Task WriteFileAsync(string fileName, byte[] text)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 8, FileOptions.WriteThrough))
            {
                await fs.WriteAsync(text, 0, text.Length);
                await fs.FlushAsync();
            }
        }

        private byte[] ReadFile(string fileName) => File.ReadAllBytes(fileName);
        private byte[] SerializeObject<T>(T value) => _ceras.Serialize(value);
    }
}
