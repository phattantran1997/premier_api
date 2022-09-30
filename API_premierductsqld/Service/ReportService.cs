using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using API_premierductsqld.Entities.request;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Entities.response.report;
using API_premierductsqld.Global;
using API_premierductsqld.Repository;
using ClosedXML.Excel;
using DTO_PremierDucts;
using DTO_PremierDucts.DBClient;
using DTO_PremierDucts.EntityResponse;
using DTO_PremierDucts.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
//using ;

namespace API_premierductsqld.Service
{
    public class ReportService
    {
        public readonly JobTimingService jobTimingService;
        static System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();
        public List<JobTimingResponse> jobTimings = new List<JobTimingResponse>();
        public List<string> userForReport = new List<string>();
        public string current = DateTime.Now.ToString("d/M/yyyy");
        private List<ReportResponse> reportResponses = new List<ReportResponse>();
        private List<StationResponse> stations;
        double total_sum_nonprod_time = 0.0;
        double total_sum_prod_time = 0.0;
        double total_total_working_time = 0.0;
        double total_metal_area_packing = 0.0;
        double total_insu_area_packing = 0.0;
        long qty_packing = 0;

        public IJobtimingRepository jobtimingRepository;
        public IStationRepository stationRepository;
        private static DBConnection dbCon;
        List<ItemsResponse> list_data_for_init_3stations;
        string[] listEmails;

        public void UserDataForReport()
        {
            jobTimings = jobtimingRepository.getAllDataJobtimingByDate(current);
            List<JobTimingResponse> temp = new List<JobTimingResponse>();
            string url = Startup.StaticConfig.GetSection("URLForAppUserAPI").Value + APIPath.HttpCommands[API_TYPE.APP_USER_GET_USER_FOR_REPORT];
            var response = client.GetAsync(url).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsAsync<ResponseData>().Result;
                List<UserReportResponse> dataResponse = JsonConvert.DeserializeObject<List<UserReportResponse>>(content.Data.ToString()); //Converts JSON string to dynamic
                userForReport = dataResponse.Select(i => i.Username).ToList();
            }
            //userForReport = jobTimings.Where(x => x.operatorID != "hung.q").GroupBy(x => x.operatorID).Select(item => new { username = item.Key }).ToList().Select(x => x.username).ToList();

            foreach (String userOnline in userForReport)
            {
                List<JobTimingResponse> tempList1 = jobTimings.Where(i => i.operatorID == userOnline).ToList();
                int a = 0;
                foreach (JobTimingResponse item in tempList1)
                {
                    if (item.jobno.Contains("on") && item.itemno == "Station")
                    {
                        a = 1;
                    }
                    if (a != 0)
                    {
                        temp.Add(item);
                    }

                }
            }
            jobTimings = temp.OrderBy(i => i.jobtime).ToList();
        }

        internal List<ItemsResponse> dataForStations()
        {
            List<ItemsResponse> rsult = new List<ItemsResponse>();
            try
            {
                if (dbCon.IsConnect())
                {
                    DataTable dataTable = new DataTable();
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(QueryGlobals.Query_JobtimingJoinTarget_1, dbCon.Connection);
                    myDataAdapter.SelectCommand.Parameters.AddWithValue("@PARAM_VAL_1", current);
                    myDataAdapter.Fill(dataTable);

                    foreach (DataRow row in dataTable.Rows)
                    {
                        ItemsResponse data = new ItemsResponse(row.Field<int>("stationno"), row.Field<string>("operatorID"),
                            row.Field<string>("filename"),
                            row.Field<string>("handle"),
                            row.Field<string>("itemno"),
                            row.Field<string>("jobno"),
                            row.Field<string>("jobday"),
                            row.Field<string>("jobtime"),
                            row.Field<string>("duration"),
                            row.Field<string>("metalarea"),
                            row.Field<string>("insuarea"));

                        data.doubleTapped = dataTable.AsEnumerable().Where(item => item.Field<string>("filename") == data.filename
                        && item.Field<string>("handle") == data.handle && item.Field<int>("stationno") == data.stationno && item.Field<string>("itemno") == data.itemNo && item.Field<string>("jobno") == data.jobno).Count();

                        rsult.Add(data);
                    }
                }

            }
            catch (Exception exception)
            {
                throw exception;

            }
            finally
            {
                dbCon.Close();
            }

            return rsult;
        }
        //get Data again when call api
        private void getData()
        {
            UserDataForReport();
            list_data_for_init_3stations = dataForStations();
        }

        private async Task<List<DispatchInforResponse>> GetDispatchDetailsInfoByList(dynamic items)
        {
            List<DispatchInforResponse> rsult = new List<DispatchInforResponse>();
            if (items is List<BoxeseRequest>)
            {
                var json = JsonConvert.SerializeObject(items);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var url = Startup.StaticConfig.GetSection("URLForQLDDataAPI").Value + APIPath.HttpCommands[API_TYPE.QLD_DISPATCH_INFO_BY_LIST_BOX];
                var responseFromAPI = await client.PostAsync(url, data);

                if (responseFromAPI.IsSuccessStatusCode)
                {
                    var content = responseFromAPI.Content.ReadAsStringAsync(); //Returns the response as JSON string
                    rsult = JsonConvert.DeserializeObject<List<DispatchInforResponse>>(content.Result); //Converts JSON string to dynamic

                }
            }
            if (items is List<string>)
            {
                var json = JsonConvert.SerializeObject(items);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                var url = Startup.StaticConfig.GetSection("URLForQLDDataAPI").Value + APIPath.HttpCommands[API_TYPE.QLD_DISPATCH_INFO_BY_LIST_JOBNO];
                var responseFromAPI = await client.PostAsync(url, data);

                if (responseFromAPI.IsSuccessStatusCode)
                {
                    var content = responseFromAPI.Content.ReadAsStringAsync(); //Returns the response as JSON string
                    rsult = JsonConvert.DeserializeObject<List<DispatchInforResponse>>(content.Result); //Converts JSON string to dynamic

                }
            }
            return rsult;
        }


        //contructor init all data
        public ReportService()
        {
            jobtimingRepository = new JobtimingRepository();
            stationRepository = new StationRepository();
            dbCon = DBConnection.Instance(Startup.StaticConfig.GetConnectionString("ConnectionForDatabase"));
            //Init stations.
            stations = stationRepository.getAllStation();
            //Init emails
            listEmails = Startup.StaticConfig.GetSection("emailForReport")
                      .AsEnumerable()
                      .Where(p => p.Value != null)
                      .Select(p => p.Value)
                      .ToArray();
        }


        public string getNameOfStation(int stationNO)
        {
            if (stationNO > 0)
            {
                var stationResult = stations.Where(item => item.stationNo == stationNO);

                if (stationResult.Any())
                {
                    var post = stationResult.Select(g => g.stationName).Single();
                    return post;
                }

            }
            return "logout";
        }

        #region [Report Data]
        public List<ReportResponse> report1data()
        {
            foreach (String userOnline in userForReport)
            {
                var firsttime = jobTimings.FirstOrDefault(x => x.jobno.Contains(" - on") && x.operatorID == userOnline);
                var firstjob = jobTimings.FirstOrDefault(item => item.itemno != "Station" && item.operatorID == userOnline);
                ReportResponse reportResponse = new ReportResponse();
                reportResponses.Add(new ReportResponse(current, userOnline,
                  (firsttime != null ? firsttime.jobtime : null),
                    (firsttime != null ? getNameOfStation(firsttime.stationNo) : null),
                    (firstjob != null ? firstjob.jobtime : null),
                    (firstjob != null ? firstjob.jobno : null)));

            }

            return reportResponses;
        }

        public List<ReportResponse> report2data()
        {
            report1data();
            reportResponses = reportResponses.OrderBy(x => x.time_start).ToList();
            foreach (ReportResponse report in reportResponses)
            {
                var item = jobTimings.OrderBy(i => i.jobtime).FirstOrDefault(i => i.jobno.Contains(" - logout") && i.operatorID == report.user);
                if (item != null)
                {
                    report.breaking_time = item.jobtime;
                }
                else
                {
                    report.breaking_time = null;

                }
            }


            return reportResponses;
        }

