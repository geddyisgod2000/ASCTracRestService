using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Web;

namespace ASCTracRestService.Models
{
    internal class GetStatusLists
    {
        internal static Dictionary<string, ASCTracFunctionStruct.OrdStatusType> GetPOStatusList(string aUserID, string aSiteID)
        {
            var retval = new Dictionary<string, ASCTracFunctionStruct.OrdStatusType>();
            string tmp = string.Empty;
            string validStatus = "'" + ascLibrary.dbConst.osOPEN + "','" + ascLibrary.dbConst.osPARTIALRECEIVED + "','" + ascLibrary.dbConst.osRECEIVING + "'";
            validStatus += ",'" + ascLibrary.dbConst.osREJECTED + "'"; //,'" + ascLibrary.dbConst.osRECEIVED + "'";
            string sql = "SELECT COUNT( PONUMBER) as NUMRECS FROM POHDR WHERE SITE_ID='" + aSiteID + "'";
            sql += " AND ( RECEIVED IN ( " + validStatus + ") OR RECEIVEDDATE>='" + DateTime.Now.ToShortDateString() + "')";
            iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmp);

            long totalCount = ascLibrary.ascUtils.ascStrToInt(tmp, 1);

            sql = "SELECT RECEIVED, COUNT( PONUMBER) as NUMRECS FROM POHDR WHERE SITE_ID='" + aSiteID + "'";
            sql += " AND ( RECEIVED IN ( " + validStatus + ") OR RECEIVEDDATE>='" + DateTime.Now.ToShortDateString() + "')";
            sql += " GROUP BY RECEIVED";

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

