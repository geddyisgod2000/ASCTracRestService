using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Receipt
{
    public class PrintPOReportController : ApiController
    {
        // string aReportType, string aPrinterID, 
        public ASCTracFunctionStruct.ascBasicReturnMessageType PrintPOReport(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {

            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdDOCUMENT, "PO Document " + aInboundMsg.inputDataList[0] + "," + aInboundMsg.inputDataList[1], aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    ASCTracFunctionStruct.Receipt.ConfReceiptType aConfigReceiptInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.Receipt.ConfReceiptType>(aInboundMsg.DataMessage);
                    errmsg = doPrintPOReport(aInboundMsg.inputDataList[0], aInboundMsg.inputDataList[1], aConfigReceiptInfo);
                }
                if (errmsg != null)
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

        private string doPrintPOReport(string aReportType, string aPrinterID, ASCTracFunctionStruct.Receipt.ConfReceiptType aConfigReceiptInfo)
        {
            string retval = string.Empty;
            try
            {
                var tmpRet = iParse.myParseNet.Globals.dbInfo.printdocument("", "", "", aConfigReceiptInfo.PONumber + "-" + aConfigReceiptInfo.ReleaseNum, aPrinterID, "R", "", "", "", "");
                if (tmpRet.Equals(ascLibrary.TDBReturnType.dbrtOK))
                    iParse.myParseNet.Globals.mydmupdate.ProcessUpdates();
                else
                    retval = ParseNet.dmascmessages.GetErrorMsg(tmpRet);
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = e.ToString();
                retval = e.Message;
            }
            return (retval);

        }
    }
}