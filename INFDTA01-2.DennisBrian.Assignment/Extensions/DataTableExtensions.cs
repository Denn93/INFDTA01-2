using INFDTA01_2.DennisBrian.Assignment.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace INFDTA01_2.DennisBrian.Assignment.Extensions
{
    internal static class DataTableExtensions
    {
        public static List<TransactionModel> ToTransactionModel(this DataTable dt)
        {
            return dt.Rows.Cast<DataRow>()
                .Select(
                    dataRow =>
                        new TransactionModel
                        {
                            OfferteId = Convert.ToInt32(dataRow["offerteid"]),
                            PersonId = Convert.ToInt32(dataRow["persoonid"]),
                            PersonName = dataRow["personName"].ToString()
                        }).ToList();
        }
    }
}