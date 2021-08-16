using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ASCTracRestService.Controllers.Production
{
    public class WOHdr 
    {

        //==================================================================================
        public string ScheduleWOHdr(ASCTracFunctionStruct.Production.WOHdrType aSchedWORec, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            try
            {
                //ascLibrary.TDBReturnType rettype = ascLibrary.TDBReturnType.dbrtOK;
                string woStatus = string.Empty;
                if (!Globals.myGetInfo.GetWOHdrInfo(aSchedWORec.Workorder_ID, "STATUS", ref woStatus))
                    retval = "Work Order " + aSchedWORec.Workorder_ID + " does not exist.";
                else if (woStatus == ascLibrary.dbConst.plCANCELLED)
                    retval = "Work Order " + aSchedWORec.Workorder_ID + " has been cancelled.";
                else if (woStatus == ascLibrary.dbConst.plFINISHED)
                    retval = "Work Order " + aSchedWORec.Workorder_ID + " already completed.";
                else
                {
                    string updStr = "STATUS='" + aSchedWORec.Status + "'";
                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "SCHED_DATETIME", aSchedWORec.Sched_Datetime.ToString());
                    //ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "ASSETID", aSchedWORec.Sched_Datetime.ToString);
                    //ascLibrary.ascStrUtils.ascAppendSetStr( ref updStr, "CUR_LOT", aSchedWORec.Sched_Datetime.ToString);

                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "DURATION", aSchedWORec.Duration.ToString());
                    if (aSchedWORec.Duration > 0)
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "SCHED_END_DATETIME", aSchedWORec.Sched_Datetime.AddMinutes(aSchedWORec.Duration).ToString());
                    if (!String.IsNullOrEmpty(aSchedWORec.Prodline))
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "PRODLINE", aSchedWORec.Prodline);
                    if (aSchedWORec.ExpDate.CompareTo(DateTime.Now) > 0)
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "EXP_DATE", aSchedWORec.ExpDate.ToShortDateString());
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "ASSETID", aSchedWORec.AssetID);
                    ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updStr, "CUR_LOT", aSchedWORec.LotID);
                    /*
                          if ( dtExpDate.Text <> '' ) and ( StrToDate( dtExpDate.Text) > ( Date + 1)) then
                            ascAppendSetStr( tmp, 'EXP_DATE', dtExpDate.Text)
                          else
                            ascAppendSetQty( tmp, 'EXP_DATE', 'NULL');

                          if ASCTracConfig.iniPPExpDateAllowedAtHdr.Value then
                          begin
                            if( dtMinExpDate.Text = '' ) then  
                              ascAppendSetQty( tmp, 'MIN_EXP_DATE', 'NULL')
                            else
                              ascAppendSetStr( tmp, 'MIN_EXP_DATE', dtMinExpDate.Text);
                          end;
                     */
                    Globals.mydmupdate.UpdateFields("WO_HDR", updStr, "WORKORDER_ID='" + aSchedWORec.Workorder_ID + "'");
                    if (woStatus != aSchedWORec.Status)
                    {
                        if (aSchedWORec.Status == ascLibrary.dbConst.plACTIVE)
                            retval = Globals.dmProd.startwo(aSchedWORec.Workorder_ID);
                        else if (aSchedWORec.Status == ascLibrary.dbConst.plPREPARING)
                            retval = Globals.dmProd.preparewo(aSchedWORec.Workorder_ID);
                    }
                    if (String.IsNullOrEmpty(retval))
                        Globals.mydmupdate.ProcessUpdates();
                }
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                retval = e.Message;
            }
            try
            {
                if (!String.IsNullOrEmpty(retval))
                    Globals.myASCLog.ProcessTran(retval, "E");
            }
            catch //(Exception e)
            {
            }
            return (retval);
        }


        //=======================================
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetWOStatusSummary(string aStatusList, string aProdLineRange, int aDatefield, int aDateFilter, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            List<ASCTracFunctionStruct.CustOrder.COSMType> myList = new List<ASCTracFunctionStruct.CustOrder.COSMType>();
            //bool fOK = true;
            try
            {
                string sql = "SELECT H.STATUS, COUNT( DISTINCT( H.WORKORDER_ID)) AS NUM_ORDERS";
                sql += ", sum( H.QTY_TO_MAKE) AS QTYNEEDED, SUM( H.CUR_QTY) AS QTYCOMPLETED";
                sql += " FROM WO_HDR H";
                sql += " WHERE H.SITE_ID='" + Globals.curSiteID + "'";
                if (!String.IsNullOrEmpty(aStatusList))
                {
                    var comma = "";
                    var statuslist = aStatusList;
                    sql += " AND H.STATUS IN ( ";
                    while (statuslist != "")
                    {
                        sql += comma + "'" + ascLibrary.ascStrUtils.ascGetNextWord(ref statuslist, ",") + "'";
                        comma = ",";
                    }
                    sql += ")";
                }
                if (!String.IsNullOrEmpty(aProdLineRange))
                {
                    string[] prodlines = aProdLineRange.Split(new Char[] { '|' });
                    if (prodlines.Length > 0)
                    {
                        if (prodlines.Length == 1)
                            sql += " and H.PRODLINE='" + prodlines[0] + "'";
                        else
                            sql += " and H.PRODLINE>='" + prodlines[0] + "' and H.PRODLINE<='" + prodlines[1] + "'";
                    }
                }
                // pickDateField.Items.Add("Schedule Date");
                var fldDate = "CONVERT( DATE, H.SCHED_DATETIME )";
                if (aDatefield == 1)
                    fldDate = "CONVERT( DATE, H.TARGET_COMPLETION_DATE )";

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
                sql += " GROUP BY H.STATUS";
                sql += " ORDER BY H.STATUS";
                /*
                    pickDateRange.Items.Add("All Dates");
                    pickDateRange.Items.Add("Today");
                    pickDateRange.Items.Add("Yesterday");
                    pickDateRange.Items.Add("Through Today");
                    pickDateRange.Items.Add("Tomorrow");
                    pickDateRange.Items.Add("Today an On");
                 */
                Globals.myASCLog.updateInputData(sql);
                Globals.myASCLog.updateSQL(sql);
                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);

                /*
                DataTable myDT = new DataTable();
                SqlDataAdapter myDA = new SqlDataAdapter(sql, myConnection);
                myConnection.Open();
                try
                {
                    myDA.MissingSchemaAction = MissingSchemaAction.Add;
                    myDA.Fill(myDT); //, "PRODLINE");

                    var mydata = myDT.AsEnumberable();


                    retval = myDS.Tables[0].AS
                }
                finally
                {
                    myConnection.Close();
                }
                 * */

                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        var rec = new ASCTracFunctionStruct.CustOrder.COSMType();
                        rec.StatusID = myReader["STATUS"].ToString();
                        if (rec.StatusID == ascLibrary.dbConst.plACTIVE)
                            rec.StatusDesc = "Active";
                        if (rec.StatusID == ascLibrary.dbConst.plCANCELLED)
                            rec.StatusDesc = "Cancelled";
                        if (rec.StatusID == ascLibrary.dbConst.plFINISHED)
                            rec.StatusDesc = "Completed";
                        if (rec.StatusID == ascLibrary.dbConst.plNOTSCHEDULED)
                            rec.StatusDesc = "Not Scheduled";
                        if (rec.StatusID == ascLibrary.dbConst.plPENDING)
                            rec.StatusDesc = "Pending";
                        if (rec.StatusID == ascLibrary.dbConst.plPREPARING)
                            rec.StatusDesc = "Preparing";
                        if (rec.StatusID == ascLibrary.dbConst.plSCHEDULED)
                            rec.StatusDesc = "Scheduled";
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
                Globals.myASCLog.fErrorData = e.ToString();

                retval.successful = false;
                retval.ErrorMessage = e.Message;
                //fOK = false;
            }
            try
            {
                //if (!fOK)
                /*
                if (!retval[0].successful)
                {
                    //myParseNet.Globals.myASCLog.fErrorData = retval[0].ReturnMessage;
                    myParseNet.Globals.myASCLog.ProcessTran(retval[0].ReturnMessage, "E");
                }
                else
                 */
                Globals.myASCLog.ProcessTran(retval.ErrorMessage, "V");
            }
            catch //(Exception e)
            {
            }
            return (retval);
        }


        // WO Details for WOSM
        public List<ASCTracFunctionStruct.Production.WOHdrType> GetWOStatusByWO(string aStatusList, string aProdLineRange, int aDatefield, int aDateFilter, ParseNet.GlobalClass Globals)
        {
            List<ASCTracFunctionStruct.Production.WOHdrType> retval = new List<ASCTracFunctionStruct.Production.WOHdrType>();
            string sql = string.Empty;
            try
            {
                sql = " H.SITE_ID='" + Globals.curSiteID + "'";
                if (!String.IsNullOrEmpty(aStatusList))
                {
                    var comma = "";
                    var statuslist = aStatusList;
                    sql += " AND H.STATUS IN ( ";
                    while (statuslist != "")
                    {
                        sql += comma + "'" + ascLibrary.ascStrUtils.ascGetNextWord(ref statuslist, ",") + "'";
                        comma = ",";
                    }
                    sql += ")";
                }
                if (!String.IsNullOrEmpty(aProdLineRange))
                {
                    string[] prodlines = aProdLineRange.Split(new Char[] { '|' });
                    if (prodlines.Length > 0)
                    {
                        if (prodlines.Length == 1)
                            sql += " and H.PRODLINE='" + prodlines[0] + "'";
                        else
                            sql += " and H.PRODLINE>='" + prodlines[0] + "' and H.PRODLINE<='" + prodlines[1] + "'";
                    }
                }
                // pickDateField.Items.Add("Schedule Date");
                var fldDate = "CONVERT( DATE, H.SCHED_DATETIME )";
                if (aDatefield == 1)
                    fldDate = "CONVERT( DATE, H.TARGET_COMPLETION_DATE )";

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
                sql += " ORDER BY " + fldDate + ", STATUS";

                ASCTracFunctionsData.Production.WOHdr myWOHdr = new ASCTracFunctionsData.Production.WOHdr();
                retval =  myWOHdr.doGetWOHdrList(sql, Globals);
            }
            catch (Exception e)
            {
                Globals.myASCLog.updateSQL(sql);
                Globals.myASCLog.fErrorData = e.ToString();
                throw( e);
            }
            return (retval);
        }

    }
}
