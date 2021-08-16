using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Receipt
{
    public class POStatusSummaryController : ApiController
    {
        // inputDataList is string aStatusList, int aDatefield, int aDateFilter,
        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetPOStatusSummary( ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdSIGN_ON, "POStatSumm ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    return (GetPOStatusSummary(aInboundMsg.inputDataList[0], Convert.ToInt32( aInboundMsg.inputDataList[1]), Convert.ToInt32( aInboundMsg.inputDataList[2])));
                }

            }
            catch (Exception e)
            {
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.fErrorData = e.ToString();
                retval.ErrorMessage = e.Message;
                retval.successful = false;
            }
            try
            {
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.ProcessTran(retval.ErrorMessage, "E");
            }
            catch //(Exception e)
            {
            }

            return (retval);
        }


        private ASCTracFunctionStruct.ascBasicReturnMessageType GetPOStatusSummary(string aStatusList, int aDatefield, int aDateFilter)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            List<ASCTracFunctionStruct.CustOrder.COSMType> myList = new List<ASCTracFunctionStruct.CustOrder.COSMType>();
            //bool fOK = true;
            try
            {
                string sql = "SELECT H.RECEIVED, COUNT( DISTINCT( H.PONUMBER)) AS NUM_ORDERS";
                sql += ", sum( D.QTY) AS QTYNEEDED, SUM( D.QTYRECEIVED) AS QTYCOMPLETED";
                sql += " FROM POHDR H";
                sql += " JOIN PODET D ON D.PONUMBER=H.PONUMBER AND D.RELEASENUM=H.RELEASENUM";
                sql += " WHERE H.SITE_ID='" + iParse.myParseNet.Globals.curSiteID + "'";
                if (aStatusList.Equals("O"))
                    sql += " AND H.RECEIVED IN ( 'O', 'L', 'M' )";
                if (aStatusList.Equals("R"))
                    sql += " AND H.RECEIVED IN ( 'R', 'C' )";
                // pickDateField.Items.Add("Schedule Date");
                var fldDate = "CONVERT( DATE, H.EXPECTEDRECEIPTDATE)";
                if (aDatefield == 1)
                    fldDate = "CONVERT( DATE, ISNULL( D.EXPECTEDRECEIPTDATE, H.EXPECTEDRECEIPTDATE) )";

                // pickDateField.Items.Add("Target Date");
                switch (aDateFilter)
                {
                    case 0: // all dates
                        break;
                    case 1: // today
                        sql += " AND " + fldDate + "='" + DateTime.Now.ToShortDateString() + "'";
                        break;
                    case 2: // yesterday
                        sql += " AND " + fldDate + "='" + DateTime.Now.AddDays(-1).ToShortDateString() + "'";
                        break;
                    case 3: // through today
                        sql += " AND " + fldDate + "<='" + DateTime.Now.ToShortDateString() + "'";
                        break;
                    case 4: // tomorrow
                        sql += " AND " + fldDate + "='" + DateTime.Now.AddDays(1).ToShortDateString() + "'";
                        break;
                    case 5: //today and on
                        sql += " AND " + fldDate + ">='" + DateTime.Now.ToShortDateString() + "'";
                        break;
                    default:
                        sql += " AND " + fldDate + "'=" + DateTime.Now.ToShortDateString() + "'";
                        break;
                }
                sql += " GROUP BY H.RECEIVED";
                sql += " ORDER BY H.RECEIVED";
                /*
                    pickDateRange.Items.Add("All Dates");
                    pickDateRange.Items.Add("Today");
                    pickDateRange.Items.Add("Yesterday");
                    pickDateRange.Items.Add("Through Today");
                    pickDateRange.Items.Add("Tomorrow");
                    pickDateRange.Items.Add("Today an On");
                 */
                iParse.myParseNet.Globals.myASCLog.updateInputData(sql);
                iParse.myParseNet.Globals.myASCLog.updateSQL(sql);
                SqlConnection myConnection = new SqlConnection(iParse.myParseNet.Globals.myDBUtils.myConnString);

                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        var rec = new ASCTracFunctionStruct.CustOrder.COSMType();
                        rec.StatusID = myReader["RECEIVED"].ToString();
                        if (rec.StatusID == ascLibrary.dbConst.osRECEIVED)
                            rec.StatusDesc = "Received";
                        if (rec.StatusID == ascLibrary.dbConst.osCLOSED)
                            rec.StatusDesc = "Closed";
                        if (rec.StatusID == ascLibrary.dbConst.osCANCELLED)
                            rec.StatusDesc = "Cancelled";
                        if (rec.StatusID == ascLibrary.dbConst.osPARTIALRECEIVED)
                            rec.StatusDesc = "Partially Received";
                        if (rec.StatusID == ascLibrary.dbConst.osRECEIVING)
                            rec.StatusDesc = "Receiving";
                        if (rec.StatusID == ascLibrary.dbConst.osOPEN)
                            rec.StatusDesc = "Open";
                        if (rec.StatusID == ascLibrary.dbConst.osREJECTED)
                            rec.StatusDesc = "Rejected";
                        if (rec.StatusID == ascLibrary.dbConst.osUNRELEASED)
                            rec.StatusDesc = "Unreleased";
                        rec.numOrders = ascLibrary.ascUtils.ascStrToInt(myReader["NUM_ORDERS"].ToString(), 0);

                        rec.QtyTotal = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYNEEDED"].ToString(), 0);
                        rec.QtyCompleted = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYCOMPLETED"].ToString(), 0);
                        rec.QtyLeft = rec.QtyTotal - rec.QtyCompleted;

                        myList.Add(rec);
                    }
                }
                finally
                {
                    myConnection.Close();
                }
                if (myList.Count == 0)
                {
                    retval.successful = false;
                    retval.ErrorMessage = "No records found";
                }
                else
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = e.ToString();

                retval.successful = false;
                retval.ErrorMessage = e.Message;
                //fOK = false;
            }
            try
            {
                iParse.myParseNet.Globals.myASCLog.ProcessTran(retval.ErrorMessage, "V");
            }
            catch //(Exception e)
            {
            }
            return (retval);

        }

    }
}
