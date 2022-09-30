using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace API_premierductsqld.Global
{

	//QUERY WITH PREFIX QUERY_*(FUNCTION_NAME)_(NUMBER OF PARAMETER)
	public class QueryGlobals
	{
		public static string Query_GetAllStation = "select * from stationManagement where updateByItemNo = 1 or updateByJobNo = 1;";
		public static string Query_GetAllDataJobtimingByDate_1 = "SELECT * FROM jobtiming WHERE JOBDAY = @PARAM_VAL_1  AND ITEMNO != 'BUTTON' AND ITEMNO != 'SWIPE' ORDER BY JOBTIME ASC;";
		public static string Query_JobtimingJoinTarget_1 = @"select target.metalarea, target.insuarea, j.operatorid, j.duration, j.itemno, j.stationno, j.jobno, j.filename, j.handle, j.jobday, j.jobtime
		from jobtiming j
		left join target_measurement as target
		on (j.jobno = target.jobno and j.filename = target.filename and j.handle = target.handle )
		where j.stationno in (6,8,46) and j.jobday = @PARAM_VAL_1
		and j.jobno not like '% - on%' and j.jobno not like '% - logout%' 
		order by j.jobtime;";

		//left join jobtiming j2
		//on(j.jobno = j2.jobno and j.jobday = j2.jobday and j.stationno = j2.stationno and j.filename = j2.filename and j.handle = j2.handle and j.jobtime<j2.jobtime)
		//and j2.operatorID is null
		public static string Query_PackingInfor_1 = @"SELECT t.*, j.jobtime, j.operatorID , j.storageInfo, j.itemno
		FROM target_measurement t
		join jobtiming j
		on (j.jobday = t.jobday and j.stationNo= t.stationNo and j.jobno = t.jobno and j.filename = t.filename and j.handle = t.handle)
		where t.jobday = @PARAM_VAL_1;";

		public static string Query_PackingInfor2_1 = @"SELECT target.*, job1.jobtime, job1.operatorID , job1.storageInfo, job1.itemno
		FROM target_measurement as target
		left join jobtiming as job1
		on (job1.jobday = target.jobday and job1.stationNo= target.stationNo and job1.jobno = target.jobno and job1.filename = target.filename and job1.handle = target.handle)
        LEFT join jobtiming as j2
        ON (job1.jobday = j2.jobday and job1.stationNo= j2.stationNo and job1.jobno = j2.jobno and job1.filename = j2.filename and job1.handle = j2.handle and job1.jobtime < j2.jobtime)
		where target.jobday = @PARAM_VAL_1 and j2.operatorID is Null
		order by job1.jobtime;";
		
		public static string Query_PackingInforWeekend_2 = @"SELECT target.*, job1.jobtime, job1.operatorID , job1.storageInfo, job1.itemno
		FROM target_measurement as target
		left join jobtiming as job1
		on (job1.jobday = target.jobday and job1.stationNo= target.stationNo and job1.jobno = target.jobno and job1.filename = target.filename and job1.handle = target.handle)
        LEFT join jobtiming as j2
        ON (job1.jobday = j2.jobday and job1.stationNo= j2.stationNo and job1.jobno = j2.jobno and job1.filename = j2.filename and job1.handle = j2.handle and job1.jobtime < j2.jobtime)
		where (STR_TO_DATE(target.jobday, '%d/%m/%Y')  between STR_TO_DATE(@PARAM_VAL_1 , '%d/%m/%Y') and STR_TO_DATE(@PARAM_VAL_2 , '%d/%m/%Y')) and j2.operatorID is Null
		order by job1.jobtime;";

		public static string Query_Report4 = @"SELECT * from report_4
		where (STR_TO_DATE(jobday, '%d/%m/%Y')  between STR_TO_DATE(@PARAM_VAL_1 , '%d/%m/%Y') and STR_TO_DATE(@PARAM_VAL_2 , '%d/%m/%Y'))";
	}
}

