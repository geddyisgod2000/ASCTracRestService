using ASCTracRestService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Http;

namespace ASCTracRestService.Controllers
{
    [Filters.ApiAuthenticationFilter]
    public class LogonController : ApiController
    {
        //[HttpPost]
        public ASCTracFunctionStruct.SignonType Signon(ASCTracFunctionStruct.inputType aInboundMsg)
        {
            ASCTracFunctionStruct.SignonType retval = new ASCTracFunctionStruct.SignonType();

            string aPassword = aInboundMsg.inputDataList[0];
            string aUserID = aInboundMsg.UserID;
            string aHHID = aInboundMsg.HHID;
            try
            {
                string aerrmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdSIGN_ON, aUserID, string.Empty, string.Empty, ref aerrmsg))
                {
                    //if (myParseNet.Globals.CheckActiveConnection(ascLibrary.dbConst.cmdSIGN_ON, ref aerrmsg))
                    {

                        //myParseNet.Globals.myASCLog.InitLog("SN", aHHID, "Signon", aUserID);
                        //myParseNet.Globals.curUserID = aUserID;
                        //myParseNet.Signon(aPassword);

                        string amsg = ascLibrary.dbConst.cmdSIGN_ON;
                        amsg += ascLibrary.dbConst.HHDELIM + aUserID;
                        amsg += ascLibrary.dbConst.HHDELIM + "WEB";
                        amsg += ascLibrary.dbConst.HHDELIM + aPassword;

                        string HHCID = aHHID;
                        if (String.IsNullOrEmpty(aHHID) ||
                            !iParse.myParseNet.Globals.myDBUtils.ifRecExists("SELECT ACTIVE_FLAG FROM ACTIVE_CONN " +
                                " WHERE HHC_ADDR='Tablet." + aHHID + "' AND CONNECTION_TYPE='T'"))
                            HHCID = GetNextHHCID(aUserID);

                        if (!String.IsNullOrEmpty(HHCID))
                            amsg = "HHIP:Tablet." + HHCID + ascLibrary.dbConst.HHDELIM + amsg;

                        string rtnmsg = iParse.myParseNet.ParseMessage(amsg);

                        if (rtnmsg.StartsWith(ascLibrary.dbConst.stOK))
                        {
                            retval.menuList = GetMenuList(aUserID);
                            if (retval.menuList.Count == 0)
                            {
                                retval.ReturnMessage = "User does not have rights to Tablet";
                            }
                            else
                            {
                                retval.languageIndex = iParse.myParseNet.Globals.curLanguageIdx;
                                retval.SiteID = iParse.myParseNet.Globals.curSiteID;
                                retval.HHCid = HHCID;
                                retval.ConnectionID = iParse.myParseNet.Globals.curHHCConnectionID;
                                retval.UnitID = iParse.myParseNet.Globals.curHHUnitID;

                                retval.ReturnMessage = ascLibrary.dbConst.stOK +  "User " + aUserID + " successful logon to site " + iParse.myParseNet.Globals.curSiteID;
                                //retval.lookupPrinterList = GetPrinterList(retval.SiteID);
                                //retval.lookupDockList = GetDocks(aUserID, retval.SiteID);
                                //retval.lookupProdlineList = GetProdLines(aUserID, retval.SiteID);
                                retval.lookupReasonCodeList = GetData.GetReasonCodes();
                                //retval.lookupMAFReaosnList = GetMAFReasonCodes();

                                retval.poStatusList = GetStatusLists.GetPOStatusList(aUserID, retval.SiteID);
                                retval.woStatusList = GetStatusLists.GetWOStatusList(aUserID, retval.SiteID);
                                retval.coStatusList = GetStatusLists.GetCOStatusList(aUserID, retval.SiteID);

                                retval.pickStatusList = GetData.GetLookupTypeListJSon("SHIPSTAT", "STATUSID", "DESCRIPTION", "STATUSID IN ( 'B', 'D', 'G', 'H', 'L', 'N', 'S')");
                                retval.lookupDockList = GetData.GetLookupTypeListJSon("DOCKS", "LOADINGBAY", "DESCRIPTION", "SITE_ID='" + retval.SiteID + "' AND ACTIVE_FLAG='T'");
                                retval.lookupLoadLocList = GetData.GetLookupTypeListJSon("LOC", "LOCATIONID", "LOCATIONID + ISNULL( '-' + LOCATIONDESCRIPTION, '')", "SITE_ID='" + retval.SiteID + "' AND TYPE='L' and LOCATIONID < 'Z'");
                                retval.lookupTruckLocList = GetData.GetLookupTypeListJSon("LOC", "LOCATIONID", "LOCATIONID + ISNULL( '-' + LOCATIONDESCRIPTION, '')", "SITE_ID='" + retval.SiteID + "' AND TYPE='T' and LOCATIONID < 'Z'");
                                retval.lookupUserList = GetData.GetLookupTypeListJSon("USERS", "USERID", "FIRSTNAME + ' ' + LASTNAME", "SITE_ID='" + retval.SiteID + "' AND ISACTIVE='T'");
                                retval.lookupLblPrinterList = GetData.GetLookupTypeListJSon("PRNTRS", "PRINTERID", "DESCRIPTION", "SITE_ID='" + retval.SiteID + "' AND TYPEID NOT IN ( 'W', 'B' ) ");
                                retval.lookupBOLPrinterList = GetData.GetLookupTypeListJSon("PRNTRS", "PRINTERID", "DESCRIPTION", "SITE_ID='" + retval.SiteID + "' AND TYPEID IN ( 'B' ) ");
                                retval.lookupDocumentCaptureTypeList = GetData.GetLookupTypeListJSon("RECVRPICS_SETUP", "DOCUMENT_TYPE", "DESCRIPTION", "SITE_ID='" + retval.SiteID + "' AND PROMPT_FLAG='T' ");
                                retval.lookupProdlineList = GetData.GetLookupTypeListJSon("PRODLINE", "PRODLINE", "DESCRIPTION", "SITE_ID='" + retval.SiteID + "' ");
                                retval.lookupCarrierList = GetData.GetLookupTypeListJSon("CARRIER", "CARRIER_ID", "ISNULL( DESCRIPTION, CARRIER_ID)", "SITE_ID='" + retval.SiteID + "' ");
                                retval.lookupShipVIAList = GetData.GetLookupTypeListJSon("SHIPVIA", "ID", "ISNULL( DESCRIPTION, ID)", string.Empty); //GetShipViaList();
                                retval.lookupMAFReaosnList = GetData.GetLookupTypeListJSon("MAF_STATUS", "STATUS", "DESCRIPTION", string.Empty);
                                retval.lookupTrailerTypeList = GetData.GetLookupTypeListJSon("CNTRTYPE", "TYPE", "DESCR", "RECTYPE='T'");

                                //retval.lookupCustList = GetData.GetLookupTypeListJSon("CUST", "CUSTID", "SHIPTONAME", string.Empty); //GetCustList();
                                retval.lookupVendorList = GetData.GetLookupTypeListJSon("VENDOR", "VENDORID", "VENDORNAME", string.Empty); //GetVendorList();
                                retval.lookupWhseList = GetData.GetLookupTypeListJSon("WHSE", "WHSE_ID", "DESCRIPTION", "SITE_ID='" + retval.SiteID + "'"); //GetWhseList();

                                // done when starting Order Entry
                                //retval.lookupCustList =  GetCustList();
                                //retval.lookupItemIDList = GetItemList();

                                string userlevel = string.Empty;
                                iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT ACCESSLEVEL2 FROM SYS_USER_GROUP WHERE ID=" + iParse.myParseNet.Globals.curuserlevel.ToString(), "", ref userlevel);
                                if (String.IsNullOrEmpty(userlevel))
                                    retval.ReturnMessage = "No User Rights for user level " + iParse.myParseNet.Globals.curuserlevel.ToString();
                                else
                                    retval.UserRights = ascLibrary.ascNetEncrypt.ascDecryptString(userlevel);
                            }

                        }
                        else
                        {
                            retval.ReturnMessage = rtnmsg.Substring(2);
                        }
                    }
                }
                else
                {
                    retval.ReturnMessage = aerrmsg;
                }

            }
            catch (Exception e)
            {
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.fErrorData = e.ToString();
                retval.ReturnMessage = e.Message;
            }
            try
            {
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.ProcessTran(retval.ReturnMessage, "E");
            }
            catch //(Exception e)
            {
            }

