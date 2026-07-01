using ClosedXML.Excel;
using Dapper;
using GeoEMS.Models;
using Microsoft.Ajax.Utilities;
using Npgsql;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace GeoEMS.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        private readonly IDbConnection _db;
        public HomeController()
        {
            string conn = ConnectionRepo.ConnectionStrings;
            _db = new NpgsqlConnection(conn);
        }
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult DataEntry()
        {            
            return View();
        }

       

        public ActionResult Maintenance()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DownloadExcel()
        {
            // Path to the Excel file inside your solution (change the path accordingly)
            string filePath = Server.MapPath("~/App_Data/template_load_data.xlsx"); // For "App_Data" folder
                                                                        // string filePath = Server.MapPath("~/wwwroot/files/sample.xlsx"); // For "wwwroot/files" folder

            // Check if file exists
            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("File not found.");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            string fileName = "sample.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }




        [HttpPost]
        public async Task<ActionResult> ImportExcelFileAsync(HttpPostedFileBase excelFile) { 
         
        DataSet ds = new DataSet();
            if (excelFile == null) {
                Session["Error"] = "Please select valid file"; 
                return RedirectToAction("DataEntry", "Home");
            }
            try
            {
                string filename = null;
                if (excelFile.ContentLength > 0) {
                    if (excelFile.FileName.EndsWith(".xlsx")) {
                        XLWorkbook workbook;
                        try
                        {
                            workbook = new XLWorkbook(excelFile.InputStream);
                        }
                        catch (Exception ex) {
                            TempData["Error"] = "An Error has been occured :" + ex.Message;
                            return RedirectToAction("Index","Home");
                        }

                        IXLWorksheet worksheet = null;

                        try
                        {
                            filename = Path.GetFileNameWithoutExtension(excelFile.FileName);
                            if (workbook.Worksheets.Count > 0) {

                                for (int i = 1; i <= workbook.Worksheets.Count; i++) {
                                    var wb = workbook.Worksheet(i);
                                    worksheet = workbook.Worksheets.Worksheet(i);
                                    string sheetname = worksheet.Name;
                                    string SheetConfig = System.Configuration.ConfigurationManager.AppSettings["SheetConfig"].ToString();
                                    if (wb != null && sheetname.Trim() == SheetConfig) {
                                    DataTable dt = new DataTable();
                                        bool firstrow = true;

                                        foreach (IXLRow row in worksheet.RowsUsed()) {
                                            var rowNumber = row.RowNumber();
                                            if (firstrow)
                                            {
                                                try
                                                {
                                                    foreach (IXLCell cell in row.Cells())
                                                    {
                                                        dt.Columns.Add(cell.Value.ToString());
                                                    }

                                                    firstrow = false;
                                                }
                                                catch (Exception ex)
                                                {
                                                    string msg = ex.Message;
                                                }
                                            }
                                            else {
                                                int d = 0;
                                                DataRow toInsert = dt.NewRow();
                                                foreach (IXLCell cell in row.Cells(1,dt.Columns.Count)) {
                                                    try
                                                    {
                                                        if (!string.IsNullOrEmpty(cell.Value.ToString())) {
                                                            toInsert[d] = cell.Value.ToString();
                                                        }
                                                    }
                                                    catch (Exception ex) { 
                                                    
                                                    }
                                                    d++;
                                                }
                                                dt.Rows.Add(toInsert);
                                            }
                                        }
                                        ds.Tables.Add(dt);  

                                        
                                    }
                                }

                            }
                        }
                        catch (Exception ex) {
                            TempData["Error"] = ex.ToString();
                            return RedirectToAction("DataEntry", "Home");
                        }
                    }
                    else
                    {
                        TempData["Error"] = "File Not Allowed";
                        return RedirectToAction("DataEntry", "Home");
                    }
                }
                else {
                    TempData["Error"] = "File Not Allowed";
                    return RedirectToAction("DataEntry", "Home");
                }
            }
            catch (Exception ex) {
                TempData["Error"] = ex.ToString();
                return RedirectToAction("DataEntry", "Home");
            }

            if (ds.Tables.Count > 0) {
                string SheetConfig = System.Configuration.ConfigurationManager.AppSettings["SheetConfig"].ToString();
                EMSModel model = new EMSModel();
                DataTable dataTable = ds.Tables[0];
                string msg = await model.BulkImportAsync(dataTable);
                TempData["Success"] = msg;
            }

            return RedirectToAction("DataEntry", "Home");
        }


        [HttpGet]
        public ActionResult GetAllGpsData()
        {
           IEnumerable<Gps_data> datas = new List<Gps_data>();
            EMSModel model = new EMSModel();
            datas = model.Get_All_Gps_data();

            return Json(new { status = "success", Data = datas }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteTripData(string tripId)
        {
            //IEnumerable<Gps_data> datas = new List<Gps_data>();
            EMSModel model = new EMSModel();
            string res = model.DeleteTripData(tripId);

            return Json(new { status = "success", Data = res }, JsonRequestBehavior.AllowGet);
        }




        [HttpGet]
        public ActionResult GetAllTicketData()
        {
            IEnumerable<ticket_master> datas = new List<ticket_master>();
            EMSModel model = new EMSModel();
            datas = model.Get_All_Ticket_Master();

            return Json( new { status = "success", Data = datas },JsonRequestBehavior.AllowGet );
        }



        //[HttpPost]
        //public ActionResult Upload(HttpPostedFileBase file)
        //{
        //    if (file != null && file.ContentLength > 0)
        //    {
        //        var fileName = Path.GetFileName(file.FileName);
        //        string ext = Path.GetExtension(fileName);

        //        if (ext == ".xlsx")
        //        {
        //            var path = Path.Combine(Server.MapPath("~/Images/"), fileName);
        //            file.SaveAs(path);
        //        }
        //        else {
        //            ViewBag.Message = "Your application description page.";
        //        }

        //    }

        //    return RedirectToAction("DataEntry");
        //}


        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}


        [HttpPost]
        public ActionResult ReviewDataExcel(HttpPostedFileBase excelFile)
        {
            Dictionary<string, dynamic> errors = new Dictionary<string, dynamic>();
            var duplicates = new List<TicketPreviewVM>();
            List<ValidationErrorRow> errorRows = new List<ValidationErrorRow>();
            List<TicketPreviewVM> lst = new List<TicketPreviewVM>();
            if (excelFile == null || excelFile.ContentLength == 0)
                return Content("Please upload a valid Excel file");

            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string connStr = ConfigurationManager
                                .ConnectionStrings["NpgsqlConnectionString"].ConnectionString;
            NpgsqlConnection con = new NpgsqlConnection(connStr);
            var ticketMap = new Dictionary<string, int>();
            using (var package = new ExcelPackage(excelFile.InputStream))
            {
                var sheet = package.Workbook.Worksheets[1];
                int rows = sheet.Dimension.Rows;
                for (int row = 2; row <= rows; row++)
                {
                    string ticketNo = sheet.Cells[row, 1].Text.Trim();
                    string vehicleNo = sheet.Cells[row, 2].Text.Trim();
                    string lat = sheet.Cells[row, 3].Text.Trim();
                    string lng = sheet.Cells[row, 4].Text.Trim();
                    string gpsDate = sheet.Cells[row, 5].Text.Trim();

                    ValidateRow(row, ticketNo, vehicleNo, lat, lng, gpsDate,ref errorRows);
                    
                       



                    //string ticketNo = sheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(ticketNo))
                        continue;

                    if (ticketMap.ContainsKey(ticketNo))
                        ticketMap[ticketNo]++;
                    else
                        ticketMap[ticketNo] = 1;
                }

                string checkQuery = string.Empty;
                var ticketList = ticketMap.Keys.ToList();
                var existingTickets = new HashSet<string>();

                con.Open();
                var tx = con.BeginTransaction();
                var cmd = new NpgsqlCommand("CREATE TEMP TABLE tmp_ticket_list(ticket_no TEXT);", con, tx);
                int i = cmd.ExecuteNonQuery();
                using (var writer = con.BeginTextImport("COPY tmp_ticket_list(ticket_no) FROM STDIN"))
                {
                    foreach (var t in ticketList)
                        writer.WriteLine(t);
                }

                cmd = new NpgsqlCommand(@"
        SELECT DISTINCT t.ticket_no
        FROM tmp_ticket_list t
        JOIN y_mayur.ticket_master m
             ON m.ticket_no = t.ticket_no;", con);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        existingTickets.Add(reader.GetString(0));
                }

                tx.Commit();


               
                   
                //}

               


                //using (var cmd = new NpgsqlCommand("SELECT ticket_no FROM y_mayur.ticket_master WHERE ticket_no = ANY(@tickets)", con))
                //{
                //    con.Open();
                //    cmd.Parameters.AddWithValue("@tickets", ticketList);

                //    using (var reader = cmd.ExecuteReader())
                //    {
                //        while (reader.Read())
                //        { 
                //            existingTickets.Add(reader.GetString(0));
                //        }
                //    }
                //}

    //            duplicates = ticketList
    //.Where(t => existingTickets.Contains(t))
    //.Select(t => new TicketPreviewVM { TicketNo = t })
    //.ToList();

                foreach(string ticket in existingTickets)
                {
                    ValidationErrorRow v1 = new ValidationErrorRow
                    {

                        TicketNo = ticket,

                        ErrorMessage = "Duplicate in Database"
                    };
                    errorRows.Add(v1);
                }

                //errors.Add("Duplicate", duplicates);
                var first1000 = errorRows.Take(1000).ToList();   // BEST

                errors.Add("InvalidData", first1000);
                errors.Add("InvalidDataCount", errorRows.Count);
                //foreach (var key in ticketMap.Keys)
                //{
                //    checkQuery= @"SELECT COUNT(1) FROM  y_mayur.ticket_master where ticket_no='"+key.ToString()+"'";
                //    int cntTicket = _db.ExecuteScalar<int>(checkQuery);
                //    if(cntTicket > 0)
                //    {
                //        lst.Add(new TicketPreviewVM { TicketNo = key.ToString() });
                //    }

                //}




                //         var preview = ticketMap
                //.Take(100)
                //.Select(x => new TicketPreviewVM
                //{
                //    TicketNo = x.Key,
                //    QuestionCount = x.Value
                //}).ToList();


                //using (var conn = new NpgsqlConnection(connStr))
                //{
                //    conn.Open();

                //    using (var tran = conn.BeginTransaction())
                //    {
                //        try
                //        {
                //            for (int row = 2; row <= rows; row++) // start from row 2
                //            {
                //                var cmd = new NpgsqlCommand(@"
                //                INSERT INTO quiz_questions
                //                (question_text, option_a, option_b, option_c, option_d,
                //                 correct_option, marks, difficulty, category)
                //                VALUES
                //                (@question, @a, @b, @c, @d, @correct, @marks, @difficulty, @category)
                //            ", conn);

                //                cmd.Parameters.AddWithValue("@question", sheet.Cells[row, 1].Text);
                //                cmd.Parameters.AddWithValue("@a", sheet.Cells[row, 2].Text);
                //                cmd.Parameters.AddWithValue("@b", sheet.Cells[row, 3].Text);
                //                cmd.Parameters.AddWithValue("@c", sheet.Cells[row, 4].Text);
                //                cmd.Parameters.AddWithValue("@d", sheet.Cells[row, 5].Text);
                //                cmd.Parameters.AddWithValue("@correct", sheet.Cells[row, 6].Text);
                //                cmd.Parameters.AddWithValue("@marks", int.Parse(sheet.Cells[row, 7].Text));
                //                cmd.Parameters.AddWithValue("@difficulty", sheet.Cells[row, 8].Text);
                //                cmd.Parameters.AddWithValue("@category", sheet.Cells[row, 9].Text);

                //                cmd.ExecuteNonQuery();
                //            }

                //            tran.Commit();
                //        }
                //        catch (Exception ex)
                //        {
                //            tran.Rollback();
                //            return Content("Error: " + ex.Message);
                //        }
                //    }
                //}
            }

            return Json(new
            {
                success = true,
                data = errors
            });

            //return Content("Excel uploaded successfully");
        }

        // [HttpPost]
        //// [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
        // public JsonResult GetExcelData()
        // {
        //     var request = System.Web.HttpContext.Current.Request;

        //     int draw = string.IsNullOrEmpty(request["draw"])? 1 : Convert.ToInt32(request["draw"]);

        //     int start = string.IsNullOrEmpty(request["start"])? 0: Convert.ToInt32(request["start"]);

        //     int length = string.IsNullOrEmpty(request["length"])? 10: Convert.ToInt32(request["length"]);
        //     int totalRecords;
        //     var data = new List<GeoFenceVM>();

        //     using (var conn = new NpgsqlConnection(ConnectionRepo.ConnectionStrings))
        //     {
        //         conn.Open();

        //         // 1️⃣ Total count
        //         using (var countCmd = new NpgsqlCommand(
        //             "SELECT COUNT(*) FROM  y_mayur.ticket_master", conn))
        //         {
        //             totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
        //         }

        //         // 2️⃣ Page data
        //         using (var cmd = new NpgsqlCommand(@"
        //     SELECT ticket_no, vehicle_no,TO_CHAR(created_at, 'DD-MM-YYYY HH24:MI:SS') as created_at
        //     FROM  y_mayur.ticket_master
        //     ORDER BY id desc
        //     LIMIT @limit OFFSET @offset
        // ", conn))
        //         {
        //             cmd.Parameters.AddWithValue("@limit", length);
        //             cmd.Parameters.AddWithValue("@offset", start);

        //             using (var reader = cmd.ExecuteReader())
        //             {
        //                 while (reader.Read())
        //                 {
        //                     data.Add(new GeoFenceVM
        //                     {
        //                         TicketNo = reader.GetString(0),
        //                         vehicle_no = reader.GetString(1),
        //                         created_at = reader.GetString(2)
        //                     });
        //                 }
        //             }
        //         }
        //     }

        //     return Json(new
        //     {
        //         draw = draw,
        //         recordsTotal = totalRecords,
        //         recordsFiltered = totalRecords,
        //         data = data
        //     });

        // }


        [HttpPost]
        public JsonResult GetExcelData()
        {
            string fromDate = Request["fromDate"];
            string toDate = Request["toDate"];

            DateTime fromDt, toDt;
            bool hasValidRange =
                DateTime.TryParse(fromDate, out fromDt) &&
                DateTime.TryParse(toDate, out toDt) &&
                fromDt <= toDt;

            var data = new List<GeoFenceVM>();

            using (var conn = new NpgsqlConnection(ConnectionRepo.ConnectionStrings))
            {
                conn.Open();

                string sql = "SELECT DISTINCT ON (vehicle_no) " + "id, ticket_no, vehicle_no, created_at " + "FROM y_mayur.ticket_master " + "WHERE 1=1 ";

                // 🔹 Default: last 2 months
                if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
                {
                    sql += " AND created_at >= NOW() - INTERVAL '3 months' ";
                }
                else
                {
                    sql += " AND created_at >= '" + fromDate + "'::timestamp " + " AND created_at < ('" + toDate + "'::timestamp + INTERVAL '1 day') ";

                }

                // 🔹 Required for DISTINCT ON
                sql += " ORDER BY vehicle_no, created_at DESC ";

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
                    {
                        cmd.Parameters.AddWithValue("@fromDate", DateTime.Parse(fromDate));
                        cmd.Parameters.AddWithValue("@toDate", DateTime.Parse(toDate));
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new GeoFenceVM
                            {
                                TicketNo = reader["ticket_no"].ToString(),
                                vehicle_no = reader["vehicle_no"].ToString(),
                                created_at = Convert.ToDateTime(reader["created_at"])
                                             .ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }

            return Json(new { data }, JsonRequestBehavior.AllowGet);
        }





        [HttpPost]
        public ActionResult UploadToServer(HttpPostedFileBase excelFile)
        {
            try
            {
                string uploadPath = Server.MapPath("~/Uploads/");
                Directory.CreateDirectory(uploadPath);
                Guid txnId = Guid.NewGuid();
                string excelPath = Path.Combine(uploadPath, Guid.NewGuid() + ".xlsx");
                excelFile.SaveAs(excelPath);

                // 1️⃣ Convert Excel → CSV
                string csvPath = ConvertExcelToCsv(excelPath, txnId);

                // 2️⃣ COPY to PostgreSQL
                //ErrorMessage em1= CopyCsvToPostgres(csvPath);
                ErrorMessage em1 = ImportExcelInChunks(excelPath, txnId);
                if (em1.success)
                {
                    ErrorMessage em2=InsertIntoFinalTable(txnId);
                    if(em2.success)
                    {
                        ErrorMessage em3 = InsertInTicketMaster(txnId);

                        return Json(new { success = true, message = "Data Inserted Successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = em2.errorMessage });
                    }
                }
                else
                {
                    return Json(new { success = false, message = em1.errorMessage });
                }
                
                
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ErrorMessage ImportExcelInChunks(string excelPath, Guid txnId)
        {
            ErrorMessage e1 = new ErrorMessage();
            e1.success = true;
            e1.errorMessage = "Success";

            try
            {
                const int CHUNK_SIZE = 10000;

                using (var conn = new NpgsqlConnection(
                    ConnectionRepo.ConnectionStrings))
                {
                    conn.Open();

                    using (var package = new ExcelPackage(new FileInfo(excelPath)))
                    {
                        var sheet = package.Workbook.Worksheets[1];


                        int currentCount = 0;
                        int totalRows = sheet.Dimension.Rows;
                        int current = 0;



                        try
                        {
                            var writer = conn.BeginTextImport(@"
            COPY y_mayur.gps_data_stg
            (txn_id, ticket_no, vehicle_no, lat, lng, log_date)
            FROM STDIN WITH (FORMAT csv, NULL '')
        ");

                            for (int r = 2; r <= totalRows; r++)
                            {
                                string ticketNo = sheet.Cells[r, 1].Text;
                                string vehicleNo = sheet.Cells[r, 2].Text;
                                string lat = sheet.Cells[r, 3].Text;
                                string lng = sheet.Cells[r, 4].Text;
                                string logDate = sheet.Cells[r, 5].Text;

                                // date clean
                                DateTime dt;
                                if (!DateTime.TryParse(logDate, out dt))
                                    logDate = "";
                                else
                                    logDate = dt.ToString("yyyy-MM-dd HH:mm:ss");

                                writer.WriteLine(
                                    $"\"{txnId}\",\"{ticketNo}\",\"{vehicleNo}\",\"{lat}\",\"{lng}\",\"{logDate}\""
                                );

                                currentCount++;

                                // 🔥 CHUNK COMMIT
                                if (currentCount % CHUNK_SIZE == 0)
                                {
                                    writer.Dispose(); // flush COPY
                                    writer = conn.BeginTextImport(@"
                    COPY y_mayur.gps_data_stg
                    (txn_id, ticket_no, vehicle_no, lat, lng, log_date)
                    FROM STDIN WITH (FORMAT csv, NULL '')
                ");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            e1.success = false;
                            e1.errorMessage = ex.Message;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                e1.success = false;
                e1.errorMessage = ex.Message;
            }

            return e1;
        
        }
            
        


        public ErrorMessage CopyCsvToPostgres(string csvPath)
        {
            ErrorMessage chk = new ErrorMessage();
            





            try
            {
                using (var conn = new NpgsqlConnection(
                    ConnectionRepo.ConnectionStrings))
                {

                    conn.Open();

                    using (var writer = conn.BeginTextImport(@"
        COPY y_mayur.gps_data_stg
        (txn_id,ticket_no, vehicle_no, lat, lng, log_date)
        FROM STDIN WITH (FORMAT csv, HEADER false)
    "))
                    {

                        using (var reader = new StreamReader(csvPath))
                        {

                            while (!reader.EndOfStream)
                            {
                                writer.WriteLine(reader.ReadLine());
                            }
                        }
                    }
                }
                chk.success = true;
                chk.errorMessage = "Success";
            }
            catch(Exception ex)
            {
                chk.success = false;
                chk.errorMessage ="CopyToDb:"+ ex.Message;
            }
            return chk;
        }

        public string ConvertExcelToCsv(string excelPath,Guid txnId)
        {
            string csvPath = Path.ChangeExtension(excelPath, ".csv");
            
            using (var package = new ExcelPackage(new FileInfo(excelPath)))
            using (var writer = new StreamWriter(csvPath))
            {
                var sheet = package.Workbook.Worksheets[1];
                int rows = sheet.Dimension.Rows;
                int cols = sheet.Dimension.Columns;

                for (int r = 2; r <= rows; r++) // skip header
                {
                    string ticketNo = sheet.Cells[r, 1].Text;
                    string vehicleNo = sheet.Cells[r, 2].Text;
                    string lat = sheet.Cells[r, 3].Text;
                    string lng = sheet.Cells[r, 4].Text;
                    string logDate = sheet.Cells[r, 5].Text;

                    DateTime dt;
                    if (!DateTime.TryParse(logDate, out dt))
                        logDate = "";
                    else
                        logDate = dt.ToString("yyyy-MM-dd HH:mm:ss");

                    // txn_id is FIRST column
                    string csvRow=$"{txnId},{ticketNo},{vehicleNo},{lat},{lng},{logDate}";
                    //writer.WriteLine(
                    //    $"\"{txnId}\",\"{ticketNo}\",\"{vehicleNo}\",\"{lat}\",\"{lng}\",\"{logDate}\""
                    //);
                    writer.WriteLine(csvRow);

                    //var values = new List<string>();

                    //for (int c = 1; c <= cols-1; c++)
                    //{
                    //    var text = sheet.Cells[r, c].Text
                    //        .Replace("\"", "\"\"");
                    //    values.Add($"\"{text}\"");
                    //}

                    //writer.WriteLine(string.Join(",", values));
                }
            }

            return csvPath;
        }

        public ErrorMessage InsertIntoFinalTable(Guid txnId)
        {
            ErrorMessage chk = new ErrorMessage();
            try
            {
                using (var conn = new NpgsqlConnection(
                    ConnectionRepo.ConnectionStrings))
                {

                    conn.Open();

                    //string lastCtid = "(0,0)";
                    //int rowsInserted;
                    string lastCtid = "(0,0)";
                    string nextCtid;

                    do
                    {
                        nextCtid = _db.ExecuteScalar<string>(@"
        WITH batch AS (
            SELECT ctid
            FROM y_mayur.gps_data_stg
            WHERE txn_id = @txn_id
              AND ctid > @last_ctid::tid
            ORDER BY ctid
            LIMIT 10000
        ),
        ins AS (
            INSERT INTO y_mayur.gps_data
            (ticket_no, vehicle_no, lat, lng, log_date)
            SELECT
                s.ticket_no,
                s.vehicle_no,

                CASE
                    WHEN s.lat ~ '^-?\d+(\.\d+)?$'
                    THEN s.lat::DOUBLE PRECISION
                    ELSE NULL
                END,

                CASE
                    WHEN s.lng ~ '^-?\d+(\.\d+)?$'
                    THEN s.lng::DOUBLE PRECISION
                    ELSE NULL
                END,

                CASE
                    WHEN s.log_date ~ '^\d{4}-\d{2}-\d{2}'
                    THEN s.log_date::TIMESTAMP
                    ELSE NULL
                END
            FROM y_mayur.gps_data_stg s
            JOIN batch b ON s.ctid = b.ctid
            RETURNING 1
        )
        SELECT MAX(ctid)::text FROM batch;
    ", new { txn_id = txnId, last_ctid = lastCtid });

                        lastCtid = nextCtid;

                    } while(!string.IsNullOrEmpty(nextCtid));






                    //    string sql = @"
                    //        INSERT INTO y_mayur.gps_data
                    //(ticket_no, vehicle_no, lat, lng, log_date)
                    //SELECT
                    //    ticket_no,
                    //    vehicle_no,

                    //    -- latitude
                    //    CASE
                    //        WHEN lat ~ '^-?\d+(\.\d+)?$'
                    //        THEN lat::DOUBLE PRECISION
                    //        ELSE NULL
                    //    END,

                    //    -- longitude
                    //    CASE
                    //        WHEN lng ~ '^-?\d+(\.\d+)?$'
                    //        THEN lng::DOUBLE PRECISION
                    //        ELSE NULL
                    //    END,

                    //    -- log_date
                    //    CASE
                    //        WHEN log_date ~ '^\d{4}-\d{2}-\d{2}'
                    //        THEN log_date::TIMESTAMP
                    //        ELSE NULL
                    //    END

                    //FROM y_mayur.gps_data_stg

                    //;
                    //    ";

                    //    using (var cmd = new NpgsqlCommand(sql, conn))
                    //    {
                    //       int a= cmd.ExecuteNonQuery();
                    //    }
                }
                chk.success = true;
                chk.errorMessage = "Success";
            }
            catch(Exception ex)
            {
                chk.success = false;
                chk.errorMessage = "InsertFinalTable:" + ex.Message;
            }

            return chk;
        }

        public ErrorMessage InsertInTicketMaster(Guid txnId)
        {
            ErrorMessage e = new ErrorMessage();
            try
            {
                string connStr = ConfigurationManager
                               .ConnectionStrings["NpgsqlConnectionString"].ConnectionString;
                NpgsqlConnection con = new NpgsqlConnection(connStr);
                using (con = new NpgsqlConnection(connStr))
                {
                    con.Open();
                    string sql = @"
    INSERT INTO y_mayur.ticket_master
    (ticket_no, vehicle_no, created_at)
    SELECT DISTINCT
        ticket_no,
        vehicle_no,
        NOW()
    FROM y_mayur.gps_data_stg
    WHERE txn_id = @txn_id
    ON CONFLICT (ticket_no) DO NOTHING;
";

                    con.Execute(sql, new { txn_id = txnId });
                }
                e.success = true;
                e.errorMessage = "";
            }
            catch(Exception ex)
            {
                e.success = false;
                e.errorMessage = ex.Message;
            }
            return e;
        }


        [HttpPost]
        public ActionResult UploadToServer1(HttpPostedFileBase excelFile)
        {
            var duplicates = new List<TicketPreviewVM>();
            List<TicketPreviewVM> lst = new List<TicketPreviewVM>();
            if (excelFile == null || excelFile.ContentLength == 0)
                return Content("Please upload a valid Excel file");

            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            string connStr = ConfigurationManager
                                .ConnectionStrings["NpgsqlConnectionString"].ConnectionString;
            NpgsqlConnection con = new NpgsqlConnection(ConnectionRepo.ConnectionStrings);
            var ticketMap = new Dictionary<string, int>();
            using (var package = new ExcelPackage(excelFile.InputStream))
            {
                var sheet = package.Workbook.Worksheets[1];
                int rows = sheet.Dimension.Rows;
                for (int row = 2; row <= rows; row++)
                {
                    string ticketNo = sheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrEmpty(ticketNo))
                        continue;

                    if (ticketMap.ContainsKey(ticketNo))
                        ticketMap[ticketNo]++;
                    else
                        ticketMap[ticketNo] = 1;
                }

                string checkQuery = string.Empty;
                var ticketList = ticketMap.Keys.ToList();
                var existingTickets = new HashSet<string>();

                using (var cmd = new NpgsqlCommand("SELECT ticket_no FROM y_mayur.ticket_master WHERE ticket_no = ANY(@tickets)", con))
                {
                    con.Open();
                    cmd.Parameters.AddWithValue("@tickets", ticketList);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            existingTickets.Add(reader.GetString(0));
                        }
                    }
                }

                duplicates = ticketList
    .Where(t => existingTickets.Contains(t))
    .Select(t => new TicketPreviewVM { TicketNo = t })
    .ToList();

                //foreach (var key in ticketMap.Keys)
                //{
                //    checkQuery= @"SELECT COUNT(1) FROM  y_mayur.ticket_master where ticket_no='"+key.ToString()+"'";
                //    int cntTicket = _db.ExecuteScalar<int>(checkQuery);
                //    if(cntTicket > 0)
                //    {
                //        lst.Add(new TicketPreviewVM { TicketNo = key.ToString() });
                //    }

                //}




                //         var preview = ticketMap
                //.Take(100)
                //.Select(x => new TicketPreviewVM
                //{
                //    TicketNo = x.Key,
                //    QuestionCount = x.Value
                //}).ToList();


                //using (var conn = new NpgsqlConnection(connStr))
                //{
                //    conn.Open();

                //    using (var tran = conn.BeginTransaction())
                //    {
                //        try
                //        {
                //            for (int row = 2; row <= rows; row++) // start from row 2
                //            {
                //                var cmd = new NpgsqlCommand(@"
                //                INSERT INTO quiz_questions
                //                (question_text, option_a, option_b, option_c, option_d,
                //                 correct_option, marks, difficulty, category)
                //                VALUES
                //                (@question, @a, @b, @c, @d, @correct, @marks, @difficulty, @category)
                //            ", conn);

                //                cmd.Parameters.AddWithValue("@question", sheet.Cells[row, 1].Text);
                //                cmd.Parameters.AddWithValue("@a", sheet.Cells[row, 2].Text);
                //                cmd.Parameters.AddWithValue("@b", sheet.Cells[row, 3].Text);
                //                cmd.Parameters.AddWithValue("@c", sheet.Cells[row, 4].Text);
                //                cmd.Parameters.AddWithValue("@d", sheet.Cells[row, 5].Text);
                //                cmd.Parameters.AddWithValue("@correct", sheet.Cells[row, 6].Text);
                //                cmd.Parameters.AddWithValue("@marks", int.Parse(sheet.Cells[row, 7].Text));
                //                cmd.Parameters.AddWithValue("@difficulty", sheet.Cells[row, 8].Text);
                //                cmd.Parameters.AddWithValue("@category", sheet.Cells[row, 9].Text);

                //                cmd.ExecuteNonQuery();
                //            }

                //            tran.Commit();
                //        }
                //        catch (Exception ex)
                //        {
                //            tran.Rollback();
                //            return Content("Error: " + ex.Message);
                //        }
                //    }
                //}
            }

            return Json(new
            {
                success = true,
                data = duplicates
            });

            //return Content("Excel uploaded successfully");
        }


        public bool ValidateRow(
    int rowNo,
    string ticketNo,
    string vehicleNo,
    string lat,
    string lng,
    string gpsDate,
    ref List<ValidationErrorRow> errors)
        {
            List<string> errorList = new List<string>();

            if (string.IsNullOrWhiteSpace(ticketNo))
                errorList.Add("Ticket No missing");

            if (string.IsNullOrWhiteSpace(vehicleNo))
                errorList.Add("Vehicle No missing");

            if (!double.TryParse(lat, out double latVal) || latVal < -90 || latVal > 90)
                errorList.Add("Invalid latitude");

            if (!double.TryParse(lng, out double lngVal) || lngVal < -180 || lngVal > 180)
                errorList.Add("Invalid longitude");

            if (!DateTime.TryParse(gpsDate, out _))
                errorList.Add("Invalid GPS date");

            if (errorList.Any())
            {
                errors.Add(new ValidationErrorRow
                {
                    RowNumber = rowNo,
                    TicketNo = ticketNo,
                    VehicleNo = vehicleNo,
                    Lat = lat,
                    Lng = lng,
                    GpsDate = gpsDate,
                    ErrorMessage = string.Join(", ", errorList)
                });
                return false;
            }

            return true;
        }

        [HttpGet]
        public JsonResult GetDashboardCounts()
        {
            string sql = @"SELECT COUNT(ticket_no) AS ""TotalTickets"", COUNT(DISTINCT vehicle_no) AS ""TotalVehicles"" FROM y_mayur.ticket_master;";

            var result = _db.QueryFirstOrDefault(sql);

            return Json(new
            {
                totalTickets = result?.TotalTickets ?? 0,
                totalVehicles = result?.TotalVehicles ?? 0
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult DeleteMultipleTrips(List<string> tripIds)
        {
            if (tripIds == null || tripIds.Count == 0)
                return Json(new { success = false, message = "No records selected" });

            //Convert list → 'id1','id2','id3'
            string ids = string.Join(",", tripIds.Select(x => $"'{x.Replace("'", "''")}'"));

            using (var conn = new NpgsqlConnection(ConnectionRepo.ConnectionStrings))
            {
                conn.Open();

                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // Delete from gps_data
                        string sqlGps ="DELETE FROM y_mayur.gps_data " + "WHERE ticket_no IN (" + ids + ")";

                        conn.Execute(sqlGps, transaction: tran);

                        // Delete from ticket_master
                        string sqlTicket ="DELETE FROM y_mayur.ticket_master " + "WHERE ticket_no IN (" + ids + ")";

                        conn.Execute(sqlTicket, transaction: tran);

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return Json(new { success = false, message = ex.Message });
                    }
                }
            }

            return Json(new { success = true });
        }


    }
}