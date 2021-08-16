using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.Receipt
{
    public class POInfoController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetPOInfo(string aPONumber, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdSTART_RX, "GetPOInfo", aUserID, aSiteID, ref errmsg))
                {
                    var myPOData = new ASCTracFunctionStruct.Receipt.ConfReceiptType();
                    var myPOInfo = new ASCTracFunctionsData.Receipt.POInfo();
                    string ponum = aPONumber;
                    string relnum = aPONumber;
                    iParse.myParseNet.Globals.dmRecv.parseponums(aPONumber, ref ponum, ref relnum);
                    myPOInfo.GetConfirmReceiptPOInfo(ponum, relnum, ref myPOData, ref errmsg);
                    if( String.IsNullOrEmpty( errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myPOData);

                }
                if ( !String.IsNullOrEmpty(errmsg))
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetPOList(int aVendorFilterType, string aVendorData, int aFiltertype, string aFilterData, string aSiteID, string aUserID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdGET_POS, "GetGOList", aUserID, aSiteID, ref errmsg))
                {
                    var myPOInfo = new ASCTracFunctionsData.Receipt.POInfo();
                    var myList = myPOInfo.GetPOList(aVendorFilterType, aVendorData, aFiltertype, aFilterData, true, iParse.myParseNet.Globals);
                    if( myList.Count == 0)
                    {
                        retval.ErrorMessage = "No Records found";
                        retval.successful = false;
                    }
                    else
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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
