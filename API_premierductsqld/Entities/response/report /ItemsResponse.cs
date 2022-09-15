using System;
using System.Collections.Generic;

namespace API_premierductsqld.Entities.response.report
{
	public class ItemsResponse
	{
        public int stationno { get; set; } = 0;
        public string employeeName { get; set; } = string.Empty;
        public string filename { get; set; } = string.Empty;
        public string handle { get; set; } = string.Empty;
        public string jobno { get; set; } = string.Empty;
        public string jobday { get; set; } = string.Empty;
        public string jobtime { get; set; } = string.Empty;
        public string metal
        {
            get; set;
        } = string.Empty;
        public string insu
        {
            get; set;
        } = string.Empty;
        public string duration { get; set; } = string.Empty;
        public string itemNo { get; set; } = string.Empty;
        public int doubleTapped { get; set; } = 0;
        //public List<ReportForCheckingDoubleResponse> doubleScanItems { get; set; }
        public ItemsResponse(int stationno, string employeeName, string filename, string handle, string itemNo, string jobno, string jobday, string jobtime, string duration, string metal, string insu)
        {
            this.stationno = stationno;
            this.employeeName = employeeName;
            this.filename = filename;
            this.handle = handle;
            this.itemNo = itemNo;
            this.jobday = jobday;
            this.jobtime = jobtime;
            this.jobno = jobno;
            this.duration = duration;
            this.metal = metal;
            this.insu = insu;
        }

    }
}

