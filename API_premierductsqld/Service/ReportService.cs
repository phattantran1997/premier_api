using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Repository.impl;
using API_premierductsqld.Repository.@interface;
using DTO_PremierDucts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace API_premierductsqld.Service
{
    public class ReportService
    {
        //public readonly AppDbContext dataContext;
        public readonly JobTimingService jobTimingService;
        static System.Net.Http.HttpClient client = new System.Net.Http.HttpClient();

        public List<JobTimingResponse> jobTimings = new List<JobTimingResponse>();
        public List<String> userOnlines = new List<String>();
        public string current = DateTime.Now.ToString("d/M/yyyy");
        
        private List<ReportResponse> reportResponses = new List<ReportResponse>();
        private List<StationResponse> stations = new List<StationResponse>();
        public string file_name_weekly;
        public double total_sum_nonprod_time = 0;
        public double total_sum_prod_time = 0;
        public double total_total_working_time = 0;
        public IJobtimingRepository jobtimingRepository;
        public IStationRepository stationRepository;

        //contructor init all data
        public ReportService()
        {
            jobtimingRepository = new JobtimingRepository();
            stationRepository = new StationRepository();

            //this.dataContext = dataContext;
            stations = stationRepository.getAllStation();


            jobTimings = jobtimingRepository.getAllDataJobtimingByDate(current);

            List<JobTimingResponse> temp = new List<JobTimingResponse>();
            //userOnlines = jobTimings.Where(x => x.operatorID != "hung.q").GroupBy(x => x.operatorID).Select(item => new { username = item.Key }).ToList().Select(x => x.username).ToList();
            getAllUserDataForReport();
            foreach (String userOnline in userOnlines)
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

        public void getAllUserDataForReport()
        {
            string url = Startup.StaticConfig.GetSection("URLForAppUserAPI").Value + "/user/getUserForReport";
            //Sends request to retrieve data from the web service for the specified Uri
            var response = client.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsAsync<ResponseData>().Result;
                List<OperatorReponse> dataResponse = JsonConvert.DeserializeObject<List<OperatorReponse>>(content.Data.ToString()); //Converts JSON string to dynamic
                userOnlines = dataResponse.Select(i => i.Username).ToList();
            }
        }
        public string getNameOfStation(int stationNO)
        {
            if (stationNO > 0)
                return stations.Where(item => item.stationNo == stationNO).Select(g => g.stationName).First();
            return "logout";
        }



        //done
        public async Task<ActionResult<List<ReportResponse>>> report1data()
        {


            foreach (String userOnline in userOnlines)
            {
               
                var firsttime = jobTimings.FirstOrDefault(x => x.jobno.Contains(" - on") && x.operatorID == userOnline);
                var firstjob = jobTimings.FirstOrDefault(item => item.itemno != "Station" && item.operatorID == userOnline);


                ReportResponse reportResponse = new ReportResponse();

             

                reportResponses.Add(new ReportResponse(current, userOnline,


                   (firsttime!=null ? firsttime.jobtime : null),
                     (firsttime != null ? getNameOfStation(firsttime.stationNo) : null),
                     (firstjob != null ? firstjob.jobtime : null),
                     (firstjob != null ? firstjob.jobno : null)));

            }

            return reportResponses;
        }

        //break time = first logout
        //report calculate breake_time
        public async Task<ActionResult<List<ReportResponse>>> report2data()
        {
            report1data();
            reportResponses = reportResponses.OrderBy(x => x.time_start).ToList();
            foreach (ReportResponse report in reportResponses)
            {

                var item = jobTimings.OrderBy(i => i.jobtime).FirstOrDefault(i => i.jobno.Contains(" - logout") && i.operatorID == report.user);
                //var item = jobTimings.Where(i => i.jobno.Contains(" - logout") && i.operatorID == report.user).OrderBy(i => i.jobtime).Reverse().Skip(1)
                //    .FirstOrDefault();


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
        public async Task<ActionResult<List<ReportResponse>>> report3data()
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


        public async Task<ActionResult<List<ReportResponse>>> report4data()
        {
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

                    if (jobtimingsWithout[i].jobno.Contains(" - logout"))
                    {
                        double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                        double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;

                        total_break_time_on_each_user = total_break_time_on_each_user + (time2 - time1);
                        break_quantity++;


                    }
                    if (jobtimingsWithout[i].itemno == "Station")
                    {

                        double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                        double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;

                        time_non_production_on_each_user = time_non_production_on_each_user + (time2 - time1);

                    }
                    else
                    {

                        double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                        double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;

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

        public DataTable createDBForWeekend()
        {
            DataTable table = new DataTable();
            table.Columns.Add("DATE", typeof(string));
            table.Columns.Add("NO", typeof(string));
            table.Columns.Add("NAME", typeof(string));
            table.Columns.Add("NPROD-T", typeof(string));
            table.Columns.Add("PROD-T", typeof(string));
            table.Columns.Add("TOTAL HOURS", typeof(string));
            return table;
        }

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
            if(date == null)
            {
                string currentdate = DateTime.Now.ToString("dMyyyy");
                File.Delete(currentdate + ".csv");

            }
            else
            {
                File.Delete(date + ".csv");
            }
        }

        public void report1(string date)
        {
            if (date != null)
            {
                current = date;
                jobTimings = jobtimingRepository.getAllDataJobtimingByDate(date);

            }
            DataTable table = createDataTable(1);

            var reportResponses = report1data();

            int i = 1;
            foreach (ReportResponse respone in reportResponses.Result.Value)
            {

                table.Rows.Add(respone.date, i, respone.user, respone.time_start, respone.area,
                    respone.time_first_job, respone.job_no);
                i++;

            }


            foreach (ReportResponse respone in reportResponses.Result.Value)
            {
                getJobTimingDetail(current, respone.user, table);
            }

            ToCSV(table);
            SendEmail();
        }


        public void report2(string date)
        {
            if (date != null)
            {
                current = date;
                jobTimings = jobtimingRepository.getAllDataJobtimingByDate(date);
            }
            DataTable table = createDataTable(2);

            //add data
            var reportResponses = report2data();

            int i = 1;
            foreach (ReportResponse respone in reportResponses.Result.Value)
            {
                table.Rows.Add(respone.date, i, respone.user, respone.time_start, respone.area,
                    respone.time_first_job, respone.job_no, respone.breaking_time);
                i++;

            }
            foreach (ReportResponse respone in reportResponses.Result.Value)
            {
                getJobTimingDetail(current, respone.user, table);
            }

            ToCSV(table);
            SendEmail();
        }



        public void report3(string date)
        {
            if (date != null)
            {
                current = date;
                jobTimings = jobtimingRepository.getAllDataJobtimingByDate(date);
            }
            DataTable table = createDataTable(3);

            //add data
            var reportResponses = report3data();
            int i = 1;
            foreach (ReportResponse respone in reportResponses.Result.Value)
            {
                table.Rows.Add(respone.date, i, respone.user, respone.time_start, respone.area,
                    respone.time_first_job, respone.job_no, respone.breaking_time,
                    respone.time_back, respone.area_back,
                    respone.time_of_jab,
                    respone.job_after_back);
                i++;

            }
            foreach (ReportResponse respone in reportResponses.Result.Value)
            {
                getJobTimingDetail(current, respone.user, table);
            }

            ToCSV(table);
            SendEmail();
        }



        public void report4(string date)
        {
            if (date != null)
            {
                current = date;
                jobTimings = jobtimingRepository.getAllDataJobtimingByDate(date);
            }
            DataTable table = createDataTable(4);

            //add data
            var reportResponses = report4data();
            int i = 1;
            foreach (ReportResponse respone in reportResponses.Result.Value)
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

            foreach (ReportResponse respone in reportResponses.Result.Value)
            {


                getJobTimingDetail(current, respone.user, table);
            }

            ToCSV(table);
            SendEmail();
        }

        public string calculateDuration(string a, string b)
        {
            double time1 = TimeSpan.Parse(a).TotalSeconds;
            double time2 = TimeSpan.Parse(b).TotalSeconds;

            return Convert.ToString(TimeSpan.FromSeconds(time2 - time1));

        }

        public async void getJobTimingDetail(string jobday, string operatorID, DataTable table)
        {
            List<JobTimingResponse> jobDetailHistory = new List<JobTimingResponse>();
            jobDetailHistory = jobTimings.Where(x =>
            x.operatorID == operatorID).ToList();
            if (jobDetailHistory.Count > 0)
            {
                table.Rows.Add();

                table.Rows.Add("DATE", "JOB NO", "TIME START", "TIME WORKED", "USER NAME", "STATION", "ITEM");
            }


            for (var i = 0; i < jobDetailHistory.Count; i++)
            {
                string duration = "0";
                if (i == jobDetailHistory.Count - 1)
                {
                    table.Rows.Add(current, jobDetailHistory[i].jobno, jobDetailHistory[i].jobtime);
                    break;

                }
                if (jobDetailHistory[i + 1] != null)
                {
                    duration = calculateDuration(jobDetailHistory[i].jobtime, jobDetailHistory[i + 1].jobtime);

                }
                table.Rows.Add(current, jobDetailHistory[i].jobno, jobDetailHistory[i].jobtime, duration, operatorID, getNameOfStation(jobDetailHistory[i].stationNo), jobDetailHistory[i].itemno);


            }
        }

        //delete file using :  File.Delete(filePath);
        public void ToCSV(DataTable dtDataTable)
        {
            StreamWriter sw = new StreamWriter(current.Replace("/","") + ".csv", false);
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

        public void ToCSVWeekend(DataTable dtDataTable)
        {


            StreamWriter sw = new StreamWriter(file_name_weekly + ".xlsx", false);
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
        public void SendEmail()
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                mail.From = new MailAddress("noreply@premierducts.com.au");


                var myArray = Startup.StaticConfig.GetSection("emailForReport")
                      .AsEnumerable()
                      .Where(p => p.Value != null)
                      .Select(p => p.Value)
                      .ToArray();

                foreach (string email in myArray)
                {
                    mail.To.Add(email);
                }
          


                mail.Subject = "Report Daily";
                mail.Body = "mail with attachment";

                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(current.Replace("/", "") + ".csv");
                mail.Attachments.Add(attachment);

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("noreply@premierducts.com.au", "Wondergood101!");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                Debug.Write("mail Send");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw ex;
            }

        }

        public void SendEmailWeekend()
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.office365.com");
                mail.From = new MailAddress("noreply@premierducts.com.au");
                var myArray = Startup.StaticConfig.GetSection("emailForReport")
                       .AsEnumerable()
                       .Where(p => p.Value != null)
                       .Select(p => p.Value)
                       .ToArray();

                foreach (string email in myArray)
                {
                    mail.To.Add(email);
                }

                mail.Subject = "Report Weekly";
                mail.Body = "mail with attachment";
                System.Net.Mail.Attachment attachment;
                attachment = new System.Net.Mail.Attachment(file_name_weekly+".xlsx");
                mail.Attachments.Add(attachment);
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("noreply@premierducts.com.au", "Wondergood101!");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                Debug.Write("mail Send");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }


        public void reportForEachDate(String date, List<ReportWeekendResponse> ListReportWeekendResponse)
        {
            List<JobTimingResponse> weekendjobTimings = jobtimingRepository.getAllDataJobtimingByDate(date);

            List<JobTimingResponse> templist = new List<JobTimingResponse>();
            List<String> weekenduserOnlines = weekendjobTimings.Where(x => x.operatorID != "hung.q").GroupBy(x => x.operatorID).Select(item => new { username = item.Key }).ToList().Select(x => x.username).ToList();

            foreach (String userOnline in weekenduserOnlines)
            {

                List<JobTimingResponse> tempList1 = weekendjobTimings.Where(i => i.operatorID == userOnline).ToList();
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

            weekendjobTimings = templist;

            foreach (String user in weekenduserOnlines)
            {


                var jobtimingsWithout = weekendjobTimings.Where(item => item.operatorID == user).ToList();

                double total_break_time_on_each_user = 0;
                double time_production_on_each_user = 0;
                double time_non_production_on_each_user = 0;
                int break_quantity = 0;
                for (int i = 0; i < jobtimingsWithout.Count - 1; i++)
                {

                    if (jobtimingsWithout[i].jobno.Contains(" - logout"))
                    {
                        double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                        double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;

                        total_break_time_on_each_user = total_break_time_on_each_user + (time2 - time1);
                        break_quantity++;


                    }
                    if (jobtimingsWithout[i].itemno == "Station")
                    {

                        double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                        double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;

                        time_non_production_on_each_user = time_non_production_on_each_user + (time2 - time1);

                    }
                    else
                    {

                        double time1 = TimeSpan.Parse(jobtimingsWithout[i].jobtime).TotalSeconds;
                        double time2 = TimeSpan.Parse(jobtimingsWithout[i + 1].jobtime).TotalSeconds;

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

        public string reportForWeekend()
        {
         DateTime startOfWeek = DateTime.Today.AddDays(
         (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek -
         (int)DateTime.Today.DayOfWeek);
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
            var x = reportWeekendResponses.GroupBy(i => i.user).Select(cl =>
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
   

            file_name_weekly = "weekly_" + valueOfWeek.First().Replace("/", "") + "_" + valueOfWeek.Last().Replace("/", "");

            table.Rows.Add("FROM", valueOfWeek.First(), "TO", valueOfWeek.Last(), "", "" , "", "");
            table.Rows.Add();

            int count = 0;
            double total_break_time_on_each_user = 0;
            double time_production_on_each_user = 0;
            double time_non_production_on_each_user = 0;
            int break_quantity = 0;
            double total_working_time = 0;
            for (int i = 0; i < x.Count; i++)
            {



                table.Rows.Add(i + 1, x[i].user, x[i].total_job,
                                    (int)TimeSpan.FromSeconds(x[i].sum_time_break).TotalHours + TimeSpan.FromSeconds(x[i].sum_time_break).ToString(@"\:mm\:ss"),
                                    x[i].break_quantity,
                                    (int)TimeSpan.FromSeconds(x[i].sum_nonprod_time).TotalHours + TimeSpan.FromSeconds(x[i].sum_nonprod_time).ToString(@"\:mm\:ss"),
                                    (int)TimeSpan.FromSeconds(x[i].sum_prod_time).TotalHours + TimeSpan.FromSeconds(x[i].sum_prod_time).ToString(@"\:mm\:ss"),
                                    (int)TimeSpan.FromSeconds(x[i].total_working_time).TotalHours + TimeSpan.FromSeconds(x[i].total_working_time).ToString(@"\:mm\:ss"));

                total_break_time_on_each_user += x[i].sum_time_break;
                time_production_on_each_user += x[i].sum_prod_time;
                time_non_production_on_each_user += x[i].sum_nonprod_time;
                break_quantity += x[i].break_quantity;
                total_working_time += x[i].total_working_time;
            }

            table.Rows.Add("", "", "TOTAL",
                                (int)TimeSpan.FromSeconds(total_break_time_on_each_user).TotalHours + TimeSpan.FromSeconds(total_break_time_on_each_user).ToString(@"\:mm\:ss"),
                                break_quantity,
                                (int)TimeSpan.FromSeconds(time_non_production_on_each_user).TotalHours + TimeSpan.FromSeconds(time_non_production_on_each_user).ToString(@"\:mm\:ss"),
                                (int)TimeSpan.FromSeconds(time_production_on_each_user).TotalHours + TimeSpan.FromSeconds(time_production_on_each_user).ToString(@"\:mm\:ss"),
                                (int)TimeSpan.FromSeconds(total_working_time).TotalHours + TimeSpan.FromSeconds(total_working_time).ToString(@"\:mm\:ss"));

            ToCSVWeekend(table);
            SendEmailWeekend();

            return valueOfWeek.ToString();
        }

    }
}
