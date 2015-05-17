using INFDTA01_2.DennisBrian.Assignment.DataAccess;
using INFDTA01_2.DennisBrian.Assignment.Extensions;
using INFDTA01_2.DennisBrian.Assignment.Models;
using System.Collections.Generic;

namespace INFDTA01_2.DennisBrian.Assignment.Repository
{
    internal class WineRepository
    {
        private Database _db;

        public WineRepository(Database db)
        {
            _db = db;
        }

        public List<TransactionModel> GetTransactionModel()
        {
            string query = "SELECT offerteid, pers.persoonid, pers.name personname FROM transactie trans " +
                            "INNER JOIN persoon pers ON pers.persoonid = trans.persoonid";
            return _db.Get(query).ToTransactionModel();
        }
    }
}