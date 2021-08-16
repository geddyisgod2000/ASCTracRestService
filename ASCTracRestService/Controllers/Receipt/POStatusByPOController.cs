using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Receipt
{
    public class POStatusByPOController : ApiController
    {
        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetPOStatusByPO(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdSIGN_ON, "POStatusByPO ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myList = GetPOStatusByPO(aInboundMsg.inputDataList[0], Convert.ToInt32(aInboundMsg.inputDataList[1]), Convert.ToInt32(aInboundMsg.inputDataList[2]));
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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

        private List<ASCTracFunctionStruct.Receipt.ConfReceiptType> GetPOStatusByPO(string aStatusList, int aDatefield, int aDateFilter)
        {
            List<ASCTracFunctionStruct.Receipt.ConfReceiptType> retval = null;
            string sql = string.Empty;
            try
            {
                sql = " H.SITE_ID='" + iParse.myParseNet.Globals.curSiteID + "'";
                if (!String.IsNullOrEmpty(aStatusList))
                    sql += " AND H.RECEIVED='" + aStatusList + "'";

                var fldDate = "CONVERT( DATE, MIN( H.EXPECTEDRECEIPTDATE))";
                if (aDatefield == 1)
                    fldDate = "CONVERT( DATE, ISNULL( MIN( D.EXPECTEDRECEIPTDATE), MIN( H.EXPECTEDRECEIPTDATE)) )";

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
                var orderBy = " ORDER BY " + fldDate + ", H.RECEIVED";

                retval = doGetPOHdrList(sql, orderBy);
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.updateSQL(sql);
                iParse.myParseNet.Globals.myASCLog.fErrorData = e.ToString();
                throw (e);
            }
            return (retval);

        }

        private List<ASCTracFunctionStruct.Receipt.ConfReceiptType> doGetPOHdrList(string aWhereStr, string aOrderByStr)
        {
            List<ASCTracFunctionStruct.Receipt.ConfReceiptType> retval = new List<ASCTracFunctionStruct.Receipt.ConfReceiptType>();
            string sql = "SELECT H.PONUMBER, H.RELEASENUM, H.VENDORID, V.VENDORNAME, H.RECEIVED, S.DESCRIPTION AS STATUS_DESCRIPTION";
            sql += ", SUM( D.QTYRECEIVED) AS TOTAL_RECV, SUM( D.QTY) AS TOTAL_QTY";
            sql += " FROM POHDR H";
            sql += " JOIN PODET D ON D.PONUMBER=H.PONUMBER AND H.RELEASENUM=D.RELEASENUM";
            sql += " LEFT JOIN RECVSTAT S ON S.STATUSID=H.RECEIVED";
            sql += " LEFT JOIN VENDOR V ON V.VENDORID=H.VENDORID";
            sql += " WHERE " + aWhereStr;
            sql += " GROUP BY H.PONUMBER, H.RELEASENUM, H.VENDORID, V.VENDORNAME, H.RECEIVED, S.DESCRIPTION ";
            sql += " " + aOrderByStr;
            try
            {
                SqlConnection myConnection = new SqlConnection(iParse.myParseNet.Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        //double qtytomake = ascLibrary.ascUtils.ascStrToDouble(myReader["QTY_TO_MAKE"].ToString(), 0);
                        //double qtymade = ascLibrary.ascUtils.ascStrToDouble(myReader["CUR_QTY"].ToString(), 0);
                        var rec = new ASCTracFunctionStruct.Receipt.ConfReceiptType();
                        rec.PONumber = myReader["PONUMBER"].ToString();
                        rec.ReleaseNum = myReader["RELEASENUM"].ToString();
                        rec.VendorID = myReader["VENDORID"].ToString();
                        rec.VendorName = myReader["VENDORNAME"].ToString();
                        rec.Status = myReader["RECEIVED"].ToString();
                        rec.Status_Description = myReader["STATUS_DESCRIPTION"].ToString();
                        double qty = ascLibrary.ascUtils.ascStrToDouble(myReader["TOTAL_QTY"].ToString(), 0);
                        if (qty <= 0)
                            rec.Percent_Complete = 0;
                        else
                            rec.Percent_Complete = (ascLibrary.ascUtils.ascStrToDouble(myReader["TOTAL_RECV"].ToString(), 0) / qty) * 100;

                        retval.Add(rec);
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.updateSQL(sql);
                iParse.myParseNet.Globals.myASCLog.fErrorData = e.ToString();
                throw (e);
            }
            return (retval);
        }

    }
}
