# DataPairs
使用SQLite和文件存储数据键值的类库
### master为Newtonsoft.Json版本，一个是System.Text.Json分支，一个是Ceras分支

## 使用方法非常简单,比如原先有如下数据模型需要存储：
```
    public class Data
    {
        public decimal A { get; set; }
        public List<decimal> B { get; set; } = new List<decimal>();
    }
```
## 只需以下操作即可存储,注意：下例子为Xamarin中存储为sqlite数据库例子，还能存成文件格式，看下构造即可明了
```
    public class Data
    {
        private static readonly IDataPair<Data> pair = new DataPair<Data>("Data", Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Data.dll"), StorageType.Xamarin);
        public decimal A { get; set; }
        public List<decimal> B { get; set; } = new List<decimal>();
        public static async Task<Data> GetValueAsync()
        {
            var datas = await pair.TryGetValueAsync();
            return datas;
        }

        public static async Task TrySaveChangeAsync(Data data)
        {
            await pair.TryInitOrUpdateAsync(data);
        }
    }
```
## 最后可在初始化程序时调用以下代码，数据库中保存的键值对将会在内存中构建，使之后的操作不卡顿：
```
    await Data.GetValueAsync();
```
