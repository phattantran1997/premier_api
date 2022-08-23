using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using API_premierductsqld.Entities;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Repository.impl;
using API_premierductsqld.Repository.@interface;
using API_qlddata.Entity.request;
using DTO_PremierDucts;
using Newtonsoft.Json;

namespace API_premierductsqld.Service
{
    public class JobTimingService
    {


        public IJobtimingRepository jobtimingRepository;
        public IStationRepository stationRepository;


        public JobTimingService()
        {

            jobtimingRepository = new JobtimingRepository();
            stationRepository = new StationRepository();

        }
        public ResponseData getAllDataJobtimingByDate(string date)
        {

            ResponseData responseData = new ResponseData();
            List<JobTimingResponse> response = jobtimingRepository.getAllDataJobtimingByDate(date);
            if (response != null && response.Count > 0)
            {
                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = response ;

            }
            else
            {
                responseData.Code = ERROR_CODE.PREMIERDB_DATA_IS_NULL;
                responseData.Data = "Data is null or empty";
            }
            return responseData;
             
        }

        public ResponseData getCurrentJobByUsers(string jobday, List<string> users)
        {

            ResponseData responseData = new ResponseData();
            try
            {
                List<GetCurrentJobTimingsResponse> listLastest = new List<GetCurrentJobTimingsResponse>();

                foreach (string user in users)
                {
                    var result = jobtimingRepository.getCurrentJobByUser(jobday, user);

                    if (result != null)
                        listLastest.Add(result);
                }
                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = listLastest; ;

            }
            catch(Exception e)
            {
                responseData.Code = ERROR_CODE.FAIL;
                responseData.Data = e.Message.ToString();
            }
         
          
            return responseData;

            }

        internal async Task<ResponseData> getAllDataTabs3Async(string date, string end, string token)
        {
            ResponseData responseData = new ResponseData();
            List<AllDataTab3Response> allresponse = new List<AllDataTab3Response>();
            List<JobNoandStationNoResponse> jobNoandStationNoResponses = jobtimingRepository.getListJobNoAndStationNo(date, end);
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            // Pass the handler to httpclient(from you are calling api)
            HttpClient client = new HttpClient(clientHandler);

            client.DefaultRequestHeaders.Add("Token", token);
            String url_qldata_listjobno = Startup.StaticConfig.GetSection("URLForQLDDataAPI").Value + "/dashboard/total/all/m2";

          

            var json = JsonConvert.SerializeObject(jobNoandStationNoResponses.Select(x=>x.jobNo).ToList().Distinct());
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            var pos = await client.PostAsync(url_qldata_listjobno, stringContent);

            if (pos.IsSuccessStatusCode)
            {

                var x = pos.Content.ReadAsAsync<ResponseData>().Result;
                List<M2DataResponse> m2qldata = JsonConvert.DeserializeObject<List<M2DataResponse>>(x.Data.ToString());

                foreach(JobNoandStationNoResponse ii  in jobNoandStationNoResponses)
                {
                    DataStaionTab3 dataStaion = stationRepository.getDurationOfStation(ii.jobNo, ii.stationNo);
                    allresponse.Add(new AllDataTab3Response(m2qldata.Where(x=>x.jobNO ==ii.jobNo).FirstOrDefault(), dataStaion));

                }

            }
            responseData.Code = ERROR_CODE.SUCCESS;
            responseData.Data = allresponse;
            return responseData;
        }

        internal ResponseData getAllDataJobtimingByUserAndDate(string user, string date)
        {
            ResponseData responseData = new ResponseData();
            List<JobTimingResponse> response = jobtimingRepository.getAllDataJobtimingByDate(date);
            if (response != null && response.Count > 0)
            {
                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = response;

            }
            else
            {
                responseData.Code = ERROR_CODE.PREMIERDB_DATA_IS_NULL;
                responseData.Data = "Data is null or empty";
            }
            return responseData;
        }

        public ResponseData getListJobNoString(string date, string end)
        {
            ResponseData responseData = new ResponseData();
            List<string> response=  jobtimingRepository.getListJobNoString(date, end);
            if(response!=null && response.Count > 0)
            {
                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = response ;

            }
            else
            {
                responseData.Code = ERROR_CODE.PREMIERDB_DATA_IS_NULL;
                responseData.Data = "Data is null or empty" ;
            }
            return responseData;
        }


        public async Task<List<Qlddataresponse>> TestAsync(string date)
        {
            HttpClient client = new HttpClient();
            List<Qlddataresponse> rsult = new List<Qlddataresponse>() ;

            List<string> distinct =  jobtimingRepository.getListJobNoString(date,"");


            var json = JsonConvert.SerializeObject(distinct);
            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = Startup.StaticConfig.GetSection("URLForQLDDataAPI").Value + "/dashboard/get/all/qldata";
            var responseFromAPI = await client.PostAsync(url, data);

            if (responseFromAPI.IsSuccessStatusCode)
            {

                var content = responseFromAPI.Content.ReadAsStringAsync(); //Returns the response as JSON string


                rsult = JsonConvert.DeserializeObject<List<Qlddataresponse>>(content.Result); //Converts JSON string to dynamic



            }
            return rsult;
        }
        //public async Task<List<JobTimingResponse>> listJobTimingOnTabJobs(string time_start , string time_end, string stationName)
        //{
        //    HttpClient client = new HttpClient();
        //    List<JobTimingResponse> jobTimingResponses = jobtimingRepository.listJobTimingOnTabJobs(time_start, time_end);


