using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ASCTracRestService.Controllers.CustOrder
{
    class CustOrderInfo
    {
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOInfo(string aOrderNumber, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            ASCTracFunctionStruct.CustOrder.CustOrderInfoType myCOInfo = new ASCTracFunctionStruct.CustOrder.CustOrderInfoType();
            try
            {
                string orderInfo = string.Empty;
                string fieldlist = "SITE_ID, PICKSTATUS, SHIPTOCUSTID, SHIPTONAME, REQUIREDSHIPDATE, " +
                    " DATEADD(day, 0, DATEDIFF(day, 0, SCHEDDATE)) + DATEADD(day, 0 - DATEDIFF(day, 0, SCHEDTTIME), SCHEDTTIME), " +
                    " CARRIER, SCHEDPICKERID, TRUCKAVAIL, STAGE_LOC, TRAILER_NUM";
                if( !Globals.myGetInfo.GetOrderInfo( aOrderNumber, fieldlist, ref orderInfo ))
                {
                    retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ORDERNUM);
                }
                else if ( ascLibrary.ascStrUtils.GetNextWord( ref orderInfo) != Globals.curSiteID)
                {
                    string[] arrstr = { aOrderNumber };
                    retval.ErrorMessage = ParseNet.dmascmessages.formatmessagebyid(ParseNet.TASCMessageType.PERR_CP_WRONG_SITE, arrstr);
                }
                else
                {
                    myCOInfo.PickStatus = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                    if (myCOInfo.PickStatus.Equals(ascLibrary.dbConst.ssCONF_SHIP))
                        retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtSHIPPED);
                    else if (myCOInfo.PickStatus.Equals(ascLibrary.dbConst.ssCANCELLED))
                        retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                    else
                    {
                        myCOInfo.OrderNumber = aOrderNumber;
                        myCOInfo.CustID = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                        myCOInfo.CustName = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                        myCOInfo.CustIDAndName = myCOInfo.CustID + " - " + myCOInfo.CustName;
                        myCOInfo.requiredShipDate = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref orderInfo), DateTime.MinValue);
                        myCOInfo.Sched_Datetime = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref orderInfo), DateTime.MinValue);
                        myCOInfo.CarrierID = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                        // need for scheduling
                        myCOInfo.SchedPickerID = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                        myCOInfo.TruckAvail = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                        myCOInfo.PickToLocID = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                        myCOInfo.TrailerID = ascLibrary.ascStrUtils.GetNextWord(ref orderInfo);
                        myCOInfo.fNeedSignature = Globals.dmMiscOrder.NeedSignatureForCustomer( "C", myCOInfo.CustID);

                        string tmp = string.Empty;
                        Globals.myDBUtils.ReadFieldFromDB("SELECT DESCRIPTION FROM SHIPSTAT WHERE STATUSID='" + myCOInfo.PickStatus + "'", "", ref tmp);
                        myCOInfo.PickStatus_Description = tmp;

                        CustOrder myCO = new CustOrder();
                        myCOInfo.DetailXMLInfo = myCO.GetOrdrDet(myCOInfo.OrderNumber, Globals);
                    }
                }

                retval.successful = (String.IsNullOrEmpty(retval.ErrorMessage));
                if (!retval.successful)
                {
                    retval.ErrorMessage += "\r\nOrder Number: " + aOrderNumber;
                    Globals.myASCLog.fErrorData = retval.ErrorMessage;
                    Globals.myASCLog.ProcessTran(retval.ErrorMessage, "X");
                }
                else
                {
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myCOInfo);
                }
            }
            catch( Exception ex)
            {
                Globals.myASCLog.fErrorData = ex.ToString();
                Globals.myASCLog.ProcessTran(ex.Message, "X");
                retval.successful = false;
                retval.ErrorMessage= ex.Message;
            }

            return (retval);
        }

        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOList(int aCustFilterType, string aCustData, int aFiltertype, string aFilterData, bool aCurrentUserOnly, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            List<ASCTracFunctionStruct.CustOrder.CustOrderInfoType> myList = new List<ASCTracFunctionStruct.CustOrder.CustOrderInfoType>();
            try
            {
                string fieldlist = "H.ORDERNUMBER, H.ORDERTYPE, H.SITE_ID, H.PICKSTATUS, H.SHIPTOCUSTID, H.SHIPTONAME, H.REQUIREDSHIPDATE, " +
                    " DATEADD(day, 0, DATEDIFF(day, 0, H.SCHEDDATE)) + DATEADD(day, 0 - DATEDIFF(day, 0, H.SCHEDTTIME), H.SCHEDTTIME) as SCHED_DATETIME, " +
                    " H.SCHEDPICKERID, H.TRUCKAVAIL, H.STAGE_LOC, H.TRAILER_NUM," +
                    " H.CARRIER, D.LOADINGBAY, D.DURATION";
                fieldlist += ", SS.DESCRIPTION AS PICKSTATUSDESC";

                string sql = "SELECT " + fieldlist + " FROM ORDRHDR H";
                sql += " LEFT JOIN SHIPSTAT SS ON SS.STATUSID=H.PICKSTATUS";
                sql += " LEFT JOIN DOCKSCHD D ON D.CO_ORDERNUM=H.ORDERNUMBER";
                sql += " WHERE H.SITE_ID='" + Globals.curSiteID + "'";
                if( !String.IsNullOrEmpty( aCustData))
                {
                    if (aCustFilterType == 0)
                        sql += " AND SHIPTOCUSTID='" + aCustData + "'";
                    if (aCustFilterType == 1)
                        sql += " AND SOLDTOCUSTID='" + aCustData + "'";
                    if (aCustFilterType == 2)
                        sql += " AND SHIPTONAME LIKE '" + aCustData + "%'";
                    if (aCustFilterType == 3)
                        sql += " AND BILLTONAME LIKE '" + aCustData + "%'";
                }
                if( !String.IsNullOrEmpty( aFilterData))
                {
                    if (aFiltertype == 0)
                        sql += " AND datediff(day, H.REQUIREDSHIPDATE, '" + aFilterData + "') = 0";
                    //sql += " AND H.REQUIREDSHIPDATE>='" + aFilterData + "' AND H.REQUIREDSHIPDATE < '" + aFilterData + "'";
                    if (aFiltertype == 1)
                        sql += " AND H.CARRIER='" + aFilterData + "'";
                    if (aFiltertype == 2)
                        sql += " AND H.ORDERNUMBER IN ( SELECT CO_ORDERNUM FROM DOCKSCHD WHERE LOADINGBAY='" + aFilterData + "')";
                    if (aFiltertype == 3)
                        sql += " AND H.PICKSTATUS='" + aFilterData + "'";
                    if (aFiltertype == 4)
                        sql += " AND H.TRAILER_NUM='" + aFilterData + "'";
                    if (aFiltertype == 5)
                        sql += " AND H.ORDERTYPE='" + aFilterData + "'";
                    if (aFiltertype == 6)
                        sql += " AND datediff(day, H.CREATEDATE, '" + aFilterData + "') = 0";
                    //sql += " AND H.CREATEDATE>='" + aFilterData + "' AND H.CREATEDATE < '" + aFilterData + "'";
                }
                if (aFiltertype != 3)
                    sql += " AND H.PICKSTATUS NOT IN ( 'X', 'C')";

                if (aCurrentUserOnly)
                    sql += " AND H.CREATE_USERID='" + Globals.curUserID + "'";

                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        var myCOInfo = new ASCTracFunctionStruct.CustOrder.CustOrderInfoType();
                        myCOInfo.OrderNumber = myReader["ORDERNUMBER"].ToString();
                        myCOInfo.PickStatus = myReader["PICKSTATUS"].ToString();
                        myCOInfo.PickStatus_Description = myReader["PICKSTATUSDESC"].ToString();
                        myCOInfo.CustID = myReader["SHIPTOCUSTID"].ToString();
                        myCOInfo.CustName = myReader["SHIPTONAME"].ToString();
                        myCOInfo.CustIDAndName = myCOInfo.CustID + " - " + myCOInfo.CustName;
                        myCOInfo.requiredShipDate = ascLibrary.ascUtils.ascStrToDate(myReader["REQUIREDSHIPDATE"].ToString(), DateTime.MinValue);
                        myCOInfo.Sched_Datetime= ascLibrary.ascUtils.ascStrToDate( myReader["SCHED_DATETIME"].ToString(), DateTime.MinValue);
                        myCOInfo.CarrierID = myReader["CARRIER"].ToString();
                        myCOInfo.OrderType = myReader["ORDERTYPE"].ToString();
                        myCOInfo.Dock = myReader["LOADINGBAY"].ToString();
                        myCOInfo.Duration = ascLibrary.ascUtils.ascStrToInt(myReader["DURATION"].ToString(), 0);

                        // need for scheduling
                        myCOInfo.SchedPickerID = myReader["SCHEDPICKERID"].ToString();
                        myCOInfo.TruckAvail = myReader["TRUCKAVAIL"].ToString();
                        myCOInfo.PickToLocID = myReader["STAGE_LOC"].ToString();
                        myCOInfo.TrailerID = myReader["TRAILER_NUM"].ToString();

                        CustOrder myCO = new CustOrder();
                        myCOInfo.DetailXMLInfo = myCO.GetOrdrDet(myCOInfo.OrderNumber, Globals);

                        myList.Add(myCOInfo);
                    }
                }
                finally
                {
                    myConnection.Close();
                }

                if (myList.Count == 0)
                {
                    retval.successful = false;
                    retval.ErrorMessage = "No Customer Orders Found";
                }
                else
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
            }
            catch (Exception ex)
            {
                Globals.myASCLog.fErrorData = ex.ToString();
                Globals.myASCLog.ProcessTran(ex.Message, "X");
                retval.successful = false;
                retval.ErrorMessage = ex.Message;
            }
            return (retval);

        }

    }
}