        /*to do: Fix logic code for report 3*/
        public List<ReportResponse> report3data()
        {
            report2data();
            foreach (ReportResponse report in reportResponses)
            {
                var allListOfEachUsers = jobTimings.Where(it => it.operatorID == report.user).ToList();
                var itemContaimLogout = allListOfEachUsers.FirstOrDefault(x => x.jobno.Contains(" - logout"));
                if (itemContaimLogout != null)
                {
                    int indexItemContaimLogout = allListOfEachUsers.IndexOf(itemContaimLogout);
                    if (indexItemContaimLogout == allListOfEachUsers.Count - 1)
                    {
                        //report.t_back = "invalid";
                        //report.job_first_after_break = "invalid";
                    }
                    else
                    {
                        for (int i = indexItemContaimLogout + 1; i < allListOfEachUsers.Count; i++)
                        {
                            if (report.time_back == null && allListOfEachUsers[i].jobno.Contains((" - logout")) == false && allListOfEachUsers[i].itemno == "Station")
                            {
                                report.time_back = allListOfEachUsers[i].jobtime;
                                report.area_back = getNameOfStation(allListOfEachUsers[i].stationNo);

                            }

                            if (report.job_after_back == null && allListOfEachUsers[i].itemno != "Station" && allListOfEachUsers[i].itemno != "Button"
                                && allListOfEachUsers[i].itemno != "Swipe")
                            {

                                report.time_of_jab = allListOfEachUsers[i].jobtime;

                                report.job_after_back = allListOfEachUsers[i].jobno;

                            }

                            if (report.job_after_back != null && report.time_of_jab != null)
                            {
                                break;
                            }

                        }

                    }

                }
            }


            return reportResponses;
        }

        public List<ReportResponse> report4data()
        {
            total_sum_nonprod_time = 0.0;
            total_sum_prod_time = 0.0;
            total_total_working_time = 0.0;
            report3data();
            foreach (ReportResponse report in reportResponses)
            {
                var item = jobTimings.LastOrDefault(x => x.operatorID == report.user);

                //lastest time is signout time
                if (item != null)
                    report.time_finished = item.jobtime;

                var jobtimingsWithout = jobTimings.Where(item => item.operatorID == report.user).ToList();

                double total_break_time_on_each_user = 0;
                double time_production_on_each_user = 0;
                double time_non_production_on_each_user = 0;
                int break_quantity = 0;
                for (int i = 0; i < jobtimingsWithout.Count - 1; i++)
                {
                    double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                    double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;
                    if (jobtimingsWithout[i].jobno.Contains(" - logout"))
                    {
                        total_break_time_on_each_user = total_break_time_on_each_user + (time2 - time1);
                        break_quantity++;
                    }
                    if (jobtimingsWithout[i].itemno == "Station")
                    {

                        time_non_production_on_each_user = time_non_production_on_each_user + (time2 - time1);

                    }
                    else
                    {
                        time_production_on_each_user = time_production_on_each_user + (time2 - time1);

                    }

                }
                report.sum_time_break = Convert.ToString(TimeSpan.FromSeconds(total_break_time_on_each_user));
                double temp = time_non_production_on_each_user - total_break_time_on_each_user;
                report.sum_nonprod_time = Convert.ToString(TimeSpan.FromSeconds(temp));
                report.sum_prod_time = Convert.ToString(TimeSpan.FromSeconds(time_production_on_each_user));
                report.break_quantity = break_quantity;
                report.total_job = jobtimingsWithout.Where(item => item.itemno != "Station").GroupBy(item1 => item1.jobno).Count().ToString();

                // tính tổng cho toàn bộ non-prod;
                total_sum_nonprod_time += temp;
                total_sum_prod_time += time_production_on_each_user;

            }
            return reportResponses;
        }

