using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ASCTracRestService.Controllers.InvLookup
{
    public class InvLookup
    {
        public string GetInvList(string aItemID, bool aIncludeQC, bool aIncludeExp, bool aIncludePicked, int aFieldType, string aFieldValue, ParseNet.GlobalClass Globals)
        {
            try
            {
                string sql = "LI.SITE_ID='" + Globals.curSiteID + "'";
                if (!String.IsNullOrEmpty(aItemID))
                    sql += " AND LI.ITEMID='" + aItemID + "' ";
                //sql += " AND LI.QTYALLOC=0";
                if (!String.IsNullOrEmpty(aFieldValue))
                {
                    switch (aFieldType)
                    {
                        case 0:
                            sql += " AND ( LI.SKIDID='" + aFieldValue + "' OR LI.INV_CONTAINER_ID='" + aFieldValue + "')";
                            break;
                        case 1:
                            sql += " AND LI.LOCATIONID='" + aFieldValue + "'";
                            break;
                        case 2:
                            sql += " AND LI.LOTID='" + aFieldValue + "'";
                            break;
                        case 3:
                            sql += " AND LI.ALT_LOTID='" + aFieldValue + "'";
                            break;
                        case 4:
                            sql += " AND LI.EXPDATE='" + aFieldValue + "'";
                            break;
                        case 5:
                            sql += " AND LI.INV_CONTAINER_ID='" + aFieldValue + "'";
                            break;
                        case 6:
                            sql += " AND LI.PICKORDERNUM='" + aFieldValue + "'";
                            break;
                        case 7:
                            sql += " AND LI.SKIDID IN ( SELECT SKIDID FROM SER_NUM WHERE SER_NUM='" + aFieldValue + "')";
                            break;
                        case 100:
                            sql += " AND L.TYPE='" + aFieldValue + "'";
                            break;
                    }
                }
                if (!aIncludePicked)
                    sql += " AND LI.QTYALLOC=0";
                if (!aIncludeQC)
                    sql += " AND LI.QAHOLD='F'";
                if (!aIncludeExp)
                    sql += " AND ( LI.EXPDATE IS NULL OR LI.EXPDATE>GetDate())";
                return ("OK" + WCFUtils.GetInvList(sql, Globals));
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
                return "EX" + e.ToString();
            }

        }

        private void AddLoc(List<ASCTracFunctionStruct.Inventory.InvHistoryType> aList, string aLocation, string aDateTime, string aNotes, double aQtytotal)
        {
            if (!string.IsNullOrEmpty(aLocation))
            {
                ASCTracFunctionStruct.Inventory.InvHistoryType myrec = null;
                foreach (var rec in aList)
                {
                    if (rec.LocationID.Equals(aLocation) && ( rec.QtyTotal == aQtytotal ))
                    {
                        //fFound = true;
                        myrec = rec;
                        break;
                    }
                }
                if (myrec == null)
                {
                    myrec = new ASCTracFunctionStruct.Inventory.InvHistoryType();
                    myrec.LocationID = aLocation;
                    myrec.TranDateTime = ascLibrary.ascUtils.ascStrToDate(aDateTime, DateTime.Now);
                    myrec.Notes = aNotes;
                    myrec.QtyTotal = aQtytotal;

                    aList.Add(myrec);
                }
            }
        }

        public List<ASCTracFunctionStruct.Inventory.InvHistoryType> GetSkidHistoryList(string aSkidID, string aItemID, string aLocationID, ParseNet.GlobalClass Globals)
        {
            var retval = new List<ASCTracFunctionStruct.Inventory.InvHistoryType>();
            try
            {
                string tmp = string.Empty;
                if (String.IsNullOrEmpty(aSkidID) || aSkidID.StartsWith("-"))
                {
                    string sql = "SELECT A.NEWLOCATION, A.OLDLOCATION, A.TRANS_DATE + A.TRANS_TIME AS TRANDT, A.TRANSTYPE, T.DESCRIPTION, A.OLDQTYTOTAL, A.OLDQTYTOTAL + A.CHGQTYTOTAL AS NEWQTYTOTAL";
                    sql += "  FROM AUDITMSG A";
                    sql += "  JOIN TRANSTYP T ON T.TRANSACTIONTYPE = A.TRANSTYPE";
                    sql += "  WHERE A.ITEMID='" + aItemID + "' AND A.SITE_ID='" + Globals.curSiteID + "'";
                    sql += "  AND ( A.NEWLOCATION = '" + aLocationID + "' OR A.OLDLOCATION='" + aLocationID + "')";
                    sql += "  ORDER BY ID ";
                    SqlConnection myConnection2 = new SqlConnection(Globals.myDBUtils.myConnString);
                    SqlCommand myCommand2 = new SqlCommand(sql, myConnection2);
                    myConnection2.Open();
                    try
                    {
                        SqlDataReader myReader2 = myCommand2.ExecuteReader();
                        while (myReader2.Read())
                        {
                            try
                            {
                                AddLoc(retval, myReader2["NEWLOCATION"].ToString(), myReader2["TRANDT"].ToString(), "Audit " + myReader2["TRANSTYPE"].ToString() + " - " + myReader2["DESCRIPTION"].ToString(), ascLibrary.ascUtils.ascStrToDouble(myReader2["NEWQTYTOTAL"].ToString(), 0));
                                AddLoc(retval, myReader2["OLDLOCATION"].ToString(), myReader2["TRANDT"].ToString(), "Audit " + myReader2["TRANSTYPE"].ToString() + " - " + myReader2["DESCRIPTION"].ToString(), ascLibrary.ascUtils.ascStrToDouble(myReader2["OLDQTYTOTAL"].ToString(), 0));
                            }
                            catch
                            {
                                // ignore if field not in myreader2.
                            }
                        }
                    }
                    finally
                    {
                        myConnection2.Close();
                    }

                    string qtytotal = string.Empty;
                    Globals.myDBUtils.ReadFieldFromDB("SELECT QTYTOTAL FROM LOCITEMS WHERE ITEMID='" + aItemID + "' AND LOCATIONID='" + aLocationID + "' AND SITE_ID='" + Globals.curSiteID + "'", "", ref qtytotal);
                    AddLoc(retval, aLocationID, DateTime.Now.ToString(), "Current Location", ascLibrary.ascUtils.ascStrToDouble(qtytotal, 0));

                }
                else
                {
                    if (!Globals.myGetInfo.GetSkidInfo(aSkidID, "QTYTOTAL,PUTDOWN_DATETIME, LOCATIONID, GOTO_LOCATIONID, PHYSNEWLOC, DATELASTCOUNT, PREV_LOCATIONID, ROUTING_LOCATION", ref tmp))
                    {
                        retval.Add(new ASCTracFunctionStruct.Inventory.InvHistoryType());
                        retval[0].successful = false;
                        retval[0].ReturnMessage = "License " + aSkidID + " not found";
                    }
                    else
                    {
                        string sql = "SELECT A.NEWLOCATION, A.OLDLOCATION, A.TRANS_DATE + A.TRANS_TIME AS TRANDT, A.TRANSTYPE, T.DESCRIPTION, A.OLDQTYTOTAL, A.OLDQTYTOTAL + A.CHGQTYTOTAL AS NEWQTYTOTAL";
                        sql += "  FROM AUDITMSG A";
                        sql += "  JOIN TRANSTYP T ON T.TRANSACTIONTYPE = A.TRANSTYPE";
                        sql += "  WHERE A.SKIDID='" + aSkidID + "'";
                        sql += "  ORDER BY ID ";
                        SqlConnection myConnection2 = new SqlConnection(Globals.myDBUtils.myConnString);
                        SqlCommand myCommand2 = new SqlCommand(sql, myConnection2);
                        myConnection2.Open();
                        try
                        {
                            SqlDataReader myReader2 = myCommand2.ExecuteReader();
                            while (myReader2.Read())
                            {
                                try
                                {
                                    AddLoc(retval, myReader2["NEWLOCATION"].ToString(), myReader2["TRANDT"].ToString(), "Audit " + myReader2["TRANSTYPE"].ToString() + " - " + myReader2["DESCRIPTION"].ToString(), ascLibrary.ascUtils.ascStrToDouble(myReader2["NEWQTYTOTAL"].ToString(), 0));
                                    AddLoc(retval, myReader2["OLDLOCATION"].ToString(), myReader2["TRANDT"].ToString(), "Audit " + myReader2["TRANSTYPE"].ToString() + " - " + myReader2["DESCRIPTION"].ToString(), ascLibrary.ascUtils.ascStrToDouble(myReader2["OLDQTYTOTAL"].ToString(), 0));
                                }
                                catch
                                {
                                    // ignore if field not in myreader2.
                                }
                            }

                        }
                        finally
                        {
                            myConnection2.Close();
                        }

                        double qtytotal = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tmp), 0);
                        DateTime putdownDT = ascLibrary.ascUtils.ascStrToDate(ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.Now);
                        AddLoc(retval, ascLibrary.ascStrUtils.GetNextWord(ref tmp), putdownDT.ToString(), "Current Location", qtytotal);
                        AddLoc(retval, ascLibrary.ascStrUtils.GetNextWord(ref tmp), DateTime.Now.ToString(), "Goto Location", qtytotal);
                        AddLoc(retval, ascLibrary.ascStrUtils.GetNextWord(ref tmp), ascLibrary.ascStrUtils.GetNextWord(ref tmp), "Physical Location", qtytotal);
                        AddLoc(retval, ascLibrary.ascStrUtils.GetNextWord(ref tmp), putdownDT.ToString(), "Previous Location", qtytotal);
                        AddLoc(retval, ascLibrary.ascStrUtils.GetNextWord(ref tmp), putdownDT.ToString(), "Routing Location", qtytotal);

                    }
                }
            }
            catch (Exception e)
            {
                if (retval.Count == 0)
                    retval.Add(new ASCTracFunctionStruct.Inventory.InvHistoryType());
                retval[0].successful = false;
                retval[0].ReturnMessage = e.Message;


                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);
        }

        public ASCTracFunctionStruct.Inventory.InvCountType GetSkidInfo(string aSkidID, string aItemID, string aLocationID, ParseNet.GlobalClass Globals)
        {
            var retval = new ASCTracFunctionStruct.Inventory.InvCountType();
            try
            {
                string errmsg = string.Empty;
                string tstr = string.Empty;
                if (!String.IsNullOrEmpty(aSkidID))
                {
                    if (!Globals.myGetInfo.GetSkidInfo(aSkidID, "SITE_ID, PICKORDERNUM,ASCITEMID,ITEMID,INV_CONTAINER_ID,LOCATIONID,QTYTOTAL,QTY_DUAL_UNIT,QAHOLD", ref tstr))
                        errmsg = "License " + aSkidID + " not found";
                }
                else
                {
                    if (!Globals.myDBUtils.ReadFieldFromDB("SELECT SITE_ID, PICKORDERNUM,ASCITEMID,ITEMID,NULL,LOCATIONID,QTYTOTAL,QTY_DUAL_UNIT,QAHOLD FROM LOCITEMS WHERE ITEMID='" + aItemID + "' AND LOCATIONID='" + aLocationID + "' AND SITE_ID='" + Globals.curSiteID + "'", "", ref tstr))
                        errmsg = "Inventory Record not found";

                }
                if (string.IsNullOrEmpty(errmsg))
                {
                    if (ascLibrary.ascStrUtils.GetNextWord(ref tstr) != Globals.curSiteID)
                        errmsg = ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_GEN_WRONG_SITE);
                    else if (ascLibrary.ascStrUtils.GetNextWord(ref tstr) != string.Empty)
                        errmsg = ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_GEN_PICKED);
                    else
                    {
                        string ascItemID = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                        string itemInfo = Globals.myGetInfo.GetASCItemData(ascItemID, "DESCRIPTION, STOCK_UOM, DUAL_UNIT_ITEM");
                        retval.invRecord = new ASCTracFunctionStruct.Inventory.InvType();
                        retval.invRecord.SkidID = aSkidID;
                        retval.invRecord.ItemID = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                        retval.invRecord.ItemDescription = ascLibrary.ascStrUtils.GetNextWord(ref itemInfo);
                        retval.invRecord.StockUOM = ascLibrary.ascStrUtils.GetNextWord(ref itemInfo);
                        retval.invRecord.InvContainerID = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                        retval.invRecord.LocationID = ascLibrary.ascStrUtils.GetNextWord(ref tstr);
                        retval.invRecord.QtyTotal = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tstr), 0);
                        retval.invRecord.QtyDualUnit = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref tstr), 0);
                        if (!ascLibrary.ascStrUtils.GetNextWord(ref itemInfo).Equals("T"))
                            retval.invRecord.QtyDualUnit = -1;
                        retval.invRecord.QAHold = ascLibrary.ascStrUtils.GetNextWord(ref tstr).Equals("T");

                        retval.suggLocationID = Globals.dmMove.getnextputloc(ascItemID, aSkidID, string.Empty, ascLibrary.dbConst.cmdMOVE, false, retval.invRecord.QtyTotal, 1);
                    }
                }
            }
            catch (Exception e)
            {
                retval.successful = false;
                retval.ReturnMessage = e.Message;


                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);

        }

        //================================================================================
        public string UpdateSkid(ASCTracFunctionStruct.Inventory.InvCountType aInvRec, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            string status = "Initializing";
            try
            {
                ascLibrary.TDBReturnType tmpRetVal = ascLibrary.TDBReturnType.dbrtOK;
                string ascItemID = string.Empty;
                //string outfile = string.Empty;
                //string outfile2 = string.Empty;
                //string tmpfile = string.Empty;
                //string prntype = string.Empty;
                string submsg = string.Empty;
                string tmp = string.Empty;
                if (String.IsNullOrEmpty(aInvRec.invRecord.SkidID))
                    Globals.myDBUtils.ReadFieldFromDB("SELECT ASCITEMID FROM LOCITEMS WHERE ITEMID='" + aInvRec.invRecord.ItemID + "' AND LOCATIONID='" + aInvRec.invRecord.LocationID + "' AND SITE_ID='" + Globals.curSiteID + "'", "", ref ascItemID);
                else
                    Globals.myGetInfo.GetSkidInfo(aInvRec.invRecord.SkidID, "ASCITEMID", ref ascItemID);

                //if (!String.IsNullOrEmpty(aInvRec.lblPrinterID) && (aInvRec.qtyLbls > 0))
                //    tmpRetVal = Globals.dmSimple.GetPrinterInfo(aInvRec.lblPrinterID, ascItemID, ref outfile, ref outfile2, ref tmpfile, ref prntype);
                if (tmpRetVal.Equals(ascLibrary.TDBReturnType.dbrtOK))
                {
                    double tQty = 0;
                    if (aInvRec.NewQtyTotal <= 0)
                    {
                        status = "Scrapping";
                        if (String.IsNullOrEmpty(aInvRec.invRecord.SkidID))
                            tmpRetVal = Globals.dmInventory.scrapitemsfromloc(ascItemID, aInvRec.invRecord.LocationID, aInvRec.AdjReason, string.Empty,
                                aInvRec.Comments, aInvRec.invRecord.QtyTotal);
                        else
                            tmpRetVal = Globals.dmInventory.scrapskid(aInvRec.invRecord.SkidID, aInvRec.AdjReason, string.Empty, string.Empty, ascLibrary.dbConst.cmdUPDATE_SKID,
                            string.Empty, aInvRec.CostCenter, aInvRec.ResponsibleSiteID, aInvRec.Comments, "", false, aInvRec.invRecord.QtyTotal);
                    }
                    else if ((aInvRec.NewQtyTotal == aInvRec.invRecord.QtyTotal) && !String.IsNullOrEmpty(aInvRec.NewLocationID))
                    {
                        status = "PutDown";
                        if (String.IsNullOrEmpty(aInvRec.invRecord.SkidID))
                            tmpRetVal = Globals.dmMove.putdownitems(Globals.curTranDateTime, ascItemID, aInvRec.NewLocationID, aInvRec.invRecord.LocationID, aInvRec.fPassword, aInvRec.invRecord.QtyTotal, ascLibrary.TInventory.itTOTAL, ref submsg);
                        else
                            tmpRetVal = Globals.dmMove.putdownskid(Globals.curTranDateTime, aInvRec.invRecord.SkidID, aInvRec.NewLocationID, string.Empty, string.Empty, string.Empty,
                            aInvRec.fPassword, ascLibrary.dbConst.cmdDOWN_SKID, ref submsg, ref tmp);
                    }
                    else if ((aInvRec.NewQtyTotal != aInvRec.invRecord.QtyTotal) && (aInvRec.NewQtyDualUnit != aInvRec.invRecord.QtyDualUnit))
                    {
                        status = "Updating";
                        if (String.IsNullOrEmpty(aInvRec.invRecord.SkidID))
                            tmpRetVal = Globals.dmInventory.updateitemloc(ascLibrary.dbConst.cmdUPDATE_COUNT, ascItemID, aInvRec.invRecord.LocationID, string.Empty, string.Empty,
                                string.Empty, string.Empty, aInvRec.AdjReason, string.Empty, string.Empty, aInvRec.NewQtyTotal, true, ref tQty, ref submsg);
                        else
                            tmpRetVal = Globals.dmInventory.UpdateSkidInfo( aInvRec.invRecord.SkidID, aInvRec.NewLocationID, aInvRec.NewQtyDualUnit.ToString(), "", "", "",
                                aInvRec.AdjReason, "", "", "", aInvRec.CostCenter, aInvRec.ResponsibleSiteID, aInvRec.Comments, aInvRec.ProjectNumber, string.Empty,
                                aInvRec.NewQtyTotal, false, false, ref submsg, ref tmp);
                    }
                }
                if (tmpRetVal.Equals(ascLibrary.TDBReturnType.dbrtOK))
                {
                    if (!String.IsNullOrEmpty(aInvRec.lblPrinterID) && (aInvRec.qtyLbls > 0))
                    {
                        status += "; PrintLabel";
                        if (String.IsNullOrEmpty(aInvRec.invRecord.SkidID))
                            Globals.dmMiscFunc.AddToSkidDupWithQty(ascItemID, "-", string.Empty, aInvRec.lblPrinterID, ascLibrary.dbConst.cmdPRINT_ITEM,
                                string.Empty, string.Empty, string.Empty, string.Empty, aInvRec.Comments, true, aInvRec.qtyLbls, 0, 4, 0, 0);
                        else
                            Globals.dmMiscFunc.AddToSkidDupWithQty(aInvRec.invRecord.ItemID, aInvRec.invRecord.SkidID, string.Empty, aInvRec.lblPrinterID, ascLibrary.dbConst.cmdPRINT_LABEL,
                                string.Empty, string.Empty, string.Empty, string.Empty, aInvRec.Comments, true, aInvRec.qtyLbls, 0, 4, 0, 0);
                    }
                }
                switch (tmpRetVal)
                {
                    case ascLibrary.TDBReturnType.dbrtOK:
                    case ascLibrary.TDBReturnType.dbrtOK_NO:
                        Globals.mydmupdate.ProcessUpdates();
                        retval = ascLibrary.dbConst.stOK;
                        break;

                    case ascLibrary.TDBReturnType.dbrtBAD_PSSWD:
                        retval = ascLibrary.dbConst.stQuery + submsg;
                        break;
                    case ascLibrary.TDBReturnType.dbrtNEED_INFO:
                    case ascLibrary.TDBReturnType.dbrtUNKNOWN_ERR:
                        retval = ascLibrary.dbConst.stERR + submsg;
                        break;
                    default:
                        retval = ParseNet.dmascmessages.GetErrorMsg(tmpRetVal) + "\r\nReturn ID " + tmpRetVal.ToString();
                        break;
                }
                retval += "\r\n" + status;
            }
            catch (Exception e)
            {
                retval = ascLibrary.dbConst.stERR + status + "\r\n" + e.Message;
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);
        }

        public List<ASCTracFunctionStruct.Inventory.PhysCountLocType> GetPhysLocs(string aCountNum, string aStartLocID, string aEndLocID, string aStartItemID, string aEndItemID,
                        bool aIncludeLocCounted, bool aIncludeLocUncounted, bool aIncludeReviewed,
            bool aIncludeInvAll, bool aIncludeQtyVar, bool aIncludeLocChg, bool aIncludeLocEmpty, ParseNet.GlobalClass Globals)
        {
            var retval = new List<ASCTracFunctionStruct.Inventory.PhysCountLocType>();
            try
            {
                string sqlstr = "SELECT L.LOCATIONID, L.PHYSCOUNTSTATUS, L.LASTCOUNTDATE, L.PHYS_REVIEW_FLAG, L.PHYS_REVIEW_USERID, L.PHYS_REVIEW_DATETIME" +
                    ", COUNT( LI.SKIDID) as NUMSKIDS" +
                    ", SUM(( CASE WHEN ISNULL( LI.PHYSNEWLOC, '') <> '' THEN 1 ELSE 0 END)) AS NUMMOVES" +
                    ", SUM(( CASE WHEN LI.LASTPHYSADJ<> 0 THEN 1 ELSE 0 END)) AS NUMVARS";
                sqlstr += " FROM LOC L" +
                    " LEFT JOIN LOCITEMS LI ON ( L.LOCATIONID=LI.LOCATIONID OR L.LOCATIONID=LI.PHYSNEWLOC) AND L.SITE_ID=LI.SITE_ID";
                if (!String.IsNullOrEmpty(aStartItemID))
                    sqlstr += " AND LI.ITEMID>='" + aStartItemID + "'";
                if (!String.IsNullOrEmpty(aEndItemID))
                    sqlstr += " AND LI.ITEMID<='" + aEndItemID + "'";
                string sqlLocitems = string.Empty;
                if (!aIncludeInvAll)
                {
                    if (aIncludeQtyVar)
                    {
                        sqlLocitems = "( LI.LASTPHYSADJ<>0 )";
                    }
                    if (aIncludeLocChg)
                    {
                        if (String.IsNullOrEmpty(sqlLocitems))
                            sqlLocitems = "( ISNULL( LI.PHYSNEWLOC, '') <> '')";
                        else
                            sqlLocitems += " OR ( ISNULL( LI.PHYSNEWLOC, '') <> '')";
                    }
                    if (aIncludeLocEmpty)
                    {
                        if (String.IsNullOrEmpty(sqlLocitems))
                            sqlLocitems = "( LI.SKIDID IS NULL)";
                        else
                            sqlLocitems += " OR ( LI.SKIDID IS NULL)";
                    }
                }
                sqlstr += " WHERE L.COUNT_NUM='" + aCountNum + "' AND L.SITE_ID='" + Globals.curSiteID + "'";
                if (!String.IsNullOrEmpty(sqlLocitems))
                    sqlstr += " AND (" + sqlLocitems + ")";
                if (!String.IsNullOrEmpty(aStartLocID))
                    sqlstr += " AND L.LOCATIONID>='" + aStartLocID + "'";
                if (!String.IsNullOrEmpty(aEndLocID))
                    sqlstr += " AND L.LOCATIONID<='" + aEndLocID + "'";
                if (!aIncludeReviewed)
                    sqlstr += " AND ISNULL( L.PHYS_REVIEW_FLAG, 'F') <>'T'";

                if (aIncludeLocCounted)
                {
                    if (!aIncludeLocUncounted)
                        sqlstr += " AND L.PHYSCOUNTSTATUS IN ( 'R')";
                }
                else if (aIncludeLocUncounted)
                    sqlstr += " AND L.PHYSCOUNTSTATUS NOT IN ( 'R')";

                sqlstr += " GROUP BY L.LOCATIONID, L.PHYSCOUNTSTATUS, L.LASTCOUNTDATE, L.PHYS_REVIEW_FLAG, L.PHYS_REVIEW_USERID, L.PHYS_REVIEW_DATETIME";
                sqlstr += " ORDER BY L.LOCATIONID";
                Globals.myASCLog.updateSQL(sqlstr);
                SqlConnection myConnection2 = new SqlConnection(Globals.myDBUtils.myConnString);
                SqlCommand myCommand2 = new SqlCommand(sqlstr, myConnection2);
                myConnection2.Open();
                try
                {
                    SqlDataReader myReader2 = myCommand2.ExecuteReader();
                    while (myReader2.Read())
                    {
                        try
                        {
                            var rec = new ASCTracFunctionStruct.Inventory.PhysCountLocType();
                            rec.LocationID = myReader2["LOCATIONID"].ToString();
                            rec.numSkids = ascLibrary.ascUtils.ascStrToInt(myReader2["NUMSKIDS"].ToString(), 0);
                            rec.numVars = ascLibrary.ascUtils.ascStrToInt(myReader2["NUMVARS"].ToString(), 0);
                            rec.numMoves = ascLibrary.ascUtils.ascStrToInt(myReader2["NUMMOVES"].ToString(), 0);
                            rec.CountedDateTime = ascLibrary.ascUtils.ascStrToDate(myReader2["LASTCOUNTDATE"].ToString(), DateTime.MinValue);
                            if (myReader2["PHYSCOUNTSTATUS"].ToString() == ascLibrary.dbConst.pcREADY)
                                rec.Status = "Counted";
                            if (myReader2["PHYSCOUNTSTATUS"].ToString() == ascLibrary.dbConst.pcNONE)
                                rec.Status = "Not in Physical";
                            if (myReader2["PHYSCOUNTSTATUS"].ToString() == ascLibrary.dbConst.pcLOCATION)
                                rec.Status = "Not Counted";
                            if (myReader2["PHYSCOUNTSTATUS"].ToString() == ascLibrary.dbConst.pcPHYSICAL)
                                rec.Status = "Not Counted";
                            if( myReader2["PHYS_REVIEW_FLAG"].ToString() == "T")
                            {
                                rec.ReviewFlag = "Reviewed";
                                rec.ReviewDateTime = ascLibrary.ascUtils.ascStrToDate(myReader2["PHYS_REVIEW_DATETIME"].ToString(), DateTime.Now);
                                rec.ReviewUserID = myReader2["PHYS_REVIEW_USERID"].ToString();
                            }
                            else if( myReader2["PHYS_REVIEW_FLAG"].ToString() == "N")
                                rec.ReviewFlag = "Not Needed";
                            else
                                rec.ReviewFlag = "Not Reviewed";
                            retval.Add(rec);
                        }
                        catch
                        {
                            // ignore if field not in myreader2.
                        }
                    }
                }
                finally
                {
                    myConnection2.Close();
                }
            }
            catch (Exception e)
            {
                if (retval.Count == 0)
                    retval.Add(new ASCTracFunctionStruct.Inventory.PhysCountLocType());
                retval[0].successful = false;
                retval[0].ReturnMessage = e.ToString(); //.Message;


                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);
        }

        public List<ASCTracFunctionStruct.Inventory.InvType> GetPhysLocItems(string aCountNum, string aLocationID, ParseNet.GlobalClass Globals)
        {
            var retval = new List<ASCTracFunctionStruct.Inventory.InvType>();
            try
            {
                string sqlstr = "SELECT I.DESCRIPTION, I.STOCK_UOM, I.DUAL_UNIT_ITEM, LI.* FROM LOCITEMS LI";
                sqlstr += " JOIN ITEMMSTR I ON I.ASCITEMID=LI.ASCITEMID";
                sqlstr += " WHERE LI.LOCATIONID='" + aLocationID + "' AND LI.SITE_ID='" + Globals.curSiteID + "'";
                Globals.myASCLog.updateSQL(sqlstr);
                SqlConnection myConnection2 = new SqlConnection(Globals.myDBUtils.myConnString);
                SqlCommand myCommand2 = new SqlCommand(sqlstr, myConnection2);
                myConnection2.Open();
                try
                {
                    SqlDataReader myReader2 = myCommand2.ExecuteReader();
                    while (myReader2.Read())
                    {
                        try
                        {
                            var rec = new ASCTracFunctionStruct.Inventory.InvType();
                            rec.LocationID = myReader2["LOCATIONID"].ToString();
                            rec.SkidID = myReader2["SKIDID"].ToString();
                            rec.InvContainerID = myReader2["INV_CONTAINER_ID"].ToString();
                            rec.ItemID = myReader2["ITEMID"].ToString();
                            rec.LocationID = myReader2["LOCATIONID"].ToString();
                            rec.ItemDescription = myReader2["DESCRIPTION"].ToString();
                            rec.StockUOM = myReader2["STOCK_UOM"].ToString();

                            rec.QtyTotal = ascLibrary.ascUtils.ascStrToDouble(myReader2["QTYTOTAL"].ToString(), 0);
                            rec.QAHold = myReader2["QAHOLD"].ToString().Equals("T");
                            if (!myReader2["DUAL_UNIT_ITEM"].ToString().Equals("T"))
                                rec.QtyDualUnit = -1;
                            else
                                rec.QtyDualUnit = ascLibrary.ascUtils.ascStrToDouble(myReader2["QTY_DUAL_UNIT"].ToString(), 0);
                            rec.PhysLoc = myReader2["PHYSNEWLOC"].ToString();
                            rec.PhysAdj = ascLibrary.ascUtils.ascStrToDouble(myReader2["LASTPHYSADJ"].ToString(), 0);
                            rec.PhysCountStatus = myReader2["PHYSCOUNTSTATUS"].ToString();
                            //rec.ExpireDate

                            retval.Add(rec);
                        }
                        catch
                        {
                            // ignore if field not in myreader2.
                        }
                    }
                }
                finally
                {
                    myConnection2.Close();
                }
            }
            catch (Exception e)
            {
                if (retval.Count == 0)
                    retval.Add(new ASCTracFunctionStruct.Inventory.InvType());
                retval[0].successful = false;
                retval[0].ReturnMessage = e.ToString(); //.Message;


                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);

        }

        //=================================================
        public string RecountPhys(string aLocID, string aItemID, string aSkidID, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            string status = "Initializing";
            try
            {
                ascLibrary.TDBReturnType tmpRetVal = ascLibrary.TDBReturnType.dbrtOK;
                if (String.IsNullOrEmpty(aSkidID))
                {
                    status = "Recount Location " + aLocID;
                    tmpRetVal = Globals.dmCount.physlocreset(Globals.curTranDateTime, aLocID);
                }
                else
                {
                    status = "Recount Inventory " + aLocID;
                }
                switch (tmpRetVal)
                {
                    case ascLibrary.TDBReturnType.dbrtOK:
                        Globals.mydmupdate.ProcessUpdates();
                        retval = ascLibrary.dbConst.stOK;
                        break;
                    default: retval = ParseNet.dmascmessages.GetErrorMsg(tmpRetVal);
                        break;
                }
            }
            catch (Exception e)
            {
                retval = ascLibrary.dbConst.stERR + status + "\r\n" + e.Message;
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }

            return (retval);
        }

        public string PhysCount(string aCountNum, string aLocID, string aItemID, string aSkidID, bool aReviewOnly, double aNewQty, double aNewQtyDualUnit, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            string status = "Initializing";
            try
            {
                ascLibrary.TDBReturnType tmpRetVal = ascLibrary.TDBReturnType.dbrtOK;
                string tmp1 = string.Empty;
                string tmp2 = string.Empty;
                if (String.IsNullOrEmpty(aSkidID))
                {
                    if (!aReviewOnly)
                    {
                        string sqlstr = "SELECT DISTINCT LI.ASCITEMID FROM LOCITEMS LI ";
                        sqlstr += " WHERE LI.LOCATIONID='" + aLocID + "' AND LI.SITE_ID='" + Globals.curSiteID + "'";
                        Globals.myASCLog.updateSQL(sqlstr);
                        SqlConnection myConnection2 = new SqlConnection(Globals.myDBUtils.myConnString);
                        SqlCommand myCommand2 = new SqlCommand(sqlstr, myConnection2);
                        myConnection2.Open();
                        try
                        {
                            SqlDataReader myReader2 = myCommand2.ExecuteReader();
                            while (myReader2.Read() && tmpRetVal.Equals(ascLibrary.TDBReturnType.dbrtOK))
                            {
                                status = "Phys Count Location " + aLocID + ", Item " + myReader2["ASCITEMID"].ToString();
                                tmpRetVal = Globals.dmCount.physskidsdone(Globals.curTranDateTime, myReader2["ASCITEMID"].ToString(), aLocID, "P", aCountNum);
                            }
                        }
                        finally
                        {
                            myConnection2.Close();
                        }
                    }
                    if( tmpRetVal.Equals( ascLibrary.TDBReturnType.dbrtOK))
                    {
                        if (!aReviewOnly)
                        {
                            Globals.mydmupdate.ProcessUpdates();
                            status = "Phys Count Location Done " + aLocID;
                            tmpRetVal = Globals.dmCount.physlocdone(Globals.curTranDateTime, aLocID, string.Empty, aCountNum, ref tmp1);
                        }
                        string updstr = string.Empty;
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PHYS_REVIEW_FLAG", "T");
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PHYS_REVIEW_USERID", Globals.curUserID);
                        ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "PHYS_REVIEW_DATETIME", Globals.curTranDateTime.ToString());
                        Globals.mydmupdate.updateloc(aLocID, string.Empty, updstr);
                    }
                }
                else
                {
                    status = "Phys Count Inventory " + aLocID;
                    if (aSkidID.StartsWith("-"))
                        tmpRetVal = Globals.dmCount.physcountitemloc(Globals.curTranDateTime, aItemID, aLocID, aCountNum, aNewQty);
                    else
                        tmpRetVal = Globals.dmCount.physcountskid(Globals.curTranDateTime, aSkidID, aLocID, aNewQtyDualUnit.ToString(), string.Empty, aCountNum, string.Empty,
                            aNewQty, ref tmp1, ref tmp2);
                }
                switch (tmpRetVal)
                {
                    case ascLibrary.TDBReturnType.dbrtOK:
                        Globals.mydmupdate.ProcessUpdates();
                        retval = ascLibrary.dbConst.stOK;
                        break;
                    default: retval = ParseNet.dmascmessages.GetErrorMsg(tmpRetVal);
                        break;
                }

            }
            catch (Exception e)
            {
                retval = ascLibrary.dbConst.stERR + status + "\r\n" + e.Message;
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }

            return (retval);
        }

    }
}