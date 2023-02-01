using Microsoft.EntityFrameworkCore;

namespace DataPairs.Interfaces
{
    internal interface IPairs
    {
        #region Obsolete
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateConcurrencyException"></exception>
        [Obsolete]
        Task<bool> TryAddAsync<T>(string key, T value) where T : class;

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateConcurrencyException"></exception>
        [Obsolete]
        Task<bool> TryUpdateAsync<T>(string key, T value) where T : class;
        #endregion

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateConcurrencyException"></exception>
        Task TryAddOrUpdateAsync<T>(string key, T value) where T : class;

        /// <summary>
        /// 根据Key获取Value
        /// </summary>
        /// <param name="defaultValue">未检索到Key对应的值，返回的可空默认值</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>返回的可空默认值</returns>
        Task<T?> TryGetValueAsync<T>(string key, T? defaultValue = default) where T : class;

        /// <exception cref="ArgumentNullException"></exception>
        Task TryRemoveAsync(string key);
    }
}
