namespace standcalcwaspnet.Models
{
    public class CalcModel
    {
        public int UserID { get; set; }
        public string SessionID { get; set; }
        public string Username { get; set; }
        public int UserCount { get; set; }
        public DateTime Date { get; set; }
        public string Expression { get; set; }
        public string Result { get; set; }
    }
}
