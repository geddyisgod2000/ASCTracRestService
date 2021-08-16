using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace ASCTracRestService.Controllers.QCTest
{
    public class qc
    {
        //=======================================================================
        public ASCTracFunctionStruct.QC.QCInventoryType GetSkidQCInfo(ParseNet.GlobalClass Globals, string aRecType, string aRecID, ref string errmsg)
        {
            ASCTracFunctionStruct.QC.QCInventoryType retval = new ASCTracFunctionStruct.QC.QCInventoryType();
            try
            {
                string skidinfo = string.Empty;
                string wo = string.Empty;
                string lotid = string.Empty;
                string ascitemid = string.Empty;
                string sqlstr;
                if (aRecType.StartsWith("WO"))
                {
                    wo = aRecID;
                    sqlstr = "SELECT H.SITE_ID, H.PROD_ASCITEMID AS ASCITEMID, H.PROD_ITEMID AS ITEMID, I.DESCRIPTION";
                    sqlstr += " FROM WO_HDR H JOIN ITEMMSTR I ON I.ASCITEMID=H.PROD_ASCITEMID WHERE H.WORKORDER_ID='" + wo + "'";
                    if (!Globals.myDBUtils.ReadFieldFromDB(sqlstr, "", ref skidinfo))
                        errmsg = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_SKID);
                    else if (ascLibrary.ascStrUtils.GetNextWord(ref skidinfo) != Globals.curSiteID)
                        errmsg = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                    else
                    {
                        ascitemid = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        retval.invRecord = new ASCTracFunctionStruct.Inventory.InvType();
                        retval.invRecord.SkidID = wo;
                        retval.invRecord.ItemID = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        retval.invRecord.ItemDescription = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        //retval.QtyTotal = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref skidinfo), 0);
                        retval.qcHoldList = new List<ASCTracFunctionStruct.QC.QCReasonType>();
                        retval.qcTestList = new List<ASCTracFunctionStruct.QC.QCTests>();
                        retval.qcTestSetupList = new List<ASCTracFunctionStruct.QC.QCTestSetup>();

                    }
                }
                else
                {
                    sqlstr = "SELECT LI.SITE_ID, LI.LOTID, LI.WORKORDER_ID, LI.ASCITEMID, LI.ITEMID, I.DESCRIPTION, LI.QTYTOTAL ";
                    sqlstr += " FROM LOCITEMS LI JOIN ITEMMSTR I ON I.ASCITEMID=LI.ASCITEMID WHERE LI.SKIDID='" + aRecID+ "'";
                    if (!Globals.myDBUtils.ReadFieldFromDB(sqlstr, "", ref skidinfo))
                        errmsg = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_SKID);
                    else if (ascLibrary.ascStrUtils.GetNextWord(ref skidinfo) != Globals.curSiteID)
                        errmsg = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtWRONG_SITE);
                    else
                    {
                        lotid = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        wo = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        ascitemid = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        retval.invRecord = new ASCTracFunctionStruct.Inventory.InvType();
                        retval.invRecord.SkidID = aRecID;
                        retval.invRecord.ItemID = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        retval.invRecord.ItemDescription = ascLibrary.ascStrUtils.GetNextWord(ref skidinfo);
                        retval.invRecord.QtyTotal = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref skidinfo), 0);
                        retval.qcHoldList = new List<ASCTracFunctionStruct.QC.QCReasonType>();
                        retval.qcTestList = new List<ASCTracFunctionStruct.QC.QCTests>();
                        retval.qcTestSetupList = new List<ASCTracFunctionStruct.QC.QCTestSetup>();

                        sqlstr = "SELECT QC.REF_NUM, QC.QAHOLD, QC.REASONFORHOLD, R.DESCRIPTION AS REASON_DESCRIPTION, QC.HOLD_DATETIME, QC.EXPECTED_QC_RELEASE_DATE, QC.MAF_NUM, MAF.DESCRIPTION, MAF.MAF_STATUS_CODE, MAF.ACTION, MAF.MAF_CATID";
                        sqlstr += " FROM LOCITEMS_QC QC";
                        sqlstr += " LEFT JOIN REASNCDS R ON R.REASON_CODE=QC.REASONFORHOLD";
                        sqlstr += " LEFT JOIN MAF ON MAF.MAF_NUM=QC.MAF_NUM";
                        sqlstr += " WHERE QC.SKIDID='" + aRecID+ "'";


                        SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                        SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);
                        myConnection.Open();
                        try
                        {
                            SqlDataReader myReader = myCommand.ExecuteReader();
                            while (myReader.Read())
                            {
                                ASCTracFunctionStruct.QC.QCReasonType qcHold = new ASCTracFunctionStruct.QC.QCReasonType();
                                qcHold.RefNum = ascLibrary.ascUtils.ascStrToInt(myReader["REF_NUM"].ToString(), 0);
                                qcHold.Reason = myReader["REASONFORHOLD"].ToString();
                                qcHold.ReasonDescription = myReader["REASON_DESCRIPTION"].ToString();
                                qcHold.OnHold = myReader["QAHOLD"].Equals("T");
                                qcHold.HoldDatetime = ascLibrary.ascUtils.ascStrToDate(myReader["HOLD_DATETIME"].ToString(), DateTime.MinValue);
                                qcHold.ExpectedReleaseDate = ascLibrary.ascUtils.ascStrToDate(myReader["EXPECTED_QC_RELEASE_DATE"].ToString(), DateTime.MinValue);
                                qcHold.MafNum = ascLibrary.ascUtils.ascStrToInt(myReader["MAF_NUM"].ToString(), 0);
                                qcHold.MafAction = myReader["ACTION"].ToString();
                                qcHold.MafDescription = myReader["DESCRIPTION"].ToString();
                                qcHold.MafStatus = myReader["MAF_STATUS_CODE"].ToString();
                                qcHold.MafCatID = myReader["MAF_CATID"].ToString();
                                retval.qcHoldList.Add(qcHold);
                            }
                        }
                        finally
                        {
                            myConnection.Close();
                        }
                    }
                }
                if (String.IsNullOrEmpty(errmsg))
                {
                    sqlstr = "SELECT QC.BATCH_NUM, QC.QUESTION_NUM, QC.PROMPT, QC.ANSWER, QC.PASSFAIL, QC.TEST_DATETIME, QC.TEST_USERID, QC.HOLD_REASON";
                    sqlstr += " FROM QC_TESTS QC";
                    sqlstr += " WHERE ( QC.SKIDID='" + aRecID + "')";
                    sqlstr += " or ( QC.ASCITEMID='" + ascitemid + "' AND LOTID='" + lotid + "')";
                    sqlstr += " or ( QC.ASCITEMID='" + ascitemid + "' AND WORKORDER_ID='" + wo + "')";
                    sqlstr += " ORDER BY QC.BATCH_NUM, QC.QUESTION_NUM";

                    {
                        SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                        SqlCommand myCommand = new SqlCommand(sqlstr, myConnection);
                        myConnection.Open();
                        try
                        {
                            ASCTracFunctionStruct.QC.QCTests myTests = null;
                            string currBatch = "-1";
                            SqlDataReader myReader = myCommand.ExecuteReader();
                            while (myReader.Read())
                            {
                                if (currBatch != myReader["BATCH_NUM"].ToString())
                                {
                                    currBatch = myReader["BATCH_NUM"].ToString();
                                    myTests = new ASCTracFunctionStruct.QC.QCTests();
                                    myTests.BatchNum = currBatch;
                                    myTests.testList = new List<ASCTracFunctionStruct.QC.QCTest>();
                                    myTests.OnHold = false;
                                    retval.qcTestList.Add(myTests);
                                }

                                if (!String.IsNullOrEmpty(myReader["HOLD_REASON"].ToString()))
                                {
                                    myTests.OnHold = true;
                                    myTests.Reason = myReader["HOLD_REASON"].ToString();
                                }

                                ASCTracFunctionStruct.QC.QCTest myTest = new ASCTracFunctionStruct.QC.QCTest();
                                myTest.QuestionNum = ascLibrary.ascUtils.ascStrToInt(myReader["QUESTION_NUM"].ToString(), 0);
                                myTest.TestPrompt = myReader["PROMPT"].ToString();
                                myTest.TestAnswer = myReader["ANSWER"].ToString();
                                myTest.Passed = myReader["PASSFAIL"].ToString().Equals("T");
                                myTest.TestDatetime = ascLibrary.ascUtils.ascStrToDate(myReader["TEST_DATETIME"].ToString(), DateTime.MinValue);
                                myTest.TestUserID = myReader["TEST_USERID"].ToString();
                                myTests.testList.Add(myTest);
                            }
                        }
                        finally
                        {
                            myConnection.Close();
                        }
                    }

                    BuildQCTestList(Globals, retval, ascitemid, "I");
                    if (retval.qcTestSetupList.Count == 0)
                    {
                        var catID = Globals.myGetInfo.GetASCItemData(ascitemid, "CATID");
                        BuildQCTestList(Globals, retval, catID, "A");
                    }
                }
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                errmsg = e.Message;
            }
            try
            {
                if (!String.IsNullOrEmpty(errmsg))
                    Globals.myASCLog.ProcessTran(errmsg, "E");
            }
            catch //(Exception e)
            {
            }
            return (retval);
        }

        private void BuildQCTestList(ParseNet.GlobalClass Globals, ASCTracFunctionStruct.QC.QCInventoryType retval, string aRecID, string aRecType)
        {
            string sqlstr = "SELECT C.QUESTION_NUM, C.PROMPT, C.ANSWERTYPE, C.ANSWER1, C.ANSWER2, C.ANSWER3, C.ANSWER4";
            sqlstr += ", C.QC_PASSFAIL, C.QC_DEFAULT_REASON";
            sqlstr += " FROM CUSTOM_PROMPTS C";
            sqlstr += " WHERE C.ACTIVEFORNORMAL='T' AND C.TYPE='Q'";

            string sqlStr2 = sqlstr + " AND C.RECTYPE='" + aRecType + "' AND C.RECID='" + aRecID + "'";
            sqlstr += " ORDER BY C.QUESTION_NUM";
            {
                SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sqlStr2, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        ASCTracFunctionStruct.QC.QCTestSetup myTest = new ASCTracFunctionStruct.QC.QCTestSetup();
                        myTest.QuestionNum = ascLibrary.ascUtils.ascStrToInt(myReader["QUESTION_NUM"].ToString(), 0);
                        myTest.Prompt = myReader["PROMPT"].ToString();
                        myTest.AnswerType = myReader["ANSWERTYPE"].ToString();
                        myTest.Answer1 = myReader["ANSWER1"].ToString();
                        myTest.Answer2 = myReader["ANSWER2"].ToString();
                        myTest.Answer3 = myReader["ANSWER3"].ToString();
                        myTest.Answer4 = myReader["ANSWER4"].ToString();
                        myTest.PassFail = myReader["QC_PASSFAIL"].ToString();
                        myTest.DefaultReason = myReader["QC_DEFAULT_REASON"].ToString();
                        retval.qcTestSetupList.Add(myTest);
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
        }

        public ASCTracFunctionStruct.QC.QCInventoryType ToggleQCSkid(ParseNet.ParseNetMain myParseNet, ASCTracFunctionStruct.QC.QCInventoryType aInvType, ASCTracFunctionStruct.QC.QCReasonType aQCType, string aQCPassword, string aOverride, ref string rtnmsg)
        {
            ASCTracFunctionStruct.QC.QCInventoryType retval = new ASCTracFunctionStruct.QC.QCInventoryType();
            rtnmsg = string.Empty;
            try
            {
                string aData = aInvType.invRecord.SkidID;
                if (aQCType.OnHold)
                    aData += ascLibrary.dbConst.HHDELIM + "A";// add hold
                else
                    aData += ascLibrary.dbConst.HHDELIM + "R"; // release hold
                aData += ascLibrary.dbConst.HHDELIM + aQCPassword;
                string[] tmp = aQCType.Reason.Split('|');
                string qcReason = tmp[0].Trim();
                aData += ascLibrary.dbConst.HHDELIM + qcReason;
                aData += ascLibrary.dbConst.HHDELIM + ""; // comments
                aData += ascLibrary.dbConst.HHDELIM + "F"; // lot hold
                aData += ascLibrary.dbConst.HHDELIM + aQCType.RefNum.ToString();
                aData += ascLibrary.dbConst.HHDELIM + aQCType.MafNum.ToString();
                aData += ascLibrary.dbConst.HHDELIM + "F"; // chg hold flag
                aData += ascLibrary.dbConst.HHDELIM + aOverride;

                aData = ascLibrary.dbConst.cmdCHG_QAHOLD_SKID + ascLibrary.dbConst.HHDELIM + myParseNet.Globals.curUserID + ascLibrary.dbConst.HHDELIM + "PC" + ascLibrary.dbConst.HHDELIM + aData;

                rtnmsg = myParseNet.ParseMessage(aData); // .Globals..dmParseQC.ProcessMessage(ascLibrary.dbConst.cmdCHG_QAHOLD_SKID, aData);
                if (rtnmsg.StartsWith("OK"))
                {
                    if (!String.IsNullOrEmpty(aQCType.MafStatus))
                    {
                        string sqlstr = "SELECT QC.MAF_NUM ";
                        sqlstr += " FROM LOCITEMS_QC QC";
                        sqlstr += " LEFT JOIN REASNCDS R ON R.REASON_CODE=QC.REASONFORHOLD";
                        sqlstr += " LEFT JOIN MAF ON MAF.MAF_NUM=QC.MAF_NUM";
                        sqlstr += " WHERE QC.SKIDID='" + aInvType.invRecord.SkidID + "'";
                        sqlstr += " and QC.REASONFORHOLD='" + qcReason + "'";
                        sqlstr += " ORDER BY QC.HOLD_DATETIME DESC";
                        string mafNum = string.Empty;
                        if (myParseNet.Globals.myDBUtils.ReadFieldFromDB(sqlstr, "", ref mafNum) && !String.IsNullOrEmpty(mafNum))
                        {
                            sqlstr = string.Empty;
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref sqlstr, "DESCRIPTION", aQCType.MafDescription);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref sqlstr, "MAF_STATUS_CODE", aQCType.MafStatus);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref sqlstr, "ACTION", aQCType.MafAction);
                            ascLibrary.ascStrUtils.ascAppendSetStr(ref sqlstr, "MAF_CATID", aQCType.MafCatID);
                            myParseNet.Globals.myDBUtils.UpdateFields("MAF", sqlstr, "MAF_NUM='" + mafNum + "'");
                        }
                    }
                    retval = GetSkidQCInfo(myParseNet.Globals, "S", aInvType.invRecord.SkidID, ref rtnmsg);
                }
                else
                {
                    retval = aInvType;
                }
            }
            catch (Exception e)
            {
                myParseNet.Globals.myASCLog.fErrorData = e.ToString();
                rtnmsg = "ER" + e.Message;
            }
            try
            {
                if (!rtnmsg.StartsWith("OK"))
                    myParseNet.Globals.myASCLog.ProcessTran(rtnmsg, "E");
            }
            catch //(Exception e)
            {
            }

            return (retval);

        }

        //===============================================================
        //public string SkidQCTest(ParseNet.ParseNetMain myParseNet, string aSkidID, ASCTracFunctionStruct.QC.QCTestInstance aqcTest)
        public string SkidQCTest(ParseNet.ParseNetMain myParseNet, string aRecType, string aRecID, ASCTracFunctionStruct.QC.QCTestInstance aqcTest)
        {
            string batchnum = string.Empty;
            if (aqcTest.BatchNum > 0)
                batchnum = aqcTest.BatchNum.ToString();
            string aData = aRecID;

            string wo = string.Empty;
            string itemid = string.Empty;
            string lotid = string.Empty;
            string skidid = string.Empty;
            if (aRecType.StartsWith("W"))
            {
                //ascLibrary.ascStrUtils.GetNextWord(ref aData);
                wo = ascLibrary.ascStrUtils.GetNextWord(ref aData);
            }
            else if (aRecType.StartsWith("L"))
            {
                //ascLibrary.ascStrUtils.GetNextWord(ref aData);
                lotid = ascLibrary.ascStrUtils.GetNextWord(ref aData);
                itemid = ascLibrary.ascStrUtils.GetNextWord(ref aData);
            }
            else
                skidid = aData;

            aData = wo;
            aData += ascLibrary.dbConst.HHDELIM + itemid;
            aData += ascLibrary.dbConst.HHDELIM + lotid;
            aData += ascLibrary.dbConst.HHDELIM + skidid;
            aData += ascLibrary.dbConst.HHDELIM + string.Empty; // workcell
            aData += ascLibrary.dbConst.HHDELIM + aqcTest.QuestionNum.ToString();
            aData += ascLibrary.dbConst.HHDELIM + aqcTest.Answer;
            aData += ascLibrary.dbConst.HHDELIM + aqcTest.PassFail.ToString().Substring(0, 1);
            aData += ascLibrary.dbConst.HHDELIM + aqcTest.Reason;
            aData += ascLibrary.dbConst.HHDELIM + batchnum;
            aData += ascLibrary.dbConst.HHDELIM + aqcTest.TestDateTime.ToShortDateString();
            aData += ascLibrary.dbConst.HHDELIM + aqcTest.TestDateTime.ToShortTimeString();

            aData = ascLibrary.dbConst.cmdQC_TEST + ascLibrary.dbConst.HHDELIM + myParseNet.Globals.curUserID + ascLibrary.dbConst.HHDELIM + "PC" + ascLibrary.dbConst.HHDELIM + aData;

            string rtnmsg = myParseNet.ParseMessage(aData); // .Globals..dmParseQC.ProcessMessage(ascLibrary.dbConst.cmdCHG_QAHOLD_SKID, aData);
            return (rtnmsg);
        }

        //=======================================================================
        public string GetWOQCTests(ParseNet.GlobalClass Globals, string aWO)
        {
            string retval = string.Empty;

            try
            {
                DataSet dsLicenses = new DataSet();

                string sql = "SELECT MAX( QUESTION_NUM) FROM QC_TESTS WHERE WORKORDER_ID='" + aWO + "'";
                string tmp = string.Empty;
                if (!Globals.myDBUtils.ReadFieldFromDB(sql, "", ref tmp) || String.IsNullOrEmpty(tmp))
                    retval = "OK";
                else
                {
                    int maxQuestion = Convert.ToInt32(tmp);
                    string sql2 = string.Empty;
                    sql = "SELECT Q1.WORKORDER_ID, Q1.LOTID, Q1.SKIDID, Q1.BATCH_NUM, Q1.TEST_USERID, Q1.TEST_DATETIME, Q1.RECTYPE, Q1.PASSFAIL, Q1.PROMPT";
                    for (int i = 1; i <= maxQuestion; i++)
                    {
                        string sQNum = i.ToString();
                        sql += " , Q" + i.ToString() + ".QUESTION_NUM AS QUESTION_NUM" + i.ToString() + ", Q" + i.ToString() + ".PROMPT AS PROMPT" + i.ToString() + ", Q" + i.ToString() + ".ANSWER AS ANSWER" + i.ToString() + ", Q" + i.ToString() + ".PASSFAIL AS PASSFAIL" + i.ToString() + ", Q" + i.ToString() + ".HOLD_REASON AS HOLD_REASON" + i.ToString();
                        if (i > 1)
                            sql2 += " LEFT JOIN QC_TESTS Q" + sQNum + " ON Q" + sQNum + ".RECTYPE=Q1.RECTYPE AND Q" + sQNum + ".RECID=Q1.RECID AND Q" + sQNum + ".BATCH_NUM=Q1.BATCH_NUM AND Q" + sQNum + ".QUESTION_NUM=" + sQNum;
                    }
                    sql += " FROM QC_TESTS Q1";
                    sql += sql2;
                    sql += " WHERE Q1.WORKORDER_ID='" + aWO + "'";
                    sql += " AND Q1.QUESTION_NUM=1";
                    sql += " ORDER BY Q1.TEST_DATETIME";

                    using (SqlConnection conn = new SqlConnection(Globals.myDBUtils.myConnString))
                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        conn.Open();
                        da.MissingSchemaAction = MissingSchemaAction.Add;
                        da.Fill(dsLicenses, "QC_TESTS");

                        // Write results of query as XML to a string and return
                        StringWriter sw = new StringWriter();
                        dsLicenses.WriteXml(sw);

                        return "OK" + sw.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "E");
                return "EX" + e.Message;
            }
            return (retval);
        }


    }
}
