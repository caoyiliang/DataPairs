using DataPairs.Entities;
using DataPairs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataPairs
{
    internal class Pairs : IPairs
    {
        private readonly string _connectionString;
        private readonly JsonSerializerOptions _jsonSerializerSettings = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            IgnoreReadOnlyProperties = false,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        public Pairs(string path = "PairsDB.dll", JsonSerializerOptions? jsonSerializerSettings = null, string partialConnectionString = "data source")
        {
            if (jsonSerializerSettings is not null)
                _jsonSerializerSettings = jsonSerializerSettings;
            _connectionString = $"{partialConnectionString}={path}";
            using var context = new PairsContext(_connectionString);
            context.Database.EnsureCreated();
        }

        public async Task<bool> TryAddAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            await using var context = new PairsContext(_connectionString);
            var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
            if (pair is null)
            {
                using MemoryStream ms = new();
                await JsonSerializer.SerializeAsync(ms, value, _jsonSerializerSettings);
                ms.Position = 0;
                using var reader = new StreamReader(ms);
                await context.AddAsync(new PairsEntity()
                {
                    Key = key,
                    Value = await reader.ReadToEndAsync(),
                });
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> TryUpdateAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            return await Helper.HandleConcurrency(async () =>
            {
                await using var context = new PairsContext(_connectionString);
                var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                if (pair is null)
                    return false;
                using MemoryStream ms = new();
                await JsonSerializer.SerializeAsync(ms, value, _jsonSerializerSettings);
                ms.Position = 0;
                using var reader = new StreamReader(ms);
                var newValue = await reader.ReadToEndAsync();
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
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            await Helper.HandleConcurrency(async () =>
            {
                await using var context = new PairsContext(_connectionString);
                var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
                using MemoryStream ms = new();
                await JsonSerializer.SerializeAsync(ms, value, _jsonSerializerSettings);
                ms.Position = 0;
                using var reader = new StreamReader(ms);
                var newValue = await reader.ReadToEndAsync();
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
            });
        }

        public async Task<T?> TryGetValueAsync<T>(string key, T? defaultValue = default) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            await using var context = new PairsContext(_connectionString);
            var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
            if (pair is null) return defaultValue;
            using MemoryStream ms = new();
            using var writer = new StreamWriter(ms);
            await writer.WriteAsync(pair.Value);
            await writer.FlushAsync();
            ms.Position = 0;
            return await JsonSerializer.DeserializeAsync<T>(ms, _jsonSerializerSettings);
        }

        public async Task TryRemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            await using var context = new PairsContext(_connectionString);
            var pair = await (from d in context.Pairs where d.Key == key select d).SingleOrDefaultAsync();
            if (pair is null) return;
            context.Pairs.Remove(pair);
            await context.SaveChangesAsync();
        }
    }
}
