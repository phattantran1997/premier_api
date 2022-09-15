using System;
namespace API_premierductsqld.Entities.request
{
	public class GetDispatchInforByHandleJobNoRequest
	{

		public string jobno { get; set; }
		public string handle { get; set; }
		public string itemno { get; set; }

		public GetDispatchInforByHandleJobNoRequest()
		{
		}
	}
}

