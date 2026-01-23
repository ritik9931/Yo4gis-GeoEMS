using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;

namespace GeoEMS.Models
{
    public class Gps_data
    {
        public int id { get; set; }
        public string ticket_no { get; set; }
        public string vehicle_no { get; set; }
        public double lat { get; set; }
        public double lng { get; set; }
        public DateTime log_date { get; set; }
        public DateTime created_at { get; set; }    
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
}