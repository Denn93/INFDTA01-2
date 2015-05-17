using Excel;
using INFDTA01_2.DennisBrian.Assignment.Algorithm;
using INFDTA01_2.DennisBrian.Assignment.DataAccess;
using INFDTA01_2.DennisBrian.Assignment.Models;
using INFDTA01_2.DennisBrian.Assignment.Repository;
using System;
using System.Data;
using System.IO;

namespace INFDTA01_2.DennisBrian.Assignment
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("INFDTA Assignments");
            WineRepository repo = new WineRepository(new Database());

            Clustering clustering = new Clustering(repo.GetTransactionModel());
            clustering.CreateInitialClustering(4);

            /* foreach (TransactionMatrixModel row in clustering.TransactionMatrix)
                 Console.WriteLine(row);
 */
            Console.ReadLine();
        }

        private static DataSet InitializeExcelFile(string file)
        {
            var path = Directory.GetCurrentDirectory() + "\\" + file;
            FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read);

            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            excelReader.IsFirstRowAsColumnNames = true;
            return excelReader.AsDataSet();
        }
    }
}