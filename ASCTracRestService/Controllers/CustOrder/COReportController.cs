using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.CustOrder
{
    public class COReportController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOReportInfo(string aOrderNumber, string aReportType, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdDOCUMENT, "GetCOReportInfo", aUserID, aSiteID, ref errmsg))
                {
                    ASCTracFunctionsData.CustOrder.COReport myCOReport = new ASCTracFunctionsData.CustOrder.COReport();
                    var myData = myCOReport.getCOReportInfo("C", aOrderNumber, "", aReportType, ref errmsg);
                    if (!myData.fSuccessful)
                        errmsg = myData.ErrorMessage;
                    else
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
                }
                if( !String.IsNullOrEmpty( errmsg))
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType doCOReport(string aReportType, string aPrinterID, ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdDOCUMENT, "doCOReport", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var aCOReportInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.COShip.COShipHdr>(aInboundMsg.DataMessage);


                    ASCTracFunctionsData.CustOrder.COReport myCOReport = new ASCTracFunctionsData.CustOrder.COReport();
                    myCOReport.doReportCO(aCOReportInfo, aReportType, aPrinterID);

                    if( !aCOReportInfo.fSuccessful)
                    {
                        if (aCOReportInfo.ErrorMessage.StartsWith("ER"))
                            retval.ErrorMessage = aCOReportInfo.ErrorMessage.Substring(2);
                        else
                            retval.ErrorMessage = aCOReportInfo.ErrorMessage;
                        retval.successful = false;
                    }
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
