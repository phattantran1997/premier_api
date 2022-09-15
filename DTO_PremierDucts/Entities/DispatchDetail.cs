using System;
using System.ComponentModel;

namespace DTO_PremierDucts.Entities
{
    public class DispatchDetail
    {
        public string operatorID { get; set; }
        public string jobday { get; set; }
        public string jobtime { get; set; }
        public string duration { get; set; }
        public string receiverName { get; set; }
        public string receiverEmail { get; set; }
        public string stationName { get; set; }
        public string jobno { get; set; }
        public string drawingno { get; set; }
        public string handle { get; set; }
        public string itemno { get; set; }
        public string insulation { get; set; }
        public string galvenized { get; set; }
        public string notes { get; set; }
        public string weight { get; set; }
        public string status { get; set; }
        public double qty { get; set; }
        public string cuttype { get; set; }
        public string cid { get; set; }
        public string description { get; set; }
        public string doublewall { get; set; }
        public int pathId { get; set; }
        public string insulationarea { get; set; }
        public string metalarea { get; set; }
        public string boughtout { get; set; }
        public string linearmeter { get; set; }
        public string sectionindex { get; set; }
        public string sectiondescription { get; set; }
        public string prefixstring { get; set; }
        public string insulationSpec { get; set; }
        public string widthDim { get; set; }
        public string depthDim { get; set; }
        public string lengthangle { get; set; }
        public string connector { get; set; }
        public string material { get; set; }
        public string equipmentTag { get; set; }
        public string jobArea { get; set; }
        public string filename { get; set; }
        public DispatchDetail()
        {
        }
    }
}
