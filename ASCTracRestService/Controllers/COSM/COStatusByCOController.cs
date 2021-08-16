using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.COSM
{
    public class COStatusByCOController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOStatusByCO(string aStatusList, string aDockRange, DateTime aDate, int aDatefield, int aDateFilter, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                DateTime myDate = aDate;
                if (myDate == DateTime.MinValue)
                    myDate = DateTime.Now;
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_RESCHEDULE, "GetCOStatusByCO", aUserID, aSiteID, ref errmsg))
                {
                    COSM myCosm = new COSM();
                    retval = myCosm.GetCOStatusByCO(aStatusList, aDockRange, myDate, aDatefield, aDateFilter, iParse.myParseNet.Globals);
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
