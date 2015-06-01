using INFDTA01_2.DennisBrian.Assignment.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace INFDTA01_2.DennisBrian.Assignment.Algorithm
{
    internal class Clustering
    {
        public List<TransactionMatrixModel> TransactionMatrix { get; set; }

        public int NumberOfClusters { get; set; }

        public int NumberOfVectors { get; set; }

        public Clustering(List<TransactionModel> transactions)
        {
            TransactionMatrix = TransformData(transactions);
            NumberOfVectors = TransactionMatrix.Select(m => m.Values.Count()).First();
        }

        private List<TransactionMatrixModel> TransformData(List<TransactionModel> transactions)
        {
            int maxOffer = transactions.Max(m => m.OfferteId);
            List<TransactionMatrixModel> result = new List<TransactionMatrixModel>();

            foreach (var personTransactions in transactions.GroupBy(m => m.PersonId))
            {
                TransactionMatrixModel matrixRow = new TransactionMatrixModel { PersonId = personTransactions.Key, Values = new int[maxOffer] };

                for (int i = 0; i < maxOffer; i++)
                    matrixRow.Values[i] = personTransactions.Count(m => m.OfferteId == i + 1) > 0 ? 1 : 0;

                result.Add(matrixRow);
            }

            return result;
        }

        private double[,] CreateInitialCentroids()
        {
            double[,] result = new double[NumberOfClusters, 32];

            Random rnd = new Random();
            double value = 0.2;
            for (int i = 0; i < NumberOfClusters; i++)
            {
                for (int j = 0; j < NumberOfVectors; j++)
                {
                    result[i, j] = rnd.NextDouble(); // 32 dimensions
                }
            }

            return result;
        }

        public List<ClusterDistanceModel> StartClustering(int numberOfCentroids)
        {
            NumberOfClusters = numberOfCentroids;

            double[,] centroids = CreateInitialCentroids();
            double totalDistance = 0;

            List<ClusterDistanceModel> result = CalculateDistances(centroids); ;
            while (!ContainsEmptyCluster(result))
            {
                centroids = CreateInitialCentroids();
                result = CalculateDistances(centroids);
            }

            return RecalculateClusters(result, centroids);
        }

        private List<ClusterDistanceModel> RecalculateClusters(List<ClusterDistanceModel> initialClusters, double[,] firstCentroids)
        {
            double[,] prevCentroids = firstCentroids;
            List<ClusterDistanceModel> clusterResult = initialClusters;

            for (int i = 0; i < 500; i++)
            {
                double[,] centroids = RecalculateCentroids(clusterResult, prevCentroids);

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
            for (int i = 0; i < NumberOfClusters; i++)
            {
                for (int j = 0; j < NumberOfVectors; j++)
                {
                    if (prevCentroids[i, j] != newCentroids[i, j])
                        return false;
                }
            }

            return true;
        }

        private bool ContainsEmptyCluster(List<ClusterDistanceModel> results)
        {
            for (int i = 0; i < NumberOfClusters; i++)
            {
                int count = results.Count(m => m.CurrentCluster == i);

                if (count == 0)
                    return false;
            }

            return true;
        }

        private double[,] RecalculateCentroids(List<ClusterDistanceModel> results, double[,] prevCentroids)
        {
            double[,] centroids = new double[NumberOfClusters, 32];

            //foreach person sum total values / total in cluster

            for (int i = 0; i < NumberOfClusters; i++)
            {
                for (int j = 0; j < NumberOfVectors; j++)
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

                for (int i = 0; i < NumberOfClusters; i++)
                {
                    double distance = 0;

                    for (int j = 0; j < NumberOfVectors; j++)
                    {
                        if (personId.Values[j] == 0 && i == j)
                            continue;

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

        public double CalculateSilhouette(List<ClusterDistanceModel> clusteringResult)
        {
            Dictionary<int, double> lowestDissimilarities = new Dictionary<int, double>();
            Dictionary<int, double> sameClusterDissimilarity = new Dictionary<int, double>();

            for (int i = 0; i < NumberOfClusters; i++)
            {
                List<TransactionMatrixModel> clusterIPersons =
                    TransactionMatrix.Where(
                        m =>
                            clusteringResult.Where(n => n.CurrentCluster == i)
                                .Select(b => b.PersonId)
                                .Contains(m.PersonId)).ToList();

                for (int j = 0; j < NumberOfClusters; j++)
                {
                    List<TransactionMatrixModel> clusterJPersons =
                        TransactionMatrix.Where(
                            m =>
                                clusteringResult.Where(n => n.CurrentCluster == j)
                                    .Select(b => b.PersonId)
                                    .Contains(m.PersonId)).ToList();

                    foreach (TransactionMatrixModel personI in clusterIPersons)
                    {
                        double dissimilarity = CalculateDissimilarity(personI, clusterJPersons);

                        if (
                            clusteringResult.Where(m => m.CurrentCluster == j)
                                .Count(m => m.PersonId == personI.PersonId) == 0)
                            SaveDissimilarities(lowestDissimilarities, dissimilarity, personI.PersonId);
                        else
                            SaveDissimilarities(sameClusterDissimilarity, dissimilarity, personI.PersonId);
                    }
                }
            }

            double resultSilhouette = 0;
            foreach (KeyValuePair<int, double> lowestDissimilarity in lowestDissimilarities)
            {
                int personId = lowestDissimilarity.Key;
                double silhouette = (lowestDissimilarities[personId] - sameClusterDissimilarity[personId]) /
                                    Math.Max(lowestDissimilarities[personId], sameClusterDissimilarity[personId]);

                resultSilhouette += silhouette;
            }

            return resultSilhouette / lowestDissimilarities.Count;
        }

        private void SaveDissimilarities(Dictionary<int, double> list, double dissimilarity, int personId)
        {
            if (list.ContainsKey(personId))
            {
                double temp = list[personId];
                list[personId] = (temp > dissimilarity) ? dissimilarity : temp;
            }
            else
            {
                list.Add(personId, dissimilarity);
            }
        }

        private double CalculateDissimilarity(TransactionMatrixModel pointOne, List<TransactionMatrixModel> otherPoints)
        {
            List<double> distances = new List<double>();
            int personCount = 0;
            foreach (TransactionMatrixModel otherPoint in otherPoints)
            {
                if (otherPoints.Count == 1)
                    return 0;

                if (pointOne.PersonId == otherPoint.PersonId)
                    continue;

                double distance = 0;
                for (int i = 0; i < NumberOfVectors; i++)
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

        public double SumOfSquaredErrors(List<ClusterDistanceModel> result)
        {
            return result.Sum(m => Math.Pow(m.ClusterDistances[m.CurrentCluster], 2));
        }

        private double EuclideanDistance(double value, double value2)
        {
            return Math.Pow(value - value2, 2);
        }
    }
}