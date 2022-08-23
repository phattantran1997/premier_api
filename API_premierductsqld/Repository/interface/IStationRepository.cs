using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API_premierductsqld.Entities;
using API_premierductsqld.Entities.response;

namespace API_premierductsqld.Repository.@interface
{
    public interface IStationRepository
    {

        List<StationResponse> getAllStation();

        List<AllStationDashboardSettingsResponse> getAllStationDashboardWithRate(string date);

        StationResponse getStationDetail(int stationNo);

        DataStaionTab3 getDurationOfStation(string jobno, int stationNo);
       

    }
}
