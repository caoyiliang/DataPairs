using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("KeyValuePairsUnitTestProject")]

namespace DataPairs
{
    public static class Helper
    {
        public static T Clone<T>(T t)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var text = JsonConvert.SerializeObject(t, settings);
            return JsonConvert.DeserializeObject<T>(text, settings);
        }
        internal static async Task<T> HandleConcurrency<T>(Func<Task<T>> db)
        {
            int count = 0;
            while (true)
            {
                try
                {
                    if (count == 4)
                        await Task.Delay(10);
                    return await db();
                }
                catch (DbUpdateConcurrencyException)
                {
                    count++;
                    if (count >= 5)
                        throw;
                }
            }
        }

        internal static async Task HandleConcurrency(Func<Task> db)
        {
            int count = 0;
            while (true)
            {
                try
                {
                    if (count == 4)
                        await Task.Delay(10);
                    await db();
                    return;
                }
                catch (DbUpdateConcurrencyException)
                {
                    count++;
                    if (count >= 5)
                        throw;
                }
            }
        }
    }
}
