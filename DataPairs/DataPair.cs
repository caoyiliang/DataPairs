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
 * * 说明：DataPair.cs
 * *
********************************************************************/

using FastDeepCloner;
using DataPairs.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
