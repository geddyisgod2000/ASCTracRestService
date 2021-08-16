using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.CustOrder
{
    public class COEntryController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOEntryInfo(string aOrderNumber, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdOE_CREATEORDER, "GetCOEntryInfo", aUserID, aSiteID, ref errmsg))
                {
                    ASCTracFunctionsData.DataGlobals.InitDataGlobals(iParse.myParseNet.Globals);
                    var myOrderInfo = new ASCTracFunctionsData.CustOrder.COEntry();
                    var myData = myOrderInfo.GetCOEntryInfo(aOrderNumber, ref errmsg);
                    if (String.IsNullOrEmpty(errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
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

            return (retval);
        }

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetUsersCOEntryInfo(string aCustType, string aCustTypeData, string aItemType, string aItemTypeData, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdOE_CREATEORDER, "GetCOEntryInfo", aUserID, aSiteID, ref errmsg))
                {
                    ASCTracFunctionsData.DataGlobals.InitDataGlobals(iParse.myParseNet.Globals);
                    var myOrderInfo = new ASCTracFunctionsData.CustOrder.COEntry();
                    var myData = myOrderInfo.GetUsersCOEntryInfo(aCustType + "|" + aCustTypeData + "|" + aItemType + "|" + aItemTypeData, ref errmsg);

                    if (String.IsNullOrEmpty(errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
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

            return (retval);
        }
            [HttpPost]
        
        public ASCTracFunctionStruct.ascBasicReturnMessageType COEntrySave(string aRecType, ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdOE_CREATEORDER, "CompleteCOEntry", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    ASCTracFunctionsData.DataGlobals.InitDataGlobals(iParse.myParseNet.Globals);
                    if ( aRecType.Equals( "D"))
                    {
                        var myOrderInfo = new ASCTracFunctionsData.CustOrder.COEntry();
                        var aCOInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.COEntry.COEntryHdr>(aInboundMsg.hdrDataMessage);
                        var aCODetInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.COEntry.COEntryDet>(aInboundMsg.DataMessage);
                        errmsg = myOrderInfo.UpdateCOEntryDet(aCOInfo, aCODetInfo, iParse.myParseNet.Globals);
                        if (String.IsNullOrEmpty(errmsg))
                        {
                            var myData = myOrderInfo.GetCOEntryInfo(aCOInfo.OrderNumber, ref errmsg);
                            if (String.IsNullOrEmpty(errmsg))
                                retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
                        }
                    }
                    if( aRecType.Equals( "H"))
                    {
                        var myOrderInfo = new ASCTracFunctionsData.CustOrder.COEntry();
                        var aCOInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.COEntry.COEntryHdr>(aInboundMsg.DataMessage);
                        errmsg = myOrderInfo.UpdateCOEntryHdr(aCOInfo, iParse.myParseNet.Globals);

                        if (String.IsNullOrEmpty(errmsg))
                        {
                            var myData = myOrderInfo.GetCOEntryInfo(aCOInfo.OrderNumber, ref errmsg);
                            if (myData.OrderNumber != aCOInfo.OrderNumber)
                                errmsg = "Order Number Changed from " + aCOInfo.OrderNumber + " to " + myData.OrderNumber;
                            if (String.IsNullOrEmpty(errmsg))
                                retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
                        }
                    }
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

            return (retval);
        }

        [HttpPut]

        public ASCTracFunctionStruct.ascBasicReturnMessageType CompleteCOEntry(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdOE_CREATEORDER, "CompleteCOEntry", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    ASCTracFunctionsData.DataGlobals.InitDataGlobals(iParse.myParseNet.Globals);

                    var myOrderInfo = new ASCTracFunctionsData.CustOrder.COEntry();
                    var aCOInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.COEntry.COEntryHdr>(aInboundMsg.DataMessage);
                    errmsg = myOrderInfo.CompleteCOEntry(aCOInfo, iParse.myParseNet.Globals);
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

            return (retval);
        }
    }
}
