using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoEMS.Models
{
    public class ticket_master
    {
        public int id { get; set; }
        public string ticket_no { get; set; }
        public string vehicle_no { get; set; }
        public DateTime created_at { get; set; }
    }


}