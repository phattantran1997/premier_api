using System;
using System.Collections.Generic;
using API_premierductsqld.Entities.response;
using API_premierductsqld.Repository;
using DTO_PremierDucts;

namespace API_premierductsqld.Service
{
    public class StationService
    {
        private IStationRepository stationRepository;
        public StationService()
        {
            stationRepository = new StationRepository();
        }
        public ResponseData getAllStation()
        {

            ResponseData responseData = new ResponseData();
            List<StationResponse> response = stationRepository.getAllStation();
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

        internal ResponseData getDurationOfStation(string jobno, int stationNo)
        {
            ResponseData responseData = new ResponseData();
           
           DataStaionTab3 totalDurationStation = stationRepository.getDurationOfStation(jobno, stationNo);
           if(totalDurationStation == null)
            {
                responseData.Code = ERROR_CODE.FAIL;
                responseData.Data = "Data incorrect on query";
            }
            else
            {
                responseData.Code = ERROR_CODE.SUCCESS;

                responseData.Data = totalDurationStation;
            }
            return responseData;
        }
    }
}

