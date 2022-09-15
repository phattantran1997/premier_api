using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using API_premierductsqld.Entities;
using API_premierductsqld.Repository;
using DTO_PremierDucts.DBClient;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace API_premierductsqld.Service
{
    public class UserService
    {
        public IStationRepository stationRepository;
        private DBConnection dbCon;

        public UserService()
        {
            stationRepository = new StationRepository();

            dbCon = DBConnection.Instance(Startup.StaticConfig.GetConnectionString("ConnectionForDatabase"));
        }

        public  Task<List<StationAttendees>> GetAllOnlineUser()
        {

            List<StationAttendees> rsult = new List<StationAttendees>();

            if (dbCon.IsConnect())
            {
                try
                {

                    DataTable dataTable = new DataTable();
                    string query = "SELECT * FROM stationAttendees;";
                    MySqlDataAdapter myDataAdapter = new MySqlDataAdapter(query, dbCon.Connection);
                    myDataAdapter.Fill(dataTable);
                    foreach (DataRow row in dataTable.Rows)
                    {
                        rsult.Add(new StationAttendees(row.Field<Int32>("stationNo"), row.Field<string>("username"), row.Field<string>("name")));
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
            return Task.Run(() => rsult);

        }

     
        public List<AllStationDashboardSettingsResponse> GetAllStationWithRate(string date)
        {

            List<AllStationDashboardSettingsResponse> result = stationRepository.getAllStationDashboardWithRate(date);


            //còn phải tính rate
            return result;
        }
    }
}
