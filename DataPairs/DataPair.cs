using DataPairs.Interfaces;
using FastDeepCloner;
using Newtonsoft.Json;

namespace DataPairs
{
    public class DataPair<T> : IDataPair<T> where T : class, new()
    {
        private IPairs _pairs;
        private string _key;
        private T _value;
        private SemaphoreSlim _valueSync = new SemaphoreSlim(1, 1);
        private string _partialConnectionString = "data source";
        private string _partialConnectionStringXamarin = "Filename";
        public DataPair(string key)
        {
            _key = key;
            _pairs = new Pairs(_partialConnectionString);
        }
        public DataPair(string key, string path)
        {
            _key = key;
            _pairs = new Pairs(path, _partialConnectionString);
        }
        public DataPair(string key, StorageType storageType)
        {
            _key = key;
            switch (storageType)
            {
                case StorageType.File:
                    _pairs = new PairsFile();
                    break;
                case StorageType.SQLite:
                    _pairs = new Pairs(_partialConnectionString);
                    break;
                case StorageType.Xamarin:
                    _pairs = new Pairs(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PairsDB.dll"),
                        _partialConnectionStringXamarin);
                    break;
                default:
                    throw new ArgumentException();
            }
        }
        public DataPair(string key, string path, StorageType storageType)
        {
            _key = key;
            switch (storageType)
            {
                case StorageType.File:
                    _pairs = new PairsFile(path);
                    break;
                case StorageType.SQLite:
                    _pairs = new Pairs(path, _partialConnectionString);
                    break;
                case StorageType.Xamarin:
                    _pairs = new Pairs(path, _partialConnectionStringXamarin);
                    break;
                default:
                    throw new ArgumentException();
            }
        }
        public DataPair(string key, bool includeTypeName)
        {
            _key = key;
            _pairs = new PairsFile(includeTypeName);
        }
        public DataPair(string key, string path, bool includeTypeName)
        {
            _key = key;
            _pairs = new PairsFile(path, includeTypeName);
        }
        public DataPair(string key, JsonSerializerSettings jsonSerializerSettings)
        {
            _key = key;
            _pairs = new PairsFile(jsonSerializerSettings);
        }
        public DataPair(string key, JsonSerializerSettings jsonSerializerSettings, Formatting formatting)
        {
            _key = key;
            _pairs = new PairsFile(jsonSerializerSettings, formatting);
        }

        public async Task<bool> TryInitAsync(T value)
        {
            if (value is null) throw new ArgumentNullException("value is null");
            try
            {
                await _valueSync.WaitAsync();
                if (!(_value is null)) return false;
                if (await _pairs.TryAddAsync(_key, value))
                {
                    _value = value.Clone();
                    return true;
                }
                return false;
            }
            finally
            {
                _valueSync.Release();
            }
        }

        public async Task TryInitOrUpdateAsync(T value)
        {
            if (value is null) throw new ArgumentNullException("value is null");
            try
            {
                await _valueSync.WaitAsync();
                await _pairs.TryAddOrUpdateAsync(_key, value);
                _value = value.Clone();
            }
            finally
            {
                _valueSync.Release();
            }
        }

        public async Task<T> TryGetValueAsync()
        {
            try
            {
                await _valueSync.WaitAsync();
                if (_value is null)
                {
                    _value = await _pairs.TryGetValueAsync<T>(_key);
                }
                if (_value is null) return new T();
                return _value.Clone();
            }
            finally
            {
                _valueSync.Release();
            }
        }

        public async Task TryRemoveAsync()
        {
            try
            {
                await _valueSync.WaitAsync();
                if (_value is null) return;
                await _pairs.TryRemoveAsync(_key);
                _value = default;
            }
            finally
            {
                _valueSync.Release();
            }
        }

        public async Task<bool> TryUpdateAsync(T value)
        {
            if (value is null) throw new ArgumentNullException("value is null");
            try
            {
                await _valueSync.WaitAsync();
                if (_value is null) return false;
                if (await _pairs.TryUpdateAsync(_key, value))
                {
                    _value = value.Clone();
                    return true;
                }
                return false;
            }
            finally
            {
                _valueSync.Release();
            }
        }
    }
}
