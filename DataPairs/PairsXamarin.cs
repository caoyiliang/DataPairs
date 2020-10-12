using KeyValuePairs.Entities;
using KeyValuePairs.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using SQLite;

namespace KeyValuePairs
{
    internal class PairsXamarin : IPairs
    {
        private readonly string _path;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        public PairsXamarin() : this("PairsDB.dll") { }
        public PairsXamarin(string path)
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new MyContractResolver()
            };
            _path = path;
        }
        public async Task<bool> TryAddAsync<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            if (value is null) throw new ArgumentNullException("must have a value");
            using (var db = new SQLiteConnection(_path))
            {
                db.CreateTable<PairsEntityXamarin>();
                PairsEntityXamarin pair;
                try
                {
                    pair = db.Get<PairsEntityXamarin>(pe => pe.Key == key);
                }
                catch (Exception ex)
                {
                    db.Insert(new PairsEntityXamarin()
                    {
                        Key = key,
                        Value = JsonConvert.SerializeObject(value, _jsonSerializerSettings),
                    });
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
                using (var db = new SQLiteConnection(_path))
                {
                    db.CreateTable<PairsEntityXamarin>();
                    PairsEntityXamarin pair;
                    try
                    {
                        pair = db.Get<PairsEntityXamarin>(pe => pe.Key == key);
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    var newValue = JsonConvert.SerializeObject(value, _jsonSerializerSettings);
                    if (!pair.Value.Equals(newValue))
                    {
                        pair.Value = newValue;
                        db.Update(pair);
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
                using (var db = new SQLiteConnection(_path))
                {
                    db.CreateTable<PairsEntityXamarin>();
                    PairsEntityXamarin pair;
                    try
                    {
                        pair = db.Get<PairsEntityXamarin>(pe => pe.Key == key);
                    }
                    catch (Exception ex)
                    {
                        db.Insert(new PairsEntityXamarin()
                        {
                            Key = key,
                            Value = JsonConvert.SerializeObject(value, _jsonSerializerSettings),
                        });
                        return;
                    }
                    var newValue = JsonConvert.SerializeObject(value, _jsonSerializerSettings);
                    if (!pair.Value.Equals(newValue))
                    {
                        pair.Value = newValue;
                        db.Update(pair);
                    }
                }
            });
        }

        public async Task<T> TryGetValueAsync<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            using (var db = new SQLiteConnection(_path))
            {
                db.CreateTable<PairsEntityXamarin>();
                PairsEntityXamarin pair;
                try
                {
                    pair = db.Get<PairsEntityXamarin>(pe => pe.Key == key);
                }
                catch (Exception ex)
                {
                    return default;
                }
                return JsonConvert.DeserializeObject<T>(pair.Value, _jsonSerializerSettings);
            }
        }

        public async Task TryRemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("must have a key");
            using (var db = new SQLiteConnection(_path))
            {
                db.CreateTable<PairsEntityXamarin>();
                PairsEntityXamarin pair;
                try
                {
                    pair = db.Get<PairsEntityXamarin>(pe => pe.Key == key);
                }
                catch (Exception ex)
                {
                    return;
                }
                db.Delete(pair);
            }
        }
    }
}
