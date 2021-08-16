using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
//using System.ServiceModel;
using System.Text;


namespace ASCTracRestService.Controllers.CustOrder
{
    public class CustOrder
    {
        public string GetOrdrDet(string aCO, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            try
            {
                DataSet dsLicenses = new DataSet();
                // ConfigurationManager.AppSettings["dbconnstring"];
                string sql = "SELECT D.LINENUMBER, D.ITEMID, I.DESCRIPTION, D.QTYORDERED, D.QTYPICKED, D.QTYLOADED, D.ORDERFILLED, D.PICK_LOCATION, NULL AS PCE_TYPE";
                sql += " FROM ORDRDET D ";
                sql += " JOIN ITEMMSTR I ON I.ASCITEMID=D.ASCITEMID";
                sql += " WHERE D.ORDERNUMBER='" + aCO + "'";
                sql += " ORDER BY D.LINENUMBER";

                //if (myParseNet.Globals.myConfig.iniCPNewSuggestLogic.boolValue)
                if (Globals.myDBUtils.GetCount("PCEPICKING", "RECTYPE='C' AND RECID='" + aCO + "'") > 0)
                {
                    sql = "SELECT D.LINENUMBER, D.ITEMID, I.DESCRIPTION, ISNULL( P.QTYTOPICK, D.QTYORDERED) AS QTYORDERED, ISNULL( P.QTYPICKED, D.QTYPICKED) AS QTYPICKED, D.QTYLOADED, ISNULL( P.ORDERFILLED, D.ORDERFILLED) AS ORDERFILLED, P.PICK_LOCATION, P.PCE_TYPE";
                    sql += " FROM ORDRDET D ";
                    sql += " LEFT JOIN PCEPICKING P ON P.RECTYPE='C' AND P.RECID=D.ORDERNUMBER AND P.SEQNUM=D.LINENUMBER";
                    sql += " JOIN ITEMMSTR I ON I.ASCITEMID=D.ASCITEMID";
                    sql += " WHERE D.ORDERNUMBER='" + aCO + "'";
                    sql += " ORDER BY D.LINENUMBER";
                }
                using (SqlConnection conn = new SqlConnection(Globals.myDBUtils.myConnString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    conn.Open();
                    da.MissingSchemaAction = MissingSchemaAction.Add;
                    da.Fill(dsLicenses, "ORDRDET");

                    // Write results of query as XML to a string and return
                    StringWriter sw = new StringWriter();
                    dsLicenses.WriteXml(sw);

                    return "OK" + sw.ToString();
                }
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
                return "EX" + e.ToString();
            }

        }

        public string UpdateOrdrDet(string aCO, long aLineNum, string aPCEType, string aNewStatus, bool aClearPickLoc, ParseNet.GlobalClass Globals)
        {
            try
            {
                if (aClearPickLoc)
                {
                    if (aLineNum > 0)
                    {
                        Globals.mydmupdate.UpdateFields("ORDRDET", "PICK_LOCATION=NULL", "ORDERNUMBER='" + aCO + "' AND LINENUMBER='" + aLineNum.ToString() + "' AND ORDERFILLED='O'");
                        Globals.mydmupdate.UpdateFields("PCEPICKING", "PICK_LOCATION=NULL", "RECTYPE='C' AND RECID='" + aCO + "' AND SEQNUM='" + aLineNum.ToString() + "' AND ORDERFILLED='O'");
                    }
                    else
                    {
                        Globals.mydmupdate.UpdateFields("ORDRDET", "PICK_LOCATION=NULL", "ORDERNUMBER='" + aCO + "' AND ORDERFILLED='O'");
                        Globals.mydmupdate.UpdateFields("PCEPICKING", "PICK_LOCATION=NULL", "RECTYPE='C' AND RECID='" + aCO + "' AND ORDERFILLED='O'");
                        Globals.DeterminePickLoc(string.Empty, aCO);
                    }
                }

                if (!String.IsNullOrEmpty(aNewStatus))
                {
                    string oldStatus = string.Empty;
                    if (Globals.myGetInfo.GetOrderDetInfo(aCO, "ASCITEMID,ORDERFILLED", aLineNum.ToString(), ref oldStatus))
                    {
                        string ascItemID = ascLibrary.ascStrUtils.GetNextWord(ref oldStatus);
                        if (oldStatus != aNewStatus)
                        {
                            string updstr = string.Empty;
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ORDERFILLED", aNewStatus);
                            ascLibrary.ascStrUtils.ascAppendSetNull(ref updstr, "PICK_LOCATION");
                            ascLibrary.ascStrUtils.ascAppendSetNull(ref updstr, "PICKOPERID");
                            Globals.mydmupdate.UpdateFields("ORDRDET", updstr, "ORDERNUMBER='" + aCO + "' AND LINENUMBER='" + aLineNum.ToString() + "'");
                            string whereStr = "RECTYPE='C' AND RECID='" + aCO + "' AND SEQNUM='" + aLineNum.ToString() + "'";
                            if (aNewStatus == ascLibrary.dbConst.osCANCELLED)
                                whereStr += " AND ORDERFILLED<>'T'";
                            else
                                whereStr += " AND ORDERFILLED='" + oldStatus + "'";

                            Globals.mydmupdate.UpdateFields("PCEPICKING", updstr, whereStr);

                            string auditdata = string.Empty;
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref auditdata, "OLDVALUE", oldStatus);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref auditdata, "NEWVALUE", aNewStatus);
                            Globals.LogTrans.LogTransaction(Globals.curTranDateTime, ascLibrary.dbConst.cmdCP_MARK_DONE, Globals.curSiteID, "Line " + aLineNum.ToString(), "CHANGESTAT", ascItemID, aCO,
                                "", "", "", "", "", "", "", "", "", "", "", "", auditdata, 0, 0, 0, 0, 0);
                            Globals.mydmupdate.ProcessUpdates();

                            Globals.mydmupdate.SetItemMasterQty(ascItemID, "QTYREQUIRED");
                            Globals.mydmupdate.SetItemMasterQty(ascItemID, "QTYSCHEDULED");
                            Globals.dmCustPick.setorderstatus(aCO);
                        }
                    }
                }
                Globals.mydmupdate.ProcessUpdates();

                return (GetOrdrDet(aCO, Globals));
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
                return "EX" + e.ToString();
            }
        }

