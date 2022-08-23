using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using API_premierductsqld.Entities;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Repository.@interface;
using DTO_PremierDucts.DBClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace API_premierductsqld.Repository.impl
{
    public class StationRepository : IStationRepository
    {

        static DBConnection dbCon;

        IJobtimingRepository jobtimingRepository = new JobtimingRepository();

        public StationRepository()
        {
            dbCon = DBConnection.Instance(Startup.StaticConfig.GetConnectionString("ConnectionForDatabase"));

        }


        public List<StationResponse> getAllStation()
        {
            List<StationResponse> stations = new List<StationResponse>();

            try
            {
                if (dbCon.IsConnect())
                {
                    DataTable dataTable = new DataTable();

                    string querylist = "select * from premierductsqld.stationManagement where updateByItemNo = 1 or updateByJobNo = 1;";
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(querylist, dbCon.Connection);

                    myDataAdapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        StationResponse station = new StationResponse
                        {

                            stationName = row.Field<string>("stationName"),
                            stationGroup = row.Field<int>("stationGroup"),
                            stationStatus = row.Field<string>("stationStatus"),
                            stationNo = row.Field<int>("stationNo")
                        };

                        stations.Add(station);

                    }
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

            return stations;
        }

        public List<AllStationDashboardSettingsResponse> getAllStationDashboardWithRate(string date)
        {

            string rate = "";
            //var querylist = string.Join(",", query.Select(x => "'" + x + "'").ToArray());
            List<AllStationDashboardSettingsResponse> stations = new List<AllStationDashboardSettingsResponse>();
            try
            {
                if (dbCon.IsConnect())
                {
                    DataTable dataTable = new DataTable();

                    string querylist = "select * from premierductsqld.stationManagement where updateByItemNo = 1 or updateByJobNo = 1";
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(querylist, dbCon.Connection);

                    myDataAdapter.Fill(dataTable);

                    foreach (DataRow row in dataTable.Rows)
                    {
                        DataTable dataTable_getlattestCuurentJob = new DataTable();

                        //cach dung t2
                        //string x = $"Phat {date}";
                        string query2 = $"select jobno from premierductsqld.jobtiming where stationNo = " + row.Field<int>("stationNo") + " " +
                            "and jobday = '" + date + "' " +
                            " and itemno != 'Button' " +
                    "and itemno != 'Swipe' " +
                    "and itemno != 'Station' " +
                    "and jobno != 'Invalid' " +
                    "and itemno != ''" +
                            " order by STR_TO_DATE(jobday, '%d/%m/%Y') desc , jobtime desc;";
                        myDataAdapter = new MySqlDataAdapter(query2, dbCon.Connection);
                        myDataAdapter.Fill(dataTable_getlattestCuurentJob);
                        if (dataTable_getlattestCuurentJob.Rows.Count > 0)
                        {
                            string jobno = dataTable_getlattestCuurentJob.AsEnumerable().FirstOrDefault().Field<string>("jobno");
                            rate = jobtimingRepository.calculateIntervalEachJobNowithoutDT(row.Field<int>("stationNo"), jobno);
                        }

                        AllStationDashboardSettingsResponse station = new AllStationDashboardSettingsResponse
                        {

                            stationName = row.Field<string>("stationName"),
                            stationGroup = row.Field<int>("stationGroup"),
                            stationStatus = row.Field<string>("stationStatus"),
                            stationNo = row.Field<int>("stationNo"),
                            rate = rate

                        };

                        stations.Add(station);

                    }
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
            //List<AllStationDashboardSettingsResponse> stations = (from stationitem in _appDbContext.Station
            //                                                      where query.Contains(stationitem.stationName)
            //                                                      select new AllStationDashboardSettingsResponse
            //                                                      {
            //                                                          stationName = stationitem.stationName,
            //                                                          stationGroup = stationitem.stationGroup,
            //                                                          stationStatus = stationitem.stationStatus,
            //                                                          stationNo = stationitem.stationNo

            //                                                      }).ToList();

            return stations;
        }

        public DataStaionTab3 getDurationOfStation(string jobno, int stationNo)
        {
            DataStaionTab3 response = new DataStaionTab3();

            try
            {
                double total_duration = 0.0;
                if (dbCon.IsConnect())
                {
                    DataTable dataTable = new DataTable();

                    string querylist = "SELECT * FROM premierductsqld.jobtiming where jobno='"+jobno+"' and stationNo = "+stationNo+ " and itemno != 'Button' and itemno != 'Swipe';";
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(querylist, dbCon.Connection);

                    myDataAdapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        JobTimingResponse jobTiming = new JobTimingResponse
                        {

                            duration = row.Field<string>("duration"),
                            jobday = row.Field<string>("jobday"),
                            jobtime = row.Field<string>("jobtime"),
                            operatorID = row.Field<string>("operatorID"),
                        };
                        total_duration += TimeSpan.Parse(row.Field<string>("duration")).TotalSeconds;
                        response.history.Add(jobTiming);

                    }
                    response.totalDuration  = (int)TimeSpan.FromSeconds(total_duration).TotalHours + TimeSpan.FromSeconds(total_duration).ToString(@"\:mm\:ss");
                   
                }

            }
            catch (Exception e)
            {
                response = null;
            }
            finally
            {
                dbCon.Close();
            }

            return response;
        }

        public StationResponse getStationDetail(int stationNo)
        {
            StationResponse station = null;
            if (dbCon.IsConnect())
            {
                DataTable dataTable = new DataTable();
                //MySqlCommand cmd = new MySqlCommand(");
                //cmd.Parameters.AddWithValue("@par", stationNo);
                //cmd.CommandType = CommandType.Text;

                string query = "select * from premierductsqld.stationManagement where stationNo = " + stationNo;
                MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);

                myDataAdapter.Fill(dataTable);

                foreach (DataRow row in dataTable.Rows)
                {
                    station = new StationResponse
                    {
                        stationName = row.Field<string>("stationName"),
                        stationNo = row.Field<int>("stationNo")
                    };



                }
                dbCon.Close();
            }


            return station;

        }



    }
}
