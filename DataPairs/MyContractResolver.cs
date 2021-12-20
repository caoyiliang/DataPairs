using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace DataPairs
{
    public class MyContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var saveDataAttribute = member.GetCustomAttribute<SavePirvatePropertyAttribute>();
            if (saveDataAttribute == null)
            {
                return base.CreateProperty(member, memberSerialization);
            }
            else
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.Writable = true;
                return property;
            }
        }
    }
}
