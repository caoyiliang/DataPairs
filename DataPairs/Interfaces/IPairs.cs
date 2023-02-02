using Microsoft.EntityFrameworkCore;

namespace DataPairs.Interfaces
{
    internal interface IPairs
    {
        #region Obsolete
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete]
        Task<bool> TryAddAsync<T>(string key, T value) where T : class;

        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete]
        Task<bool> TryUpdateAsync<T>(string key, T value) where T : class;
        #endregion

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateConcurrencyException"></exception>
        Task TryAddOrUpdateAsync<T>(string key, T value) where T : class;

        /// <exception cref="ArgumentNullException"></exception>
        Task<T?> TryGetValueAsync<T>(string key, T? defaultValue = default) where T : class;

        /// <exception cref="ArgumentNullException"></exception>
        Task TryRemoveAsync(string key);
    }
}
