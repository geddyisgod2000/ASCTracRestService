using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Production
{
    public class ProdSkidController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetProdSkidList(string aWorkorder, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPROD_SKID, "GetProdSkidList ", aUserID, aSiteID, ref errmsg))
                {
                    Production myProd = new Production();
                    var myList = myProd.GetWOLicenses(aWorkorder, iParse.myParseNet.Globals);

                    retval.DataMessage = myList; // Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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


        [HttpPut]
        public ASCTracFunctionStruct.ascBasicReturnMessageType ProdNewskid(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPROD_SKID, "ProdNewskid ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.Production.ProdNewSkidType>(aInboundMsg.DataMessage);
                    Production myProd = new Production();
                    errmsg = myProd.ProdNewSkid(myData, iParse.myParseNet.Globals);
                }
                if (!string.IsNullOrEmpty(errmsg) && errmsg.StartsWith("ER"))
                {
                    retval.ErrorMessage = errmsg;
                    retval.successful = false;
                }
                else
                    retval.DataMessage = errmsg;
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
