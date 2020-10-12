using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataPairs.Interfaces
{
    internal interface IPairs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns> true if the key/value pair was added to the IPairs successfully; false if the key already exists.</returns>
        Task<bool> TryAddAsync<T>(string key, T value) where T : class;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>true if the key/value pair was replaced with newValue; false if the key was not found</returns>
        Task<bool> TryUpdateAsync<T>(string key, T value) where T : class;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateConcurrencyException"></exception>
        /// <returns></returns>
        Task TryAddOrUpdateAsync<T>(string key, T value) where T : class;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        Task<T> TryGetValueAsync<T>(string key) where T : class;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        Task TryRemoveAsync(string key);
    }
}
