using System.Collections.Generic;
using System.Data;
using API_premierductsqld.Entities.response.report;
using API_premierductsqld.Service;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Controllers
{
	[ApiController]
	[Route("report")]
	public class ReportController
	{

		private ReportService reportService;

		public ReportController()
		{
			reportService = new ReportService();
		}

		[HttpGet("1")]
		public void Report1(string date)
		{
			reportService.report1(date);

		}

		[HttpGet("2")]
		public void Report2(string date)
		{
			reportService.report2(date);

		}

		[HttpGet("3")]
		public void Report3(string date)
		{

			reportService.report3(date);

		}

		[HttpGet("4")]
		public void Report4(string date)
		{
			reportService.report4(date);

		}

		[HttpGet("weekend")]
		public string ReportForWeekend(string date)
		{
			return reportService.reportForWeekend(date);

		}

		[HttpGet("weekend/packing")]
		public void ReportForWeekendPacking(string date)
		{
			reportService.reportForWeekendPacking(date);

		}
		[HttpDelete("delete")]
		public void DeleteFiles(string date)
		{
			reportService.DeleteDataByDate(date);

		}
	}
}