            //retmsg.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(retval);
            return (retval);
        }
        /*

        [HttpDelete]
        public ASCTracFunctionStruct.ascBasicReturnMessageType Signoff(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retmsg = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            ASCTracFunctionStruct.SignonType retval = new ASCTracFunctionStruct.SignonType();
            //SignonType retval = new SignonType();
            try
            {

                string aerrmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdSIGN_ON, string.Empty, aInboundMsg.UserID, aInboundMsg.SiteID, ref aerrmsg))
                {
                    iParse.myParseNet.Globals.CheckActiveConnection(ascLibrary.dbConst.cmdSIGN_OFF, ref aerrmsg);
                }
                aerrmsg = string.Empty;
            }
            catch //(Exception e)
            {
            }
            return (retmsg);
        }
        */

        private string GetNextHHCID(string aUserID)
        {
            string retval = string.Empty;
            string tmp = string.Empty;
            string updstr = string.Empty;
            if (!iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA FROM CFGSETTINGS WHERE CFGFIELD='NEXT_TABLET_ADDRESS'", string.Empty, ref tmp))
            {
                tmp = "1";
                updstr = "INSERT INTO CFGSETTINGS";
                updstr += " ( CFGDATA, CFGFIELD, SITE_ID, USERID)";
                updstr += " VALUES ('1', 'NEXT_TABLET_ADDRESS', '&&', 'WCFSERVICE')";
                iParse.myParseNet.Globals.myDBUtils.RunSqlCommand(updstr);
            }
            while (string.IsNullOrEmpty(retval))
            {
                if (!iParse.myParseNet.Globals.myDBUtils.ifRecExists("SELECT ACTIVE_FLAG FROM ACTIVE_CONN " +
                        " WHERE HHC_ADDR='" + tmp + "' AND CONNECTION_TYPE='T'"))
                    retval = tmp;
                else
                    tmp = (ascLibrary.ascUtils.ascStrToInt(tmp, 0) + 1).ToString();
            }
            tmp = (ascLibrary.ascUtils.ascStrToInt(retval, 0) + 1).ToString();
            iParse.myParseNet.Globals.myDBUtils.RunSqlCommand("UPDATE CFGSETTINGS SET CFGDATA='" + tmp + "' WHERE CFGFIELD='NEXT_TABLET_ADDRESS'");

            return (retval);
        }

        private Dictionary<string, string> GetMenuList(string aUserID)
        {
            Dictionary<string, string> retval = new Dictionary<string, string>();
            string myPromptFieldname = "PROMPT1";
            if ((iParse.myParseNet.Globals.curLanguageIdx > 1) && (iParse.myParseNet.Globals.curLanguageIdx < 5))
                myPromptFieldname = "PROMPT" + iParse.myParseNet.Globals.curLanguageIdx.ToString();
            string sql = "SELECT HDRID, PROMPT_ID, PROMPT_DEFAULT, " + myPromptFieldname + " FROM HH_SETUP_FILES WHERE RECTYPE='T' AND REQ_FLAG='T' ORDER BY HDRID, PROMPT_ID";

            ascLibrary.ascEncrypt.ReadAccessLevel(iParse.myParseNet.Globals.curuserlevel.ToString(), "TB");

            if (ascLibrary.ascEncrypt.HasAccessRights("TB_ASCTracTablet"))
            {
                SqlConnection myConnection = new SqlConnection(iParse.myParseNet.Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        var menuid = myReader["PROMPT_ID"].ToString();
                        bool fDoit = ascLibrary.ascEncrypt.HasAccessRights("TB" + menuid);
                        if (fDoit)
                        {
                            if (menuid.Equals("X3") || (menuid.StartsWith("P")))
                            {
                                fDoit = iParse.myParseNet.Globals.myConfig.vmProduction.boolValue;
                                //if (fDoit && menuid.Equals("P3"))
                                //    fDoit = iParse.myParseNet.Globals.myConfig.vmProductionRemote.boolValue;
                            }
                        }
                        if (fDoit)
                        {
                            string desc = myReader[myPromptFieldname].ToString();
                            if (String.IsNullOrEmpty(desc))
                                desc = myReader["PROMPT_DEFAULT"].ToString();
                            retval.Add(menuid, desc + "|X" + myReader["HDRID"].ToString());
                        }
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            else
            {
                retval.Clear();
                retval.Add("Error", "No rights");
            }
            return (retval);
        }

    }
}