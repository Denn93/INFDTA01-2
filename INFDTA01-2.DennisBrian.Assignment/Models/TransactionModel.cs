namespace INFDTA01_2.DennisBrian.Assignment.Models
{
    internal class TransactionModel
    {
        public int PersonId { get; set; }

        public int OfferteId { get; set; }

        public string PersonName { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", OfferteId, PersonId, PersonName);
        }
    }
}