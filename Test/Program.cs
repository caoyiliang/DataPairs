// See https://aka.ms/new-console-template for more information

using Test;
using Microsoft.Extensions.DependencyInjection;

IServiceProvider serviceProvider;

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton(provider => { 
    var data=new Data();
    data.InitAsync().Wait();
    return data;
});

serviceProvider = serviceCollection.BuildServiceProvider();

var data = serviceProvider.GetRequiredService<Data>();
data.A = 500;
await data.TrySaveChangeAsync();
Console.WriteLine("Hello, World!");
