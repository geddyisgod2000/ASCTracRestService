using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.InvLookup
{
    public class InvLookupController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetInvList(string aItemID, bool aIncludeQC, bool aIncludeExp, bool aIncludePicked, int aFieldType, string aFieldValue, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdLOOKUP_ITEM, "GetInvList ", aUserID, aSiteID, ref errmsg))
                {
                    var myInv = new InvLookup();
                    errmsg = myInv.GetInvList(aItemID, aIncludeQC, aIncludeExp, aIncludePicked, aFieldType, aFieldValue, iParse.myParseNet.Globals);

                    if (errmsg.StartsWith("OK"))
                        retval.DataMessage = errmsg;
                    else
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

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType getInvInfo(string aSkidID, string aItemID, string aLocationID, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdLOOKUP_ITEM, "getInvInfo ", aUserID, aSiteID, ref errmsg))
                {
                    var myInv = new InvLookup();
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myInv.GetSkidInfo( aSkidID, aItemID, aLocationID, iParse.myParseNet.Globals));
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType updateInvRecord(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdUPDATE_SKID, "updateInvRecord ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myRec = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.Inventory.InvCountType>(aInboundMsg.DataMessage);
                    var myInv = new InvLookup();
                    errmsg = myInv.UpdateSkid(myRec, iParse.myParseNet.Globals);
                }
                if (!String.IsNullOrEmpty(errmsg) && !errmsg.StartsWith( "OK"))
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