        //    var json = JsonConvert.SerializeObject(new GetM2DataResquest(stationName, jobTimingResponses.Select(i => i.jobno).ToList()));
        //    var data = new StringContent(json, Encoding.UTF8, "application/json");

        //    var url = Startup.StaticConfig.GetSection("URLForQLDDataAPI").Value + "/dashboard/get/m2/detail";

          
        //    var responseFromAPI = await client.PostAsync(url, data);


        //    if (responseFromAPI.IsSuccessStatusCode)
        //    {

        //        var content = responseFromAPI.Content.ReadAsStringAsync(); //Returns the response as JSON string


        //        List<Qlddataresponse> rsult = JsonConvert.DeserializeObject<List<Qlddataresponse>>(content.Result); //Converts JSON string to dynamic


        //        foreach(JobTimingResponse job in jobTimingResponses)
        //        {
        //            job.
        //        }
        //    }


        //}
        
        public ResponseData getJobTimingsDetail(List<string> jobno)
        {
            ResponseData responseData = new ResponseData();
            List<JobTimingResponse> response =  jobtimingRepository.getJobTimingsDetail(jobno); 
            if (response != null && response.Count > 0)
            {
                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = response;

            }
            else
            {
                responseData.Code = ERROR_CODE.PREMIERDB_DATA_IS_NULL;
                responseData.Data = "Data is null or empty";
            }
            return responseData;
          
        }

        public ResponseData GetAllStationWithRate(string date)
        {

            ResponseData responseData =new ResponseData();
            List<AllStationDashboardSettingsResponse> result = stationRepository.getAllStationDashboardWithRate(date);
            if (result == null)
            {
                responseData.Code = ERROR_CODE.PREMIERDB_DATA_IS_NULL;
                responseData.Data = "DATA IS NULL";
            }
            else
            {
                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = result;
            }
            return responseData;
        }
         
        

        public async Task<ResponseData> getJobDataByStation(int station, string date)
        {
            ResponseData responseData = new ResponseData();
            try
            {
                HttpClient client = new HttpClient();
                string station_name = stationRepository.getStationDetail(station).stationName;


                //get List data in interface repository
                List<ListJobNoDashBoardResponse> rsult = jobtimingRepository.getListDetailJobNoOnDashboard(date, station);




                var json = JsonConvert.SerializeObject(new GetM2DataResquest(station_name, rsult.Select(i => i.jobno).ToList()));
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var url = Startup.StaticConfig.GetSection("URLForQLDDataAPI").Value + "/dashboard/get/m2/detail";

                //string url = configuration.GetSection("URLForQLDDataAPI").Value + "/dashboard/get/m2/detail?station=" +
                //    station_name + "&jobno="+item.jobno;
                //Sends request to retrieve data from the web service for the specified Uri
                var responseFromAPI = await client.PostAsync(url, data);

                if (responseFromAPI.IsSuccessStatusCode)
                {

                    var content = responseFromAPI.Content.ReadAsStringAsync(); //Returns the response as JSON string


                    List<M2DataResponse> dataResponse = JsonConvert.DeserializeObject<List<M2DataResponse>>(content.Result); //Converts JSON string to dynamic



                    foreach (M2DataResponse response in dataResponse)
                    {
                        ListJobNoDashBoardResponse listJobNoDashBoardResponse = rsult.Where(i => i.jobno.Equals(response.jobNO)).FirstOrDefault();
                        listJobNoDashBoardResponse.insu_m2 = (response.isu_m2).ToString("0.00");
                        listJobNoDashBoardResponse.metal_m2 = response.meta_m2.ToString("0.00");
                        listJobNoDashBoardResponse.file_name = response.pathID;
                        listJobNoDashBoardResponse.rate = calculation_rate(station_name, response, Convert.ToDouble(listJobNoDashBoardResponse.interval));
                    }



                }

                responseData.Code = ERROR_CODE.SUCCESS;
                responseData.Data = rsult;
            }
            catch(Exception e)
            {
                responseData.Code = ERROR_CODE.FAIL;
                responseData.Data = e.Message.ToString();
            }
 

            return responseData;
        }

        private string calculation_rate(string station , M2DataResponse m2DataResponse, double interval)
        {
            string result = "";
            if (station.Equals("Plasma 1") || station.Equals("Roll Form") || station.Equals("Folding")
             || station.Equals("Specialty") || station.Equals("Knock - up")
             || station.Equals("Seal Tape") || station.Equals("Coil Straight") ||

             (station.Equals("Plasma 2")) || (station.Equals("Plasma 3")))
            {
                result = (m2DataResponse.meta_m2 / interval).ToString("0.00");

                //lay metal m2


            }
            else if (station.Equals("Insulation Cutting") || station.Equals("Insulation Pinning") || station.Equals("Insulation Sorting"))
            {
                result = ((m2DataResponse.isu_m2/1000) / interval).ToString("0.00");
                //chi lay insu


            }

            else if (station.Equals("Wrapping") || station.Equals("Packing") || station.Equals("Decoil Sheet"))
            {
                result = (((m2DataResponse.isu_m2 / 1000) + m2DataResponse.meta_m2) / interval).ToString("0.00");

                //lay metal m2 

            }
            return result;
        }

      
    }
}
