using ascLibrary;
using ASCTracRestService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Receipt
{
    public class GetConfirmReceiptInfoController : ApiController
    {
        // string aOrderType, string aPONumber, string aReleaseNum,
        [HttpPut]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetConfirmReceiptInfo(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                ASCTracFunctionStruct.Receipt.ConfReceiptType myData = new ASCTracFunctionStruct.Receipt.ConfReceiptType();
                if (iParse.InitParse(ascLibrary.dbConst.cmdRX_CLOSE_PO, "Receipt " + aInboundMsg.inputDataList[0] + "," + aInboundMsg.inputDataList[1], aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myPOInfo = new ASCTracFunctionsData.Receipt.POInfo();
                    myData.OrderType = aInboundMsg.inputDataList[0];
                    if (myData.OrderType.Equals("R"))
                        myPOInfo.GetConfirmReceiptReceiverInfo(aInboundMsg.inputDataList[1], ref myData, ref errmsg);
                    else
                    {
                        string relnum = aInboundMsg.inputDataList[2];
                        if (string.IsNullOrEmpty(relnum))
                            relnum = "00";
                        myPOInfo.GetConfirmReceiptPOInfo(aInboundMsg.inputDataList[1], relnum, ref myData, ref errmsg);
                    }
                }
                if (string.IsNullOrEmpty(errmsg))
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
                else
                {
                    retval.ErrorMessage = errmsg;
                    retval.successful = false;
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

            //retmsg.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
            return (retval);
        }

        

        // string aOrderType, string aPONumber, string aReleaseNum,
        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType doConfirmReceipt(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdRX_CLOSE_PO, "Close Receipt " + aInboundMsg.inputDataList[0] + "," + aInboundMsg.inputDataList[1], aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myRecvInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.Receipt.ConfReceiptType>(aInboundMsg.DataMessage);
                    errmsg = DoConfirmReceipt(myRecvInfo);

                }
                if (!String.IsNullOrEmpty(errmsg))
                {
                    retval.ErrorMessage = errmsg;
                    retval.successful = false;
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

            //retmsg.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
            return (retval);
        }

        private string DoConfirmReceipt(ASCTracFunctionStruct.Receipt.ConfReceiptType aConfirmReceiptInfo)
        {
            string retval = string.Empty;
            try
            {
                string ordernum = aConfirmReceiptInfo.PONumber + "-" + aConfirmReceiptInfo.ReleaseNum;
                if (aConfirmReceiptInfo.OrderType.Equals("R"))
                    ordernum = aConfirmReceiptInfo.ReceiverID;

                string data = ASCTracFunctions.FuncConst.funcPO_CLOSE_PO + ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.OrderType;
                data += ascLibrary.dbConst.HHDELIM + ordernum;
                data += ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.ShipVia;
                data += ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.CarrierID + ascLibrary.dbConst.HHDELIM + ascLibrary.ascUtils.ascBoolToStr(aConfirmReceiptInfo.OnTimeFlag);
                data += ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.TrailerNum;
                data += ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.SealNum + ascLibrary.dbConst.HHDELIM + ascLibrary.ascUtils.ascBoolToStr(aConfirmReceiptInfo.SealIntactFlag);
                data += ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.WhseReceiptNumber;
                data += ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.numWoodPallets.ToString() + ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.numChepPallets.ToString() + ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.numIGPSPallets.ToString() + ascLibrary.dbConst.HHDELIM + aConfirmReceiptInfo.numMiscPallets.ToString();
                data += ascLibrary.dbConst.HHDELIM + ascLibrary.ascUtils.ascBoolToStr(aConfirmReceiptInfo.DamageFlag);
                data += ascLibrary.dbConst.HHDELIM + ascLibrary.ascUtils.ascBoolToStr(aConfirmReceiptInfo.DocumentsFlag);

                retval = iParse.myASCFunction.DoFunction(data, iParse.myParseNet.Globals.curUserID, iParse.myParseNet.Globals.curSiteID);
                if (retval.StartsWith("OK"))
                {
                    string updstr = string.Empty;
                    foreach (var rec in aConfirmReceiptInfo.customFieldList)
                    {
                        if (rec.Value.fChanged)
                        {
                            if (string.IsNullOrEmpty(rec.Value.Value.Trim()))
                                ascLibrary.ascStrUtils.ascAppendSetNull(ref updstr, rec.Key);
                            else
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, rec.Key, rec.Value.Value);
                        }
                    }
                    if (!String.IsNullOrEmpty(updstr))
                    {
                        if (aConfirmReceiptInfo.OrderType.Equals("R"))
                            iParse.myParseNet.Globals.myDBUtils.UpdateFields("RECVRHDR", updstr, "RECEIVER_ID='" + aConfirmReceiptInfo.ReceiverID + "'");
                        else
                            iParse.myParseNet.Globals.myDBUtils.UpdateFields("POHDR", updstr, "PONUMBER='" + aConfirmReceiptInfo.PONumber + "' and RELEASENUM='" + aConfirmReceiptInfo.ReleaseNum + "'");
                    }

                    if (aConfirmReceiptInfo.palletLogInfo != null)
                    {
                        ASCTracFunctions.funcPalletLog.AddPalletLog("I", ordernum, aConfirmReceiptInfo.palletLogInfo.WhseID);
                        foreach (var rec in aConfirmReceiptInfo.palletLogInfo.palletTypeList)
                        {
                            if (rec.Changed)
                            {
                                ASCTracFunctions.funcPalletLog.AddPalletTypetoPalletLog("I", ordernum,
                                    rec.fType, rec.AcctType, rec.AcctNum, rec.Qty);
                            }
                        }
                    }

                    if (aConfirmReceiptInfo.fees3PLList != null)
                    {
                        foreach (var rec in aConfirmReceiptInfo.fees3PLList)
                        {
                            if (rec.fChanged)
                            {
                                updstr = string.Empty;
                                double totalFee = rec.Fee * rec.Qty;
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CODE", rec.Code);
                                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "FEE", rec.Fee.ToString());
                                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "TOTALFEE", totalFee.ToString());
                                ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "QTY", rec.Qty.ToString());
                                ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "COMMENTS", rec.Notes);

                                if (!String.IsNullOrEmpty(rec.ID))
                                    iParse.myParseNet.Globals.myDBUtils.UpdateFields("SHORDHANDFEES", updstr, "ID=" + rec.ID);
                                else
                                {
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ORDTYPE", aConfirmReceiptInfo.OrderType);
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "ORDERNUMBER", ordernum);
                                    ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, "CREATEDATE", "GetDate()");
                                    ascLibrary.ascStrUtils.ascAppendSetStr(ref updstr, "CREATEUSERID", iParse.myParseNet.Globals.curUserID);
                                    iParse.myParseNet.Globals.myDBUtils.InsertRecord("SHORDHANDFEES", updstr);
                                }
                            }
                        }
                    }
                    retval = string.Empty;
                }
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = e.ToString();
                iParse.myParseNet.Globals.myASCLog.ProcessTran("Exception", "X");
                retval = e.Message;
            }
            return (retval);
        }

    }
}