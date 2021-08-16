using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ASCTracRestService.Controllers.COSM
{
    class COSM
    {
        private string BuildCOSMWhere(string aStatusList, string aDockRange, DateTime aDate, int aDatefield, int aDateFilter, string aSiteID)
        {
            string retval = " H.SITE_ID='" + aSiteID + "'";
            if (!String.IsNullOrEmpty(aStatusList))
            {
                var comma = "";
                var statuslist = aStatusList;
                retval += " AND H.PICKSTATUS IN ( ";
                while (statuslist != "")
                {
                    retval += comma + "'" + ascLibrary.ascStrUtils.ascGetNextWord(ref statuslist, ",") + "'";
                    comma = ",";
                }
                retval += ")";
            }
            if (!String.IsNullOrEmpty(aDockRange))
            {
                string[] docks = aDockRange.Split(new Char[] { '|' });
                if (docks.Length > 0)
                {
                    if (docks.Length == 1)
                        retval += " and H.ORDERNUMBER IN ( SELECT CO_ORDERNUM FROM DOCKSCHD WHERE SITE_ID='" + aSiteID + "' AND LOADINGBAY='" + docks[0] + "')";
                    else
                        retval += " and H.ORDERNUMBER IN ( SELECT CO_ORDERNUM FROM DOCKSCHD WHERE SITE_ID='" + aSiteID + "' AND LOADINGBAY>='" + docks[0] + "' AND LOADINGBAY<='" + docks[1] + "')";
                }
            }
            // pickDateField.Items.Add("Schedule Date");
            var fldDate = "CONVERT( DATE, H.SCHEDDATE )";
            if (aDatefield == 1)
                fldDate = "CONVERT( DATE, H.REQUIREDSHIPDATE )";
            if (aDatefield == 2)
                fldDate = "CONVERT( DATE, H.SCHEDCOMPLETIONDATE )";

            /*
pickDateRange.Items.Add("All Dates");
pickDateRange.Items.Add("Today");
pickDateRange.Items.Add("Yesterday");
pickDateRange.Items.Add("Through Today");
pickDateRange.Items.Add("Through Yesterday");
pickDateRange.Items.Add("Tomorrow");
pickDateRange.Items.Add("Today an On");
pickDateRange.Items.Add("Tomorrow an On");
 */

            switch (aDateFilter)
            {
                case 0: // all dates
                    break;
                case 1: // today
                    retval += " AND " + fldDate + "='" + aDate.ToShortDateString() + "'";
                    break;
                case 2: // yesterday
                    retval += " AND " + fldDate + "='" + aDate.AddDays(-1).ToShortDateString() + "'";
                    break;
                case 3: // through today
                    retval += " AND " + fldDate + "<='" + aDate.ToShortDateString() + "'";
                    break;
                case 4: // through yesterday
                    retval += " AND " + fldDate + "<='" + aDate.AddDays(-1).ToShortDateString() + "'";
                    break;
                case 5: // tomorrow
                    retval += " AND " + fldDate + "='" + aDate.AddDays(1).ToShortDateString() + "'";
                    break;
                case 6: //today and on
                    retval += " AND " + fldDate + ">='" + aDate.ToShortDateString() + "'";
                    break;
                case 7: //tomorrow and on
                    retval += " AND " + fldDate + ">='" + aDate.AddDays(1).ToShortDateString() + "'";
                    break;
                default:
                    retval += " AND " + fldDate + "'=" + aDate.ToShortDateString() + "'";
                    break;
            }
            return (retval);
        }


        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOStatusSummary(string aStatusList, string aDockRange, int aDatefield, int aDateFilter, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            List<ASCTracFunctionStruct.CustOrder.COSMType> myList = new List<ASCTracFunctionStruct.CustOrder.COSMType>();
            //bool fOK = true;
            try
            {
                string sql = "SELECT H.PICKSTATUS, SS.DESCRIPTION, COUNT( DISTINCT( H.ORDERNUMBER)) AS NUM_ORDERS";
                sql += ", sum( D.QTYORDERED) AS QTYNEEDED, SUM( D.QTYPICKED) AS QTYCOMPLETED, SUM( D.QTYLOADED) AS QTYLOADED";
                sql += " FROM ORDRHDR H";
                sql += " JOIN ORDRDET D ON D.ORDERNUMBER=H.ORDERNUMBER";
                sql += " LEFT JOIN SHIPSTAT SS ON SS.STATUSID=H.PICKSTATUS";
                sql += " WHERE " + BuildCOSMWhere(aStatusList, aDockRange, DateTime.Now, aDatefield, aDateFilter, Globals.curSiteID);
                sql += " GROUP BY H.PICKSTATUS, SS.DESCRIPTION";
                sql += " ORDER BY H.PICKSTATUS";
                /*
                pickDateRange.Items.Add("All Dates");
                pickDateRange.Items.Add("Today");
                pickDateRange.Items.Add("Yesterday");
                pickDateRange.Items.Add("Through Today");
                pickDateRange.Items.Add("Through Yesterday");
                pickDateRange.Items.Add("Tomorrow");
                pickDateRange.Items.Add("Today an On");
                pickDateRange.Items.Add("Tomorrow an On");
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
                        rec.StatusID = myReader["PICKSTATUS"].ToString();
                        rec.StatusDesc = myReader["DESCRIPTION"].ToString();
                        rec.numOrders = ascLibrary.ascUtils.ascStrToInt(myReader["NUM_ORDERS"].ToString(), 0);
                        rec.QtyTotal = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYNEEDED"].ToString(), 0);
                        rec.QtyCompleted = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYCOMPLETED"].ToString(), 0);
                        rec.QtyLeft = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYLOADED"].ToString(), 0); //rec.QtyTotal - rec.QtyCompleted;

                        myList.Add(rec);
                    }
                }
                finally
                {
                    myConnection.Close();
                }
                if (myList.Count == 0)
                {
                    //throw new Exception("No Records Found " + sql);
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

        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOStatusByCO(string aStatusList, string aDockRange, DateTime aDate, int aDatefield, int aDateFilter, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            List<ASCTracFunctionStruct.CustOrder.DockType> myList = new List<ASCTracFunctionStruct.CustOrder.DockType>();
            //bool fOK = true;
            try
            {
                string sql = "SELECT 'C' AS ASC_ORDERTYPE,H.PICKSTATUS AS ASC_STATUS, SS.DESCRIPTION, H.ORDERNUMBER, H.SHIPTOCUSTID, H.SHIPTONAME,";
                sql += " H.REQUIREDSHIPDATE, H.SCHEDDATE, H.SCHEDTTIME,";
                sql += " SUM( D.QTYORDERED) AS QTYORDERED, SUM( D.QTYPICKED ) AS QTYPICKED";
                sql += " FROM ORDRHDR H";
                sql += " JOIN ORDRDET D ON D.ORDERNUMBER=H.ORDERNUMBER";
                sql += " LEFT JOIN SHIPSTAT SS ON SS.STATUSID=H.PICKSTATUS";
                sql += " WHERE " + BuildCOSMWhere(aStatusList, aDockRange, aDate, aDatefield, aDateFilter, Globals.curSiteID);
                sql += " GROUP BY H.PICKSTATUS, SS.DESCRIPTION, H.ORDERNUMBER, H.SHIPTOCUSTID, H.SHIPTONAME,";
                sql += " H.REQUIREDSHIPDATE, H.SCHEDDATE, H.SCHEDTTIME";
                sql += " ORDER BY ASC_STATUS";
                Globals.myASCLog.updateInputData(sql);
                Globals.myASCLog.updateSQL(sql);
                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);


                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        var rec = new ASCTracFunctionStruct.CustOrder.DockType();
                        rec.PickStatus = myReader["ASC_STATUS"].ToString();
                        rec.PickStatus_Description = myReader["DESCRIPTION"].ToString();
                        rec.CustID = myReader["SHIPTOCUSTID"].ToString();
                        rec.CustName = myReader["SHIPTONAME"].ToString();
                        rec.CustIDAndName = rec.CustID + " - " + rec.CustName;
                        rec.OrderNumber = myReader["ORDERNUMBER"].ToString();
                        rec.OrderType = myReader["ASC_ORDERTYPE"].ToString(); // "C";
                        double dtmp = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYORDERED"].ToString(), 0);
                        if (dtmp > 0)
                            dtmp = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYPICKED"].ToString(), 0) / dtmp;
                        if (dtmp > 1)
                            dtmp = 100;
                        else
                            dtmp = dtmp * 100;
                        rec.Percent_Complete = dtmp;
                        rec.Sched_Datetime = ascLibrary.ascUtils.ascStrToDate(myReader["SCHEDDATE"].ToString(), DateTime.MinValue).Date;
                        if (rec.Sched_Datetime != DateTime.MinValue)
                        {
                            DateTime dtTmp = ascLibrary.ascUtils.ascStrToDate(myReader["SCHEDTTIME"].ToString(), DateTime.MinValue);

                            if (rec.Sched_Datetime != DateTime.MinValue)
                                rec.Sched_Datetime = rec.Sched_Datetime.Add(dtTmp.TimeOfDay);
                        }
                        rec.requiredShipDate = ascLibrary.ascUtils.ascStrToDate(myReader["REQUIREDSHIPDATE"].ToString(), DateTime.MinValue);

                        string colorname = "Scheduled";
                        if (rec.PickStatus.Equals(ascLibrary.dbConst.ssNOTSCHED))
                            colorname = "NotScheduled";
                        if (rec.PickStatus.Equals(ascLibrary.dbConst.ssPICKING))
                            colorname = "BeingPicked";
                        if (rec.PickStatus.Equals(ascLibrary.dbConst.ssPARTIAL))
                            colorname = "PartPick";
                        if (rec.PickStatus.Equals(ascLibrary.dbConst.ssLOADINGDOCK))
                            colorname = "OnDock";
                        if (rec.PickStatus.Equals(ascLibrary.dbConst.ssLOADING))
                            colorname = "Loading";
                        if (rec.PickStatus.Equals(ascLibrary.dbConst.ssONTRUCK))
                            colorname = "OnTruck";
                        string tmp = string.Empty;
                        if (Globals.myDBUtils.ReadFieldFromDB("SELECT FG_COLOR, BG_COLOR FROM COLOR WHERE COLORNAME='" + colorname + "' AND USERID='" + Globals.curUserID + "'", "", ref tmp))
                        {
                            Color myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.FGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.BGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                        }
                        else
                        {
                            Color myColor = Color.Black;
                            rec.FGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            myColor = Color.WhiteSmoke;
                            rec.BGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                        }

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
                    retval.ErrorMessage= "No records found";
                }
                else
                {
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
                }
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

    }
}
