using System;
using System.ComponentModel;

namespace DTO_PremierDucts.Entities
{
    public class DispatchDetail
    {
        public string operatorID { get; set; } = string.Empty;
        public string jobday { get; set; } = string.Empty;
        public string jobtime { get; set; } = string.Empty;
        public string duration { get; set; } = string.Empty;
        public string receiverName { get; set; } = string.Empty;
        public string receiverEmail { get; set; } = string.Empty;
        public string stationName { get; set; } = string.Empty;
        public string jobno { get; set; } = string.Empty;
        public string drawingno { get; set; } = string.Empty;
        public string handle { get; set; } = string.Empty;
        public string itemno { get; set; } = string.Empty;
        public string insulation { get; set; } = string.Empty;
        public string galvenized { get; set; } = string.Empty;
        public string notes { get; set; } = string.Empty;
        public string weight { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public double qty { get; set; } = 0;
        public string cuttype { get; set; } = string.Empty;
        public string cid { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string doublewall { get; set; } = string.Empty;
        public int pathId { get; set; } = 0;
        public string insulationarea { get; set; } = string.Empty;
        public string metalarea { get; set; } = string.Empty;
        public string boughtout { get; set; } = string.Empty;
        public string linearmeter { get; set; } = string.Empty;
        public string sectionindex { get; set; } = string.Empty;
        public string sectiondescription { get; set; } = string.Empty;
        public string prefixstring { get; set; } = string.Empty;
        public string insulationSpec { get; set; } = string.Empty;
        public string widthDim { get; set; } = string.Empty;
        public string depthDim { get; set; } = string.Empty;
        public string lengthangle { get; set; } = string.Empty;
        public string connector { get; set; } = string.Empty;
        public string material { get; set; } = string.Empty;
        public string equipmentTag { get; set; } = string.Empty;
        public string jobArea { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public string storageInfo { get; set; } = string.Empty;
        public string resetDay { get; set; } = string.Empty;
        public string resetTime { get; set; } = string.Empty;

        public DispatchDetail()
        {
        }
    }
}
