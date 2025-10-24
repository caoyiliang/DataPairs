using DataPairs.Interfaces;
using FastDeepCloner;

namespace DataPairs
{
    public class DataPair<T> : IDataPair<T> where T : class, new()
    {
        private readonly IPairs _pairs;
        private readonly string _key;
        private T? _value;
        private readonly SemaphoreSlim _valueSync = new(1, 1);
        public DataPair(string key)
        {
            _key = key;
            _pairs = new Pairs();
        }
        public DataPair(string key, string path)
        {
            _key = key;
            _pairs = new Pairs(path);
        }
        public DataPair(string key, StorageType storageType)
        {
            _key = key;
            _pairs = storageType switch
            {
                StorageType.File => new PairsFile(),
                StorageType.SQLite => new Pairs(),
                StorageType.Xamarin => new Pairs(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PairsDB.dll"), "Filename"),
                _ => throw new ArgumentException(),
            };
        }
        public DataPair(string key, string path, StorageType storageType)
        {
            _key = key;
            _pairs = storageType switch
            {
                StorageType.File => new PairsFile(path),
                StorageType.SQLite => new Pairs(path),
                StorageType.Xamarin => new Pairs(path, "Filename"),
                _ => throw new ArgumentException(),
            };
        }

        public async Task TryInitOrUpdateAsync(T value)
        {
            if (value is null) throw new ArgumentNullException("value is null");
            try
            {
                await _valueSync.WaitAsync();
                await _pairs.TryAddOrUpdateAsync(_key, value);
                _value = value.Clone(FieldType.Both);
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
                _value ??= await _pairs.TryGetValueAsync<T>(_key);
                return _value is null ? new T() : _value.Clone(FieldType.Both);
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

        #region Obsolete
        [Obsolete]
        public async Task<bool> TryUpdateAsync(T value)
        {
            if (value is null) throw new ArgumentNullException("value is null");
            try
            {
                await _valueSync.WaitAsync();
                if (_value is null) return false;
                if (await _pairs.TryUpdateAsync(_key, value))
                {
                    _value = value.Clone(FieldType.Both);
                    return true;
                }
                return false;
            }
            finally
            {
                _valueSync.Release();
            }
        }

        [Obsolete]
        public async Task<bool> TryInitAsync(T value)
        {
            if (value is null) throw new ArgumentNullException("value is null");
            try
            {
                await _valueSync.WaitAsync();
                if (_value is not null) return false;
                if (await _pairs.TryAddAsync(_key, value))
                {
                    _value = value.Clone(FieldType.Both);
                    return true;
                }
                return false;
            }
            finally
            {
                _valueSync.Release();
            }
        }
        #endregion
    }
}
