using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ASCTracRestService
{
    public class iParse
    {
        internal static ParseNet.ParseNetMain myParseNet = new ParseNet.ParseNetMain();
        internal static ASCTracFunctions.ASCTracFunctionMain myASCFunction = new ASCTracFunctions.ASCTracFunctionMain();
        private static bool fInit = false;
        internal static bool InitParse(string MsgType, string aAuditStartData, string aUserID, string aSiteID, ref string aErrMsg)
        {
            aErrMsg = string.Empty;
            bool retval = true;
            try
            {
                if (!fInit)
                {
                    string myConnStr = string.Empty;
                    try
                    {
                      myConnStr = ConfigurationManager.ConnectionStrings["ASCTracConnectionString"].ConnectionString;
                    }
                    catch
                    { }
                    if ( String.IsNullOrEmpty( myConnStr))
                        myConnStr = "packet size=4096;user id=sa;Password='XAther7G';data source=asc-ax01;persist security info=False;initial catalog=ASCTRAC904Dev";

                    //myParseNet.InitParse(  " AliasASCTrac");
                    myParseNet.InitParse(myConnStr, ref aErrMsg);

                    if (string.IsNullOrEmpty(aErrMsg))
                    {
                        myParseNet.Globals.initASCLog("ascTracRestService", "ascTracRestService", "1", "ACTrac WCF Service");
                        fInit = true;

                        myASCFunction.InitMain(myConnStr);
                    }
                    if (!String.IsNullOrEmpty(aUserID))
                        myParseNet.Globals.curUserID = aUserID.ToUpper();
                    if (!String.IsNullOrEmpty(aSiteID))
                        myParseNet.Globals.initsite(aSiteID.ToUpper());
                    if (MsgType != "")
                        myParseNet.Globals.myASCLog.InitLog(MsgType, myParseNet.Globals.curEquipHHCIpAddr, aAuditStartData, aUserID);
                    else
                        retval = false;
                    ASCTracFunctionsData.DataGlobals.myDataGlobals.Globals = myParseNet.Globals;
                }
            }
            catch (Exception ex)
            {
                aErrMsg = ex.ToString();
                retval = false;
            }
            return (retval);
        }


        internal static bool InitParse(string MsgType, string aAuditStartData, ASCTracFunctionStruct.inputType aInboundMsg, ref string aErrMsg)
        {
            aErrMsg = string.Empty;
            bool retval = true;
            try
            {
                if (!fInit)
                {
                    myParseNet.InitParse("AliasASCTrac");
                    myParseNet.Globals.initASCLog("ascTracWCFService", "ascTracWCFService", "1", "ACTrac WCF Service");
                    fInit = true;

                    myASCFunction.InitMain();
                }
                if (!String.IsNullOrEmpty(aInboundMsg.UserID))
                    myParseNet.Globals.curUserID = aInboundMsg.UserID.ToUpper();
                if (!String.IsNullOrEmpty(aInboundMsg.SiteID))
                    myParseNet.Globals.initsite(aInboundMsg.SiteID.ToUpper());
                myParseNet.Globals.curEquipHHCIpAddr = aInboundMsg.HHID;
                if (MsgType != "")
                    myParseNet.Globals.myASCLog.InitLog(MsgType, myParseNet.Globals.curEquipHHCIpAddr, aAuditStartData, myParseNet.Globals.curUserID);
                ASCTracFunctions.ASCTracFunctionMain.myParseNet = myParseNet;
                ASCTracFunctionsData.DataGlobals.myDataGlobals.Globals = myParseNet.Globals;

                myParseNet.Globals.curHHCConnectionID = aInboundMsg.ConnectionID;
                myParseNet.Globals.curHHUnitID = aInboundMsg.UnitID;
                myParseNet.Globals.curDeviceID = aInboundMsg.HHID;
                myParseNet.Globals.curDeviceType = "T";

            }
            catch (Exception ex)
            {
                aErrMsg = ex.Message;
                retval = false;
            }
            return (retval);
        }
    }
}