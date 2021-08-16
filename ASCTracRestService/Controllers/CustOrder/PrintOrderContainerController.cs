using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.CustOrder
{
    public class PrintOrderContainerController : ApiController
    {

        [HttpPut]
        public ASCTracFunctionStruct.ascBasicReturnMessageType PrintOrderContainer(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdDOCUMENT, "PrintOrderContainer " + aInboundMsg.inputDataList[0], aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myCustOrder = new CustOrder();
                    errmsg = myCustOrder.PrintOrderContainer(aInboundMsg.inputDataList[0], aInboundMsg.inputDataList[1],
                        aInboundMsg.inputDataList[2], aInboundMsg.inputDataList[3], aInboundMsg.inputDataList[4], iParse.myParseNet.Globals);

                    if (errmsg.StartsWith("OK"))
                        retval.DataMessage = errmsg.Substring(2);
                    else if (!String.IsNullOrEmpty(errmsg))
                    {
                        retval.ErrorMessage = errmsg;
                        retval.successful = false;
                    }
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
