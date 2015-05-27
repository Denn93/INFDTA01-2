using INFDTA01_2.DennisBrian.Assignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace INFDTA01_2.DennisBrian.Assignment.Algorithm
{
    internal class Clustering
    {
        private List<TransactionModel> _transactions;

        public List<TransactionMatrixModel> TransactionMatrix { get; set; }

        public Clustering(List<TransactionModel> transactions)
        {
            _transactions = transactions;
            TransactionMatrix = TransformData(transactions);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="transactions"></param>
        /// <returns></returns>
        private static List<TransactionMatrixModel> TransformData(List<TransactionModel> transactions)
        {
            int maxOffer = transactions.Max(m => m.OfferteId);
            List<TransactionMatrixModel> result = new List<TransactionMatrixModel>();

            foreach (var personTransactions in transactions.GroupBy(m => m.PersonId))
            {
                TransactionMatrixModel matrixRow = new TransactionMatrixModel { PersonId = personTransactions.Key, Values = new int[32] };

                for (int i = 0; i < maxOffer; i++)
                    matrixRow.Values[i] = personTransactions.Count(m => m.OfferteId == i + 1) > 0 ? 1 : 0;

                result.Add(matrixRow);
            }

            return result;
        }

        private double[,] CreateInitialCentroids(int numberOfCentroids)
        {
            double[,] result = new double[numberOfCentroids, 32];

            Random rnd = new Random();
            double value = 0.2;
            for (int i = 0; i < numberOfCentroids; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    result[i, j] = rnd.NextDouble(); // 32 dimensions
                }
            }

            return result;
        }

        public void StartClustering(int numberOfCentroids)
        {
            double[,] centroids = CreateInitialCentroids(numberOfCentroids);

            List<ClusterDistanceModel> result = CalculateDistances(centroids); ;
            while (!ContainsEmptyCluster(result, numberOfCentroids))
            {
                centroids = CreateInitialCentroids(numberOfCentroids);
                result = CalculateDistances(centroids);
            }

            List<ClusterDistanceModel> newResult = RecalculateClusters(result, centroids, numberOfCentroids);

            foreach (ClusterDistanceModel clusterDistanceModel in newResult)
            {
                Console.WriteLine("PersonId: {0} - CurrentCluster: {1} - Distance: {2}", clusterDistanceModel.PersonId,
                    clusterDistanceModel.CurrentCluster,
                    clusterDistanceModel.ClusterDistances[clusterDistanceModel.CurrentCluster]);
            }
        }

        private List<ClusterDistanceModel> RecalculateClusters(List<ClusterDistanceModel> initialClusters, double[,] firstCentroids, int numberOfCentroids)
        {
            double[,] prevCentroids = firstCentroids;
            List<ClusterDistanceModel> clusterResult = initialClusters;

            for (int i = 0; i < 500; i++)
            {
                double[,] centroids = RecalculateCentroids(clusterResult, numberOfCentroids, prevCentroids);

                if (!CheckCentroidIsSame(centroids, prevCentroids))
                {
                    clusterResult = CalculateDistances(centroids);
                    prevCentroids = centroids;
                }
                else
                    break;
            }

            return clusterResult;
        }

        private bool CheckCentroidIsSame(double[,] newCentroids, double[,] prevCentroids)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (prevCentroids[i, j] != newCentroids[i, j])
                        return false;
                }
            }

            return true;
        }

        private bool ContainsEmptyCluster(List<ClusterDistanceModel> results, int numberOfClusters)
        {
            for (int i = 0; i < numberOfClusters; i++)
            {
                int count = results.Count(m => m.CurrentCluster == i);

                if (count == 0)
                    return false;
            }

            return true;
        }

        private double[,] RecalculateCentroids(List<ClusterDistanceModel> results, int clusters, double[,] prevCentroids)
        {
            double[,] centroids = new double[clusters, 32];

            //foreach person sum total values / total in cluster

            for (int i = 0; i < clusters; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    List<int> persons = results.Where(m => m.CurrentCluster == i).Select(m => m.PersonId).ToList();
                    int count = TransactionMatrix.Count(m => persons.Contains(m.PersonId));

                    double sum = TransactionMatrix.Where(m => persons.Contains(m.PersonId)).Sum(m => m.Values[j]);
                    double mean = sum / count;

                    centroids[i, j] = count > 0 ? mean : prevCentroids[i, j];
                }
            }

            return centroids;
        }

        private List<ClusterDistanceModel> CalculateDistances(double[,] centroids)
        {
            List<ClusterDistanceModel> result = new List<ClusterDistanceModel>();
            foreach (TransactionMatrixModel personId in TransactionMatrix)
            {
                ClusterDistanceModel model = new ClusterDistanceModel
                {
                    PersonId = personId.PersonId,
                    ClusterDistances = new Dictionary<int, double>()
                };

                for (int i = 0; i < 4; i++)
                {
                    double distance = 0;

                    for (int j = 0; j < 32; j++)
                    {
                        distance += EuclideanDistance(personId.Values[j], centroids[i, j]);
                        /*Math.Pow(personId.Values[j] - centroids[i, j], 2);*/
                    }

                    distance = Math.Sqrt(distance);
                    model.ClusterDistances.Add(i, distance);
                }

                model.CurrentCluster = SelectCluster(model);
                result.Add(model);
            }

            return result;
        }

        private int SelectCluster(ClusterDistanceModel model)
        {
            return model.ClusterDistances.Where(
                m => m.Value.Equals(model.ClusterDistances.Min(n => n.Value)))
                .Select(m => m.Key).First();
        }

        private void CalculateSilhouette(int numberClusters, List<ClusterDistanceModel> clusteringResult)
        {
            for (int i = 0; i < 4; i++)
            {
                List<TransactionMatrixModel> clusterIPersons =
                    TransactionMatrix.Where(
                        m =>
                            clusteringResult.Where(n => n.CurrentCluster == i)
                                .Select(b => b.PersonId)
                                .Contains(m.PersonId)).ToList();

                for (int j = i; j < 4; j++)
                {
                    List<TransactionMatrixModel> clusterJPersons =
                    TransactionMatrix.Where(
                        m =>
                            clusteringResult.Where(n => n.CurrentCluster == j)
                                .Select(b => b.PersonId)
                                .Contains(m.PersonId)).ToList();

                    foreach (TransactionMatrixModel personI in clusterIPersons)
                    {
                        CalculateDissimilarity(personI, clusterJPersons);
                    }

                    clusteringResult.Where(m => m.CurrentCluster == j).Select(m => m.PersonId).ToList();
                }
            }
        }

        private double CalculateDissimilarity(TransactionMatrixModel pointOne, List<TransactionMatrixModel> otherPoints)
        {
            List<double> distances = new List<double>();
            int personCount = 0;
            foreach (TransactionMatrixModel otherPoint in otherPoints)
            {
                if (pointOne.PersonId == otherPoint.PersonId) continue;

                double distance = 0;
                for (int i = 0; i < 32; i++)
                {
                    distance += EuclideanDistance(pointOne.Values[i], otherPoint.Values[i]);
                }

                distances.Add(Math.Sqrt(distance));
                personCount++;
            }

            return distances.Sum() / personCount;

            // elke persoon
            // distance met adnere in dezelfde cluster
            // average
        }

        private double EuclideanDistance(double value, double value2)
        {
            return Math.Pow(value - value2, 2);
        }
    }
}