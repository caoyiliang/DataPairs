using DataPairs.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DataPairs.Entities
{
    internal class PairsEntity : IVersion
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
        public long VersionNum { get; set; }
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
