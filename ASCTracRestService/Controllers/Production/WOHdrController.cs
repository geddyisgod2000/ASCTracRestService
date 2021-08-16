using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Production
{
    public class WOHdrController : ApiController
    {

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetWOHdrList(string aProdline, string aDate, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_GET_WO_INFO, "GetWOHdrList ", aUserID, aSiteID, ref errmsg))
                {
                    var myDate = ascLibrary.ascUtils.ascStrToDate(aDate, DateTime.Today);
                    ASCTracFunctionsData.Production.WOHdr myWOHdr = new ASCTracFunctionsData.Production.WOHdr();
                    var myList = myWOHdr.GetWOHdrList(iParse.myParseNet.Globals, aProdline, myDate);

                    if (myList.Count == 0)
                        errmsg = "No Work Orders found";
                    if (string.IsNullOrEmpty(errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
                }
                if (!string.IsNullOrEmpty(errmsg))
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetWOHdrInfo(string aWorkorder, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_GET_WO_INFO, "GetWOHdrInfo", aUserID, aSiteID, ref errmsg))
                {
                    ASCTracFunctionsData.Production.WOHdr myWOHdr = new ASCTracFunctionsData.Production.WOHdr();
                    var myRec = myWOHdr.GetWOHdrInfo( iParse.myParseNet.Globals, aWorkorder);

                    if (myRec == null )
                        errmsg = "No Work Order found";
                    if (string.IsNullOrEmpty(errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myRec);
                }
                if (!string.IsNullOrEmpty(errmsg))
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType ScheduleWO(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_SCHED_WO_CHECK, "ScheduleWO ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.Production.WOHdrType>(aInboundMsg.DataMessage);
                    WOHdr myWOHdr = new WOHdr();
                    errmsg = myWOHdr.ScheduleWOHdr(myData, iParse.myParseNet.Globals);
                }
                if (!string.IsNullOrEmpty(errmsg))
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