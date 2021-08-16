using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ASCTracRestService.Controllers.ProdRouting
{
    public class ProdRoutingController : ApiController
    {
        // GET: ProdRouting
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetWORouting(string aUserID, string aSiteID, string WorkorderID, string Workcell, string RouteSeq)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdTA_WOROUTING_START, "GetWORouting ", aUserID, aSiteID, ref errmsg))
                {
                    var myRouting = new ASCTracFunctionsData.ProdRouting.WORoutingSeq();

                    var myData = myRouting.GetWoRoutingSeqData(WorkorderID, Workcell, RouteSeq);

                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
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