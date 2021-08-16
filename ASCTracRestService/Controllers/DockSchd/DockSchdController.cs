using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.DockSchd
{
    public class DockSchdController : ApiController
    {
        [HttpGet]
        // aID = user|site|dock|datetime
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetDockSchedList(string aID) //string aDock, DateTime aDate, ascBasicInboundMessageType aInboundMsg)
        {
            string errmsg = string.Empty;
            string data = aID;
            string userid = ascLibrary.ascStrUtils.GetNextWord(ref data);
            string siteid = ascLibrary.ascStrUtils.GetNextWord(ref data);
            string aDock = ascLibrary.ascStrUtils.GetNextWord(ref data);
            string aDate = ascLibrary.ascStrUtils.GetNextWord(ref data);

            if (iParse.InitParse(ascLibrary.dbConst.cmdXDOCK_GETLIST, "GetDockSchedList ", userid, siteid, ref errmsg))
            {
                DockSched myds = new DockSched();
                return (myds.GetDockSchedList(iParse.myParseNet.Globals, aDock, ascLibrary.ascUtils.ascStrToDate( aDate, DateTime.MinValue)));
            }
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType(errmsg);
            return (retval);
        }

        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType doDockSched(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.CustOrder.DockType aDockRec = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.CustOrder.DockType>(aInboundMsg.DataMessage);
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_RESCHEDULE, "doDockSchd", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {

                    var myDockSchd = new DockSched();
                    errmsg = myDockSchd.doDockSched(aDockRec, iParse.myParseNet.Globals);
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetNewDockSched(string aOrderType, string aOrderNum, string aDock, DateTime aDate, string aSiteID, string aUserID) // ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_RESCHEDULE, "doNewDockSchd", aUserID, aSiteID, ref errmsg))
                {
                    var myDockSchd = new DockSched();
                    retval = myDockSchd.GetNewDockSched(aOrderType, aOrderNum, aDock, aDate, iParse.myParseNet.Globals);
                }
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

            return (retval);
        }
    }
}
