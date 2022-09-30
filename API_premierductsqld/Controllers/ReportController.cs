using System.Collections.Generic;
using System.Data;
using API_premierductsqld.Entities.response.report;
using API_premierductsqld.Service;
using DTO_PremierDucts.JWT_Authentication;
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
		[SkipAuthenticationHeaders]
		public void Report1(string date)
		{
			reportService.report1(date);

		}

		[HttpGet("2")]
		[SkipAuthenticationHeaders]
		public void Report2(string date)
		{
			reportService.report2(date);

		}

		[HttpGet("3")]
		[SkipAuthenticationHeaders]
		public void Report3(string date)
		{

			reportService.report3(date);

		}

		[HttpGet("4")]
		[SkipAuthenticationHeaders]
		public void Report4(string date)
		{
			reportService.report4(date);
		}

		[HttpGet("weekend")]
		[SkipAuthenticationHeaders]
		public void ReportForWeekend(string date)
		{
			reportService.reportForWeekend(date);

		}

		[HttpGet("weekend/packing")]
		[SkipAuthenticationHeaders]
		public void ReportForWeekendPacking(string date)
		{
			reportService.reportForWeekendPacking(date);

		}
		[HttpDelete("delete")]
		[SkipAuthenticationHeaders]
		public void DeleteFiles(string date)
		{
			reportService.DeleteDataByDate(date);

		}
	}
}