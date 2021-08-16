using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ASCTracRestService.Controllers.Replen
{
    class Replen
    {
        /*
         * ZoneFilter = if "-", then range, "," then setup, else equal
         * FilterType, N=None, S=QtyScheduled>QtyinLoc, O=QtyOrdered>QtyInLoc
         * 
         */

        public ASCTracFunctionStruct.ascBasicReturnMessageType GetReplenSummary(string aZoneFilter, string aFilterType, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string sql = "SELECT L.ZONEID, Z.DESCRIPTION, COUNT( L.LOCATIONID) AS NUMLOCS" +
                    ", SUM( CASE WHEN L.REPL_QTYTOPICK > 0 THEN 1 WHEN REPL_MIN_QTY>ISNULL( QTY_IN_LOC,0) THEN 1 ELSE 0 END) AS NUM_NEEDED" +
                    ", SUM( CASE WHEN L.REPL_QTYTOPICK > 0 THEN L.REPL_QTYTOPICK WHEN REPL_MIN_QTY>ISNULL( QTY_IN_LOC,0) THEN REPL_MIN_QTY - ISNULL( QTY_IN_LOC,0) ELSE 0 END) AS QTY_NEEDED" +
                    " FROM LOC L" +
                    " LEFT JOIN ITEMQTY IQ ON IQ.ASCITEMID=L.ASCITEMID" +
                    " LEFT JOIN ZONES Z ON Z.ZONEID=L.ZONEID AND Z.SITE_ID=L.SITE_ID" +
                    " WHERE L.SITE_ID='" + Globals.curSiteID + "' AND L.TYPE='E'";
                if (!String.IsNullOrEmpty(aZoneFilter))
                {
                    string ZoneFilter = aZoneFilter;
                    if (ZoneFilter.Contains("-"))
                    {
                        sql += " and L.ZONEID>='" + ascLibrary.ascStrUtils.ascGetNextWord(ref ZoneFilter, "-") + "'";
                        sql += " and L.ZONEID<='" + ascLibrary.ascStrUtils.ascGetNextWord(ref ZoneFilter, "-") + "'";
                    }
                    else if (ZoneFilter.Contains(","))
                        sql += " AND L.ZONEID IN ( '" + ZoneFilter.Replace(" ", "").Replace(",", "','") + "')";
                    else
                        sql += " AND L.ZONEID = '" + ZoneFilter + "'";
                }
                if (aFilterType.Equals("S"))
                {
                    sql += " AND IQ.QTYSCHEDULED > L.QTY_IN_LOC";
                }
                if (aFilterType.Equals("O"))
                {
                    sql += " AND ( IQ.QTYSCHEDULED + IQ.QTYREQUIRED) > L.QTY_IN_LOC";
                }
                sql += " GROUP BY L.ZONEID, Z.DESCRIPTION" +   
                    " ORDER BY L.ZONEID";
                List<ASCTracFunctionStruct.Inventory.ReplenSummType> myList = new List<ASCTracFunctionStruct.Inventory.ReplenSummType>();
                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        myList.Add( new ASCTracFunctionStruct.Inventory.ReplenSummType( myReader["ZONEID"].ToString(), 
                            myReader["DESCRIPTION"].ToString(), 
                            ascLibrary.ascUtils.ascStrToInt( myReader["NUMLOCS"].ToString(), 0), 
                            ascLibrary.ascUtils.ascStrToInt( myReader["NUM_NEEDED"].ToString(), 0), 
                            ascLibrary.ascUtils.ascStrToDouble( myReader["QTY_NEEDED"].ToString(), 0)));
                    }
                }
                finally
                {
                    myConnection.Close();
                }

                if (myList.Count == 0)
                {
                    retval.successful = false;
                    retval.ErrorMessage = "No Replenishment Locations Found";
                }
                else
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);

            }
            catch (Exception e)
            {
                retval.successful = false;
                retval.ErrorMessage = e.Message;
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);
        }

        public ASCTracFunctionStruct.ascBasicReturnMessageType GetReplenInfoForZone(string aZoneID, string aFilterType, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string sql = "SELECT LOCATIONID, LOCATIONDESCRIPTION, REPL_MAX_QTY, REPL_MIN_QTY, REPL_QTYTOPICK";
                sql += " FROM LOC L" +
                    " LEFT JOIN ITEMQTY IQ ON IQ.ASCITEMID=L.ASCITEMID" +
                    " WHERE L.SITE_ID='" + Globals.curSiteID + "' AND L.TYPE='E' AND L.ZONEID='" + aZoneID + "'";
                if (aFilterType.Equals("S"))
                {
                    sql += " AND IQ.QTYSCHEDULED > L.QTY_IN_LOC";
                }
                if (aFilterType.Equals("O"))
                {
                    sql += " AND ( IQ.QTYSCHEDULED + IQ.QTYREQUIRED) > L.QTY_IN_LOC";
                }

                    sql += " ORDER BY L.LOCATIONID";
                List<ASCTracFunctionStruct.Inventory.ReplenLocType> myList = new List<ASCTracFunctionStruct.Inventory.ReplenLocType>();
                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        myList.Add(new ASCTracFunctionStruct.Inventory.ReplenLocType(myReader["LOCATIONID"].ToString(),
                            myReader["LOCATIONDESCRIPTION"].ToString(),
                            ascLibrary.ascUtils.ascStrToInt(myReader["REPL_MIN_QTY"].ToString(), 0),
                            ascLibrary.ascUtils.ascStrToInt(myReader["REPL_MAX_QTY"].ToString(), 0),
                            ascLibrary.ascUtils.ascStrToDouble(myReader["REPL_QTYTOPICK"].ToString(), 0)));
                    }
                }
                finally
                {
                    myConnection.Close();
                }

                if (myList.Count == 0)
                {
                    retval.successful = false;
                    retval.ErrorMessage = "No replenishment Locations Found";
                }
                else
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);

            }
            catch (Exception e)
            {
                retval.successful = false;
                retval.ErrorMessage = e.Message;
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);
        }
    }
}