using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Inspection
{
    [Filters.ApiAuthenticationFilter]
    public class InspectionController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetTrailerInspectionInfo(string aUserID, string aSiteID, string aOrderType, string aOrderNum, string aTrailerNum, string aTrailerType, double aTrailerWeight, string aOverride)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdTRAILERINSPECT_CHECK, "GetTrailerInspectionInfo ", aUserID, aSiteID, ref errmsg))
                {
                    var myInspect = new ASCTracFunctionsData.Inspection.TrailerInspection();
                    var myData = myInspect.GetInspectionData(aOrderType, aOrderNum, aTrailerNum, aTrailerType, aTrailerWeight, aOverride);

                    if (string.IsNullOrEmpty(errmsg))
                    {
                        foreach( var rec in myData.myList)
                        {
                            if( rec.isAnswerList)
                            {
                                string tmp = rec.AnswerList[0];
                                rec.AnswerList.Clear();
                                while( !String.IsNullOrEmpty( tmp))
                                {
                                    string tmp2 = ascLibrary.ascStrUtils.ascGetNextWord(ref tmp, ",");
                                    if (!String.IsNullOrEmpty(tmp2))
                                        rec.AnswerList.Add(tmp2);
                                }
                            }
                        }
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
                    }
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

        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType UpdateInspection(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdTRAILERINSPECT_DATA, "UpdateInspection ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.Inspection.TrailerInspectionHeader>(aInboundMsg.DataMessage);
                    foreach (var rec in myData.myList)
                    {
                        iParse.myParseNet.Globals.mydmupdate.InitUpdate();
                        var ret = iParse.myParseNet.Globals.dmInspect.doShipmentInspect(myData.InspectionID, rec.fAnswer, "", "", "", "", rec.inspectIdx, "");
                        if (!ret.StartsWith(ascLibrary.dbConst.stOK))
                        {
                            retval.successful = false;
                            retval.ErrorMessage = ret.Substring(2);
                            break;
                        }
                        else
                            iParse.myParseNet.Globals.mydmupdate.ProcessUpdates();
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