using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.CustOrder
{
    public class COInfoController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetGOInfo(string aOrderNumber, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_GET_ORDER, "GetGOInfo", aUserID, aSiteID, ref errmsg))
                {
                    CustOrderInfo myOrderInfo = new CustOrderInfo();
                    return (myOrderInfo.GetCOInfo(aOrderNumber, iParse.myParseNet.Globals));
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

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetGOList(int aCustFilterType, string aCustData, int aFiltertype, string aFilterData, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_GET_ORDER, "GetGOList", aUserID, aSiteID, ref errmsg))
                {
                    CustOrderInfo myOrderInfo = new CustOrderInfo();
                    return (myOrderInfo.GetCOList(aCustFilterType, aCustData, aFiltertype, aFilterData, true, iParse.myParseNet.Globals));
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

        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType CalcPCE(string aOrderType, string aOrderNum, string aLineNum, ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_GET_ORDER, "GetGOInfo", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {

                    if (aOrderType.Equals("C"))
                        errmsg = iParse.myASCFunction.DoFunction(ASCTracFunctions.FuncConst.funcCO_CALC_PCE
                            + ascLibrary.dbConst.HHDELIM + aOrderNum
                            + ascLibrary.dbConst.HHDELIM + aLineNum, aInboundMsg.UserID, string.Empty);
                    else if (aOrderType.Equals("P"))
                        errmsg = iParse.myASCFunction.DoFunction(ASCTracFunctions.FuncConst.funcPO_CALC_PCE
                            + ascLibrary.dbConst.HHDELIM + aOrderNum
                            + ascLibrary.dbConst.HHDELIM + aLineNum, aInboundMsg.UserID, string.Empty);
                    else
                        errmsg = iParse.myASCFunction.DoFunction(ASCTracFunctions.FuncConst.funcCO_CALC_PCE_GROUP
                            + ascLibrary.dbConst.HHDELIM + aOrderType
                            + ascLibrary.dbConst.HHDELIM + aOrderNum, aInboundMsg.UserID, string.Empty);

                    if( String.IsNullOrEmpty( errmsg) || errmsg.StartsWith( ascLibrary.dbConst.stOK))
                    {
                    }
                    else
                    {
                        retval.ErrorMessage = errmsg;
                        retval.successful = false;
                    }
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

        [HttpPost]
        public ASCTracFunctionStruct.ascBasicReturnMessageType UpdateOrdrDet(string aCO, string aLineNum, string aPCEType, string aNewStatus, string aClearPickLoc, ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdCP_UPDATE_ORDER, "UpdateOrdrDet", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myCustOrder = new CustOrder();
                    errmsg = myCustOrder.UpdateOrdrDet(aCO, ascLibrary.ascUtils.ascStrToInt( aLineNum, -1), aPCEType, aNewStatus, aClearPickLoc.StartsWith("T"), iParse.myParseNet.Globals);
                    if (String.IsNullOrEmpty(errmsg) || errmsg.StartsWith(ascLibrary.dbConst.stOK))
                    {
                    }
                    else
                    {
                        retval.ErrorMessage = errmsg;
                        retval.successful = false;
                    }
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

