using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using API_premierductsqld.Entities;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Repository.@interface;
using DTO_PremierDucts.DBClient;
using DTO_PremierDucts.Utils;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace API_premierductsqld.Repository.impl
{
    public class JobtimingRepository : IJobtimingRepository
    {

        //private readonly AppDbContext _appDbContext;

        private List<string> distinct = new List<string>();

        private DBConnection dbCon;


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
                    string query = "select * " +
                        "from premierductsqld.jobtiming where jobday = '" + date + "' and itemno != 'Button' and itemno != 'Swipe' order by jobtime asc;";
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
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
                       row.Field<string>("itemno"),"",""));
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

                    string query = "select j.jobno, j.jobday, j.id, j.operatorID, j.jobtime , j.stationNo, s.stationName, j.duration from premierductsqld.jobtiming j join premierductsqld.stationManagement s" +
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


        //public List<JobTimingResponse> getAllJobNoByDateWithoutStation(string date)
        //{
        //    try
        //    {
        //        List<JobTimingResponse> result = _appDbContext.JobTiming.Where(item => item.jobday == date
        //     && item.itemno != "Button"
        //         && item.itemno != "Swipe"
        //         && item.itemno != "Station"
        //         && item.itemno != "").GroupBy(item => item.jobno).Select(g => new JobTimingResponse
        //         {
        //             jobno = g.Key.ToString()
        //         }).ToList();
        //        return result;

        //    }
        //    catch (Exception e)
        //    {
        //        throw e;

        //    }
        //}




        public List<ListJobNoDashBoardResponse> getListDetailJobNoOnDashboard(string date, int stationNo)
        {
            List<ListJobNoDashBoardResponse> responses = new List<ListJobNoDashBoardResponse>();

            try
            {
                DataTable dataTable = new DataTable();
                DataTable dataTable1 = new DataTable();

                string query = "select jobno, COUNT(operatorID) as people from premierductsqld.jobtiming where jobday = '" + date + "' " +
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
                        string query2 = "select jobno, j.stationNo, s.stationName,jobtime, jobday, j.duration from premierductsqld.jobtiming j join premierductsqld.stationManagement s" +
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

                    //interval
                    var timer = new Stopwatch();
                    timer.Start();
                    item.interval = calculateIntervalEachJobNo(stationNo, dataTable1, dr.Field<string>("jobno"));
                    //jobnoList.Add(dr.Field<string>("jobno"));
                    timer.Stop();

                    TimeSpan timeTaken = timer.Elapsed;
                    string foo = "Time to interval: " + timeTaken.ToString(@"m\:ss\.fff");
                    Console.WriteLine(foo);

                    item.jobno = dr["jobno"].ToString();
                    item.people = Convert.ToInt32(dr["people"].ToString());
                    responses.Add(item);

                }

                //List <jobno,interval> select time_to_sec(duration) from jobtiming where jobno in ()

                //get all job no in specific day
                //var getJTFromCurrentDay = _appDbContext.JobTiming.Where(item => item.jobday.Equals(date)).ToList();

                ////lấy hết job no với station no
                //List<ListJobNoDashBoardResponse> rsult = (from jobitem in getJTFromCurrentDay
                //                                          orderby jobitem.jobtime
                //                                          where ( jobitem.stationNo == stationNo
                //                                         && jobitem.itemno != "Button"
                //                                         && jobitem.itemno != "Swipe"
                //                                         && jobitem.itemno != "Station"
                //                                         && jobitem.jobno != "Invalid"
                //                                         && jobitem.itemno != "")

                //                                          group jobitem by jobitem.jobno into g
                //                                          select new ListJobNoDashBoardResponse
                //                                          {
                //                                              jobno = g.Key,
                //                                              people = g.Select(item => item.operatorID
                //                                              ).Distinct().Count()

                //                                          }).ToList();



                //query 1 lan voi database
                //var data = (from j in _appDbContext.JobTiming
                //            where rsult.Select(i=>i.jobno).Contains(j.jobno)
                //            orderby j.jobday, j.jobtime
                //            select new
                //            {
                //                jobno=j.jobno,
                //                station = j.stationNo,
                //                duration = j.duration
                //            }).ToList();
                //foreach (ListJobNoDashBoardResponse item in rsult)
                //{

                //    var data = (from j in _appDbContext.JobTiming
                //                where j.jobno == item.jobno
                //                orderby j.jobday, j.jobtime
                //                join station in _appDbContext.Station
                //                on j.stationNo equals station.stationNo
                //                select new
                //                {
                //                    station = station.stationName,
                //                    duration = j.duration
                //                }).ToList();

                //    double sum = data.Where(i =>i.jobno.Equals(item.jobno) && !String.IsNullOrEmpty(i.duration)).Select(i => TimeSpan.Parse(i.duration).TotalSeconds).Sum(); 
                //    item.status_current = stationRepository.getStationDetail( data.Where(i => i.jobno.Equals(item.jobno)).LastOrDefault().station).stationName;
                //    item.labour_time = (int)TimeSpan.FromSeconds(sum).TotalHours + TimeSpan.FromSeconds(sum).ToString(@"\:mm\:ss");
                //    item.interval = calculateIntervalEachJobNo( stationNo, item.jobno);


                //}

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
                //jobTimingDataPerdate = jobTimingDataPerdate.OrderBy(y => DateTime.ParseExact(y.jobday, "d/M/yyyy", CultureInfo.InvariantCulture)).ThenBy(z => DateTime.ParseExact(z.jobtime, "HH:mm:ss", CultureInfo.InvariantCulture)).ToList();

                //JobTiming temp = jobTimingDataPerdate.FirstOrDefault();

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
                    //temp = jobTiming;


                    tempjobtime = jobTiming.Field<string>("jobtime");
                    tempjobday = jobTiming.Field<string>("jobday");
                    tempduration = jobTiming.Field<string>("duration");

                    ////khac ngay thi cong don
                    //if (!jobTiming.jobday.Equals(temp.jobday))
                    //{

                    //    total = total.Add(≈≈.Parse(temp.duration));

                    //}
                    ////in same day
                    //else
                    //{
                    //    if (TimeSpan.Parse(jobTiming.jobtime) > endtemp)
                    //    {
                    //        total = total.Add(TimeSpan.Parse(temp.duration));


                    //    }
                    //    else if (endjobtming >= endtemp)
                    //    {
                    //        min = TimeSpan.Parse(temp.jobtime);
                    //        max = endjobtming;
                    //        //total = total.Add(endjobtming.Subtract(TimeSpan.Parse(temp.jobtime)));

                    //    }
                    //    else
                    //    {

                    //    }



                    //}
                }


                if (min != max)
                {
                    total = total.Add(max.Subtract(min));
                }
                else
                {
                    total = total.Add(TimeSpan.Parse(tempduration));

                }
                //List<JobTiming> lisJobtimingWithStaionAndJob = jobTimingDataPerdate.Where(i => i.jobno.Equals(jobNo) && i.stationNo == stationNo).OrderBy(i => i.jobtime).ToList();
                //var targetList = jobTimings.Select(a => new JobTimingResponse()).ToList();


                //TimeSpan total = new TimeSpan();
                //TimeSpan total_bet_interval = new TimeSpan();


                //foreach (JobTiming jobTimingResponse in lisJobtimingWithStaionAndJob)
                //{
                //    var resul = jobTimingDataPerdate.Where(i => i.operatorID.Equals(jobTimingResponse.operatorID)).OrderBy(i => i.jobtime).ToList();


                //    int index = resul.FindIndex(i => i.id == jobTimingResponse.id);
                //    if(index == resul.Count-1)
                //    {
                //        continue;
                //    }
                //    if (min == max)
                //    {
                //        min = TimeSpan.Parse(jobTimingResponse.jobtime);

                //        max = TimeSpan.Parse(resul.Skip(index + 1).FirstOrDefault().jobtime);

                //    }

                //    else
                //    {
                //        TimeSpan max_temp = TimeSpan.Parse(resul.Skip(index + 1).FirstOrDefault().jobtime);
                //        if (TimeSpan.Parse(jobTimingResponse.jobtime) < max)
                //        {
                //            if (max_temp > max)
                //            {
                //                max = max_temp;
                //            }
                //        }
                //        else
                //        {
                //            total = total.Add(max.Subtract(min));
                //            min = TimeSpan.Parse(jobTimingResponse.jobtime);
                //            max = max_temp;
                //        }

                //    }


                //}

                //total = total.Add(max.Subtract(min));
                ////return (int)total.TotalHours + total.ToString(@"\:mm\:ss");


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
                //dbCon = DBConnection.Instance(Startup.StaticConfig.GetConnectionString("ConnectionForDatabase"));

                DataTable dataTable = new DataTable();
                if (dbCon.IsConnect())
                {
                    string query = "select * from premierductsqld.jobtiming where stationNo = " + stationNo + " and jobno ='" + jobNO + "'  order by STR_TO_DATE(jobday, '%d/%m/%Y') asc , jobtime asc;";
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
            //dbCon = DBConnection.Instance(Startup.StaticConfig.GetConnectionString("ConnectionForDatabase"));

            if (dbCon.IsConnect())
            {
                try
                {

                    DataTable all_jobtiming = new DataTable();

                    var joinedPenalty = string.Join(",", jobno.Select(x => "'" + x + "'").ToArray());


                    string query = "select j.jobno, j.jobday, j.id, j.operatorID, j.jobtime , j.stationNo, s.stationName, j.duration from premierductsqld.jobtiming j join premierductsqld.stationManagement s" +
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
            //dbCon = DBConnection.Instance(Startup.StaticConfig.GetConnectionString("ConnectionForDatabase"));

            if (dbCon.IsConnect())
            {
                try
                {
                    string query = "select distinct j.jobno from premierductsqld.jobtiming j where j.itemno != 'Button' and j.itemno != 'Swipe' and j.itemno != 'Station' and j.itemno !='' and j.jobday = '" + date + "'; ";

                    if (StringUtils.CheckNullAndEmpty(end))
                    {
                        query = "SELECT distinct jobno FROM premierductsqld.jobtiming where  (STR_TO_DATE(jobday, '%d/%m/%Y') between STR_TO_DATE('" + date + "' , '%d/%m/%Y') and STR_TO_DATE('" + end + "' , '%d/%m/%Y')) and itemno != 'Button' and itemno != 'Swipe' and itemno !='' and itemno != 'Station';";

                    }
                    DataTable distinct_jobtiming_currentdate = new DataTable();
                    //get distinc jobno by day
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
                    myDataAdapter.Fill(distinct_jobtiming_currentdate);

                    int count = distinct_jobtiming_currentdate.Rows.Count;
                    foreach (DataRow row in distinct_jobtiming_currentdate.Rows)
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

                    string query = "SELECT distinct jobno FROM premierductsqld.jobtiming where  (STR_TO_DATE(jobday, '%d/%m/%Y') between STR_TO_DATE('"+date_start+"' , '%d/%m/%Y') and STR_TO_DATE('"+date_end+"' , '%d/%m/%Y')) and itemno != 'Button' and itemno != 'Swipe' and itemno != 'Station';";
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

                    string query = "SELECT distinct jobno, stationNo FROM premierductsqld.jobtiming where  (STR_TO_DATE(jobday, '%d/%m/%Y') between STR_TO_DATE('" + date + "' , '%d/%m/%Y') and STR_TO_DATE('" + end + "' , '%d/%m/%Y')) and itemno != 'Button' and itemno != 'Swipe' and itemno !='' and itemno != 'Station';";

                
                    DataTable distinct_jobtiming_currentdate = new DataTable();
                    //get distinc jobno by day
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
                    myDataAdapter.Fill(distinct_jobtiming_currentdate);

                    int count = distinct_jobtiming_currentdate.Rows.Count;
                    foreach (DataRow row in distinct_jobtiming_currentdate.Rows)
                    {
                        responses.Add(new JobNoandStationNoResponse( row.Field<string>("jobno"), row.Field<int>("stationNo")));
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
                        "from premierductsqld.jobtiming where jobday = '" + date + "' and itemno != 'Button' and itemno != 'Swipe' order by jobtime asc;";
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
