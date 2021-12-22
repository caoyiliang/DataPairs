using Ceras;
using DataPairs.Entities;
using DataPairs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataPairs
{
    internal class Pairs : IPairs
    {
        private readonly string _connectionString;
        CerasSerializer _ceras;
        public Pairs() : this("data source")
        {
        }
        public Pairs(string partialConnectionString) : this("PairsDB.dll", partialConnectionString)
        {
        }
        public Pairs(string path, string partialConnectionString)
        {
            _connectionString = $"{partialConnectionString}={path}";
            using (var context = new PairsContext(_connectionString))
            {
                context.Database.EnsureCreated();
            }
            _ceras = new();
        }
        public async Task<bool> TryAddAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            using (var context = new PairsContext(_connectionString))
            {
                var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                if (pair is null)
                {
                    await context.AddAsync(new PairsEntity()
                    {
                        Key = key,
                        Value = _ceras.Serialize(value),
                    });
                    await context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
        }

        public async Task<bool> TryUpdateAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            return await Helper.HandleConcurrency(async () =>
            {
                using (var context = new PairsContext(_connectionString))
                {
                    var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                    if (pair is null)
                        return false;
                    var newValue = _ceras.Serialize(value);
                    if (!pair.Value.Equals(newValue))
                    {
                        pair.Value = newValue;
                        context.Update(pair);
                        await context.SaveChangesAsync();
                    }
                    return true;
                }
            });
        }
        public async Task TryAddOrUpdateAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            await Helper.HandleConcurrency(async () =>
            {
                using (var context = new PairsContext(_connectionString))
                {
                    var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                    var newValue = _ceras.Serialize(value);
                    if (pair is null)
                    {
                        await context.AddAsync(new PairsEntity()
                        {
                            Key = key,
                            Value = newValue,
                        });
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        if (!pair.Value.Equals(newValue))
                        {
                            pair.Value = newValue;
                            context.Update(pair);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            });
        }

        public async Task<T> TryGetValueAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            using (var context = new PairsContext(_connectionString))
            {
                var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                if (pair is null) return default;
                return _ceras.Deserialize<T>(pair.Value);
            }
        }

        public async Task TryRemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            using (var context = new PairsContext(_connectionString))
            {
                var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                if (pair is null) return;
                context.Pairs.Remove(pair);
                await context.SaveChangesAsync();
            }
        }
    }
}
