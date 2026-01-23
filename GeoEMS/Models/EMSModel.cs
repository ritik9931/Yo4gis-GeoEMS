using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using GeoEMS.Models;
using Dapper;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Data.Common;
using System.Web.Http.Results;

namespace GeoEMS.Models
{
    public class EMSModel
    {
        private readonly IDbConnection _db;
        public EMSModel()
        {
            string conn = ConnectionRepo.ConnectionStrings;
            _db = new NpgsqlConnection(conn);
        }

        public UserLogin Login(string username, string password)
        {
            UserLogin user = new UserLogin();
            _db.Open();
            try
            {
                var dynamicparameters = new DynamicParameters();
                dynamicparameters.Add("username_input", username);
                dynamicparameters.Add("password_input", password);

                user = _db.QueryFirstOrDefault<UserLogin>("get_user_details", dynamicparameters, null, 120, CommandType.StoredProcedure);
                _db.Close();
            }
            catch (Exception ex)
            {
                _db.Close();
            }


            return user;

        }


        public async Task<string> BulkImportAsync(DataTable dtList)
        {
            string msg = null;

            try
            {
                DataTable table = dtList;
                DataTable table1 = dtList;
                var distinctIds = table1.DefaultView.ToTable(true,"TicketNo");

                // Perform bulk insert using Dapper and COPY
                using (var connection = new NpgsqlConnection(ConnectionRepo.ConnectionStrings))
                {
                    await connection.OpenAsync();


                    using (var writer = connection.BeginTextImport($"COPY y_mayur.gps_data (ticket_no, vehicle_no, lat, lng, log_date, created_at) FROM STDIN WITH (FORMAT CSV, HEADER FALSE)"))
                    {
                        using (var stringWriter = new StringWriter())
                        {
                            // Write DataTable rows to a StringWriter as CSV
                            foreach (DataRow row in table.Rows)
                            {
                                string inputDate = row[4].ToString();
                                string result = ConvertStringToDateTime(inputDate);
                                string outputFormat = "yyyy-MM-dd HH:mm:ss";
                                string createDate = "";
                                string inputFormat = "dd-MM-yyyy HH:mm:ss";
                                string currentDate = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                                createDate = DateTime.Now.ToString(outputFormat);
                                //DateTime? result1 = ConvertStringToDateTime(DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                                //if (DateTime.TryParseExact(currentDate, inputFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                //{
                                //    createDate= parsedDate.ToString(outputFormat); // Convert to required format
                                //}

                                // Try to parse the input date
                                //bool isSuccess = DateTime.TryParse(inputDate, out result);
                                if (result != null) {
                                    string csvRow = $"{row[0]},{row[1]},{row[2]},{row[3]},{result},{createDate}";
                                    stringWriter.WriteLine(csvRow);
                                }
                                
                            }

                            // Flush the StringWriter content to the COPY command
                            writer.Write(stringWriter.ToString());
                        }
                    }

                    using (var writer = connection.BeginTextImport($"COPY y_mayur.ticket_master (ticket_no, vehicle_no, created_at) FROM STDIN WITH (FORMAT CSV, HEADER FALSE)"))
                    {
                        using (var stringWriter = new StringWriter())
                        {
                            string ticketNo1 = null;
                            // Write DataTable rows to a StringWriter as CSV
                            foreach (DataRow row in table1.Rows)
                            {
                                string ticketNo2 = row[0].ToString();
                                if (ticketNo1 != ticketNo2)
                                {
                                    ticketNo1 = row[0].ToString();
                                    string currentD = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    string csvRow = $"{row[0]},{row[1]},{currentD}";
                                    stringWriter.WriteLine(csvRow);
                                }
                                else {
                                    //if (ticketNo == row[0].ToString()) { continue; }
                                    //ticketNo = row[0].ToString();
                                    //string csvRow = $"{row[0]},{row[1]},{DateTime.Now}";
                                    //stringWriter.WriteLine(csvRow);
                                }
                                

                            }

                            // Flush the StringWriter content to the COPY command
                            writer.Write(stringWriter.ToString());
                        }
                    } 

                    msg = "Bulk import completed successfully!";
                }
            }
            catch (Exception ex)
            {
                _db.Close();
                msg = ex.Message;
            }
            finally {
                _db.Close();
            }

            return msg;
        }


