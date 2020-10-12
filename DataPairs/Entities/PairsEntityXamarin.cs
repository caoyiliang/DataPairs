using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyValuePairs.Entities
{
    internal class PairsEntityXamarin
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public override bool Equals(object other)
        {
            if (other is null) return false;
            var pe = other as PairsEntityXamarin;
            if (pe is null) return false;
            return this.Key == pe.Key && this.Value == pe.Value;
        }
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }
}
