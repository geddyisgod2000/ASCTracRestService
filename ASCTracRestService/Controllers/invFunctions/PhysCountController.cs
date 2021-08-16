using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.invFunctions
{
    public class PhysCountController : ApiController
    {

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCounts(string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdLOC_PHYS, "GetCounts ", aUserID, aSiteID, ref errmsg))
                {
                    var myList = WCFUtils.GetDictionaryList( iParse.myParseNet.Globals.myDBUtils.myConnString, "COUNT_HDR", string.Empty, "STATUS NOT IN ( 'C', 'X') AND SITE_ID='" + aSiteID + "'", "COUNT_NUM", "DESCRIPTION", ref errmsg);

                    if (string.IsNullOrEmpty( errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetPhysLocs(string aCountNum, string aStartLocID, string aEndLocID, string aStartItemID, string aEndItemID,
            bool aIncludeLocCounted, bool aIncludeLocUncounted, bool aIncludeReviewed,
            bool aIncludeInvAll, bool aIncludeQtyVar, bool aIncludeLocChg, bool aIncludeLocEmpty, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdLOC_PHYS, "GetPhysList ", aUserID, aSiteID, ref errmsg))
                {
                    var myInv = new InvLookup.InvLookup();
                    var myList = myInv.GetPhysLocs(aCountNum, aStartLocID, aEndLocID, aStartItemID, aEndItemID,
                        aIncludeLocCounted, aIncludeLocUncounted, aIncludeReviewed,
                        aIncludeInvAll, aIncludeQtyVar, aIncludeLocChg, aIncludeLocEmpty, iParse.myParseNet.Globals);

                    if ( String.IsNullOrEmpty( errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetPhysLocItems(string aCountNum, string aLocationID, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdLOC_PHYS, "GetPhysLocItems ", aUserID, aSiteID, ref errmsg))
                {
                    var myInv = new InvLookup.InvLookup();
                    var myList = myInv.GetPhysLocItems(aCountNum, aLocationID, iParse.myParseNet.Globals);
                    if (myList.Count > 0)
                        iParse.myParseNet.Globals.myASCLog.ProcessTran(retval.ErrorMessage, "E");

                    if (errmsg.StartsWith("OK"))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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

        [HttpPut]
        public ASCTracFunctionStruct.ascBasicReturnMessageType RecountPhys(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdLOC_PHYS, "RecountPhys ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myInv = new InvLookup.InvLookup();
                    errmsg= myInv.RecountPhys(aInboundMsg.inputDataList[1], aInboundMsg.inputDataList[2], aInboundMsg.inputDataList[3], iParse.myParseNet.Globals);

                    iParse.myParseNet.Globals.myASCLog.ProcessTran(errmsg, "E");
                    if (errmsg.StartsWith("OK"))
                    {
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
        //public ASCTracFunctionStruct.ascBasicReturnMessageType PhysCount(string aCountNum, string aLocID, string aItemID, string aSkidID, bool aReviewOnly, double aNewQty, double aNewQtyDualUnit, ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        public ASCTracFunctionStruct.ascBasicReturnMessageType PhysCount(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {

            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdLOC_PHYS, "RecountPhys ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myInv = new InvLookup.InvLookup();
                    errmsg= myInv.PhysCount(aInboundMsg.inputDataList[0], aInboundMsg.inputDataList[1], aInboundMsg.inputDataList[2], aInboundMsg.inputDataList[3],
                        aInboundMsg.inputDataList[4].StartsWith("T"),
                        ascLibrary.ascUtils.ascStrToDouble(aInboundMsg.inputDataList[5], 0), ascLibrary.ascUtils.ascStrToDouble(aInboundMsg.inputDataList[6], 0), 
                        iParse.myParseNet.Globals);
                    iParse.myParseNet.Globals.myASCLog.ProcessTran(errmsg, "E");
                    if (errmsg.StartsWith("OK"))
                    {
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


    }
}
