using Microsoft.EntityFrameworkCore;

namespace DataPairs.Interfaces
{
    public interface IDataPair<T> where T : class, new()
    {
        #region Obsolete
        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete]
        Task<bool> TryInitAsync(T value);

        /// <exception cref="ArgumentNullException"></exception>
        [Obsolete]
        Task<bool> TryUpdateAsync(T value);
        #endregion

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateConcurrencyException"></exception>
        Task TryInitOrUpdateAsync(T value);

        /// <exception cref="ArgumentNullException"></exception>
        Task<T> TryGetValueAsync();

        /// <exception cref="ArgumentNullException"></exception>
        Task TryRemoveAsync();
    }
}
