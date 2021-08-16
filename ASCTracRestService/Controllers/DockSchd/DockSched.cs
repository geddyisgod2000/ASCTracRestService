using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ASCTracRestService.Controllers.DockSchd
{
    public class DockSched
    {
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetDockSchedList(ParseNet.GlobalClass Globals, string aDock, DateTime aDate)
        {
            return GetDockSchedList(Globals, "SHIPSTATUS NOT IN ( 'C', 'X', 'Q') " +
            " AND D.SCHEDDATE>='" + aDate.ToShortDateString() + "' AND D.SCHEDDATE<='" + aDate.AddDays(1).ToShortDateString() + "'" +
            " AND D.LOADINGBAY = '" + aDock + "'");
        }

        public ASCTracFunctionStruct.ascBasicReturnMessageType GetDockSchedList(ParseNet.GlobalClass Globals, string aWhereStr)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            List<ASCTracFunctionStruct.CustOrder.DockType> myList = new List<ASCTracFunctionStruct.CustOrder.DockType>();
            string whereStr = "D.SITE_ID='" + Globals.curSiteID + "'";
            whereStr += " AND " + aWhereStr;
            //bool fOK = true;
            try
            {
                string sqlstr = "SELECT D.TYPE_FLAG, D.LOADINGBAY, D.SCHEDDATE, D.SCHEDTTIME, D.DURATION, D.SCHED_ID, D.SITE_ID" +
                    ",D.CO_ORDERNUM, D.PO_ORDERNUM, D.LOADPLAN_NUM, D.RECEIVER_ID, D.ASN_NUM, D.CFT_NUM, D.RMA_NUM" +
                    ", D.YARD_LOC, D.SHIPSTATUS, D.COMPLETE_FLAG, D.CARRIERS, D.CUSTID, D.VENDORID, D.RECUR_ID " +
                    ", SS.DESCRIPTION AS STATUS_DESC, D.SHIPMENT_ID, D.IN_TRAILER_NUM";
                sqlstr += " FROM DOCKSCHD D LEFT JOIN SHIPSTAT SS ON SS.STATUSID=D.SHIPSTATUS";
                sqlstr += " WHERE " + whereStr;
                sqlstr += " ORDER BY D.SCHEDDATE, D.SCHEDTTIME";
                /*
                    if wlcInOutFlag.GetComboValue( wlcInOutFlag.Text) = 'I' then
                      sqlstr += "  AND D.LOADINGBAY IN ( SELECT LOADINGBAY FROM DOCKS WHERE ' + aSiteFilter + ' AND IN_OUT_FLAG IN ( ''I'', ''B''))'
                    else if wlcInOutFlag.GetComboValue( wlcInOutFlag.Text) = 'O' then
                      sqlstr += "  AND D.LOADINGBAY IN ( SELECT LOADINGBAY FROM DOCKS WHERE ' + aSiteFilter + ' AND IN_OUT_FLAG IN ( ''O'', ''B''))';
                 */


                Globals.myASCLog.updateInputData(sqlstr);
                Globals.myASCLog.updateSQL(sqlstr);
                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);


                SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);
                myConnection.Open();
                try
                {
                    string tstr = string.Empty;
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        string typeColorName = string.Empty;
                        var rec = new ASCTracFunctionStruct.CustOrder.DockType();
                        rec.PickStatus = myReader["SHIPSTATUS"].ToString();
                        rec.PickStatus_Description = myReader["STATUS_DESC"].ToString();
                        rec.OrderType = myReader["TYPE_FLAG"].ToString();
                        rec.SchedID = myReader["SCHED_ID"].ToString();
                        rec.Dock = myReader["LOADINGBAY"].ToString();
                        rec.CarrierID = myReader["CARRIERS"].ToString();

                        switch (myReader["TYPE_FLAG"].ToString())
                        {
                            case "C":
                                rec.OrderTypeDesc = "CO#";
                                rec.OrderNumber = myReader["CO_ORDERNUM"].ToString();
                                typeColorName = "DS-Outbound";
                                if (Globals.myGetInfo.GetOrderInfo(rec.OrderNumber, "REQUIREDSHIPDATE,SHIPTOCUSTID, SHIPTONAME", ref tstr))
                                {
                                    rec.requiredShipDate = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tstr), DateTime.MinValue);
                                    rec.CustID = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                                    rec.CustName = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                                    rec.CustIDAndName = rec.CustID + " - " + rec.CustName;
                                }

                                sqlstr = "SELECT SUM( QTYORDERED), SUM( QTYPICKED) FROM ORDRDET WHERE ORDERNUMBER='" + rec.OrderNumber + "'";
                                if (Globals.myDBUtils.ReadFieldFromDB(sqlstr, "", ref tstr))
                                {
                                    double dtmp = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tstr), 0);
                                    if (dtmp > 0)
                                        dtmp = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tstr), 0) / dtmp;
                                    if (dtmp > 1)
                                        dtmp = 100;
                                    else
                                        dtmp = dtmp * 100;
                                    rec.Percent_Complete = dtmp;

                                }
                                break;
                            case "L":
                                rec.OrderTypeDesc = "Loadplan #";
                                rec.OrderNumber = myReader["LOADPLAN_NUM"].ToString();
                                typeColorName = "DS-Outbound";
                                break;
                            case "H":
                                rec.OrderTypeDesc = "Shipment";
                                rec.OrderNumber = myReader["SHIPMENT_ID"].ToString();
                                typeColorName = "DS-Outbound";
                                break;


                            case "A":
                                rec.OrderTypeDesc = "ASN #";
                                rec.OrderNumber = myReader["ASN_NUM"].ToString();
                                typeColorName = "DS-Inbound";
                                break;
                            case "F":
                                rec.OrderTypeDesc = "CFT #";
                                rec.OrderNumber = myReader["CFT_NUM"].ToString();
                                typeColorName = "DS-Inbound";
                                break;
                            case "M":
                                rec.OrderTypeDesc = "RMA #";
                                rec.OrderNumber = myReader["RMA_NUM"].ToString();
                                typeColorName = "DS-Inbound";
                                break;

                            case "P":
                                rec.OrderTypeDesc = "PO#";
                                rec.OrderNumber = myReader["PO_ORDERNUM"].ToString();
                                typeColorName = "DS-Inbound";
                                string ponum = string.Empty;
                                string relnum = string.Empty;
                                Globals.dmRecv.parseponums(rec.OrderNumber, ref ponum, ref relnum);
                                if (Globals.myGetInfo.GetPOHdrInfo(ponum, relnum, "RECEIVED,EXPECTEDRECEIPTDATE,VENDORID", ref tstr))
                                {
                                    rec.PickStatus = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                                    rec.requiredShipDate = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tstr), DateTime.MinValue);
                                    rec.CustID = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                                    Globals.myGetInfo.GetVendorInfo(tstr, "VENDORNAME", ref tstr);
                                    rec.CustName = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                                    rec.CustIDAndName = rec.CustID + " - " + rec.CustName;
                                }

                                sqlstr = "SELECT SUM( QTY), SUM( QTYRECEIVED) FROM PODET WHERE PONUMBER='" + ponum + "' AND RELEASENUM='" + relnum + "'";
                                if (Globals.myDBUtils.ReadFieldFromDB(sqlstr, "", ref tstr))
                                {
                                    double dtmp = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tstr), 0);
                                    if (dtmp > 0)
                                        dtmp = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tstr), 0) / dtmp;
                                    if (dtmp > 1)
                                        dtmp = 100;
                                    else
                                        dtmp = dtmp * 100;
                                    rec.Percent_Complete = dtmp;

                                }
                                break;
                            case "R":
                                rec.OrderTypeDesc = "Receiver ID";
                                rec.OrderNumber = myReader["RECEIVER_ID"].ToString();
                                typeColorName = "DS-Inbound";
                                break;

                            case "S":
                                rec.OrderTypeDesc = "Customer";
                                rec.OrderNumber = myReader["CUSTID"].ToString();
                                typeColorName = "DS-Other";
                                break;
                            case "V":
                                rec.OrderTypeDesc = "Vendor";
                                rec.OrderNumber = myReader["VENDORID"].ToString();
                                typeColorName = "DS-Other";
                                break;
                            case "U":
                                rec.OrderTypeDesc = "Unload Trailer";
                                rec.OrderNumber = string.Empty; //myReader["VENDORID"].ToString();
                                typeColorName = "DS-Other";
                                break;
                            case "Z":
                                rec.OrderTypeDesc = "Available Trailer";
                                rec.OrderNumber = string.Empty; //myReader["VENDORID"].ToString();
                                typeColorName = "DS-Other";
                                break;
                            case "B":
                                rec.OrderTypeDesc = "Dock Unavailable";
                                rec.OrderNumber = string.Empty; //myReader["VENDORID"].ToString();
                                typeColorName = "DS-Other";
                                break;
                            default:
                                rec.OrderTypeDesc = "Other";
                                rec.OrderNumber = myReader["SCHED_ID"].ToString();
                                break;
                        }
                        rec.Sched_Datetime = ascLibrary.ascUtils.ascStrToDate(myReader["SCHEDDATE"].ToString(), DateTime.MinValue).Date;
                        if (rec.Sched_Datetime != DateTime.MinValue)
                        {
                            DateTime dtTmp = ascLibrary.ascUtils.ascStrToDate(myReader["SCHEDTTIME"].ToString(), DateTime.MinValue);

                            if (rec.Sched_Datetime != DateTime.MinValue)
                                rec.Sched_Datetime = rec.Sched_Datetime.Add(dtTmp.TimeOfDay);
                        }
                        rec.Duration = ascLibrary.ascUtils.ascStrToInt(myReader["DURATION"].ToString(), 60);

                        string colorname = "Scheduled";
                        if (rec.OrderType.Equals("C") || rec.OrderType.Equals("L") || rec.OrderType.Equals("H"))
                        {
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
                        }

                        if (rec.OrderType.Equals("P"))
                        {
                            rec.PickStatus_Description = "Receipt Status " + rec.PickStatus;
                            colorname = string.Empty;
                            if (rec.PickStatus.Equals(ascLibrary.dbConst.osRECEIVED))
                            {
                                rec.PickStatus_Description = "Received";
                                colorname = "Received";
                            }
                            if (rec.PickStatus.Equals(ascLibrary.dbConst.osRECEIVING))
                            {
                                rec.PickStatus_Description = "Receiving";
                                //colorname = "Received";
                            }
                            if (rec.PickStatus.Equals(ascLibrary.dbConst.osPARTIALRECEIVED))
                            {
                                rec.PickStatus_Description = "Partially Received";
                                //colorname = "Received";
                            }
                            if (rec.PickStatus.Equals(ascLibrary.dbConst.osOPEN))
                            {
                                rec.PickStatus_Description = "Unreceived";
                                //colorname = "Received";
                            }
                            if (rec.PickStatus.Equals(ascLibrary.dbConst.osCLOSED))
                            {
                                rec.PickStatus_Description = "Closed";
                                colorname = "Closed";
                            }
                            if (rec.PickStatus.Equals(ascLibrary.dbConst.osCANCELLED))
                            {
                                rec.PickStatus_Description = "Cancelled";
                                colorname = "Cancelled";
                            }
                        }

                        string tmp = string.Empty;
                        if (!String.IsNullOrEmpty(colorname) && Globals.myDBUtils.ReadFieldFromDB("SELECT FG_COLOR, BG_COLOR FROM COLOR WHERE COLORNAME='" + colorname + "' AND USERID='" + Globals.curUserID + "'", "", ref tmp))
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

                        if (!String.IsNullOrEmpty(typeColorName) && Globals.myDBUtils.ReadFieldFromDB("SELECT FG_COLOR, BG_COLOR FROM COLOR WHERE COLORNAME='" + typeColorName + "' AND USERID='" + Globals.curUserID + "'", "", ref tmp))
                        {
                            Color myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.FGOrderTypeColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            myColor = Color.FromArgb(Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0)));
                            rec.BGOrderTypeColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                        }
                        else
                        {
                            Color myColor = Color.Black;
                            rec.FGOrderTypeColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
                            myColor = Color.WhiteSmoke;
                            rec.BGOrderTypeColor = myColor.R.ToString() + "," + myColor.G.ToString() + "," + myColor.B.ToString(); // Convert.ToInt32(ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0));
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
                    retval.ErrorMessage = "No scheduled records found."; // +whereStr;
                }
                else
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran("Exception\r\n" + e.ToString(), "X");

                retval.successful = false;
                retval.ErrorMessage = e.Message;
                //  fOK = false;
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
            catch (Exception e)
            {
                Globals.myASCLog.ProcessTran("Writing return message to app log\r\n" + e.ToString(), "X");
            }
            return (retval);
        }

        public ASCTracFunctionStruct.ascBasicReturnMessageType GetNewDockSched(string aOrderType, string aOrderNum, string aDock, DateTime aDate, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            ASCTracFunctionStruct.CustOrder.DockType myrec = new ASCTracFunctionStruct.CustOrder.DockType();
            try
            {
                string tmp = string.Empty;
                string updstr = string.Empty;
                string carrier = string.Empty;
                bool fAppt = false;

                switch (aOrderType)
                {
                    case "C":
                        if (!Globals.myGetInfo.GetOrderInfo(aOrderNum, "SITE_ID, CARRIER, PICKSTATUS", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ORDERNUM);

                        else if (ascLibrary.ascStrUtils.GetNextWord(ref tmp) != Globals.curSiteID)
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                        else
                        {
                            carrier = ascLibrary.ascStrUtils.GetNextWord(ref tmp);
                            if (tmp.Equals(ascLibrary.dbConst.ssCONF_SHIP) || tmp.Equals(ascLibrary.dbConst.ssCONF_QCWAIT))
                                retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtSHIPPED);
                            else if (tmp.Equals(ascLibrary.dbConst.ssCANCELLED))
                                retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                            else
                            {

                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CO_ORDERNUM", aOrderNum);
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "O");
                                myrec.OrderTypeDesc = "Customer Order";
                            }
                        }
                        break;
                    case "L":
                        if (!Globals.myGetInfo.GetLoadPlanInfo(aOrderNum, "SITE_ID, STATUS", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ORDERNUM);
                        else if (ascLibrary.ascStrUtils.GetNextWord(ref tmp) != Globals.curSiteID)
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                        else if (tmp.Equals(ascLibrary.dbConst.ssCONF_SHIP) || tmp.Equals(ascLibrary.dbConst.ssCONF_QCWAIT))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtSHIPPED);
                        else if (tmp.Equals(ascLibrary.dbConst.ssCANCELLED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                        else
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "LOADPLAN_NUM", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "O");
                            myrec.OrderTypeDesc = "Load Plan";
                        }
                        break;
                    case "H":
                        if (!Globals.myGetInfo.GetShipHdrInfo(aOrderNum, "SITE_ID, STATUS", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ORDERNUM);
                        else if (ascLibrary.ascStrUtils.GetNextWord(ref tmp) != Globals.curSiteID)
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                        else if (tmp.Equals(ascLibrary.dbConst.ssCONF_SHIP) || tmp.Equals(ascLibrary.dbConst.ssCONF_QCWAIT))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtSHIPPED);
                        else if (tmp.Equals(ascLibrary.dbConst.ssCANCELLED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                        else
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPMENT_ID", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "O");
                            myrec.OrderTypeDesc = "Shipment ID";
                        }
                        break;
                    case "P":
                        string relnum = string.Empty;
                        string ponum = aOrderNum;
                        Globals.dmMiscRecv.parseponum(ref ponum, ref relnum);

                        if (!Globals.myGetInfo.GetPOHdrInfo(ponum, relnum, "SITE_ID, RECEIVED", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_PONUM);
                        else if (ascLibrary.ascStrUtils.GetNextWord(ref tmp) != Globals.curSiteID)
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                        else if (tmp.Equals(ascLibrary.dbConst.osCLOSED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtFINISHED);
                        else if (tmp.Equals(ascLibrary.dbConst.osCANCELLED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                        else
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PO_ORDERNUM", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "I");
                            myrec.OrderTypeDesc = "Purchase Order";
                        }
                        break;
                    case "R":
                        if (!Globals.myGetInfo.GetRecvrHdrInfo(aOrderNum, "SITE_ID, RECEIVED", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_RECEIVER);
                        else if (ascLibrary.ascStrUtils.GetNextWord(ref tmp) != Globals.curSiteID)
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                        else if (tmp.Equals(ascLibrary.dbConst.osCLOSED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtFINISHED);
                        else if (tmp.Equals(ascLibrary.dbConst.osCANCELLED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                        else
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "RECEIVER_ID", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "I");
                            myrec.OrderTypeDesc = "Receiver";
                        }
                        break;
                    case "A":
                        if (!Globals.myGetInfo.GetASNHdrInfo(aOrderNum, "", "SITE_ID, STATUS", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ORDERNUM);
                        else if (ascLibrary.ascStrUtils.GetNextWord(ref tmp) != Globals.curSiteID)
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                        else if (tmp.Equals("Y"))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtFINISHED);
                        else if (tmp.Equals(ascLibrary.dbConst.osCANCELLED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                        else
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ASN_NUM", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "I");
                            myrec.OrderTypeDesc = "ASN";
                        }
                        break;
                    case "M":
                        if (!Globals.myGetInfo.GetRMAHdrInfo(aOrderNum, "SITE_ID, STATUS", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ORDERNUM);
                        else if (ascLibrary.ascStrUtils.GetNextWord(ref tmp) != Globals.curSiteID)
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                        else if (tmp.Equals(ascLibrary.dbConst.osCLOSED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtFINISHED);
                        else if (tmp.Equals(ascLibrary.dbConst.osCANCELLED))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtCANCELLED);
                        else
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "RECEIVER_ID", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "I");
                            myrec.OrderTypeDesc = "RMA";
                        }
                        break;
                    case "S":
                        if (!Globals.myGetInfo.GetCustInfo(aOrderNum, "CUSTID", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_CUST);
                        else
                        {
                            fAppt = true;
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CUSTID", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "O");
                            myrec.OrderTypeDesc = "Customer";
                        }
                        break;
                    case "V":
                        if (!Globals.myGetInfo.GetVendorInfo(aOrderNum, "VENDORID", ref tmp))
                            retval.ErrorMessage = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_VENDOR);
                        else
                        {
                            fAppt = true;
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CUSTID", aOrderNum);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "O");
                            myrec.OrderTypeDesc = "Vendor";
                        }
                        break;
                    case "U":
                    case "B":
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CO_ORDERNUM", aOrderNum);
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPFLAG", "O");
                        myrec.OrderTypeDesc = "Maintenance";
                        break;
                    default:
                        retval.ErrorMessage = "Invalid Order Type " + aOrderType;
                        break;
                }

                if (!String.IsNullOrEmpty(retval.ErrorMessage))
                    retval.successful = false;
                else
                {
                    retval.successful = true;
                    string sDuration = string.Empty;
                    Globals.myGetInfo.GetDockTypeInfo(aOrderType, "DEFAULT_DURATION", ref sDuration);
                    if (String.IsNullOrEmpty(sDuration))
                        sDuration = Globals.myConfig.iniDockTimeMinutes.Value;
                    string shipstatus = ascLibrary.dbConst.ssSCHEDULED;
                    string schedID = string.Empty;
                    long duration = 0;
                    DateTime SchedDateTime = aDate;
                    if (aOrderType.Equals("S") || // customer appt
                         aOrderType.Equals("B") || // blocked maint
                         aOrderType.Equals("U")) // unload trailer
                    {
                    }
                    else
                    {

                        if (!Globals.myGetInfo.GetDockSchedInfo(aOrderType, aOrderNum, "SHIPSTATUS, SCHED_ID,SCHEDDATE, SCHEDTTIME, DURATION,LOADINGBAY", true, ref tmp))
                        {

                        }
                        else
                        {
                            shipstatus = ascLibrary.ascStrUtils.GetNextWord(ref tmp);
                            schedID = ascLibrary.ascStrUtils.GetNextWord(ref tmp);
                            DateTime tSchedDateTime = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.MinValue).Date;
                            string sTime = ascLibrary.ascStrUtils.GetNextWord(ref tmp);
                            duration = ascLibrary.ascUtils.ascStrToInt(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0);
                            if (tSchedDateTime != DateTime.MinValue)
                            {
                                var tDT = ascLibrary.ascUtils.ascStrToDate(sTime, DateTime.MinValue);
                                if (tDT != DateTime.MinValue)
                                {
                                    tSchedDateTime = tSchedDateTime.Add(tDT.TimeOfDay);
                                    SchedDateTime = SchedDateTime.Add(tDT.TimeOfDay);
                                }

                                if (!shipstatus.Equals(ascLibrary.dbConst.ssCONF_SHIP) || !shipstatus.Equals(ascLibrary.dbConst.ssCANCELLED))
                                    myrec.ReturnMessage = "Order already Scheduled\r\nDock " + ascLibrary.ascStrUtils.GetNextWord(ref tmp) + "\r\nat " + SchedDateTime;
                            }
                        }
                    }
                    if (String.IsNullOrEmpty(myrec.ReturnMessage))
                    {
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "TYPE_FLAG", aOrderType);
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SITE_ID", Globals.curSiteID);
                        ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "DURATION", sDuration);
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "LOADINGBAY", aDock);
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHEDDATE", aDate.ToShortDateString());
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHEDTTIME", aDate.ToShortTimeString());
                        ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CARRIERS", carrier);
                        if (fAppt)
                        {
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "APPOINT_DATE", aDate.ToShortDateString());
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "APPOINT_TTIME", aDate.ToShortTimeString());
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPSTATUS", ascLibrary.dbConst.ssAPPT);
                        }
                        else
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPSTATUS", shipstatus);
                        Globals.mydmupdate.InsertRecord("DOCKSCHD", updstr);
                        Globals.mydmupdate.ProcessUpdates();
                        Globals.myDBUtils.ReadFieldFromDB("SELECT MAX( SCHED_ID) FROM DOCKSCHD", string.Empty, ref schedID);
                    }
                    myrec.OrderType = aOrderType;
                    myrec.OrderNumber = aOrderNum;
                    myrec.PickStatus = ascLibrary.dbConst.ssSCHEDULED;
                    myrec.PickStatus_Description = "Scheduled";
                    myrec.SchedID = schedID;
                    myrec.Sched_Datetime = SchedDateTime;
                    if (duration > 0)
                        myrec.Duration = duration;
                    else
                        myrec.Duration = ascLibrary.ascUtils.ascStrToInt(sDuration, Globals.myConfig.iniDockTimeMinutes.IntValue);
                    myrec.CarrierID = carrier;
                    myrec.Dock = aDock;

                    /****
          if TypeFlag = 'C' then
          begin
            ascAppendsetStr( updStr, 'SCHEDDATE', DateToStr( aDefaultDT));
            ascAppendsetStr( updStr, 'SCHEDTTIME', ASCTimeToStr( aDefaultDT));
            if ordernum <> '' then
              dmupdate.UpdateFields( 'ORDRHDR', updStr, 'ORDERNUMBER=''' + orderNum + '''');
            
            dmmiscFunc.GetOrderInfo( ordernum, 'CARRIER, SOLDTOCUSTID', custid);
            carrier := ascGetNextWord( custid, HHDELIM);
            
            updStr := 'CO_ORDERNUM=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'O');
          end;
          if ( TypeFlag = 'U' ) or ( TypeFlag = 'B' ) then
          begin
            updStr := 'CO_ORDERNUM=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'O');
            dmmiscFunc.GetOrderInfo( ordernum, 'CARRIER, SOLDTOCUSTID', custid);
            carrier := ascGetNextWord( custid, HHDELIM);
          end;                                     
          if TypeFlag = 'L' then
          begin
            dmmiscFunc.GetLoadPlanInfo( ordernum, 'CARRIER_ID', carrier);
            ascAppendsetStr( updStr, 'SCHEDULED_DATE', DateToStr( aDefaultDT));
            //ascAppendsetStr( updStr, 'SCHEDTTIME', diDockScheduler.Items[diDockScheduler.row].Caption);
            dmupdate.UpdateFields( 'LOADPLAN', updStr, 'LOADPLAN=''' + orderNum + '''');
            
            updStr := 'LOADPLAN_NUM=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'O');
          end;                                     
          if TypeFlag = 'P' then
          begin
            dmmiscFunc.GetPORealHdrInfo( ordernum, 'CARRIERNAME, VENDORID', custid);
            carrier := ascGetNextWord( custid, HHDELIM);
            
            updStr := 'PO_ORDERNUM=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'I');
          end;                                     
          if TypeFlag = 'A' then
          begin
            updStr := 'ASN_NUM=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'I');
          end;                                     
          if TypeFlag = 'F' then
          begin
            updStr := 'CFT_NUM=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'I');
          end;                                     
          if TypeFlag = 'R' then
          begin
            updStr := 'RECEIVER_ID=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'I');
          end;                                     
          if TypeFlag = 'M' then
          begin
            updStr := 'RMA_NUM=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'I');
          end;                                     
          if TypeFlag = 'S' then
          begin
            updStr := 'CUSTID=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'O');
            custID := OrderNum;
          end;                                     
          if TypeFlag = 'V' then
          begin
            updStr := 'VENDORID=''' + orderNum + '''';
            ascappendSetStr( updStr, 'SHIPFLAG', 'I');
            custID := OrderNum;
          end;                                     
          
          ascAppendsetStr( updStr, 'TYPE_FLAG', typeFlag);
          ascAppendsetStr( updStr, 'SITE_ID', Globals.curSiteID );
          if sDuration <> '' then
            ascAppendsetQty( updStr, 'DURATION', sDuration)
          else
            ascAppendsetQty( updStr, 'DURATION', InttoStr( ascTracConfig.iniDockTimeMinutes.Value));
          AscAppendSetStrIfNotEmpty(updStr, 'CUSTID', custID);
          AscAppendSetStrIfNotEmpty(updStr, 'CARRIERS', carrier);
          if dockSchedID <> '' then
            updStr := '';
          ascAppendsetStr( updStr, 'LOADINGBAY', thisDock);
          ascAppendsetStr( updStr, 'SCHEDDATE', DateToStr( aDefaultDT));
          ascAppendsetStr( updStr, 'SCHEDTTIME', ASCTimeToStr( aDefaultDT));
          if fAppt then
          begin
            ascAppendsetStr( updStr, 'APPOINT_DATE', DateToStr( aDefaultDT));
            ascAppendsetStr( updStr, 'APPOINT_TTIME', ASCTimeToStr( aDefaultDT));
          end;  
          if dockschedID = '' then
          begin
            if not fAppt then
              ascAppendsetStr( updStr, 'SHIPSTATUS', ssSCHEDULED)
            else
              ascAppendsetStr( updStr, 'SHIPSTATUS', ssAPPT);
          end;  
          if dockSchedID <> '' then
            dmupdate.UpdateFields( 'DOCKSCHD', updStr, 'SCHED_ID=' + dockSchedID )
          else
          begin
            dmupdate.insertRecord( 'DOCKSCHD', updStr);
                    ****/
                }

                if (String.IsNullOrEmpty(retval.ErrorMessage))
                {
                    Globals.mydmupdate.ProcessUpdates();
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myrec);
                }
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran("Exception\r\n" + e.ToString(), "X");

                retval.successful = false;
                retval.ErrorMessage = e.Message;
            }
            return (retval);
        }

        public string doDockSched(ASCTracFunctionStruct.CustOrder.DockType aDockRec, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            try
            {
                string tmp = string.Empty;
                if (!Globals.myGetInfo.GetDockSchedidInfo(aDockRec.SchedID, "COMPLETE_FLAG", ref tmp))
                    retval = "Dock Schedule Record " + aDockRec.SchedID + " not found";
                else if (tmp.Equals("T"))
                    retval = aDockRec.OrderTypeDesc + " " + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_GEN_COMPLETED);

                if (String.IsNullOrEmpty(retval))
                {
                    /* User rights
                      fCanEditCO := aCanEdit and globals.HasEditRights( 'mnuProcessesScheduling');
                      fCanEditLP := aCanEdit and globals.HasEditRights( 'Loadplan2');
                      fCanEditPO := aCanEdit and globals.HasEditRights( 'mnuFileReceiptsExpected');
                      fCanEditASN := aCanEdit and globals.HasEditRights( 'mnuFileReceiptsASNInbound');
                      fCanEditRecvr := aCanEdit and globals.HasEditRights( 'mnuTablesReceiptsReceivers');
                      fCanEditCFT := aCanEdit and globals.HasEditRights( 'mnuReceiptsContainerizedFreight');
                      fCanEditRMA := aCanEdit and globals.HasEditRights( 'mnuFileReceiptsRMAs');
                     */

                    string updstr = string.Empty;
                    switch (aDockRec.OrderType)
                    {
                        case "C": // update ordrhdr and dockschd
                        case "L": // update Loadplan, ordrhdr and dockschd
                            if (aDockRec.newPickStatus != aDockRec.PickStatus)
                            {
                                if (aDockRec.newPickStatus.Equals("N"))
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PICKSTATUS", aDockRec.newPickStatus);
                                else if (aDockRec.newPickStatus.Equals("C"))
                                {
                                    updstr = "PICKSTATUS=( CASE WHEN TRUCKAVAIL IN ( 'Y', 'T') THEN 'G' ELSE 'L' END )";
                                    //ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PICKSTATUS", aDockRec.newPickStatus);
                                }
                                else
                                    updstr = "PICKSTATUS=( CASE WHEN PICKSTATUS IN ( 'I', 'N', 'U') THEN 'S' ELSE PICKSTATUS END )";
                            }

                            if (aDockRec.Sched_Datetime != aDockRec.newSched_Datetime)
                            {
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHEDDATE", aDockRec.newSched_Datetime.ToShortDateString());
                            
                            //if( aDockRec.Sched_Datetime.time)
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHEDTTIME", aDockRec.newSched_Datetime.ToShortTimeString());
                            }
                            if (aDockRec.OrderType.Equals("C"))
                                Globals.mydmupdate.UpdateFields("ORDRHDR", updstr, "ORDERNUMBER='" + aDockRec.OrderNumber + "'");
                            else
                                Globals.mydmupdate.UpdateFields("ORDRHDR", updstr, "LINK_NUM = '" + aDockRec.OrderNumber + "'");
                            break;
                        case "H": // update shipment, Loadplan, ordrhdr and dockschd
                            if (aDockRec.newPickStatus != aDockRec.PickStatus)
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "STATUS", aDockRec.newPickStatus);
                            if (!aDockRec.newDock.Equals(aDockRec.Dock))
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "LOADINGBAY", aDockRec.newDock);
                            if (!string.IsNullOrEmpty(updstr))
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "EXPORT", "F");

                            /*
                              ascappendSetStr( updStr, 'TRAILER_NUM', uppercase( edTrailerNum.Text));
                              ascappendSetStr( updStr, 'SHIPPED_AS_DATE', edShippedAsDate.Text);
                              ascappendSetQty( updStr, 'SHIPMENT_NUM_FOR_DAY', edShipmentNum.Text);
                              ascappendSetStr( updStr, 'EXPORT', 'F');
                            */
                            Globals.mydmupdate.UpdateFields("SHIPHDR", updstr, "SHIPMENT_ID=" + aDockRec.OrderNumber);
                            break;
                        case "A": // update ASN
                            break;
                        case "F": // update CFT
                            break;
                        case "M": // update RMA
                            break;
                        case "P": // update PO/Receiver
                            {
                                if (!aDockRec.newCarrierID.Equals(aDockRec.CarrierID))
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CARRIERNAME", aDockRec.newCarrierID);
                                string relnum = string.Empty;
                                string ponum = aDockRec.OrderNumber;
                                Globals.dmMiscRecv.parseponum(ref ponum, ref relnum);
                                Globals.mydmupdate.UpdateFields("POHDR", updstr, "PONUMBER='" + ponum + "' AND RELEASENUM='" + relnum + "'");
                            }
                            break;
                        case "R": // update Receiver/PO
                            if (!aDockRec.newCarrierID.Equals(aDockRec.CarrierID))
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CARRIERNAME", aDockRec.newCarrierID);
                            Globals.mydmupdate.UpdateFields("RECVRHDR", updstr, "RECEIVER_ID='" + aDockRec.OrderNumber + "'");
                            break;
                        case "S": // update cusotmer appt
                            break;
                        case "V": // update vendor appt
                            break;
                        case "U": // unload trailer
                            break;
                        case "Z": // load trailer
                            break;
                        case "B": // other /maintenance
                            break;
                        default: // ????
                            break;
                    }

                    if (String.IsNullOrEmpty(retval))
                    {
                        updstr = string.Empty;
                        if ((aDockRec.newPickStatus != aDockRec.PickStatus) && aDockRec.newPickStatus.Equals("N"))
                        {
                            Globals.mydmupdate.DeleteRecord("DOCKSCHD", "SCHED_ID=" + aDockRec.SchedID);
                        }
                        else
                        {
                            //if (aDockRec.newPickStatus != aDockRec.PickStatus)
                            {
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SHIPSTATUS", aDockRec.newPickStatus);
                                if (aDockRec.newPickStatus.Equals("C"))
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "COMPLETE_FLAG", "T");
                            }

                            //if( aDockRec.Duration != aDockRec.newDuration)
                            ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "DURATION", aDockRec.newDuration.ToString());
                            //if( aDockRec.Sched_Datetime != aDockRec.newSched_Datetime)
                            {
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHEDDATE", aDockRec.newSched_Datetime.ToShortDateString());
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "SCHEDTTIME", aDockRec.newSched_Datetime.ToShortTimeString());
                            }
                            //if( aDockRec.newDock != aDockRec.Dock)
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "LOADINGBAY", aDockRec.newDock);
                            //if( aDockRec.newCarrierID != aDockRec.CarrierID)
                            ascLibrary.ascStrUtils.AscAppendSetStrIfNotEmpty(ref updstr, "CARRIERS", aDockRec.newCarrierID);
                            Globals.mydmupdate.UpdateFields("DOCKSCHD", updstr, "SCHED_ID=" + aDockRec.SchedID);
                        }

                        Globals.mydmupdate.ProcessUpdates();
                    }
                }
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran("Exception", "X");
                retval = e.Message;
            }
            return (retval);
        }
    }
}