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
 * * 说明：PairsFile.cs
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
