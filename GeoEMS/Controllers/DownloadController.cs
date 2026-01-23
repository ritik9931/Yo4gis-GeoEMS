using GeoEMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace GeoEMS.Controllers
{
    public class DownloadController : ApiController
    {
        // GET: api/Download

        [Route("api/GeoEMS/Getdata")]
        [HttpGet]
        public IHttpActionResult Getdata(string ticket = "Na")
        {
            ResponceData<dynamic> responce = new ResponceData<dynamic>();

            EMSModel model = new EMSModel();
            IEnumerable <Gps_data> data = null;
            if (ticket == "Na")
            {
                data = model.Get_All_Gps_data();
            }
            else {
                data = model.Get_All_Gps_data_By_Ticket(ticket);
            }
            if (data != null)
            {
                responce.Data = data;
                responce.message = "success";
                responce.status = ResultCodeType.SUCCESS;
            }
            else
            {
                responce.message = "failed";
                responce.status = ResultCodeType.FAIL;
            }



            return Ok(responce);
        }


        // GET: api/Download/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Download
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Download/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Download/5
        public void Delete(int id)
        {
        }
    }
}
