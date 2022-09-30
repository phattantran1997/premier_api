using System;
using System.Collections.Generic;

namespace DTO_PremierDucts
{

	public enum API_TYPE
	{
		#region [APP_USER]
		LOGIN = 0,
		APP_USER_GET_ONLINE_USER,
		APP_USER_GET_OFFLINE_USER,
		APP_USER_GET_USER_FOR_REPORT,
		#endregion

		#region [PREMIER_DUCTS]
		PREMIER_DUCTS_GET_ALL_JOBTIMING,
		PREMIER_DUCTS_GET_ALL_JOBTIMING_DETAIL_BY_DATE,
		PREMIER_DUCTS_GET_STATION_DATA,
		PREMIER_DUCTS_GET_ALL_JOBTIMING_DATES,
		PREMIER_DUCTS_GET_ALL_DURATION_STATION,
		#endregion

		#region [QLD_DATA]
		QLD_GET_ALL,
		QLD_TOTAL_ALL_M2,
		QLD_DISPATCH_INFO_BY_LIST_BOX,
		QLD_DISPATCH_INFO_BY_LIST_JOBNO,
		QLD_ALL_ITEMS_PER_CAGE
		#endregion


	}

	public class APIPath
	{
		public static Dictionary<API_TYPE, String> HttpCommands = new Dictionary<API_TYPE, string>()
		{
           #region [APP_USER]
           {API_TYPE.LOGIN, "/user/login" },
		   {API_TYPE.APP_USER_GET_OFFLINE_USER, "/user/getOfflineUsers" },
		   {API_TYPE.APP_USER_GET_ONLINE_USER, "/user/getOnlineUsers" },
		   {API_TYPE.APP_USER_GET_USER_FOR_REPORT, "/user/getUserForReport" },
           #endregion

            #region [PREMIER_DUCTS]
            {API_TYPE.PREMIER_DUCTS_GET_ALL_JOBTIMING, "/jobtiming/data/detail" },
			{API_TYPE.PREMIER_DUCTS_GET_ALL_JOBTIMING_DETAIL_BY_DATE, "/jobtiming/all/data/by_date"},
			{API_TYPE.PREMIER_DUCTS_GET_STATION_DATA, "/station/all"},
			{API_TYPE.PREMIER_DUCTS_GET_ALL_JOBTIMING_DATES, "/jobtiming/list/dates" },
			{API_TYPE.PREMIER_DUCTS_GET_ALL_DURATION_STATION, "/station/all/duration" },

            #endregion

            #region [QLD_DATA]
            {API_TYPE.QLD_GET_ALL, "/dashboard/get/all/qldata"},
			{API_TYPE.QLD_TOTAL_ALL_M2, "/dashboard/total/all/m2/by-station"},
			{API_TYPE.QLD_DISPATCH_INFO_BY_LIST_BOX,"/reportQLD/dispatch/info/box" },
			{API_TYPE.QLD_DISPATCH_INFO_BY_LIST_JOBNO,"/reportQLD/dispatch/info/jobno" },
			{API_TYPE.QLD_ALL_ITEMS_PER_CAGE,"/reportQLD/dispatch/info/jobno" }
            
            #endregion
        };

	}
}

