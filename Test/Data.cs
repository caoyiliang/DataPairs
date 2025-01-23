using DataPairs;
using DataPairs.Interfaces;

namespace Test
{
    internal class Data
    {
        public decimal A { get; set; }
        public List<decimal> B { get; set; } = new List<decimal>();

        private static readonly IDataPair<Data> _pair = new DataPair<Data>(nameof(Data));

        public async Task InitAsync()
        {
            var data = await _pair.TryGetValueAsync();
            foreach (var item in data.GetType().GetProperties())
            {
                GetType().GetProperty(item.Name)!.SetValue(this, item.GetValue(data));
            }
        }

        public async Task TrySaveChangeAsync()
        {
            await _pair.TryInitOrUpdateAsync(this);
        }
    }
}