        public static string ConvertStringToDateTime(string inputDate)
        {
            try
            {
                // Define the expected format of the input date
                string format = "dd-MM-yyyy HH:mm:ss";
                string format1 = "dd/MM/yyyy HH:mm:ss";
                string outputFormat = "yyyy-MM-dd HH:mm:ss";
                string[] formats = {
    "dd/MM/yyyy HH:mm:ss",
    "MM/dd/yyyy HH:mm:ss",
    "yyyy-MM-dd HH:mm:ss",
    "dd-MM-yyyy HH:mm:ss",
    "dd.MM.yyyy HH:mm:ss",
    "yyyy/MM/dd HH:mm:ss",
    "MM-dd-yyyy HH:mm:ss",
    "dd/MM/yyyy",
    "yyyy-MM-dd",
    "HH:mm:ss"
};
                if (DateTime.TryParseExact(inputDate, format1, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate1))
                {
                    return parsedDate1.ToString(outputFormat);
                }


                // Attempt to parse the input string
                if (DateTime.TryParseExact(inputDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate.ToString(outputFormat); 
                }
                else
                {
                    //throw new FormatException("Input date format does not match ddMMyyyy HH:mm:ss.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }
        
        
        public IEnumerable<Gps_data> Get_All_Gps_data() {

            //string sqlQuery = "SELECT DISTINCT ON (ticket_no) id, ticket_no, vehicle_no, lat, lng, log_date, created_at FROM y_mayur.gps_data ORDER BY ticket_no, created_at DESC;";
            string sqlQuery = "SELECT id, ticket_no, vehicle_no, lat, lng, log_date, created_at FROM y_mayur.gps_data ORDER BY ticket_no, created_at DESC;";
            IEnumerable<Gps_data> list = _db.Query<Gps_data>(sqlQuery);

            return list;
        }

        public string DeleteTripData(string tripId)
        {
            string result = "SUCCESS";
            try
            {

                //string sqlQuery = "SELECT DISTINCT ON (ticket_no) id, ticket_no, vehicle_no, lat, lng, log_date, created_at FROM y_mayur.gps_data ORDER BY ticket_no, created_at DESC;";
                string sqlQuery = "delete FROM y_mayur.gps_data where ticket_no='" + tripId + "';";
                int rowEffected = _db.Execute(sqlQuery);

                sqlQuery = "delete FROM y_mayur.ticket_master where ticket_no='" + tripId + "';";
                rowEffected = _db.Execute(sqlQuery);
            }
            catch(Exception ex)
            {
                result = "FAILED";
            }

            return result;
        }

        public IEnumerable<Gps_data> Get_All_Gps_data_By_Ticket(string ticket_no)
        {

            //string sqlQuery = "SELECT DISTINCT ON (ticket_no) id, ticket_no, vehicle_no, lat, lng, log_date, created_at FROM y_mayur.gps_data ORDER BY ticket_no, created_at DESC;";
            string sqlQuery = "SELECT id, ticket_no, vehicle_no, lat, lng, log_date, created_at FROM y_mayur.gps_data where ticket_no = '"+ticket_no+"'  ORDER BY ticket_no, created_at DESC;";
            IEnumerable<Gps_data> list = _db.Query<Gps_data>(sqlQuery);

            return list;
        }

        public IEnumerable<ticket_master> Get_All_Ticket_Master()
        {
            string sqlQuery = "SELECT id, ticket_no, vehicle_no, created_at FROM y_mayur.ticket_master  ORDER BY ticket_no, created_at DESC limit 500;";

            IEnumerable<ticket_master> list = _db.Query<ticket_master>(sqlQuery);

            return list;
        }

        public IEnumerable<ticket_master> Get_All_Ticket_Master_By_Ticket(string ticket_no)
        {
            string sqlQuery = "SELECT id, ticket_no, vehicle_no, created_at FROM y_mayur.ticket_master where ticket_no = '"+ticket_no+"' ORDER BY ticket_no, created_at DESC;";

            IEnumerable<ticket_master> list = _db.Query<ticket_master>(sqlQuery);

            return list;
        }


    }
    public class TicketPreviewVM
    {
        public string TicketNo { get; set; }
        //public int QuestionCount { get; set; }
    }

    public class GeoFenceVM {
        public string TicketNo { get; set; }
        public string vehicle_no { get; set; }
        public string created_at { get; set; }
    }

    public class ErrorMessage
    {
        public Boolean success { get; set; }
        public string errorMessage { get; set; }
    }

    public class ValidationErrorRow
    {
        public int RowNumber { get; set; }
        public string TicketNo { get; set; }
        public string VehicleNo { get; set; }
        public string Lat { get; set; }
        public string Lng { get; set; }
        public string GpsDate { get; set; }
        public string ErrorMessage { get; set; }
    }

}