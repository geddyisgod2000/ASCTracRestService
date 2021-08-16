using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Production.WOSM
{
    public class WOSMController : ApiController
    {
        //myClient.GetWOStatusSummaryAsync(statusList, prodlines, pickDateField.SelectedIndex, pickDateRange.SelectedIndex, Globals.curBasicMessage);
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetWOStatusSummary(string aByWO, string aStatusList, string aProdLineRange, int aDatefield, int aDateFilter, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdPP_GET_WO_LIST, "GetWOStatusSummary ", aUserID, aSiteID, ref errmsg))
                {
                    if (aByWO.StartsWith("T"))
                    {
                        var myWOHdr = new WOHdr();
                        var myList = myWOHdr.GetWOStatusByWO(aStatusList, aProdLineRange, aDatefield, aDateFilter, iParse.myParseNet.Globals);
                        if (myList.Count == 0)
                        {
                            errmsg = "No Work Orders Found";
                        }
                        else
                            retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
                    }
                    else
                    {
                        var myWOHdr = new WOHdr();
                        retval = myWOHdr.GetWOStatusSummary(aStatusList, aProdLineRange, aDatefield, aDateFilter, iParse.myParseNet.Globals);
                    }
                }
                if( !String.IsNullOrEmpty( errmsg))
                {
                    retval.successful = false;
                    retval.ErrorMessage = errmsg;
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
