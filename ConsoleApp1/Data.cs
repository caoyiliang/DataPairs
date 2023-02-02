using DataPairs;
using DataPairs.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    internal class Data
    {
        public decimal A { get; set; }
        public List<decimal> B { get; set; } = new List<decimal>();

        private static readonly IDataPair<Data> _pair = new DataPair<Data>(typeof(Data).Name);

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
