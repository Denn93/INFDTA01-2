using Excel;
using INFDTA01_2.DennisBrian.Assignment.Algorithm;
using INFDTA01_2.DennisBrian.Assignment.DataAccess;
using INFDTA01_2.DennisBrian.Assignment.Models;
using INFDTA01_2.DennisBrian.Assignment.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace INFDTA01_2.DennisBrian.Assignment
{
    internal class Program
    {
        private static Tuple<double, List<ClusterDistanceModel>> result = new Tuple<double, List<ClusterDistanceModel>>(0.0,
            new List<ClusterDistanceModel>());

        private static void Main(string[] args)
        {
            Console.WriteLine("INFDTA Assignments");
            WineRepository repo = new WineRepository(new Database());

            Clustering clustering = new Clustering(repo.GetTransactionModel());

            for (int i = 0; i < 1000; i++)
            {
                List<ClusterDistanceModel> clusterDistance = clustering.StartClustering(5);
                double sse = clustering.SumOfSquaredErrors(clusterDistance);

                if (sse < result.Item1 || result.Item1.Equals(0.0))
                {
                    result = new Tuple<double, List<ClusterDistanceModel>>(sse, clusterDistance);
                }
            }

            PrintResult(clustering);

            Console.ReadLine();
        }

        private static void PrintResult(Clustering clustering)
        {
            double totalDistance = 0;
            foreach (var clusterDistanceModel in result.Item2.GroupBy(m => m.CurrentCluster).Where(m => m.Count() > 3))
            {
                Console.WriteLine("CurrentCluster - {0}", clusterDistanceModel.Key);

                Dictionary<int, List<int>> offerValues = new Dictionary<int, List<int>>();
                foreach (ClusterDistanceModel distanceModel in clusterDistanceModel)
                {
                    for (int i = 0; i < clustering.NumberOfVectors; i++)
                    {
                        if (
                            clustering.TransactionMatrix.Single(m => m.PersonId == distanceModel.PersonId).Values[i] ==
                            1)
                        {
                            if (offerValues.ContainsKey(i))
                                offerValues[i].Add(distanceModel.PersonId);
                            else
                                offerValues.Add(i, new List<int> { distanceModel.PersonId });
                        }
                    }

                    totalDistance += distanceModel.ClusterDistances[distanceModel.CurrentCluster];
                }

                foreach (KeyValuePair<int, List<int>> offerValue in offerValues.Where(m => m.Value.Count > 3).OrderByDescending(m => m.Value.Count))
                {
                    Console.WriteLine("Offer ID {0} taken by Person: {1}", offerValue.Key,
                        string.Join(",", offerValue.Value.ToArray()));
                }

                Console.WriteLine("\n\n");
            }

            double silhouette = clustering.CalculateSilhouette(result.Item2);
            double sse = result.Item1;
            Console.WriteLine("Silhouette: {0}\nTotal Distance: {1}\nSSE: {2}", silhouette, totalDistance, sse);
        }
    }
}