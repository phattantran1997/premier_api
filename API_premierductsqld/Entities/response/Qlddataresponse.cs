using System;
namespace API_premierductsqld.Entities.response
{
    public class Qlddataresponse
    {
        public string jobNO { get; set; }
        public double meta_m2 { get; set; }
        public double isu_m2 { get; set; }
        public string cuttype { get; set; }
        public UInt32 pathID { get; set; }
        public string pathValue { get; set; }
        public string stationName { get; set; }
        public Qlddataresponse()
        {
        }
    }
}
