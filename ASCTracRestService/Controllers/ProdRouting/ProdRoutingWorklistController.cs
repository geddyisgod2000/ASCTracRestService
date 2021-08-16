using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ASCTracRestService.Controllers.ProdRouting
{
    public class ProdRoutingWorklistController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetWORoutingWorklist(string aUserID, string aSiteID, string WorkorderID, string Workcell, string RouteSeq)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdTA_WOROUTING_START, "GetWORoutingWorklist ", aUserID, aSiteID, ref errmsg))
                {
                    var myRouting = new ASCTracFunctionsData.ProdRouting.WORouting();

                    var myData = myRouting.GetWoRoutingWorklistData(WorkorderID, Workcell, RouteSeq);

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

        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType UpdateWORoutingWorklist(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_WIP_ROUTING_UPDATE_STEP, "UpdateWORoutingWorklist ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.Production.WORoutingType>(aInboundMsg.DataMessage);
                    var ret = iParse.myParseNet.Globals.dmProdRouting.GetWORoutingUpdateStep(myData.Workorder_ID, myData.RouteSeq, myData.RouteStep, myData.Answer, ref errmsg);

                    switch (ret)
                    {
                        case ascLibrary.TDBReturnType.dbrtOK:
                            iParse.myParseNet.Globals.mydmupdate.ProcessUpdates();
                            if (errmsg.Equals("0"))
                            {
                                myData.RouteStep = -1;
                            }
                            else
                            {
                                var myRouting = new ASCTracFunctionsData.ProdRouting.WORouting();
                                myData = myRouting.GetWoRoutingWorklistData(myData.Workorder_ID, "", myData.RouteSeq);
                            }
                            errmsg = String.Empty;
                            retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
                            break;
                        case ascLibrary.TDBReturnType.dbrtNOT_ACTIVE:
                            errmsg = "Step no longer active";
                            break;
                        case ascLibrary.TDBReturnType.dbrtFAILED:
                            errmsg = ascLibrary.dbConst.stERR + "Invalid Entry";
                            break;
                        default:
                            errmsg= ParseNet.ParseNetMain.GetErrorMsg(ret);
                            break;
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