        public DataTable dataPackingReport()
        {
            DataTable dataTable = new DataTable();
            //columns  
            dataTable.Columns.Add("DATE", typeof(string));
            dataTable.Columns.Add("TIME", typeof(string));
            dataTable.Columns.Add("USER NAME", typeof(string));
            dataTable.Columns.Add("FILE PATH", typeof(string));
            dataTable.Columns.Add("FILE NAME", typeof(string));
            dataTable.Columns.Add("HANDLE", typeof(string));
            dataTable.Columns.Add("JOB NUMBER", typeof(string));
            dataTable.Columns.Add("STORAGE INFORMATION", typeof(string));
            dataTable.Columns.Add("ITEM NUMBER", typeof(string));
            dataTable.Columns.Add("DESCRIPTION", typeof(string));
            dataTable.Columns.Add("DIMENSION: w x d x l", typeof(string));
            dataTable.Columns.Add("METALAREA", typeof(double));
            dataTable.Columns.Add("INSULATION THICKNESS", typeof(string));
            dataTable.Columns.Add("INSULATIONAREA", typeof(double));

            if (dbCon.IsConnect())
            {
                try
                {
                    DataTable resultFromQuery = new DataTable();
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(QueryGlobals.Query_PackingInfor2_1, dbCon.Connection);
                    myDataAdapter.SelectCommand.Parameters.AddWithValue("@PARAM_VAL_1", current);
                    myDataAdapter.Fill(resultFromQuery);

                    List<BoxeseRequest> listBoxese = resultFromQuery.AsEnumerable().Select(item => new BoxeseRequest
                    {
                        filename = item.Field<string>("filename"),
                        handle = item.Field<string>("handle")
                    }).Distinct().ToList();
                    List<DispatchInforResponse> rsult = GetDispatchDetailsInfoByList(listBoxese).Result;

                    foreach (DataRow row in resultFromQuery.Rows)
                    {
                        var itemInDispatchDetail = rsult.FirstOrDefault(item => item.handle == row.Field<string>("handle").ToUpper()
                        && item.filename == row.Field<string>("filename"));

                        string pathvalue = itemInDispatchDetail?.pathValue ?? string.Empty;
                        string[] pathvalue_array = pathvalue.Split("/");
                        if (pathvalue_array.Count() > 1)
                            pathvalue_array = StringUtils.SubArray(pathvalue_array, 2, pathvalue_array.Count() - 3);

                        string description = itemInDispatchDetail?.description ?? string.Empty;
                        //int start = description.IndexOf("(") + 1;
                        //int end = description.IndexOf(")", start);
                        //description = description.Substring(start, end - start);
                        dataTable.Rows.Add(row.Field<string>("jobday"),
                               row.Field<string>("jobtime"),
                               row.Field<string>("operatorID"),
                               string.Join("/", pathvalue_array),
                               row.Field<string>("filename"),
                               row.Field<string>("handle").ToUpper(),
                               row.Field<string>("jobno"),
                               row.Field<string>("storageInfo"),
                               row.Field<string>("itemno"),
                               Regex.Match(description, @"\(([^)]*)\)").Groups[1].Value,
                               string.Format("{0} x  {1} x {2}", itemInDispatchDetail?.widthDim ?? "0", itemInDispatchDetail?.depthDim ?? "0", itemInDispatchDetail?.lengthangle ?? "0"),
                               Math.Round(Convert.ToDouble(row.Field<string>("metalarea")), 2),
                               itemInDispatchDetail?.insulationSpec ?? string.Empty,
                               Math.Round(Convert.ToDouble(row.Field<string>("insuarea")) / 1000000, 2));
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

            return dataTable;
        }
        internal DataTable dataInsulationCutting()
        {
            DataTable table = new DataTable();
            try
            {
                table.Columns.Add("DATE", typeof(string));
                table.Columns.Add("JOBNO", typeof(string));
                table.Columns.Add("EMPLOYEE", typeof(string));
                table.Columns.Add("START TIME", typeof(string));
                table.Columns.Add("FINISH TIME", typeof(string));
                table.Columns.Add("DURATION", typeof(string));
                table.Columns.Add("QTY", typeof(string));
                table.Columns.Add("QTY ITEM INSU", typeof(string));
                table.Columns.Add("METALAREA", typeof(string));
                table.Columns.Add("INSULATION", typeof(string));
                var listForInsulationCutting = list_data_for_init_3stations.Where(item => item.stationno == 6).ToList();
                var groupedJobNoList = listForInsulationCutting
                .GroupBy(u => u.jobno)
                .Select(grp => grp.ToList())
                .ToList();

                List<string> distinctlistJobno = listForInsulationCutting.Select(item => item.jobno).Distinct().ToList();
                var dispatchResponse = GetDispatchDetailsInfoByList(distinctlistJobno).Result;
                double insu_item_withoutinsu = 0.0;
                double metal_item_withoutinsu = 0.0;
                double sum_duration = 0.0;
                double sum_insu = 0.0;
                double sum_metal = 0.0;
                int sum_items_with_insu = 0;
                foreach (List<ItemsResponse> parentJobno in groupedJobNoList)
                {
                    //dùng temp để tạo ra biến thiết kế dạng bậc thang trong excel
                    List<string> temp = new List<string>();

                    var dispatchInforByJobno = dispatchResponse.Where(i => i.jobno == parentJobno.FirstOrDefault().jobno).Select(item => new { item.jobno, item.filename, item.handle, item.metalarea, item.insulationarea }).Distinct().ToList();
                    var items_withInsu = dispatchInforByJobno.Where(item => Convert.ToDouble(item.insulationarea) != 0).ToList();
                    foreach (ItemsResponse childJobno in parentJobno)
                    {
                        if (!temp.Contains(childJobno.jobno))
                        {
                            //first row contain all data of job
                            table.Rows.Add(
                            childJobno.jobday,
                            childJobno?.jobno ?? string.Empty,
                            childJobno?.employeeName ?? string.Empty,
                            childJobno?.jobtime ?? string.Empty,
                            TimeSpan.FromSeconds(TimeSpan.Parse(childJobno.jobtime).TotalSeconds + TimeSpan.Parse(childJobno.duration).TotalSeconds),
                            childJobno?.duration ?? string.Empty,
                            dispatchInforByJobno.Count,
                            items_withInsu.Count,
                            Math.Round(dispatchInforByJobno.Sum(item => Convert.ToDouble(item.metalarea)), 2),
                            Math.Round(dispatchInforByJobno.Sum(item => Convert.ToDouble(item.insulationarea)) / 1000000, 2)
                            );
                            temp.Add(childJobno.jobno);
                        }
                        else
                        {
                            table.Rows.Add(
                            "",
                            "",
                            childJobno?.employeeName ?? string.Empty,
                            childJobno?.jobtime ?? string.Empty,
                            TimeSpan.FromSeconds(TimeSpan.Parse(childJobno.jobtime).TotalSeconds + TimeSpan.Parse(childJobno.duration).TotalSeconds),
                            childJobno?.duration ?? string.Empty, "", "", "", "");

                        }
                        sum_duration += TimeSpan.Parse(childJobno.duration).TotalSeconds;
                    }
                    sum_items_with_insu += items_withInsu.Count;
                    sum_insu += Math.Round(dispatchInforByJobno.Sum(item => Convert.ToDouble(item.insulationarea)) / 1000000, 2);
                    sum_metal += Math.Round(dispatchInforByJobno.Sum(item => Convert.ToDouble(item.metalarea)), 2);
                    insu_item_withoutinsu += Math.Round(items_withInsu.Sum(item => Convert.ToDouble(item.insulationarea)) / 1000000, 2);
                    metal_item_withoutinsu += Math.Round(items_withInsu.Sum(item => Convert.ToDouble(item.metalarea)), 2);

                    table.Rows.Add("TOTAL", "", "", "", "", TimeSpan.FromSeconds(parentJobno.Sum(item => TimeSpan.Parse(item.duration).TotalSeconds)),
                        "", "metal: " + Math.Round(items_withInsu.Sum(item => Convert.ToDouble(item.metalarea)), 2) + "\n" +
                        "insulation: " + Math.Round(items_withInsu.Sum(item => Convert.ToDouble(item.insulationarea)) / 1000000, 2), "", "");
                    table.Rows.Add();
                }
                table.Rows.Add("TOTAL", "", "", "", "", (int)TimeSpan.FromSeconds(sum_duration).TotalHours + TimeSpan.FromSeconds(sum_duration).ToString(@"\:mm\:ss"), "", "metal: " + metal_item_withoutinsu + "\n" + "insulation: " + insu_item_withoutinsu, sum_metal, sum_insu);
                table.Rows.Add("RATE", "", "", "", "", Math.Round((sum_duration / 60) / sum_items_with_insu, 2) + " (minutes / number of items with insu)", "", Math.Round(metal_item_withoutinsu / sum_items_with_insu, 2) + "(metal m2 / number of items with insu)", "", "");
            }
            catch (Exception e)
            {
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine(line);
            }


            return table;
        }
        internal DataTable dataSealAndTape()
        {
            DataTable table = new DataTable();
            try
            {
                //columns
                table.Columns.Add("DATE", typeof(string));
                table.Columns.Add("EMPLOYEE", typeof(string));
                table.Columns.Add("FILE PATH", typeof(string));
                table.Columns.Add("JOBNO", typeof(string));
                table.Columns.Add("FILENAME", typeof(string));
                table.Columns.Add("HANDLE", typeof(string));
                table.Columns.Add("ITEM NO", typeof(string));
                table.Columns.Add("ITEM DESCRIPTION", typeof(string));
                table.Columns.Add("DIMENSION", typeof(string));
                table.Columns.Add("START TIME", typeof(string));
                table.Columns.Add("FINISH TIME", typeof(string));
                table.Columns.Add("DURATION", typeof(string));
                table.Columns.Add("METALAREA", typeof(double));
                table.Columns.Add("INSULATIONAREA", typeof(double));
                table.Columns.Add("STATUS", typeof(string));
                table.Columns.Add("OVERLAPPING", typeof(string));

                var listForSealAndTape = list_data_for_init_3stations.Where(item => item.stationno == 8).ToList();
                var groupEmployeeList = listForSealAndTape
                .GroupBy(u => u.employeeName)
                .Select(grp => grp.ToList())
                .ToList();

                List<BoxeseRequest> listBoxese = listForSealAndTape.Select(item => new BoxeseRequest
                {
                    filename = item.filename,
                    handle = item.handle
                }).Distinct().ToList();

                List<DispatchInforResponse> dispatchResponse = GetDispatchDetailsInfoByList(listBoxese).Result;
                double sum_duration = 0.0;
                foreach (List<ItemsResponse> parent in groupEmployeeList)
                {
                    var total_duration_for_each_person = parent.Sum(item => TimeSpan.Parse(item.duration).TotalSeconds);
                    sum_duration += total_duration_for_each_person;

                    foreach (ItemsResponse r in parent)
                    {
                        var itemInDispatchDetail = dispatchResponse.FirstOrDefault(item => item.handle == r.handle.ToUpper()
                            && item.filename == r.filename);

                        string pathvalue = itemInDispatchDetail?.pathValue ?? string.Empty;
                        string[] pathvalue_array = pathvalue.Split("/");
                        if (pathvalue_array.Count() > 1)
                            pathvalue_array = StringUtils.SubArray(pathvalue_array, 2, pathvalue_array.Count() - 3);

                        table.Rows.Add(r.jobday, r.employeeName,
                                 string.Join("/", pathvalue_array),
                                 r.jobno, r.filename,
                                 r.handle, r.itemNo,
                                 Regex.Match(itemInDispatchDetail?.description ?? string.Empty, @"\(([^)]*)\)").Groups[1].Value,
                                 string.Format("{0} x  {1} x {2}", itemInDispatchDetail?.widthDim ?? "0", itemInDispatchDetail?.depthDim ?? "0", itemInDispatchDetail?.lengthangle ?? "0"),
                                 r.jobtime,
                                 TimeSpan.FromSeconds(TimeSpan.Parse(r.jobtime).TotalSeconds + TimeSpan.Parse(r.duration).TotalSeconds),
                                 r.duration,
                                 Math.Round(Convert.ToDouble(itemInDispatchDetail?.metalarea ?? "0.0"), 2),
                                 Math.Round(Convert.ToDouble(itemInDispatchDetail?.insulationarea ?? "0.0") / 1000000, 2),
                                 (string.IsNullOrEmpty(r.metal) && string.IsNullOrEmpty(r.insu)) ? "Not Completed" : "Completed",
                                 r.doubleTapped);
                        r.insu = itemInDispatchDetail?.insulationarea ?? "0.0";
                        r.metal = itemInDispatchDetail?.metalarea ?? "0.0";
                    }
                    var item_for_total = parent.Select(item => new { item.jobno, item.filename, item.handle, item.metal, item.insu }).Distinct().ToList();
                    table.Rows.Add("TOTAL OF " + parent[0].employeeName, "", "", "", "", "", item_for_total.Count, "", "", "", "", (int)TimeSpan.FromSeconds(total_duration_for_each_person).TotalHours + TimeSpan.FromSeconds(total_duration_for_each_person).ToString(@"\:mm\:ss"), Math.Round(item_for_total.Sum(item => Convert.ToDouble(item.metal)), 2), Math.Round(item_for_total.Sum(item => Convert.ToDouble(item.insu)) / 1000000, 2), "", "");
                    table.Rows.Add();
                }
                var x = listForSealAndTape.Select(item => new { item.filename, item.handle, item.metal, item.insu }).Distinct().ToList();
                table.Rows.Add("TOTAL", "", "", "", "", "", x.Count, "", "", "", "", (int)TimeSpan.FromSeconds(sum_duration).TotalHours + TimeSpan.FromSeconds(sum_duration).ToString(@"\:mm\:ss"), Math.Round(x.Sum(item => Convert.ToDouble(item.metal)), 2), Math.Round(x.Sum(item => Convert.ToDouble(item.insu)) / 1000000, 2), "", "");
                table.Rows.Add("RATE", "", "", "", "", "", "", "", "", "", "", Math.Round(sum_duration / x.Count, 2) + " (seconds/number items)", 0.0, 0.0, "", "");
            }
            catch (Exception e)
            {
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine(line);
            }
            return table;
        }
        internal DataTable dataStraightFinish()
        {
            DataTable table = new DataTable();
            try
            {
                //columns
                table.Columns.Add("DATE", typeof(string));
                table.Columns.Add("EMPLOYEE", typeof(string));
                table.Columns.Add("FILE PATH", typeof(string));
                table.Columns.Add("JOBNO", typeof(string));
                table.Columns.Add("FILENAME", typeof(string));
                table.Columns.Add("HANDLE", typeof(string));
                table.Columns.Add("ITEM NO", typeof(string));
                table.Columns.Add("ITEM DESCRIPTION", typeof(string));
                table.Columns.Add("DIMENSION", typeof(string));
                table.Columns.Add("START TIME", typeof(string));
                table.Columns.Add("FINISH TIME", typeof(string));
                table.Columns.Add("DURATION", typeof(string));
                table.Columns.Add("METALAREA", typeof(double));
                table.Columns.Add("INSULATIONAREA", typeof(double));
                table.Columns.Add("STATUS", typeof(string));
                table.Columns.Add("OVERLAPPING", typeof(string));

                var listforStraightFinish = list_data_for_init_3stations.Where(item => item.stationno == 46).ToList();
                var groupEmployeeList = listforStraightFinish
                .GroupBy(u => u.employeeName)
                .Select(grp => grp.ToList())
                .ToList();

                List<BoxeseRequest> listBoxese = listforStraightFinish.Select(item => new BoxeseRequest
                {
                    filename = item.filename,
                    handle = item.handle
                }).Distinct().ToList();

                List<DispatchInforResponse> dispatchResponse = GetDispatchDetailsInfoByList(listBoxese).Result;
                double sum_duration = 0.0;

                foreach (List<ItemsResponse> parent in groupEmployeeList)
                {
                    var total_duration_for_each_person = parent.Sum(item => TimeSpan.Parse(item.duration).TotalSeconds);
                    sum_duration += total_duration_for_each_person;

                    foreach (ItemsResponse r in parent)
                    {
                        var itemInDispatchDetail = dispatchResponse.FirstOrDefault(item => item.handle == r.handle.ToUpper()
                            && item.filename == r.filename);

                        string pathvalue = itemInDispatchDetail?.pathValue ?? string.Empty;
                        string[] pathvalue_array = pathvalue.Split("/");
                        if (pathvalue_array.Count() > 1)
                            pathvalue_array = StringUtils.SubArray(pathvalue_array, 2, pathvalue_array.Count() - 3);

                        table.Rows.Add(r.jobday, r.employeeName,
                                 string.Join("/", pathvalue_array),
                                 r.jobno, r.filename,
                                 r.handle, r.itemNo,
                                 Regex.Match(itemInDispatchDetail?.description ?? string.Empty, @"\(([^)]*)\)").Groups[1].Value,
                                 string.Format("{0} x  {1} x {2}", itemInDispatchDetail?.widthDim ?? "0", itemInDispatchDetail?.depthDim ?? "0", itemInDispatchDetail?.lengthangle ?? "0"),
                                 r.jobtime,
                                 TimeSpan.FromSeconds(TimeSpan.Parse(r.jobtime).TotalSeconds + TimeSpan.Parse(r.duration).TotalSeconds),
                                 r.duration,
                                 Math.Round(Convert.ToDouble(itemInDispatchDetail?.metalarea ?? "0.0"), 2),
                                 Math.Round(Convert.ToDouble(itemInDispatchDetail?.insulationarea ?? "0.0") / 1000000, 2),
                                 (string.IsNullOrEmpty(r.metal) && string.IsNullOrEmpty(r.insu)) ? "Not Completed" : "Completed",
                                 r.doubleTapped);
                        r.insu = itemInDispatchDetail?.insulationarea ?? "0.0";
                        r.metal = itemInDispatchDetail?.metalarea ?? "0.0";

                    }
                    var item_for_total = parent.Select(item => new { item.jobno, item.filename, item.handle, item.metal, item.insu }).Distinct().ToList();
                    table.Rows.Add("TOTAL OF " + parent[0].employeeName, "", "", "", "", "", item_for_total.Count, "", "", "", "", (int)TimeSpan.FromSeconds(total_duration_for_each_person).TotalHours + TimeSpan.FromSeconds(total_duration_for_each_person).ToString(@"\:mm\:ss"), Math.Round(item_for_total.Sum(item => Convert.ToDouble(item.metal)), 2), Math.Round(item_for_total.Sum(item => Convert.ToDouble(item.insu)) / 1000000, 2), "", "");
                    table.Rows.Add();
                }
                var x = listforStraightFinish.Select(item => new { item.filename, item.handle, item.metal, item.insu }).Distinct().ToList();
                table.Rows.Add("TOTAL", "", "", "", "", "", x.Count, "", "", "", "", (int)TimeSpan.FromSeconds(sum_duration).TotalHours + TimeSpan.FromSeconds(sum_duration).ToString(@"\:mm\:ss"), Math.Round(x.Sum(item => Convert.ToDouble(item.metal)), 2), Math.Round(x.Sum(item => Convert.ToDouble(item.insu)) / 1000000, 2), "", "");
                table.Rows.Add("RATE", "", "", "", "", "", "", "", "", "", "", Math.Round(sum_duration / x.Count, 2) + " (seconds/number items)", 0.0, 0.0, "", "");
            }
            catch (Exception e)
            {
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine(line);
            }
            return table;
        }

        #endregion
        public DataTable createDataTable(int report_number)
        {
            DataTable table = new DataTable();
            //columns  
            table.Columns.Add("DATE", typeof(string));
            table.Columns.Add("NO", typeof(string));
            table.Columns.Add("NAME", typeof(string));
            table.Columns.Add("T-START", typeof(string));
            table.Columns.Add("AREA", typeof(string));
            table.Columns.Add("T-1ST JOB", typeof(string));
            table.Columns.Add("JOB NO", typeof(string));
            if (report_number == 2)
            {
                table.Columns.Add("BREAK", typeof(string));
            }
            else if (report_number == 3)
            {
                table.Columns.Add("BREAK", typeof(string));
                table.Columns.Add("T-BACK", typeof(string));
                table.Columns.Add("AREA-BACK", typeof(string));
                table.Columns.Add("T-1ST JAB", typeof(string));
                table.Columns.Add("JOB AFTER BREAK", typeof(string));
            }
            else if (report_number == 4)
            {
                table.Columns.Add("BREAK", typeof(string));
                table.Columns.Add("T-BACK", typeof(string));
                table.Columns.Add("AREA-BACK", typeof(string));
                table.Columns.Add("T-1ST JAB", typeof(string));
                table.Columns.Add("JOB AFTER BREAK", typeof(string));
                table.Columns.Add("T-FINISHED", typeof(string));

                table.Columns.Add("JOB", typeof(string));
                table.Columns.Add("BREAK HRS", typeof(string));
                table.Columns.Add("BK-QUAN", typeof(string));
                table.Columns.Add("NPROD-T", typeof(string));
                table.Columns.Add("PROD-T", typeof(string));
                table.Columns.Add("TOTAL-T", typeof(string));
            }

            return table;

        }
        public void DeleteDataByDate(string date)
        {

            if (date != null)
            {
                current = date;
            }
            File.Delete(current.Replace("/", "") + ".xlsx");
        }
        #region [Report]
        public void report1(string date)
        {
            if (date != null)
            {
                current = date;
            }
            getData();
            DataTable table = createDataTable(1);

            var reportResponses = report1data();

            int i = 1;
            foreach (ReportResponse respone in reportResponses)
            {

                table.Rows.Add(respone.date, i, respone.user, respone.time_start, respone.area,
                    respone.time_first_job, respone.job_no);
                i++;

            }

            foreach (ReportResponse respone in reportResponses)
            {
                getJobTimingDetail(current, respone.user, table);
            }
            toCSVWithSheets(1, table, null, dataInsulationCutting(), dataSealAndTape(), dataStraightFinish());
            EmailUtils.SendEmail(current.Replace("/", ""), "Report Daily", listEmails);
        }

        public void report2(string date)
        {
            if (date != null)
            {
                current = date;
            }
            getData();

            DataTable table = createDataTable(2);

            //add data
            var reportResponses = report2data();

            int i = 1;
            foreach (ReportResponse respone in reportResponses)
            {
                table.Rows.Add(respone.date, i, respone.user, respone.time_start, respone.area,
                    respone.time_first_job, respone.job_no, respone.breaking_time);
                i++;

            }
            foreach (ReportResponse respone in reportResponses)
            {
                getJobTimingDetail(current, respone.user, table);
            }

            DataTable packing_table = dataPackingReport();
            DataTable insulationCutting_table = dataInsulationCutting();
            DataTable sealtape_table = dataSealAndTape();
            DataTable straightFinish_table = dataStraightFinish();

            toCSVWithSheets(2, table, packing_table, insulationCutting_table, sealtape_table, straightFinish_table);
            //toCSVWithSheets(2, null, null, dataInsulationCutting(), dataSealAndTape(), null);
            EmailUtils.SendEmail(current.Replace("/", ""), "Report Daily", listEmails);
        }

        public void report3(string date)
        {
            if (date != null)
            {
                current = date;
            }
            getData();

            DataTable table = createDataTable(3);

            //add data
            var reportResponses = report3data();
            int i = 1;
            foreach (ReportResponse respone in reportResponses)
            {
                table.Rows.Add(respone.date, i, respone.user, respone.time_start, respone.area,
                    respone.time_first_job, respone.job_no, respone.breaking_time,
                    respone.time_back, respone.area_back,
                    respone.time_of_jab,
                    respone.job_after_back);
                i++;

            }
            foreach (ReportResponse respone in reportResponses)
            {
                getJobTimingDetail(current, respone.user, table);
            }
            DataTable insulationCutting_table = dataInsulationCutting();
            DataTable sealtape_table = dataSealAndTape();
            DataTable straightFinish_table = dataStraightFinish();
            toCSVWithSheets(3, table, null, insulationCutting_table, sealtape_table, straightFinish_table);
            EmailUtils.SendEmail(current.Replace("/", ""), "Report Daily", listEmails);
        }

        public void report4(string date)
        {
            if (date != null)
            {
                current = date;
            }
            getData();

            DataTable table = createDataTable(4);

            //add data
            var reportResponses = report4data();
            int i = 1;

            foreach (ReportResponse respone in reportResponses)
            {
                double sign_in = TimeSpan.Parse(respone.time_start != null ? respone.time_start : "00:00:00").TotalSeconds;
                double sign_out = TimeSpan.Parse(respone.time_finished != null ? respone.time_finished : "00:00:00").TotalSeconds;
                double total_breaking = TimeSpan.Parse(respone.sum_time_break != null ? respone.sum_time_break : "00:00:00").TotalSeconds;
                double temp = sign_out - sign_in - total_breaking;
                total_total_working_time += temp;
                table.Rows.Add(respone.date,
                    i,
                    respone.user,
                    respone.time_start,
                    respone.area,
                    respone.time_first_job,
                    respone.job_no,
                    respone.breaking_time,
                    respone.time_back,
                    respone.area_back,
                    respone.time_of_jab,
                    respone.job_after_back,
                    respone.time_finished,
                    respone.total_job,
                    respone.sum_time_break,
                    respone.break_quantity,
                    respone.sum_nonprod_time,
                    respone.sum_prod_time,
                    Convert.ToString(TimeSpan.FromSeconds(temp))
                    );
                i++;

            }
            table.Rows.Add();

            table.Rows.Add("", "", "", "", "", "", "", "", "", "", "", "", "", "", "",
                "TOTAL",
              (int)TimeSpan.FromSeconds(total_sum_nonprod_time).TotalHours + TimeSpan.FromSeconds(total_sum_nonprod_time).ToString(@"\:mm\:ss"),
               (int)TimeSpan.FromSeconds(total_sum_prod_time).TotalHours + TimeSpan.FromSeconds(total_sum_prod_time).ToString(@"\:mm\:ss"),
               (int)TimeSpan.FromSeconds(total_total_working_time).TotalHours + TimeSpan.FromSeconds(total_total_working_time).ToString(@"\:mm\:ss"));

            foreach (ReportResponse respone in reportResponses)
            {
                getJobTimingDetail(current, respone.user, table);
            }
            DataTable packing_table = dataPackingReport();
            DataTable insulationCutting_table = dataInsulationCutting();
            DataTable sealtape_table = dataSealAndTape();
            DataTable straightFinish_table = dataStraightFinish();
            toCSVWithSheets(4, table, packing_table, insulationCutting_table, sealtape_table, straightFinish_table);
            EmailUtils.SendEmail(current.Replace("/", ""), "Report Daily", listEmails);

            //Save data on table report_4
            SaveData(new Report4DataEachDateRequest
            {
                jobday = current,
                total_sum_nonprod_time = total_sum_nonprod_time,
                total_sum_prod_time = total_sum_prod_time,
                total_total_working_time = total_total_working_time,
                totalMetalArea = packing_table.AsEnumerable().Sum(item => item.Field<double>("METALAREA")),
                totalInsulationlArea = packing_table.AsEnumerable().Sum(item => item.Field<double>("INSULATIONAREA")),
                qty = packing_table.AsEnumerable().Select(item => item.Field<string>("ITEM NUMBER")).Count(),
            });
        }

        private void SaveData(Report4DataEachDateRequest request)
        {
            try
            {
                if (dbCon.IsConnect())
                {
                    string insertData = "insert into report_4 (jobday,total_sum_nonprod_time,total_sum_prod_time,total_total_working_time, totalMetalArea, totalInsulationlArea , qty) " +
                        "values (@jobday , @total_sum_nonprod_time , @total_sum_prod_time , @total_total_working_time, @totalMetalArea, @totalInsulationlArea , @qty)";
                    MySqlCommand command = new MySqlCommand(insertData, dbCon.Connection);

                    command.Parameters.AddWithValue("@jobday", request.jobday);
                    command.Parameters.AddWithValue("@total_sum_nonprod_time", request.total_sum_nonprod_time);
                    command.Parameters.AddWithValue("@total_sum_prod_time", request.total_sum_prod_time);
                    command.Parameters.AddWithValue("@total_total_working_time", request.total_total_working_time);
                    command.Parameters.AddWithValue("@totalMetalArea", request.totalMetalArea);
                    command.Parameters.AddWithValue("@totalInsulationlArea", request.totalInsulationlArea);
                    command.Parameters.AddWithValue("@qty", request.qty);
                    dbCon.Connection.Open();
                    int result = command.ExecuteNonQuery();
                    Debug.WriteLine("\n" + result);
                    command.Dispose();

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
        #endregion
        public void getJobTimingDetail(string jobday, string operatorID, DataTable table)
        {
            List<JobTimingResponse> jobDetailHistory = new List<JobTimingResponse>();
            jobDetailHistory = jobTimings.Where(x =>
            x.operatorID == operatorID).ToList();
            if (jobDetailHistory.Count > 0)
            {
                table.Rows.Add();
                table.Rows.Add("DATE", "JOB NO", "TIME START", "TIME WORKED", "USER NAME", "STATION", "ITEMNO");
            }

            for (var i = 0; i < jobDetailHistory.Count; i++)
            {
                table.Rows.Add(current, jobDetailHistory[i].jobno, jobDetailHistory[i].jobtime, jobDetailHistory[i].duration, operatorID, getNameOfStation(jobDetailHistory[i].stationNo), jobDetailHistory[i].itemno);
            }
        }
        public void ToCSV(DataTable dtDataTable)
        {
            StreamWriter sw = new StreamWriter(current.Replace("/", "") + ".csv", false);
            //headers    
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        #region [REPORT FOR WEEKS]
        public void reportForEachDate(String date, List<ReportWeekendResponse> ListReportWeekendResponse)
        {
            List<JobTimingResponse> list_jobtiming_by_date = jobtimingRepository.getAllDataJobtimingByDate(date);

            List<JobTimingResponse> templist = new List<JobTimingResponse>();
            List<string> list_user = list_jobtiming_by_date.Where(x => x.operatorID != "hung.q").GroupBy(x => x.operatorID).Select(item => new { username = item.Key }).ToList().Select(x => x.username).ToList();

            foreach (String userOnline in list_user)
            {

                List<JobTimingResponse> tempList1 = list_jobtiming_by_date.Where(i => i.operatorID == userOnline).ToList();
                int a = 0;
                foreach (JobTimingResponse item in tempList1)
                {
                    if (item.jobno.Contains("on") && item.itemno == "Station")
                    {
                        a = 1;
                    }
                    if (a != 0)
                    {
                        templist.Add(item);
                    }

                }

            }

            list_jobtiming_by_date = templist;

            foreach (String user in list_user)
            {
                var jobtimingsWithout = list_jobtiming_by_date.Where(item => item.operatorID == user).ToList();

                double total_break_time_on_each_user = 0;
                double time_production_on_each_user = 0;
                double time_non_production_on_each_user = 0;
                int break_quantity = 0;
                for (int i = 0; i < jobtimingsWithout.Count - 1; i++)
                {
                    double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                    double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;
                    if (jobtimingsWithout[i].jobno.Contains(" - logout"))
                    {
                        total_break_time_on_each_user = total_break_time_on_each_user + (time2 - time1);
                        break_quantity++;


                    }
                    if (jobtimingsWithout[i].itemno == "Station")
                    {
                        time_non_production_on_each_user = time_non_production_on_each_user + (time2 - time1);

                    }
                    else
                    {
                        time_production_on_each_user = time_production_on_each_user + (time2 - time1);

                    }

                }

                int total_job = jobtimingsWithout.Where(item => item.itemno != "Station").GroupBy(item1 => item1.jobno).Count();
                double sum_time_break = total_break_time_on_each_user;
                double sum_nonprod_time = time_non_production_on_each_user - total_break_time_on_each_user;
                double sum_prod_time = time_production_on_each_user;

                ListReportWeekendResponse.Add(new ReportWeekendResponse(user, total_job, sum_time_break, break_quantity,
                 sum_nonprod_time, sum_prod_time, sum_nonprod_time + sum_prod_time));

            }
        }

        public void reportForWeekend(string date)
        {
            DateTime dateTime = DateTime.Today;
            if (date != null)
            {
                dateTime = DateTime.ParseExact(date, "d/M/yyyy", CultureInfo.InvariantCulture);
            }
            DateTime startOfWeek = dateTime.AddDays(
            (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek -
            (int)dateTime.DayOfWeek);
            DataTable table = new DataTable();

            List<String> valueOfWeek = Enumerable
              .Range(1, 6)
              .Select(i => startOfWeek
                 .AddDays(i)
                 .ToString("d/M/yyyy")).ToList();

            List<ReportWeekendResponse> reportWeekendResponses = new List<ReportWeekendResponse>();
            foreach (String dateinweek in valueOfWeek)
            {
                reportForEachDate(dateinweek, reportWeekendResponses);
            }
            var groupByListResponse = reportWeekendResponses.GroupBy(i => i.user).Select(cl =>
              new
              {
                  user = cl.Key,
                  total_job = cl.Sum(c => c.total_job),
                  sum_time_break = cl.Sum(c => c.sum_time_break),
                  break_quantity = cl.Sum(c => c.break_quantity),
                  sum_nonprod_time = cl.Sum(c => c.sum_nonprod_time),
                  sum_prod_time = cl.Sum(c => c.sum_prod_time),
                  total_working_time = cl.Sum(c => c.total_working_time)
              }).ToList();


            //add data


            //columns  
            table.Columns.Add("NO", typeof(string));
            table.Columns.Add("NAME", typeof(string));
            table.Columns.Add("JOB", typeof(string));
            table.Columns.Add("BREAK HRS", typeof(string));
            table.Columns.Add("BK-QUAN", typeof(string));
            table.Columns.Add("NPROD-T", typeof(string));
            table.Columns.Add("PROD-T", typeof(string));
            table.Columns.Add("TOTAL-T", typeof(string));

            string file_name_weekly = "weekly_" + valueOfWeek.First().Replace("/", "") + "_" + valueOfWeek.Last().Replace("/", "");

            table.Rows.Add("FROM", valueOfWeek.First(), "TO", valueOfWeek.Last(), "", "", "", "");
            table.Rows.Add();

            double total_break_time_on_each_user = 0;
            double time_production_on_each_user = 0;
            double time_non_production_on_each_user = 0;
            int break_quantity = 0;
            double total_working_time = 0;
            for (int i = 0; i < groupByListResponse.Count; i++)
            {

                table.Rows.Add(i + 1, groupByListResponse[i].user, groupByListResponse[i].total_job,
                                    (int)TimeSpan.FromSeconds(groupByListResponse[i].sum_time_break).TotalHours + TimeSpan.FromSeconds(groupByListResponse[i].sum_time_break).ToString(@"\:mm\:ss"),
                                    groupByListResponse[i].break_quantity,
                                    (int)TimeSpan.FromSeconds(groupByListResponse[i].sum_nonprod_time).TotalHours + TimeSpan.FromSeconds(groupByListResponse[i].sum_nonprod_time).ToString(@"\:mm\:ss"),
                                    (int)TimeSpan.FromSeconds(groupByListResponse[i].sum_prod_time).TotalHours + TimeSpan.FromSeconds(groupByListResponse[i].sum_prod_time).ToString(@"\:mm\:ss"),
                                    (int)TimeSpan.FromSeconds(groupByListResponse[i].total_working_time).TotalHours + TimeSpan.FromSeconds(groupByListResponse[i].total_working_time).ToString(@"\:mm\:ss"));

                total_break_time_on_each_user += groupByListResponse[i].sum_time_break;
                time_production_on_each_user += groupByListResponse[i].sum_prod_time;
                time_non_production_on_each_user += groupByListResponse[i].sum_nonprod_time;
                break_quantity += groupByListResponse[i].break_quantity;
                total_working_time += groupByListResponse[i].total_working_time;
            }

            table.Rows.Add("", "", "TOTAL",
                                (int)TimeSpan.FromSeconds(total_break_time_on_each_user).TotalHours + TimeSpan.FromSeconds(total_break_time_on_each_user).ToString(@"\:mm\:ss"),
                                break_quantity,
                                (int)TimeSpan.FromSeconds(time_non_production_on_each_user).TotalHours + TimeSpan.FromSeconds(time_non_production_on_each_user).ToString(@"\:mm\:ss"),
                                (int)TimeSpan.FromSeconds(time_production_on_each_user).TotalHours + TimeSpan.FromSeconds(time_production_on_each_user).ToString(@"\:mm\:ss"),
                                (int)TimeSpan.FromSeconds(total_working_time).TotalHours + TimeSpan.FromSeconds(total_working_time).ToString(@"\:mm\:ss"));

            ToCSVWeekend(file_name_weekly, table);
            EmailUtils.SendEmail(file_name_weekly, "REPORT WEEKLY", listEmails);
            File.Delete(file_name_weekly + ".xlsx");
        }

        //public void reportForWeekendPacking(string date)
        //{
        //    DateTime dateTime = DateTime.Today;
        //    if (date != null)
        //    {
        //        dateTime = DateTime.ParseExact(date, "d/M/yyyy", CultureInfo.InvariantCulture);

        //    }
        //    DateTime startOfWeek = dateTime.AddDays(
        //    (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek -
        //    (int)dateTime.DayOfWeek);
        //    DataTable table = new DataTable();
        //    DataTable resultFromQuery = new DataTable();
        //    List<String> valueOfWeek = Enumerable
        //      .Range(1, 6)
        //      .Select(i => startOfWeek
        //         .AddDays(i)
        //         .ToString("d/M/yyyy")).ToList();
        //    string file_name_weekly = "weekly_packing_" + valueOfWeek.First().Replace("/", "") + "_" + valueOfWeek.Last().Replace("/", "");

        //    table.Columns.Add("DATE", typeof(string));
        //    table.Columns.Add("QTY", typeof(string));
        //    table.Columns.Add("METALAREA", typeof(string));
        //    table.Columns.Add("INSULATIONAREA", typeof(string));
        //    table.Columns.Add("PRODUCTION-HR", typeof(string));
        //    table.Columns.Add("NON-PRODUCTION-HR", typeof(string));
        //    table.Columns.Add("TOTAL HOURS", typeof(string));

        //    try
        //    {
        //        if (dbCon.IsConnect())
        //        {
        //            MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(QueryGlobals.Query_PackingInforWeekend_2, dbCon.Connection);
        //            myDataAdapter.SelectCommand.CommandTimeout = 200;
        //            myDataAdapter.SelectCommand.Parameters.AddWithValue("@PARAM_VAL_1", valueOfWeek.First());
        //            myDataAdapter.SelectCommand.Parameters.AddWithValue("@PARAM_VAL_2", valueOfWeek.Last());
        //            myDataAdapter.Fill(resultFromQuery);

        //            double sum_insu = 0.0;
        //            double sum_metal = 0.0;
        //            double sum_time_production = 0.0;
        //            double sum_time_non_production = 0.0;
        //            double sum_time_working = 0.0;
        //            int item_count = 0;
        //            foreach (string dateinweek in valueOfWeek)
        //            {
        //                current = dateinweek;
        //                //jobTimings = jobtimingRepository.getAllDataJobtimingByDate(current);
        //                //userForReport = jobTimings.Where(x => x.operatorID != "hung.q").GroupBy(x => x.operatorID).Select(item => new { username = item.Key }).ToList().Select(x => x.username).ToList();
        //                reportResponses = new List<ReportResponse>();
        //                UserDataForReport();
        //                report4data();
        //                foreach (ReportResponse respone in reportResponses)
        //                {
        //                    double sign_in = TimeSpan.Parse(respone.time_start != null ? respone.time_start : "00:00:00").TotalSeconds;
        //                    double sign_out = TimeSpan.Parse(respone.time_finished != null ? respone.time_finished : "00:00:00").TotalSeconds;
        //                    double total_breaking = TimeSpan.Parse(respone.sum_time_break != null ? respone.sum_time_break : "00:00:00").TotalSeconds;
        //                    double temp = sign_out - sign_in - total_breaking;
        //                    total_total_working_time += temp;
        //                }
        //                var childResult = resultFromQuery.AsEnumerable().Where(item => item.Field<string>("jobday") == dateinweek).ToList();
        //                var metal = Math.Round(childResult.Sum(row => Convert.ToDouble(row.Field<string>("metalarea"))), 2); // use correct data type
        //                var insu = Math.Round(childResult.Sum(row => Convert.ToDouble(row.Field<string>("insuarea")) / 1000000), 2);


        //                item_count += childResult.Count;
        //                table.Rows.Add(dateinweek, childResult.Count, metal, insu,
        //                      (int)TimeSpan.FromSeconds(total_sum_prod_time).TotalHours + TimeSpan.FromSeconds(total_sum_prod_time).ToString(@"\:mm\:ss"),
        //               (int)TimeSpan.FromSeconds(total_sum_nonprod_time).TotalHours + TimeSpan.FromSeconds(total_sum_nonprod_time).ToString(@"\:mm\:ss"),
        //               (int)TimeSpan.FromSeconds(total_total_working_time).TotalHours + TimeSpan.FromSeconds(total_total_working_time).ToString(@"\:mm\:ss"));
        //                table.Rows.Add();
        //                sum_metal += metal;
        //                sum_insu += insu;
        //                sum_time_production += total_sum_prod_time;
        //                sum_time_non_production += total_sum_nonprod_time;
        //                sum_time_working += total_total_working_time;
        //            }

        //            table.Rows.Add("TOTAL", item_count, Math.Round( sum_metal,2), Math.Round( sum_insu,2), (int)TimeSpan.FromSeconds(sum_time_production).TotalHours + TimeSpan.FromSeconds(sum_time_production).ToString(@"\:mm\:ss"),
        //     (int)TimeSpan.FromSeconds(sum_time_non_production).TotalHours + TimeSpan.FromSeconds(sum_time_non_production).ToString(@"\:mm\:ss"),
        //     (int)TimeSpan.FromSeconds(sum_time_working).TotalHours + TimeSpan.FromSeconds(sum_time_working).ToString(@"\:mm\:ss"));
        //            ToCSVWeekend(file_name_weekly, table);
        //            EmailUtils.SendEmail(file_name_weekly, "REPORT WEEKLY PACKING", listEmails);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //    finally
        //    {
        //        dbCon.Close();
        //        File.Delete(file_name_weekly + ".xlsx");
        //    }


        //}

        public void reportForWeekendPacking(string date)
        {
            DateTime dateTime = DateTime.Today;
            if (date != null)
            {
                dateTime = DateTime.ParseExact(date, "d/M/yyyy", CultureInfo.InvariantCulture);

            }
            DateTime startOfWeek = dateTime.AddDays(
            (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek -
            (int)dateTime.DayOfWeek);
            DataTable table = new DataTable();
            DataTable resultFromQuery = new DataTable();
            List<String> valueOfWeek = Enumerable
              .Range(1, 6)
              .Select(i => startOfWeek
                 .AddDays(i)
                 .ToString("d/M/yyyy")).ToList();
            string file_name_weekly = "weekly_packing_" + valueOfWeek.First().Replace("/", "") + "_" + valueOfWeek.Last().Replace("/", "");

            table.Columns.Add("DATE", typeof(string));
            table.Columns.Add("QTY", typeof(string));
            table.Columns.Add("METALAREA", typeof(double));
            table.Columns.Add("INSULATIONAREA", typeof(double));
            table.Columns.Add("PRODUCTION-HR", typeof(string));
            table.Columns.Add("NON-PRODUCTION-HR", typeof(string));
            table.Columns.Add("TOTAL HOURS", typeof(string));

            try
            {
                double sum_time_non_production = 0.0;
                double sum_time_production = 0.0;
                double sum_time_working = 0.0;
                if (dbCon.IsConnect())
                {
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(QueryGlobals.Query_Report4, dbCon.Connection);
                    myDataAdapter.SelectCommand.Parameters.AddWithValue("@PARAM_VAL_1", valueOfWeek.First());
                    myDataAdapter.SelectCommand.Parameters.AddWithValue("@PARAM_VAL_2", valueOfWeek.Last());
                    myDataAdapter.Fill(resultFromQuery);

                    foreach (DataRow row in resultFromQuery.Rows)
                    {
                        table.Rows.Add(row.Field<string>("jobday"), row.Field<string>("qty"),
                            Math.Round(row.Field<Single>("totalMetalArea"),2), Math.Round(row.Field<Single>("totalInsulationlArea"),2),
                             (int)TimeSpan.FromSeconds(row.Field<Single>("total_sum_prod_time")).TotalHours + TimeSpan.FromSeconds(row.Field<Single>("total_sum_prod_time")).ToString(@"\:mm\:ss"),
                             (int)TimeSpan.FromSeconds(row.Field<Single>("total_sum_nonprod_time")).TotalHours + TimeSpan.FromSeconds(row.Field<Single>("total_sum_nonprod_time")).ToString(@"\:mm\:ss"),
                             (int)TimeSpan.FromSeconds(row.Field<Single>("total_total_working_time")).TotalHours + TimeSpan.FromSeconds(row.Field<Single>("total_total_working_time")).ToString(@"\:mm\:ss"));
                        sum_time_non_production += row.Field<Single>("total_sum_nonprod_time");
                        sum_time_production += row.Field<Single>("total_sum_prod_time");
                        sum_time_working += row.Field<Single>("total_total_working_time");

                    }

                    table.Rows.Add("TOTAL", resultFromQuery.AsEnumerable().Sum(item => Convert.ToDouble( item.Field<string>("qty"))), Math.Round(resultFromQuery.AsEnumerable().Sum(item => item.Field<Single>("totalMetalArea")), 2), Math.Round(resultFromQuery.AsEnumerable().Sum(item => item.Field<Single>("totalInsulationlArea")), 2),
                        (int)TimeSpan.FromSeconds(sum_time_production).TotalHours + TimeSpan.FromSeconds(sum_time_production).ToString(@"\:mm\:ss"),
             (int)TimeSpan.FromSeconds(sum_time_non_production).TotalHours + TimeSpan.FromSeconds(sum_time_non_production).ToString(@"\:mm\:ss"),
             (int)TimeSpan.FromSeconds(sum_time_working).TotalHours + TimeSpan.FromSeconds(sum_time_working).ToString(@"\:mm\:ss"));
                    ToCSVWeekend(file_name_weekly, table);
                    EmailUtils.SendEmail(file_name_weekly, "REPORT WEEKLY PACKING", listEmails);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                dbCon.Close();
                File.Delete(file_name_weekly + ".xlsx");
            }


        }
        public void ToCSVWeekend(string file_name, DataTable dtDataTable)
        {
            XLWorkbook wb = new XLWorkbook();
            var ST_sheet = wb.Worksheets.Add("Report Weekly Packing");
            var ST_table = ST_sheet.Cell(1, 1).InsertTable(dtDataTable);
            ST_sheet.ColumnWidth = 15;
            wb.SaveAs(file_name + ".xlsx");

        }

        #endregion
        private void toCSVWithSheets(int period, params DataTable[] report_dataTable)
        {
            XLWorkbook wb = new XLWorkbook();
            if (report_dataTable[0] != null)
            {
                var worksheet_packing_dataTable = wb.Worksheets.Add("Report Daily");
                worksheet_packing_dataTable.Cell(1, 1).Value = "REPORT PERIOD:";
                worksheet_packing_dataTable.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet_packing_dataTable.Cell(1, 1).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(1, 2).Value = period; //REPORT_PERIOD_STRING
                worksheet_packing_dataTable.Cell(1, 2).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(2, 1).Value = "REPORT STATION NAME:";
                worksheet_packing_dataTable.Cell(2, 1).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet_packing_dataTable.Cell(2, 2).Value = "PACKING";
                worksheet_packing_dataTable.Cell(2, 2).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(3, 1).Value = "REPORT CREATED TIME:";
                worksheet_packing_dataTable.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet_packing_dataTable.Cell(3, 1).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(3, 2).Value = current;
                worksheet_packing_dataTable.Cell(3, 2).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(3, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                var table_pharmacies = worksheet_packing_dataTable.Cell(5, 1).InsertTable(report_dataTable[0]);
                worksheet_packing_dataTable.ColumnWidth = 15;

            }
            if (report_dataTable[1] != null)
            {
                var worksheet_packing_dataTable = wb.Worksheets.Add("Packing Report");
                worksheet_packing_dataTable.Cell(1, 1).Value = "REPORT PERIOD:";
                worksheet_packing_dataTable.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet_packing_dataTable.Cell(1, 1).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(1, 2).Value = period; //REPORT_PERIOD_STRING
                worksheet_packing_dataTable.Cell(1, 2).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(2, 1).Value = "REPORT STATION NAME:";
                worksheet_packing_dataTable.Cell(2, 1).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet_packing_dataTable.Cell(2, 2).Value = "PACKING";
                worksheet_packing_dataTable.Cell(2, 2).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(3, 1).Value = "REPORT CREATED TIME:";
                worksheet_packing_dataTable.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                worksheet_packing_dataTable.Cell(3, 1).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(3, 2).Value = current;
                worksheet_packing_dataTable.Cell(3, 2).Style.Font.Bold = true;
                worksheet_packing_dataTable.Cell(3, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                var table_pharmacies = worksheet_packing_dataTable.Cell(5, 1).InsertTable(report_dataTable[1]);
                worksheet_packing_dataTable.ColumnWidth = 15;
                table_pharmacies.Theme = XLTableTheme.TableStyleLight18;
                table_pharmacies.ShowTotalsRow = true;
                table_pharmacies.Field("ITEM NUMBER").TotalsRowFunction = XLTotalsRowFunction.Count;
                table_pharmacies.Field("METALAREA").TotalsRowFunction = XLTotalsRowFunction.Sum;
                table_pharmacies.Field("INSULATIONAREA").TotalsRowFunction = XLTotalsRowFunction.Sum;

                table_pharmacies.Field(0).TotalsRowLabel = "TOTAL SUM:";

            }
            if (report_dataTable[2] != null)
            {
                var IC_sheet = wb.Worksheets.Add("Insulation Cutting");
                var table_pharmacies = IC_sheet.Cell(1, 1).InsertTable(report_dataTable[2]);
                IC_sheet.Cell(report_dataTable[2].Rows.Count, 1).Style.Font.Bold = true;
                IC_sheet.Cell(report_dataTable[2].Rows.Count + 1, 1).Style.Font.Bold = true;
                IC_sheet.RangeUsed().AddConditionalFormat().WhenContains("TOTAL").Fill.SetBackgroundColor(XLColor.Yellow);
                IC_sheet.RangeUsed().AddConditionalFormat().WhenContains("RATE").Fill.SetBackgroundColor(XLColor.Yellow);
                IC_sheet.ColumnWidth = 20;
                table_pharmacies.Theme = XLTableTheme.TableStyleLight10;
            }
            if (report_dataTable[3] != null)
            {
                var ST_sheet = wb.Worksheets.Add("Seal Tape");

                var ST_table = ST_sheet.Cell(1, 1).InsertTable(report_dataTable[3]);
                ST_sheet.Cell(report_dataTable[3].Rows.Count, 1).Style.Font.Bold = true;
                ST_sheet.Cell(report_dataTable[3].Rows.Count + 1, 1).Style.Font.Bold = true;
                ST_sheet.RangeUsed().AddConditionalFormat().WhenContains("TOTAL").Fill.SetBackgroundColor(XLColor.Yellow);
                ST_sheet.RangeUsed().AddConditionalFormat().WhenContains("RATE").Fill.SetBackgroundColor(XLColor.Yellow);
                ST_sheet.ColumnWidth = 20;
                ST_table.Theme = XLTableTheme.TableStyleLight11;

            }
            if (report_dataTable[4] != null)
            {
                var SF_sheet = wb.Worksheets.Add("Straight Finish");
                var SF_table = SF_sheet.Cell(1, 1).InsertTable(report_dataTable[4]);
                SF_sheet.Cell(report_dataTable[4].Rows.Count, 1).Style.Font.Bold = true;
                SF_sheet.Cell(report_dataTable[4].Rows.Count + 1, 1).Style.Font.Bold = true;
                SF_sheet.RangeUsed().AddConditionalFormat().WhenContains("TOTAL").Fill.SetBackgroundColor(XLColor.Yellow);
                SF_sheet.RangeUsed().AddConditionalFormat().WhenContains("RATE").Fill.SetBackgroundColor(XLColor.Yellow);
                SF_sheet.ColumnWidth = 20;
                SF_table.Theme = XLTableTheme.TableStyleLight12;
            }

            wb.SaveAs(current.Replace("/", "") + ".xlsx");

        }

    }
}
