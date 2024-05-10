using System.ComponentModel.DataAnnotations;

namespace DataPairs.Entities
{
    internal class PairsEntity
    {
        [Key]
        public string Key { get; set; } = null!;

        [ConcurrencyCheck]
        public byte[] Value { get; set; } = null!;

        public override bool Equals(object? other)
        {
            if (other is null) return false;
            if (other is not PairsEntity pe) return false;
            return Key == pe.Key && Value == pe.Value;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
