using System;
namespace DTO_PremierDucts.EntityResponse
{
    public class DispatchInforResponse
    {
        public string description { get; set; }
        public string insulationSpec { get; set; }
        public string widthDim { get; set; }
        public string depthDim { get; set; }
        public string lengthangle { get; set; }
        public string pathValue { get; set; }
        public string jobno { get; set; }
        public string handle { get; set; }
        public string filename { get; set; }
        public string itemno { get; set; }
        public string operatorID { get; set; }
        public string metalarea { get; set; }
        public string insulationarea { get; set; }
        public string jobtime { get; set; }
        public string duration { get; set; }
        public string jobday { get; set; }
        public string storageInfo { get; set; } = string.Empty;
        public string resetDay { get; set; } = string.Empty;
        public string resetTime { get; set; } = string.Empty;
    }
}

