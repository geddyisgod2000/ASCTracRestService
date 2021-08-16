using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers.QCTest
{
    public class QCTestController : ApiController
    {
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetQCTestResults(string aWorkorder, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdQC_TEST_LIST, "GetQCTestResults ", aUserID, aSiteID, ref errmsg))
                {
                    var myObj = new qc();
                    var myList = myObj.GetWOQCTests(iParse.myParseNet.Globals, aWorkorder);

                    if (myList.StartsWith("EX"))
                    {
                        errmsg = myList.Substring(2);
                    }
                    else
                        retval.DataMessage = myList; // Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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
        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetQCTestInfo(string aRecType, string aRecID, string aUserID, string aSiteID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdQC_TEST_LIST, "GetQCTests ", aUserID, aSiteID, ref errmsg))
                {
                    var myObj = new qc();
                    var myList = myObj.GetSkidQCInfo(iParse.myParseNet.Globals, aRecType, aRecID, ref errmsg);

                    if (String.IsNullOrEmpty(errmsg))
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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

        [HttpPut]
        public ASCTracFunctionStruct.ascBasicReturnMessageType doQCTest( string aRecType, string aRecID, ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdQC_TEST, "doQCTests ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    var myObj = new qc();
                    var myData = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.QC.QCTestInstance>(aInboundMsg.DataMessage);
                    errmsg = myObj.SkidQCTest(iParse.myParseNet, aRecType, aRecID, myData);

                    //if (String.IsNullOrEmpty(errmsg))
                    //    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
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
