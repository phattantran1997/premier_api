using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using API_premierductsqld.Entities;
using API_premierductsqld.Entities.response;
using Microsoft.AspNetCore.Mvc;

namespace API_premierductsqld.Repository.@interface
{
    public interface IJobtimingRepository
    {
        public GetCurrentJobTimingsResponse getCurrentJobByUser(string date, string operatorID);

        //public List<GetHistoryByJobNoResponse> getAllDataJobTimingByJobNo(string jobno);

        public List<ListJobNoDashBoardResponse> getListDetailJobNoOnDashboard(string date, int stationNo);

        public string calculateIntervalEachJobNo( int stationNo, DataTable dataTable, string jobNO);

        public string calculateIntervalEachJobNowithoutDT(int stationNo, string jobNO);

        List<JobTimingResponse> getJobTimingsDetail(List<string> jobno);
        //use
        List<string> getListJobNoString(string date, string end);

        List<JobNoandStationNoResponse> getListJobNoAndStationNo(string date, string end);
        //use
        List<JobTimingResponse> getAllDataJobtimingByDate(string date);

        List<JobTimingResponse> getAllDataJobtimingByUserAndDate(string user, string date);

        List<JobTimingResponse> listJobTimingOnTabJobs(string date_start , string date_end);
        

    }
}
