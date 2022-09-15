using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using API_premierductsqld.Entities;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Global;
using DTO_PremierDucts.DBClient;
using DTO_PremierDucts.Entities;
using DTO_PremierDucts.Utils;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace API_premierductsqld.Repository
{
	public interface IJobtimingRepository
	{
		public GetCurrentJobTimingsResponse getCurrentJobByUser(string date, string operatorID);

		public List<ListJobNoDashBoardResponse> getListDetailJobNoOnDashboard(string date, int stationNo);

		public string calculateIntervalEachJobNo(int stationNo, DataTable dataTable, string jobNO);

		public string calculateIntervalEachJobNowithoutDT(int stationNo, string jobNO);

		List<JobTimingResponse> getJobTimingsDetail(List<string> jobno);
		//use
		List<string> getListJobNoString(string date, string end);

		List<JobNoandStationNoResponse> getListJobNoAndStationNo(string date, string end);
		//use
		List<JobTimingResponse> getAllDataJobtimingByDate(string date);

		List<JobTimingResponse> getAllDataJobtimingByUserAndDate(string user, string date);

		List<JobTimingResponse> listJobTimingOnTabJobs(string date_start, string date_end);

	}

	public class JobtimingRepository : IJobtimingRepository
	{

		//private readonly AppDbContext _appDbContext;

		private List<string> distinct = new List<string>();

		static DBConnection dbCon;

		public JobtimingRepository()
		{
			dbCon = DBConnection.Instance(Startup.StaticConfig.GetConnectionString("ConnectionForDatabase"));

		}

		public List<JobTimingResponse> getAllDataJobtimingByDate(string date)
		{
			List<JobTimingResponse> rsult = new List<JobTimingResponse>();

			if (dbCon.IsConnect())
			{
				try
				{
					DataTable dataTable = new DataTable();
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(QueryGlobals.Query_GetAllDataJobtimingByDate_1, dbCon.Connection);
					myDataAdapter.SelectCommand.Parameters.AddWithValue("@PARAM_VAL_1", date);
					myDataAdapter.Fill(dataTable);
					foreach (DataRow row in dataTable.Rows)
					{
						rsult.Add(new JobTimingResponse(row.Field<string>("jobno"),
							row.Field<string>("operatorID"),
							row.Field<string>("jobday"),
							row.Field<string>("jobtime"),
							row.Field<Int32>("id"),
					   row.Field<Int32>("stationNo"),
					   row.Field<string>("duration"),
					   row.Field<string>("filename"),
					   row.Field<string>("handle"),
					   row.Field<string>("itemno"), "", ""));
					}

				}
				catch (Exception e)
				{
					throw e;

				}
				finally
				{
					dbCon.Close();
				}

			}


			return rsult;

		}


		public GetCurrentJobTimingsResponse getCurrentJobByUser(string date, string operatorID)
		{
			GetCurrentJobTimingsResponse response = new GetCurrentJobTimingsResponse();

			if (dbCon.IsConnect())
			{
				try
				{
					DataTable dataTable = new DataTable();

					string query = "select j.jobno, j.jobday, j.id, j.operatorID, j.jobtime , j.stationNo, s.stationName, j.duration from jobtiming j join stationManagement s" +
							" on j.stationNo = s.stationNo where j.itemno != 'Button' and j.itemno != 'Swipe' and j.itemno != 'Station' and jobday = '" + date + "' and j.operatorID ='" + operatorID + "'  order by j.jobtime desc limit 1;";
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
					myDataAdapter.Fill(dataTable);
					foreach (DataRow row in dataTable.Rows)
					{
						response = new GetCurrentJobTimingsResponse
						{
							operatorID = row.Field<string>("operatorID"),
							jobno = row.Field<string>("jobno"),
							jobtime = row.Field<string>("jobtime"),
							duration = row.Field<string>("duration"),
							stationNo = row.Field<Int32>("stationNo"),
							stationName = row.Field<string>("stationName")

						};
					}

				}
				catch (Exception e)
				{
					throw e;

				}
				finally
				{
					dbCon.Close();
				}

			}
			return response;

		}

		public List<ListJobNoDashBoardResponse> getListDetailJobNoOnDashboard(string date, int stationNo)
		{
			List<ListJobNoDashBoardResponse> responses = new List<ListJobNoDashBoardResponse>();

			try
			{
				DataTable dataTable = new DataTable();
				DataTable dataTable1 = new DataTable();

				string query = "select jobno, COUNT(operatorID) as people from jobtiming where jobday = '" + date + "' " +
					"and stationNo = " + stationNo + " " +
					"and itemno != 'Button' " +
					"and itemno != 'Swipe' " +
					"and itemno != 'Station' " +
					"and jobno != 'Invalid' " +
					"and itemno != '' group by jobno; ";
				if (dbCon.IsConnect())
				{

					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.ConnectionString);
					myDataAdapter.Fill(dataTable);

					var joinedPenalty = string.Join(",", dataTable.AsEnumerable().Select(x => "'" + x.Field<string>("jobno") + "'").ToArray());

					if (!String.IsNullOrEmpty(joinedPenalty))
					{
						string query2 = "select jobno, j.stationNo, s.stationName,jobtime, jobday, j.duration from jobtiming j join stationManagement s" +
					  " on j.stationNo = s.stationNo where j.jobno in (" + joinedPenalty + ")  order by STR_TO_DATE(j.jobday, '%d/%m/%Y') asc,j.jobtime asc;";

						myDataAdapter = new MySqlDataAdapter(query2, dbCon.ConnectionString);
						myDataAdapter.Fill(dataTable1);
					}
				}


				List<string> jobnoList = new List<string>();
				foreach (DataRow dr in dataTable.Rows)
				{

					ListJobNoDashBoardResponse item = new ListJobNoDashBoardResponse();

					//sum of duration
					double sum = dataTable1.AsEnumerable()
					.Where(r => r.Field<string>("jobno").Equals(dr["jobno"]) && !String.IsNullOrEmpty(r.Field<string>("duration")))
					.Sum(r => TimeSpan.Parse(r.Field<string>("duration")).TotalSeconds);

					//status station current
					item.status_current = dataTable1.AsEnumerable().Where(i => i.Field<string>("jobno").Equals(dr["jobno"].ToString()))
						.LastOrDefault().Field<string>("stationName");

					//convert sum to Timespan
					item.labour_time = (int)TimeSpan.FromSeconds(sum).TotalHours + TimeSpan.FromSeconds(sum).ToString(@"\:mm\:ss");
					item.jobno = dr["jobno"].ToString();
					item.people = Convert.ToInt32(dr["people"].ToString());
					responses.Add(item);

				}
			}
			catch (Exception e)
			{
				throw e;

			}
			finally
			{
				dbCon.Close();
			}

			return responses;

		}

		public string calculateIntervalEachJobNo(int stationNo, DataTable dataTable, string jobNo)
		{

			try
			{
				DataTable dataTable1 = dataTable.AsEnumerable()
							.Where(r => r.Field<Int32>("stationNo") == stationNo && r.Field<string>("jobno").Equals(jobNo)).CopyToDataTable();


				if (dataTable1.Rows.Count == 0)
				{
					return "00:00:00";
				}
				else if (dataTable1.Rows.Count == 1)
				{
					return TimeSpan.Parse(dataTable1.AsEnumerable().FirstOrDefault().Field<string>("duration")).TotalMinutes.ToString("0.00");
				}
				string tempjobtime = dataTable1.Rows[0].Field<string>("jobtime");
				string tempjobday = dataTable1.Rows[0].Field<string>("jobday");
				string tempduration = dataTable1.Rows[0].Field<string>("duration");

				TimeSpan min = new TimeSpan();
				TimeSpan max = new TimeSpan();

				TimeSpan total = new TimeSpan();
				dataTable1.Rows.Remove(dataTable1.Rows[0]);

				foreach (DataRow jobTiming in dataTable1.Rows)
				{
					TimeSpan endtemp = TimeSpan.Parse(tempjobtime).Add(TimeSpan.Parse(tempduration));
					TimeSpan endjobtming = TimeSpan.Parse(jobTiming.Field<string>("jobtime")).Add(TimeSpan.Parse(jobTiming.Field<string>("duration")));

					//6h - 6h15 ->temp
					//6h10 - 6h20 ->jobtimg ->temp
					//6h15 - 6h30 -> jt
					//6h20 - 6h40
					if (jobTiming.Field<string>("jobday").Equals(tempjobday) && TimeSpan.Parse(jobTiming.Field<string>("jobtime")) < endtemp
						&& endjobtming > endtemp)
					{
						if (min == max)
						{
							min = TimeSpan.Parse(tempjobtime);

						}
						max = endjobtming;
					}
					else
					{
						if (jobTiming.Field<string>("jobday").Equals(tempjobday) && endjobtming < endtemp)
						{
							continue;
						}

						if (min != max)
						{
							total = total.Add(max.Subtract(min));
							min = max = new TimeSpan();

						}
						else
						{
							total = total.Add(TimeSpan.Parse(tempduration));

						}

					}

					tempjobtime = jobTiming.Field<string>("jobtime");
					tempjobday = jobTiming.Field<string>("jobday");
					tempduration = jobTiming.Field<string>("duration");

				}


				if (min != max)
				{
					total = total.Add(max.Subtract(min));
				}
				else
				{
					total = total.Add(TimeSpan.Parse(tempduration));

				}

				return total.TotalMinutes.ToString("0.00");

			}
			catch (Exception e)
			{
				throw e;

			}

		}

		public string calculateIntervalEachJobNowithoutDT(int stationNo, string jobNO)
		{
			try
			{
				DataTable dataTable = new DataTable();
				if (dbCon.IsConnect())
				{
					string query = "select * from jobtiming where stationNo = " + stationNo + " and jobno ='" + jobNO + "'  order by STR_TO_DATE(jobday, '%d/%m/%Y') asc , jobtime asc;";
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.ConnectionString);
					myDataAdapter.Fill(dataTable);
				}


				if (dataTable.Rows.Count == 0)
				{
					return "00:00:00";
				}
				else if (dataTable.Rows.Count == 1)
				{
					return TimeSpan.Parse(dataTable.AsEnumerable().FirstOrDefault().Field<string>("duration")).TotalMinutes.ToString("0.00");
				}

				string tempjobtime = dataTable.Rows[0].Field<string>("jobtime");
				string tempjobday = dataTable.Rows[0].Field<string>("jobday");
				string tempduration = dataTable.Rows[0].Field<string>("duration");

				TimeSpan min = new TimeSpan();
				TimeSpan max = new TimeSpan();

				TimeSpan total = new TimeSpan();
				dataTable.Rows.Remove(dataTable.Rows[0]);

				foreach (DataRow jobTiming in dataTable.Rows)
				{
					TimeSpan endtemp = TimeSpan.Parse(tempjobtime).Add(TimeSpan.Parse(tempduration));
					TimeSpan endjobtming = TimeSpan.Parse(jobTiming.Field<string>("jobtime")).Add(TimeSpan.Parse(jobTiming.Field<string>("duration")));

					if (jobTiming.Field<string>("jobday").Equals(tempjobday) && TimeSpan.Parse(jobTiming.Field<string>("jobtime")) < endtemp
						&& endjobtming > endtemp)
					{
						if (min == max)
						{
							min = TimeSpan.Parse(tempjobtime);

						}
						max = endjobtming;
					}
					else
					{
						if (jobTiming.Field<string>("jobday").Equals(tempjobday) && endjobtming < endtemp)
						{
							continue;
						}

						if (min != max)
						{
							total = total.Add(max.Subtract(min));
							min = max = new TimeSpan();

						}
						else
						{
							total = total.Add(TimeSpan.Parse(tempduration));

						}

					}


					tempjobtime = jobTiming.Field<string>("jobtime");
					tempjobday = jobTiming.Field<string>("jobday");
					tempduration = jobTiming.Field<string>("duration");


				}


				if (min != max)
				{
					total = total.Add(max.Subtract(min));
				}
				else
				{
					total = total.Add(TimeSpan.Parse(tempduration));

				}

				return total.TotalMinutes.ToString("0.00");

			}
			catch (Exception e)
			{
				throw e;

			}
		}

		public List<JobTimingResponse> getJobTimingsDetail(List<string> jobno)
		{
			List<JobTimingResponse> response = new List<JobTimingResponse>();
			if (dbCon.IsConnect())
			{
				try
				{
					DataTable all_jobtiming = new DataTable();
					var joinedPenalty = string.Join(",", jobno.Select(x => "'" + x + "'").ToArray());
					string query = "select distinct j.*, s.stationName from jobtiming j join stationManagement s" +
					   " on j.stationNo = s.stationNo where j.itemno != 'Button' and j.itemno != 'Swipe' and j.itemno != 'Station' and j.jobno in (" + joinedPenalty + ") ;";
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
					myDataAdapter.Fill(all_jobtiming);

					foreach (DataRow row in all_jobtiming.Rows)
					{
						JobTimingResponse jobTimingResponse = new JobTimingResponse();
						jobTimingResponse.jobno = row.Field<string>("jobno");
						jobTimingResponse.jobday = row.Field<string>("jobday");
						jobTimingResponse.id = row.Field<int>("id");
						jobTimingResponse.operatorID = row.Field<string>("operatorID");
						jobTimingResponse.jobtime = row.Field<string>("jobtime");
						jobTimingResponse.stationNo = row.Field<int>("stationNo");
						jobTimingResponse.stationName = row.Field<string>("stationName");
						jobTimingResponse.duration = row.Field<string>("duration");
						jobTimingResponse.itemno = row.Field<string>("itemno");
						jobTimingResponse.handle = row.Field<string>("handle");

						response.Add(jobTimingResponse);
					}

				}
				catch (Exception e)
				{
					throw e;

				}
				finally
				{
					dbCon.Close();
				}
			}
			return response;
		}

		public List<string> getListJobNoString(string date, string end)
		{
			if (dbCon.IsConnect())
			{
				try
				{
					string query = "select distinct j.jobno from jobtiming j where j.itemno != 'Button' and j.itemno != 'Swipe' and j.itemno != 'Station' and j.itemno !='' and j.jobday = '" + date + "'; ";

					if (StringUtils.CheckNullAndEmpty(end))
					{
						query = "SELECT distinct jobno FROM jobtiming where  (STR_TO_DATE(jobday, '%d/%m/%Y') between STR_TO_DATE('" + date + "' , '%d/%m/%Y') and STR_TO_DATE('" + end + "' , '%d/%m/%Y')) and itemno != 'Button' and itemno != 'Swipe' and itemno !='' and itemno != 'Station';";

					}
					DataTable dataTable = new DataTable();

					//get distinc jobno by day
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
					myDataAdapter.Fill(dataTable);

					int count = dataTable.Rows.Count;
					foreach (DataRow row in dataTable.Rows)
					{
						distinct.Add(row.Field<string>("jobno"));
					}
				}
				catch (Exception e)
				{
					throw e;

				}
				finally
				{
					dbCon.Close();
				}
			}
			return distinct;
		}

		public List<JobTimingResponse> listJobTimingOnTabJobs(string date_start, string date_end)
		{
			List<JobTiming> rsult = new List<JobTiming>();

			if (dbCon.IsConnect())

			{
				try
				{

					DataTable dataTable = new DataTable();

					string query = "SELECT distinct jobno FROM jobtiming where  (STR_TO_DATE(jobday, '%d/%m/%Y') between STR_TO_DATE('" + date_start + "' , '%d/%m/%Y') and STR_TO_DATE('" + date_end + "' , '%d/%m/%Y')) and itemno != 'Button' and itemno != 'Swipe' and itemno != 'Station';";
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
					myDataAdapter.Fill(dataTable);
					foreach (DataRow row in dataTable.Rows)
					{
						rsult.Add(new JobTiming
						{

							jobno = row.Field<string>("jobno")
						});
					}
				}
				catch (Exception e)
				{
					throw e;

				}
				finally
				{
					dbCon.Close();
				}

			}
			return rsult.Select(a => new JobTimingResponse()).ToList();

		}

		public List<JobNoandStationNoResponse> getListJobNoAndStationNo(string date, string end)
		{
			List<JobNoandStationNoResponse> responses = new List<JobNoandStationNoResponse>();
			if (dbCon.IsConnect())
			{
				try
				{
					string query = "SELECT distinct jobno, stationNo FROM jobtiming where  (STR_TO_DATE(jobday, '%d/%m/%Y') between STR_TO_DATE('" + date + "' , '%d/%m/%Y') and STR_TO_DATE('" + end + "' , '%d/%m/%Y')) and itemno != 'Button' and itemno != 'Swipe' and itemno !='' and itemno != 'Station';";
					DataTable dataTable = new DataTable();
					//get distinc jobno by day
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
					myDataAdapter.Fill(dataTable);

					int count = dataTable.Rows.Count;
					foreach (DataRow row in dataTable.Rows)
					{
						responses.Add(new JobNoandStationNoResponse(row.Field<string>("jobno"), row.Field<int>("stationNo")));
					}
				}
				catch (Exception e)
				{
					throw e;

				}
				finally
				{
					dbCon.Close();
				}
			}
			return responses;
		}

		public List<JobTimingResponse> getAllDataJobtimingByUserAndDate(string user, string date)
		{
			List<JobTiming> rsult = new List<JobTiming>();

			if (dbCon.IsConnect())
			{
				try
				{
					DataTable dataTable = new DataTable();
					string query = "select * " +
						"from jobtiming where jobday = '" + date + "' and itemno != 'Button' and itemno != 'Swipe' order by jobtime asc;";
					MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
					myDataAdapter.Fill(dataTable);
					foreach (DataRow row in dataTable.Rows)
					{
						rsult.Add(new JobTiming(row.Field<string>("jobno"), row.Field<string>("operatorID"), row.Field<string>("jobday"), row.Field<string>("jobtime"), row.Field<Int32>("id"),
					   row.Field<Int32>("stationNo"), row.Field<string>("duration"), row.Field<string>("filename"), row.Field<string>("handle"), row.Field<string>("itemno")));
					}
				}
				catch (Exception e)
				{
					throw e;

				}
				finally
				{
					dbCon.Close();
				}
			}
			return rsult.Select(a => new JobTimingResponse()).ToList();
		}
	}
}
