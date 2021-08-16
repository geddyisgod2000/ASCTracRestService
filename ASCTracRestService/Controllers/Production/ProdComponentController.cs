using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Production
{
    public class ProdComponentController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetProdComponentList(string aWorkorder, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_GET_PICK_LIST, "GetProdComponentList ", aUserID, aSiteID, ref errmsg))
                {
                    var myObj = new Consumption();
                    var myList = myObj.GetWOComponents(aWorkorder, iParse.myParseNet.Globals);

                    if (myList.StartsWith("EX"))
                    {
                        errmsg = myList.Substring(2);
                    }
                    else
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

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetProdComponentLicensesList(string aWorkorder, string aSeqNum, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_GET_PICK_LIST, "GetProdComponentLicensesList ", aUserID, aSiteID, ref errmsg))
                {
                    var myObj = new Consumption();
                    var myList = myObj.GetWOComponentLicenses(aWorkorder, ascLibrary.ascUtils.ascStrToInt( aSeqNum, 0), iParse.myParseNet.Globals);

                    if (myList.StartsWith("EX"))
                    {
                        errmsg = myList.Substring(2);
                    }
                    else
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType WOIssueComponent(string aWorkorder_ID, string aSeqNum, string aSkidID, string aFGSkidID, string aItemID, string aLocationID, string aQty, ASCTracFunctionStruct.ascBasicInboundMessageType ainboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_GET_PICK_LIST, "WOIssueComponent ", ainboundMsg.UserID, ainboundMsg.SiteID, ref errmsg))
                {
                    var myObj = new Consumption();
                    var myreturndata = myObj.WOIssueComponent(aWorkorder_ID, ascLibrary.ascUtils.ascStrToInt( aSeqNum, 0), aSkidID, aFGSkidID, aItemID, aLocationID, ascLibrary.ascUtils.ascStrToDouble(aQty, 0), iParse.myParseNet.Globals);

                    if (!myreturndata.StartsWith( "OK"))
                    {
                        errmsg = myreturndata.Substring(2);
                    }
                    else
                        retval.DataMessage = myreturndata; // Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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
