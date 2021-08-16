using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.COSM
{
    public class COSMSummaryController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOStatusSummary(string aStatusList, string aDockRange, int aDatefield, int aDateFilter, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_RESCHEDULE, "GetCOStatusSummary", aUserID, aSiteID, ref errmsg))
                {
                    COSM myCosm = new COSM();
                    retval = myCosm.GetCOStatusSummary(aStatusList, aDockRange, aDatefield, aDateFilter, iParse.myParseNet.Globals);
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
