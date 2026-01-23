using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoEMS.Models
{
    public class ConnectionRepo
    {
        public static string ConnectionStrings
        {
            get
            {
                var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["NpgsqlConnectionString"];
                if (connectionString == null)
                {
                    throw new InvalidOperationException("Connection string 'NpgsqlConnectionString' is not defined.");
                }
                return connectionString.ConnectionString;
            }
        }
    }
}