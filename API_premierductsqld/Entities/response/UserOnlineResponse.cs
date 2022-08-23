using System;
namespace API_premierductsqld.Entities
{
    public class StationAttendees
    {
        public StationAttendees(int stationNo, string userName, string Name)
        {
            this.stationNo = stationNo;
            this.username = userName;
            this.name = Name;
        }


        public int stationNo { get; set; }
        public string username { get; set; }
        public string name { get; set; }
    }
}
