using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataPairs.Interfaces
{
    public interface IDataPair<T> where T : class, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns> true if the key/value pair was added to the IAKeyValuePair successfully; false if the key already exists.</returns>
        Task<bool> TryInitAsync(T value);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>true if the key/value pair was replaced with newValue; false if the key was not found</returns>
        Task<bool> TryUpdateAsync(T value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateConcurrencyException"></exception>
        /// <returns></returns>
        Task TryInitOrUpdateAsync(T value);
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        Task<T> TryGetValueAsync();
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        Task TryRemoveAsync();
    }
}
