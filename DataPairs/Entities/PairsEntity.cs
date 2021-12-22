using System.ComponentModel.DataAnnotations;

namespace DataPairs.Entities
{
    internal class PairsEntity
    {
        [Key]
        public string Key { get; set; }

        [ConcurrencyCheck]
        public byte[] Value { get; set; }

        public override bool Equals(object other)
        {
            if (other is null) return false;
            var pe = other as PairsEntity;
            if (pe is null) return false;
            return this.Key == pe.Key && this.Value == pe.Value;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
