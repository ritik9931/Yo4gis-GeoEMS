using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Web.Http;

namespace GeoEMS.Controllers
{
    public class LogDataController : ApiController
    {
       
        [Route("api/LogData/Getdata")]
        [HttpGet]
        public IHttpActionResult Getdata(string ticket)
        {

            ResponceData<List<GpsDataModel>> responce = new ResponceData<List<GpsDataModel>>();

            try
            {
                List<GpsDataModel> list = new List<GpsDataModel>();

                string cs = ConfigurationManager.ConnectionStrings["NpgsqlConnectionString"].ConnectionString;

                using (NpgsqlConnection con = new NpgsqlConnection(cs))
                {
                    string query = "SELECT id, ticket_no, vehicle_no, lat, lng, " +
                                   "log_date, created_at, latlng_add " +
                                   "FROM y_mayur.gps_data " +
                                   "WHERE ticket_no = '" + ticket + "'";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        con.Open();

                        using (NpgsqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                list.Add(new GpsDataModel
                                {
                                    id = Convert.ToInt32(dr["id"]),
                                    ticket_no = dr["ticket_no"].ToString(),
                                    vehicle_no = dr["vehicle_no"].ToString(),
                                    lat = dr["lat"].ToString(),
                                    lng = dr["lng"].ToString(),
                                    log_date = Convert.ToDateTime(dr["log_date"]),
                                    created_at = Convert.ToDateTime(dr["created_at"]),
                                    latlng_add = dr["latlng_add"].ToString()
                                });
                            }
                        }
                    }
                }

                responce.status = ResultCodeType.SUCCESS;
                responce.message = "Data fetched successfully";
                responce.Data = list;

                return Ok(responce);
            }
            catch (Exception ex)
            {
                responce.status = ResultCodeType.FAIL;
                responce.message = ex.Message;
                responce.Data = null;

                return Ok(responce);
            }
        }

        [Route("api/LogData/GetdataByTicketandDate")]
        [HttpGet]
        public IHttpActionResult GetdataByTicketandDate(string ticket,string startdate,string enddate)
        {
            ResponceData<List<GpsDataModel>> responce = new ResponceData<List<GpsDataModel>>();

            try
            {
                List<GpsDataModel> list = new List<GpsDataModel>();

                string cs = ConfigurationManager.ConnectionStrings["NpgsqlConnectionString"].ConnectionString;

                using (NpgsqlConnection con = new NpgsqlConnection(cs))
                {
                    string query ="SELECT id, ticket_no, vehicle_no, lat, lng, " + "log_date, created_at, latlng_add " + "FROM y_mayur.gps_data " +
                                    "WHERE ticket_no = '" + ticket + "' " + "AND log_date BETWEEN '" + startdate + "' AND '" + enddate + "'";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        con.Open();

                        using (NpgsqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                list.Add(new GpsDataModel
                                {
                                    id = Convert.ToInt32(dr["id"]),
                                    ticket_no = dr["ticket_no"].ToString(),
                                    vehicle_no = dr["vehicle_no"].ToString(),
                                    lat = dr["lat"].ToString(),
                                    lng = dr["lng"].ToString(),
                                    log_date = Convert.ToDateTime(dr["log_date"]),
                                    created_at = Convert.ToDateTime(dr["created_at"]),
                                    latlng_add = dr["latlng_add"].ToString()
                                });
                            }
                        }
                    }
                }

                responce.status = ResultCodeType.SUCCESS;
                responce.message = "Data fetched successfully";
                responce.Data = list;

                return Ok(responce);
            }
            catch (Exception ex)
            {
                responce.status = ResultCodeType.FAIL;
                responce.message = ex.Message;
                responce.Data = null;

                return Ok(responce);
            }
        }

        [Route("api/LogData/GetdataByDateRange")]
        [HttpGet]
        public IHttpActionResult GetdataByDateRange(string startdate,string enddate)
        {
            ResponceData<List<GpsDataModel>> responce = new ResponceData<List<GpsDataModel>>();

            try
            {
                List<GpsDataModel> list = new List<GpsDataModel>();

                string cs = ConfigurationManager.ConnectionStrings["NpgsqlConnectionString"].ConnectionString;

                using (NpgsqlConnection con = new NpgsqlConnection(cs))
                {
                    string query ="SELECT id, ticket_no, vehicle_no, lat, lng, " + "log_date, created_at, latlng_add " + "FROM y_mayur.gps_data " + "WHERE log_date >= '" + startdate + " 00:00:00' " + "AND log_date <= '" + enddate + " 23:59:59'";


                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                    {
                        con.Open();

                        using (NpgsqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                list.Add(new GpsDataModel
                                {
                                    id = Convert.ToInt32(dr["id"]),
                                    ticket_no = dr["ticket_no"].ToString(),
                                    vehicle_no = dr["vehicle_no"].ToString(),
                                    lat = dr["lat"].ToString(),
                                    lng = dr["lng"].ToString(),
                                    log_date = Convert.ToDateTime(dr["log_date"]),
                                    created_at = Convert.ToDateTime(dr["created_at"]),
                                    latlng_add = dr["latlng_add"].ToString()
                                });
                            }
                        }
                    }
                }

                responce.status = ResultCodeType.SUCCESS;
                responce.message = "Data fetched successfully";
                responce.Data = list;

                return Ok(responce);
            }
            catch (Exception ex)
            {
                responce.status = ResultCodeType.FAIL;
                responce.message = ex.Message;
                responce.Data = null;

                return Ok(responce);
            }
        }


    }
    class ResponceData<T>
    {
        public string message { get; set; }
        public ResultCodeType status { get; set; }
        public T Data { get; set; }
    }
    public enum ResultCodeType
    {
        SUCCESS = 200,
        FAIL = 400,
        LOADING = 2,
        WARNING = 300
    }

    public class GpsDataModel
    {
        public int id { get; set; }
        public string ticket_no { get; set; }
        public string vehicle_no { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public DateTime log_date { get; set; }
        public DateTime created_at { get; set; }
        public string latlng_add { get; set; }
    }


}