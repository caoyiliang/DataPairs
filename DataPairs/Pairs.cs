using DataPairs.Entities;
using DataPairs.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DataPairs
{
    internal class Pairs : IPairs
    {
        private readonly string _connectionString;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            PreserveReferencesHandling = PreserveReferencesHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            TypeNameHandling = TypeNameHandling.All,
            ContractResolver = new MyContractResolver(),
        };

        public Pairs(string path = "PairsDB.dll", JsonSerializerSettings? jsonSerializerSettings = null, string partialConnectionString = "data source")
        {
            if (jsonSerializerSettings is not null)
                _jsonSerializerSettings = jsonSerializerSettings;
            _connectionString = $"{partialConnectionString}={path}";
            using var context = new PairsContext(_connectionString);
            context.Database.EnsureCreated();
        }

        public async Task<bool> TryAddAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key), "must have a key");
            if (value is null) throw new ArgumentNullException(nameof(value), "must have a value");
            await using var context = new PairsContext(_connectionString);
            var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
            if (pair is null)
            {
                await context.AddAsync(new PairsEntity()
                {
                    Key = key,
                    Value = JsonConvert.SerializeObject(value, _jsonSerializerSettings),
                });
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> TryUpdateAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key), "must have a key");
            if (value is null) throw new ArgumentNullException(nameof(value), "must have a value");
            return await Helper.HandleConcurrency(async () =>
            {
                await using var context = new PairsContext(_connectionString);
                var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                if (pair is null)
                    return false;
                var newValue = JsonConvert.SerializeObject(value, _jsonSerializerSettings);
                if (!pair.Value.Equals(newValue))
                {
                    pair.Value = newValue;
                    context.Update(pair);
                    await context.SaveChangesAsync();
                }
                return true;
            });
        }

        public async Task TryAddOrUpdateAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key), "must have a key");
            if (value is null) throw new ArgumentNullException(nameof(value), "must have a value");
            await Helper.HandleConcurrency(async () =>
            {
                await using var context = new PairsContext(_connectionString);
                var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                if (pair is null)
                {
                    await context.AddAsync(new PairsEntity()
                    {
                        Key = key,
                        Value = JsonConvert.SerializeObject(value, _jsonSerializerSettings),
                    });
                    await context.SaveChangesAsync();
                }
                else
                {
                    var newValue = JsonConvert.SerializeObject(value, _jsonSerializerSettings);
                    if (!pair.Value.Equals(newValue))
                    {
                        pair.Value = newValue;
                        context.Update(pair);
                        await context.SaveChangesAsync();
                    }
                }
            });
        }

        public async Task<T?> TryGetValueAsync<T>(string key, T? defaultValue = default) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key), "must have a key");
            await using var context = new PairsContext(_connectionString);
            var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
            if (pair is null) return defaultValue;
            return JsonConvert.DeserializeObject<T>(pair.Value, _jsonSerializerSettings);
        }

        public async Task TryRemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key), "must have a key");
            await using var context = new PairsContext(_connectionString);
            var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
            if (pair is null) return;
            context.Pairs.Remove(pair);
            await context.SaveChangesAsync();
        }
    }
}
