using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Replen
{
    public class ReplenController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetReplenInfoForZone(string aZoneID, string aReplenFilterType, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdREPL_GETLIST, "GetReplenInfoForZone ", aUserID, aSiteID, ref errmsg))
                {
                    var myObj = new Replen();
                    retval = myObj.GetReplenInfoForZone(aZoneID, aReplenFilterType, iParse.myParseNet.Globals);
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
