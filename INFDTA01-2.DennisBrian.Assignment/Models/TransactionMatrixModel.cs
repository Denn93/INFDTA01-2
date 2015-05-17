using System;
using System.Collections.Generic;

namespace INFDTA01_2.DennisBrian.Assignment.Models
{
    internal class TransactionMatrixModel
    {
        public int PersonId { get; set; }

        public Dictionary<int, int> Values { get; set; }

        public override string ToString()
        {
            return PersonId + " - " + string.Concat(Values);
        }
    }
}