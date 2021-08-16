using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.CustOrder
{
    public class InvAvailController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetInvAvail(string aOrderNumber, string aLineNum, string aIncludeQC, string aIncludeExp, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_GET_ORDER, "GetGOInfo", aUserID, aSiteID, ref errmsg))
                {
                    var myCustOrder = new CustOrder();
                    errmsg = myCustOrder.GetInvAvail(aOrderNumber, ascLibrary.ascUtils.ascStrToInt(aLineNum, 0), aIncludeQC.StartsWith("T"), aIncludeExp.StartsWith("T"), iParse.myParseNet.Globals);
                }
                if (errmsg.StartsWith("OK"))
                    retval.DataMessage = errmsg;
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