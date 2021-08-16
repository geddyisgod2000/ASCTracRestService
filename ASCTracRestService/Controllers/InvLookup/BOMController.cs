using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.InvLookup
{
    public class BOMController : ApiController
    {

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetBOMAvailList(string aItemID, string aQty, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_GET_BOMDETAILS, "GetBOMAvailList ", aUserID, aSiteID, ref errmsg))
                {
                    var myObject = new BOMLookup();
                    retval = myObject.GetBOMAvailList(aItemID, ascLibrary.ascUtils.ascStrToDouble( aQty, 0), iParse.myParseNet.Globals);
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
