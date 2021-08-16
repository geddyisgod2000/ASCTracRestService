using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.QCTest
{
    public class QCHoldController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetSkidQCInfo(string aSkidID, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdQCSKID_INFO, "GetSkidQCInfo ", aUserID, aSiteID, ref errmsg))
                {
                    var myObj = new qc();
                    var myList = myObj.GetSkidQCInfo(iParse.myParseNet.Globals, "S", aSkidID, ref errmsg);

                    if (String.IsNullOrEmpty(errmsg))
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


        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType ToggleQCSkid( ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCHG_QAHOLD_SKID, "ToggleQCSkid ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myHeaderData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.QC.QCInventoryType>(aInboundMsg.hdrDataMessage);
                    var myData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.QC.QCReasonType>(aInboundMsg.DataMessage);
                    var myObj = new qc();
                    var myReturnData = myObj.ToggleQCSkid(iParse.myParseNet, myHeaderData, myData, myData.fQCPassword, myData.fQCOverride, ref errmsg);

                    retval.successful = String.IsNullOrEmpty(errmsg) || (errmsg.StartsWith("OK"));
                    if (retval.successful)
                    {
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myReturnData);
                        errmsg = string.Empty;
                    }
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