        public string GetInvAvail(string aCO, long aLineNum, bool aIncludeQC, bool aIncludeExp, ParseNet.GlobalClass Globals)
        {
            try
            {
                string ascitemid = string.Empty;
                Globals.myGetInfo.GetOrderDetInfo(aCO, "ASCITEMID", aLineNum.ToString(), ref ascitemid);

                string sql = "LI.ASCITEMID='" + ascitemid + "'";
                sql += " AND LI.QTYALLOC=0";
                if (!aIncludeQC)
                    sql += " AND LI.QAHOLD='F'";
                if (!aIncludeExp)
                    sql += " AND ( LI.EXPDATE IS NULL OR LI.EXPDATE>GetDate())";
                return( "OK" + WCFUtils.GetInvList(sql, Globals));
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
                return "EX" + e.ToString();
            }
        }

        private void AddVessel(string aCO, string aContainerID, ASCTracFunctionStruct.ascBasicReturnMessageType retval, ParseNet.GlobalClass Globals)
        {
            var shipmentID = Globals.dmCustPick.updatetrailer(aCO, aContainerID, "", "", "", "", "", true);
            Globals.mydmupdate.ProcessUpdates();

            GetVesselList(aCO, aContainerID, retval, Globals);
        }

        //==================================================================================================================================
        private void GetVesselList(string aCO, string aContainerID, ASCTracFunctionStruct.ascBasicReturnMessageType retval, ParseNet.GlobalClass Globals)
        {
            List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo> myList = new List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo>();
            string sqlstr = "SELECT SH.TRAILER_NUM, SH.SHIPMENT_ID, SH.START_DATETIME, S.ORDERNUM, D.PO_ORDERNUM, D.RECEIVER_ID, D.ASN_NUM, SH.CREATE_USERID, SH.STATUS, H.PICKSTATUS, SS.DESCRIPTION";
            sqlstr += " FROM SHIPHDR SH";
            sqlstr += " LEFT JOIN SHIPMENT S ON SH.SHIPMENT_ID=S.SHIPMENT_ID";
            sqlstr += " LEFT JOIN DOCKSCHD D ON SH.SHIPMENT_ID=D.SHIPMENT_ID";
            sqlstr += " LEFT JOIN ORDRHDR H ON H.ORDERNUMBER=S.ORDERNUM";
            sqlstr += " LEFT JOIN SHIPSTAT SS ON SS.STATUSID=H.PICKSTATUS";
            sqlstr += " WHERE SH.SITE_ID='" + Globals.curSiteID + "' and SH.STATUS<>'C'";
            if (!String.IsNullOrEmpty(aCO))
            {
                sqlstr += " AND ( S.ORDERNUM='" + aCO + "' OR D.PO_ORDERNUM='" + aCO + "')";
            }
            if (!String.IsNullOrEmpty(aContainerID))
                sqlstr += " AND SH.TRAILER_NUM='" + aContainerID + "'";
            SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    var rec = new ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo();
                    rec.OrderNumber = myReader["ORDERNUM"].ToString();
                    if( String.IsNullOrEmpty( rec.OrderNumber))
                        rec.OrderNumber = myReader["PO_ORDERNUM"].ToString();
                    rec.ContainerID = myReader["TRAILER_NUM"].ToString();
                    rec.ASNContainerID= myReader["SHIPMENT_ID"].ToString();
                    rec.OrderStatusDesc = myReader["DESCRIPTION"].ToString();
                    if (myReader["STATUS"].ToString().Equals("D"))
                        rec.ContainerStatusDesc = "Loading";
                    else if (myReader["STATUS"].ToString().Equals("S"))
                        rec.ContainerStatusDesc = "Scheduled";
                    else if (myReader["STATUS"].ToString().Equals("G"))
                        rec.ContainerStatusDesc = "Loaded";
                    else if (myReader["STATUS"].ToString().Equals("T"))
                        rec.ContainerStatusDesc = "BOL Printed";
                    else if (myReader["STATUS"].ToString().Equals("N"))
                        rec.ContainerStatusDesc = "Not Scheduled";
                    else
                        rec.ContainerStatusDesc = "Status " + myReader["STATUS"].ToString();
                    rec.CreateDate = ascLibrary.ascUtils.ascStrToDate(myReader["START_DATETIME"].ToString(), DateTime.MinValue);
                    rec.CreateUserID = myReader["CREATE_USERID"].ToString();
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
                retval.ErrorMessage = "No Trailer records found"; //\r\n" + sqlstr;
            }
            else
                retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
        }

        //==================================================================================================================================
        private void GetParcelList(string aCO, string aContainerID, ASCTracFunctionStruct.ascBasicReturnMessageType retval, ParseNet.GlobalClass Globals)
        {
            List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo> myList = new List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo>();
            string sqlstr = "SELECT P.ORDERNUMBER, P.TRACKING_NUM, P.TRANS_DATE, P.VOID, P.USERID, H.PICKSTATUS, SS.DESCRIPTION";
            sqlstr += " FROM PARCEL P JOIN ORDRHDR H ON H.ORDERNUMBER=P.ORDERNUMBER";
            sqlstr += " LEFT JOIN SHIPSTAT SS ON SS.STATUSID=H.PICKSTATUS";
            sqlstr += " WHERE H.SITE_ID='" + Globals.curSiteID + "'";
            if (!String.IsNullOrEmpty(aCO))
                sqlstr += " AND P.ORDERNUMBER='" + aCO + "'";
            if (!String.IsNullOrEmpty(aContainerID))
                sqlstr += " AND P.TRACKING_NUM='" + aContainerID + "'";
            SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    var rec = new ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo();
                    rec.OrderNumber = myReader["ORDERNUMBER"].ToString();
                    rec.ContainerID = myReader["TRACKING_NUM"].ToString();
                    rec.OrderStatusDesc = myReader["DESCRIPTION"].ToString();
                    if (myReader["VOID"].Equals("Y") || myReader["VOID"].Equals("Y"))
                        rec.ContainerStatusDesc = "Void";
                    else
                        rec.ContainerStatusDesc = "OK";
                    rec.CreateDate = ascLibrary.ascUtils.ascStrToDate(myReader["TRANS_DATE"].ToString(), DateTime.MinValue);
                    rec.CreateUserID = myReader["USERID"].ToString();
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
                retval.ErrorMessage = "No Parcel records found"; //\r\n" + sqlstr;
            }
            else
                retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
        }

        //==================================================================================================================================
        private void GetContainrList(string aCO, string aContainerID, ASCTracFunctionStruct.ascBasicReturnMessageType retval, ParseNet.GlobalClass Globals)
        {
            List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo> myList = new List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo>();
            string sqlstr = "SELECT C.ORDERNUM, C.CONTAINER_ID, C.ASN_CONTAINER_ID, H.PICKSTATUS, SS.DESCRIPTION, C.MASTER_CONTAINER_ID, C.MASTER_FLAG";
            sqlstr += " , MIN( C.PICK_DATETIME) AS PICK_DATETIME, MIN( C.PICK_USERID) AS PICK_USERID";
            sqlstr += " FROM CONTAINR C JOIN ORDRHDR H ON H.ORDERNUMBER=C.ORDERNUM";
            sqlstr += " LEFT JOIN SHIPSTAT SS ON SS.STATUSID=H.PICKSTATUS";
            sqlstr += " WHERE H.SITE_ID='" + Globals.curSiteID + "'";
            if (!String.IsNullOrEmpty(aCO))
                sqlstr += " AND C.ORDERNUM='" + aCO + "'";
            if (!String.IsNullOrEmpty(aContainerID))
                sqlstr += " AND ( C.CONTAINER_ID='" + aContainerID + "' OR C.ASN_CONTAINER_ID='" + aContainerID + "' or MASTER_CONTAINER_ID='" + aContainerID + "')";
            sqlstr += " GROUP BY C.ORDERNUM, C.CONTAINER_ID, C.ASN_CONTAINER_ID, H.PICKSTATUS, SS.DESCRIPTION, C.MASTER_CONTAINER_ID, C.MASTER_FLAG";
            SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
            SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);
            myConnection.Open();
            try
            {
                SqlDataReader myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    var rec = new ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo();
                    rec.OrderNumber = myReader["ORDERNUM"].ToString();
                    rec.ContainerID = myReader["CONTAINER_ID"].ToString();
                    rec.ASNContainerID = myReader["ASN_CONTAINER_ID"].ToString();
                    rec.OrderStatusDesc = myReader["DESCRIPTION"].ToString();
                    if (myReader["MASTER_FLAG"].ToString().Equals("T"))
                        rec.ContainerStatusDesc = "Master";
                    else if (!String.IsNullOrEmpty(myReader["MASTER_CONTAINER_ID"].ToString()))
                        rec.ContainerStatusDesc = "Master Tote " + myReader["MASTER_CONTAINER_ID"].ToString();
                    else
                        rec.ContainerStatusDesc = "Standard";
                    rec.CreateDate = ascLibrary.ascUtils.ascStrToDate(myReader["PICK_DATETIME"].ToString(), DateTime.MinValue);
                    rec.CreateUserID = myReader["PICK_USERID"].ToString();
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
                retval.ErrorMessage = "No Container records found"; //\r\n" + sqlstr;
            }
            else
                retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
        }

        public ASCTracFunctionStruct.ascBasicReturnMessageType GetOrderContainerLookup(string aCO, string aContainerID, string aCntrType, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            //List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo> myList = new List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo>();
            try
            {
                if (aCntrType.Equals("V"))
                    GetVesselList(aCO, aContainerID, retval, Globals);
                if (aCntrType.Equals("P"))
                    GetParcelList(aCO, aContainerID, retval, Globals);
                if (aCntrType.Equals("T"))
                    GetContainrList(aCO, aContainerID, retval, Globals);
                Globals.myASCLog.ProcessTran(retval.ErrorMessage, "E");
            }
            catch( Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");

                retval.successful = false;
                retval.ErrorMessage = e.Message;
            }
            return (retval);
        }

        public ASCTracFunctionStruct.ascBasicReturnMessageType NewVessel(string aCO, string aContainerID, string aCntrType, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            //List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo> myList = new List<ASCTracFunctionStruct.CustOrder.OrderContainerLookupInfo>();
            try
            {
                if (aCntrType.Equals("V"))
                    AddVessel(aCO, aContainerID, retval, Globals);
                //if (aCntrType.Equals("P"))
                //    GetParcelList(aCO, aContainerID, retval, Globals);
                //if (aCntrType.Equals("T"))
                //    GetContainrList(aCO, aContainerID, retval, Globals);
                Globals.myASCLog.ProcessTran(retval.ErrorMessage, "E");
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");

                retval.successful = false;
                retval.ErrorMessage = e.Message;
            }
            return (retval);
        }


        private void AddDupSkid(string aCO, string aContainer, string aTranType, string aReportType, string aPrinterID, ParseNet.GlobalClass Globals)
        {
            string updStr = string.Empty;
            if (String.IsNullOrEmpty(aContainer))
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "ORIG_SKIDID", aContainer);
            else
                ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "ORIG_SKIDID", aCO);
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "REQUEST_TIME", DateTime.Now.ToString());
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "PRIORITY", Globals.dmMiscFunc.getDupSkidPriority(aTranType, 5).ToString());
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "ITEMID", aCO);
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "QTY_LABELS", "1");
            ascLibrary.ascStrUtils.ascAppendSetQty(ref updStr, "QTY_DUPLICATE", "0");
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "PRINTERID", aPrinterID);
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "USERID", Globals.curUserID);
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "TRANTYPE", aTranType);
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "MISC1", aReportType);
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "REMOTE_FLAG", "F");
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "SITE_ID", Globals.curSiteID);
            ascLibrary.ascStrUtils.ascAppendSetStr(ref updStr, "PROCESS", "T");

            Globals.myDBUtils.InsertRecord("DUP_SKID", updStr);
        }

        public string PrintOrderContainer(string aCO, string aContainerID, string aCntrType, string aReportType, string aPrinterID, ParseNet.GlobalClass Globals)
        {
            string retval = "ER" + "No Report for " + aCntrType + "," + aReportType;
            try
            {
                if (aCntrType.Equals("V"))
                {
                    //GetVesselList(aCO, aContainerID, myList, Globals);
                    retval = "ER" + "No Vessel Reports available";
                }
                if (aCntrType.Equals("P"))
                {
                    retval = "ER" + "No Parcel Reports available";
                    //GetParcelList(aCO, aContainerID, myList, Globals);
                }
                if (aCntrType.Equals("T"))
                {
                    //GetContainrList(aCO, aContainerID, myList, Globals);
                    if (aReportType.Equals("L"))
                    {
                        // print tote label
                        retval = "OKPrinting Tote label";
                        AddDupSkid(aCO, aContainerID, ascLibrary.dbConst.cmdCP_ASN_PRINT, aReportType, aPrinterID, Globals);

                    }
                    if (aReportType.Equals("P"))
                    {
                        // print tote packlist
                        retval = "OKPrinting Tote Packlist";
                        AddDupSkid(aCO, aContainerID, ascLibrary.dbConst.cmdDOCUMENT, aReportType, aPrinterID, Globals);
                    }
                }
                if (aCntrType.Equals("O")) // just order report
                {

                    switch (aReportType)
                    {
                        case "B": // BOL
                        case "P": // Packlist
                        case "K": // Picklist
                            retval = "OKPrinting Order Report";
                            AddDupSkid(aCO, "", ascLibrary.dbConst.cmdDOCUMENT, aReportType, aPrinterID, Globals);
                            break;
                    }
                }

            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
                return "EX" + e.ToString();
            }
            return(retval);
        }
    }
}