                        long statusCount = ascLibrary.ascUtils.ascStrToInt(myReader["NUMRECS"].ToString(), 0);
                        double statusPerc = (statusCount * 100) / totalCount;
                        string statusDesc = string.Empty;
                        if (myReader["RECEIVED"].ToString().Equals(ascLibrary.dbConst.osOPEN))
                            statusDesc = "Open";
                        if (myReader["RECEIVED"].ToString().Equals(ascLibrary.dbConst.osPARTIALRECEIVED))
                            statusDesc = "Partial";
                        if (myReader["RECEIVED"].ToString().Equals(ascLibrary.dbConst.osRECEIVING))
                            statusDesc = "Receiving";
                        if (myReader["RECEIVED"].ToString().Equals(ascLibrary.dbConst.osRECEIVED))
                            statusDesc = "Received";
                        if (myReader["RECEIVED"].ToString().Equals(ascLibrary.dbConst.osREJECTED))
                            statusDesc = "Rejected";
                        if (myReader["RECEIVED"].ToString().Equals(ascLibrary.dbConst.osCLOSED))
                            statusDesc = "Closed";
                        if (!String.IsNullOrEmpty(statusDesc))
                        {
                            iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT FG_COLOR,BG_COLOR FROM COLOR WHERE COLORNAME='" + statusDesc + "' AND USERID='" + aUserID + "'", "", ref tmp);
                            var rec = new ASCTracFunctionStruct.OrdStatusType();
                            rec.Description = statusDesc;
                            rec.NumOrders = statusCount;
                            rec.TotalOrders = totalCount;
                            rec.Percent = statusPerc;
                            Color myColor;// = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            if (string.IsNullOrEmpty(tmp) || statusDesc.Equals("Partial"))
                                myColor = Color.Black;
                            else
                                myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.FGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            //rec.StatusColor = "#" + myColor.B.ToString("X2") + myColor.G.ToString("X2") + myColor.R.ToString("X2");
                            if (string.IsNullOrEmpty(tmp) || statusDesc.Equals("Partial"))
                                myColor = Color.LightGreen;
                            else
                                myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.BGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            retval.Add(statusDesc, rec);
                        }
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = "GetPOStatusList" + e.ToString();
            }
            return (retval);
        }

        internal static Dictionary<string, ASCTracFunctionStruct.OrdStatusType> GetWOStatusList(string aUserID, string aSiteID)
        {
            var retval = new Dictionary<string, ASCTracFunctionStruct.OrdStatusType>();
            string tmp = string.Empty;
            string validStatus = "'" + ascLibrary.dbConst.plACTIVE + "','" + ascLibrary.dbConst.plNOTSCHEDULED + "','" + ascLibrary.dbConst.plPENDING + "'";
            validStatus += ",'" + ascLibrary.dbConst.plPREPARING + "'"; //,'" + ascLibrary.dbConst.osRECEIVED + "'";
            string sql = "SELECT COUNT( WORKORDER_ID) as NUMRECS FROM WO_HDR WHERE SITE_ID='" + aSiteID + "'";
            sql += " AND ( STATUS IN ( " + validStatus + ") OR COMPLETE_DATETIME>='" + DateTime.Now.ToShortDateString() + "')";
            iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmp);

            long totalCount = ascLibrary.ascUtils.ascStrToInt(tmp, 1);

            sql = "SELECT STATUS,COUNT( WORKORDER_ID) as NUMRECS FROM WO_HDR WHERE SITE_ID='" + aSiteID + "'";
            sql += " AND ( STATUS IN ( " + validStatus + ") OR COMPLETE_DATETIME>='" + DateTime.Now.ToShortDateString() + "')";
            sql += " GROUP BY STATUS";

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

                        long statusCount = ascLibrary.ascUtils.ascStrToInt(myReader["NUMRECS"].ToString(), 0);
                        double statusPerc = (statusCount * 100) / totalCount;
                        string statusDesc = string.Empty;
                        if (myReader["STATUS"].ToString().Equals(ascLibrary.dbConst.plACTIVE))
                            statusDesc = "Active";
                        if (myReader["STATUS"].ToString().Equals(ascLibrary.dbConst.plFINISHED))
                            statusDesc = "Completed";
                        if (myReader["STATUS"].ToString().Equals(ascLibrary.dbConst.plNOTSCHEDULED))
                            statusDesc = "NotScheduled";
                        if (myReader["STATUS"].ToString().Equals(ascLibrary.dbConst.plPENDING))
                            statusDesc = "Pending";
                        if (myReader["STATUS"].ToString().Equals(ascLibrary.dbConst.plPREPARING))
                            statusDesc = "Preparing";
                        if (myReader["STATUS"].ToString().Equals(ascLibrary.dbConst.plSCHEDULED))
                            statusDesc = "Scheduled";
                        if (!String.IsNullOrEmpty(statusDesc))
                        {
                            iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT FG_COLOR,BG_COLOR FROM COLOR WHERE COLORNAME='" + statusDesc + "' AND USERID='" + aUserID + "'", "", ref tmp);
                            var rec = new ASCTracFunctionStruct.OrdStatusType();
                            rec.Description = statusDesc;
                            rec.NumOrders = statusCount;
                            rec.TotalOrders = totalCount;
                            rec.Percent = statusPerc;
                            Color myColor;// = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            if (string.IsNullOrEmpty(tmp) || statusDesc.Equals("Partial"))
                                myColor = Color.Black;
                            else
                                myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.FGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            //rec.StatusColor = "#" + myColor.B.ToString("X2") + myColor.G.ToString("X2") + myColor.R.ToString("X2");
                            if (string.IsNullOrEmpty(tmp) || statusDesc.Equals("Partial"))
                                myColor = Color.LightGreen;
                            else
                                myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.BGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            retval.Add(statusDesc, rec);
                        }
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = "GetWOStatusList" + e.ToString();
            }

            return (retval);
        }

        internal static Dictionary<string, ASCTracFunctionStruct.OrdStatusType> GetCOStatusList(string aUserID, string aSiteID)
        {
            var retval = new Dictionary<string, ASCTracFunctionStruct.OrdStatusType>();
            string tmp = string.Empty;
            string validStatus = "'" + ascLibrary.dbConst.ssLOADING + "','" + ascLibrary.dbConst.ssLOADINGDOCK + "','" + ascLibrary.dbConst.ssNO_INVENTORY + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssNOTSCHED + "'"; //,'" + ascLibrary.dbConst.osRECEIVED + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssONTRUCK + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssPARTIAL + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssPICK_COMPLETE + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssPICKING + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssSCHEDULED + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssUNLOADING + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssUNLOCKED + "'";
            validStatus += ",'" + ascLibrary.dbConst.ssWEB_PENDING + "'";
            string sql = "SELECT COUNT( ORDERNUMBER) as NUMRECS FROM ORDRHDR WHERE SITE_ID='" + aSiteID + "'";
            sql += " AND ( PICKSTATUS IN ( " + validStatus + ") OR SCHEDDATE>='" + DateTime.Now.ToShortDateString() + "')";
            iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmp);

            long totalCount = ascLibrary.ascUtils.ascStrToInt(tmp, 1);

            sql = "SELECT PICKSTATUS, COUNT( ORDERNUMBER) as NUMRECS FROM ORDRHDR WHERE SITE_ID='" + aSiteID + "'";
            sql += " AND ( PICKSTATUS IN ( " + validStatus + ") OR SCHEDDATE>='" + DateTime.Now.ToShortDateString() + "')";
            sql += " GROUP BY PICKSTATUS";

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
                        string colorname = string.Empty;
                        long statusCount = ascLibrary.ascUtils.ascStrToInt(myReader["NUMRECS"].ToString(), 0);
                        double statusPerc = (statusCount * 100) / totalCount;
                        string statusDesc = string.Empty;
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssCONF_SHIP))
                        {
                            colorname = "ConfirmShip";
                            statusDesc = "Shipped";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssLOADINGDOCK))
                        {
                            colorname = "OnDock";
                            statusDesc = "Completed";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssONTRUCK))
                        {
                            colorname = "OnDock";
                            statusDesc = "Completed";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssPICK_COMPLETE))
                        {
                            colorname = "OnDock";
                            statusDesc = "Completed";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssNO_INVENTORY))
                        {
                            colorname = "NotSched";
                            statusDesc = "Not Started";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssNOTSCHED))
                        {
                            colorname = "NotSched";
                            statusDesc = "Not Started";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssLOADING))
                        {
                            colorname = "BeingPicked";
                            statusDesc = "In Process";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssPARTIAL))
                        {
                            colorname = "BeingPicked";
                            statusDesc = "In Process";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssPICKING))
                        {
                            colorname = "BeingPicked";
                            statusDesc = "In Process";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssSCHEDULED))
                        {
                            statusDesc = "Scheduled";
                            colorname = "Scheduled";
                        }
                        if (myReader["PICKSTATUS"].ToString().Equals(ascLibrary.dbConst.ssUNLOADING))
                        {
                            statusDesc = "Unloading";
                            colorname = "Unloading";
                        }
                        if (!String.IsNullOrEmpty(statusDesc))
                        {
                            if (retval.ContainsKey(statusDesc))
                            {
                                retval[statusDesc].NumOrders += statusCount;
                                retval[statusDesc].Percent = (retval[statusDesc].NumOrders * 100) / totalCount;
                            }
                            else
                            {
                                iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT FG_COLOR,BG_COLOR FROM COLOR WHERE COLORNAME='" + statusDesc + "' AND USERID='" + aUserID + "'", "", ref tmp);
                                var rec = new ASCTracFunctionStruct.OrdStatusType();
                                rec.Description = statusDesc;
                                rec.NumOrders = statusCount;
                                rec.TotalOrders = totalCount;
                                rec.Percent = statusPerc;
                                Color myColor;// = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                                if (string.IsNullOrEmpty(tmp) || statusDesc.Equals("Partial"))
                                    myColor = Color.Black;
                                else
                                    myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                                rec.FGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                                //rec.StatusColor = "#" + myColor.B.ToString("X2") + myColor.G.ToString("X2") + myColor.R.ToString("X2");
                                if (string.IsNullOrEmpty(tmp) || statusDesc.Equals("Partial"))
                                    myColor = Color.LightGreen;
                                else
                                    myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                                rec.BGStatusColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                                retval.Add(statusDesc, rec);
                            }
                        }
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = "GetCOStatusList " + e.ToString();
            }

            return (retval);
        }


    }
}