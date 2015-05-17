using System.Collections.Generic;

namespace INFDTA01_2.DennisBrian.Assignment.Models
{
    internal class ClusterDistanceModel
    {
        public int PersonId { get; set; }

        public string PersonName { get; set; }

        public int CurrentCluster { get; set; }

        public Dictionary<int, double> ClusterDistances { get; set; }
    }
}