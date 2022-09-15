using System;
namespace API_premierductsqld.Entities.response.report
{
	public class ReportPackingStationResponse
	{

		public string jobno { get; set; }
		public string jobday { get; set; }
		public int stationNo { get; set; }
		public string filename { get; set; }
		public string handle { get; set; }
		public string metalarea { get; set; }
		public string insuarea { get; set; }
		public ReportPackingStationResponse(string jobno, string jobday, int stationNo, string filename, string handle, string metalarea, string insuarea)
		{
			this.jobno = jobno;
			this.jobday = jobday;
			this.stationNo = stationNo;
			this.filename = filename;
			this.handle = handle;
			this.metalarea = metalarea;
			this.insuarea = insuarea;
		}
	}
}

