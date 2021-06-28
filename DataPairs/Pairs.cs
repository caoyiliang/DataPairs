/********************************************************************
 * *
 * * 使本项目源码或本项目生成的DLL前请仔细阅读以下协议内容，如果你同意以下协议才能使用本项目所有的功能，
 * * 否则如果你违反了以下协议，有可能陷入法律纠纷和赔偿，作者保留追究法律责任的权利。
 * *
 * * 1、你可以在开发的软件产品中使用和修改本项目的源码和DLL，但是请保留所有相关的版权信息。
 * * 2、不能将本项目源码与作者的其他项目整合作为一个单独的软件售卖给他人使用。
 * * 3、不能传播本项目的源码和DLL，包括上传到网上、拷贝给他人等方式。
 * * 4、以上协议暂时定制，由于还不完善，作者保留以后修改协议的权利。
 * *
 * * Copyright ©2013-? yzlm Corporation All rights reserved.
 * * 作者： 曹一梁 QQ：347739303
 * * 请保留以上版权信息，否则作者将保留追究法律责任。
 * *
 * * 创建时间：2021-06-28
 * * 说明：Pairs.cs
 * *
********************************************************************/

using DataPairs.Entities;
using DataPairs.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace DataPairs
{
    internal class Pairs : IPairs
    {
        private readonly string _connectionString;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        public Pairs() : this("data source")
        {
        }
        public Pairs(string partialConnectionString) : this("PairsDB.dll", partialConnectionString)
        {
        }
        public Pairs(string path, string partialConnectionString)
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                TypeNameHandling = TypeNameHandling.All,
                ContractResolver = new MyContractResolver(),
            };
            _connectionString = $"{partialConnectionString}={path}";
            using (var context = new PairsContext(_connectionString))
            {
                context.Database.EnsureCreated();
            }
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
                        Value = JsonConvert.SerializeObject(value, _jsonSerializerSettings),
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
                     var newValue = JsonConvert.SerializeObject(value, _jsonSerializerSettings);
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
                return JsonConvert.DeserializeObject<T>(pair.Value, _jsonSerializerSettings);
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
