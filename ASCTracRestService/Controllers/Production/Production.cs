using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace ASCTracRestService.Controllers.Production
{
    public class Production
    {
        //===================================================================================
        //public string ProdNewSkid(ProdNewSkidType aNewSkidRec, ParseNet.GlobalClass Globals)
        public string ProdNewSkid(ASCTracFunctionStruct.Production.ProdNewSkidType aNewSkidRec, ParseNet.GlobalClass Globals)
        {
            string retval = ascLibrary.dbConst.stOK;

            try
            {
                ascLibrary.TDBReturnType rettype = ascLibrary.TDBReturnType.dbrtOK;
                Globals.myASCLog.InitLog(ascLibrary.dbConst.cmdPROD_ADD_SKID, "", aNewSkidRec.Workorder_ID, Globals.curUserID);
                string ascitemid = string.Empty;
                string newSkid = string.Empty;
                string lotid = string.Empty;
                string invcontainer = string.Empty;
                string submsg = string.Empty;
                double alotqty = 0;
                if (!Globals.myGetInfo.GetWOHdrInfo(aNewSkidRec.Workorder_ID, "PRODLINE,PROD_ASCITEMID", ref ascitemid))
                    retval = "Work Order " + aNewSkidRec.Workorder_ID + " does not exist.";
                else
                {
                    string fvalidate = "0";
                    if (Globals.myConfig.iniPPValidSkid.Value == "V")
                        fvalidate = "1";

                    string prodline = ascLibrary.ascStrUtils.GetNextWord(ref ascitemid);
                    rettype = Globals.dmInventory.prodaddskid(prodline, aNewSkidRec.Workorder_ID, ascitemid, aNewSkidRec.PrinterID, "",
                        aNewSkidRec.SkidID, "", "", aNewSkidRec.ExpDate, "", "", aNewSkidRec.AssetID, "", "", aNewSkidRec.overridePassword,
                        aNewSkidRec.QtyMade, 0, 0, fvalidate, false, aNewSkidRec.numSkids, aNewSkidRec.QtyLabels,
                        ref newSkid, ref lotid, ref invcontainer, ref submsg, ref alotqty);
                    switch (rettype)
                    {
                        case ascLibrary.TDBReturnType.dbrtOK:
                            {
                                Globals.mydmupdate.ProcessUpdates();
                                string tmp = "F";
                                if (Globals.myConfig.iniPPValidSkid.Value == "V")
                                {
                                    tmp = Globals.dmSerial.getsernumprodflag(ascitemid) + ascLibrary.dbConst.HHDELIM;
                                    if (tmp != "F")
                                        tmp = tmp + Globals.dmSerial.checkforserialtoconsume(aNewSkidRec.Workorder_ID, "");
                                }
                                retval = ascLibrary.dbConst.stOK + newSkid + ascLibrary.dbConst.HHDELIM
                                          + lotid + ascLibrary.dbConst.HHDELIM
                                          + alotqty.ToString() + ascLibrary.dbConst.HHDELIM
                                          + invcontainer + ascLibrary.dbConst.HHDELIM
                                          + tmp + ascLibrary.dbConst.HHDELIM; // serial flag
                            }
                            break;
                        case ascLibrary.TDBReturnType.dbrtNEED_INFO: retval = ascLibrary.dbConst.stQuery + submsg;
                            break;
                        case ascLibrary.TDBReturnType.dbrtOVER_SKID_QTY: retval = ascLibrary.dbConst.stQuery + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROD_OVER_VARIANCE); //'Variance on producing~is more than allowed~on license, please override.';
                            break;
                        case ascLibrary.TDBReturnType.dbrtNO_ASSET:
                            retval = ascLibrary.dbConst.stERR + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_PROD_NO_ASSET);
                            break;
                        case ascLibrary.TDBReturnType.dbrtMIXED:
                            retval = ascLibrary.dbConst.stQuery + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROD_MIXED_LOT);
                            break;
                        case ascLibrary.TDBReturnType.dbrtNO_QTY:
                            {
                                retval = ascLibrary.dbConst.stERR + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_PROD_NO_COMPONENTS); //'Components not~at Production area~to produce License.';
                            }
                            break;
                        case ascLibrary.TDBReturnType.dbrtOVER_QTY:
                            retval = ascLibrary.dbConst.stERR + ascLibrary.ascStrUtils.ascFormatStr(ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_PROD_OVER_QTY), new string[] { aNewSkidRec.numSkids.ToString(), aNewSkidRec.QtyMade.ToString() });
                            break;
                        case ascLibrary.TDBReturnType.dbrtNO_ORDERNUM: retval = ascLibrary.dbConst.stERR + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_PROD_NO_WO); //'No active workorder to produce against.';
                            break;
                        case ascLibrary.TDBReturnType.dbrtWRONG_DATE: retval = ascLibrary.dbConst.stERR + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_PROD_FUTURE); //'Production Date cannot be in the future.';
                            break;
                        case ascLibrary.TDBReturnType.dbrtWRONG_SITE: retval = ascLibrary.dbConst.stERR + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_VALID_WO_WRONG_SITE); //'WO at another site.';
                            break;
                        case ascLibrary.TDBReturnType.dbrtNOT_PREBLEND: retval = ascLibrary.dbConst.stERR + ascLibrary.dbConst.stERR + ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PERR_GEN_NEED_PREBLEND);
                            break;
                        default: retval = ParseNet.dmascmessages.GetErrorMsg(rettype);
                            break;
                    }
                    retval = retval.Replace("~", "\r\n");
                }
            }
            catch (Exception e)
            {
                Globals.myASCLog.fErrorData = e.ToString();
                retval = ascLibrary.dbConst.stERR + e.Message;
            }
            try
            {
                if (!String.IsNullOrEmpty(retval))
                    Globals.myASCLog.ProcessTran(retval, "E");
            }
            catch //(Exception e)
            {
            }
            return (retval);
        }

        //=======================================================================
        public string GetWOLicenses(string aWO, ParseNet.GlobalClass Globals)
        {
            try
            {
                DataSet dsLicenses = new DataSet();

                // ConfigurationManager.AppSettings["dbconnstring"];
                string sql = "SELECT SKIDID, QTYTOTAL, QAHOLD, REASONFORHOLD, DATETIMEPROD, LOCATIONID, LOTID, EXPDATE, QTY_NOT_VALID";
                sql += " FROM LOCITEMS WHERE WORKORDER_ID='" + aWO + "' AND SITE_ID='" + Globals.curSiteID + "'";
                sql += " UNION";
                sql += " SELECT SKIDID, QTYTOTAL, 'F' AS QAHOLD, NULL AS REASONFORHOLD, DATETIMEPROD, 'HISTORY' AS LOCATIONID, LOTID, EXPDATE, 0 AS QTY_NOT_VALID";
                sql += " FROM OLDLCITM WHERE WORKORDER_ID='" + aWO + "' AND SITE_ID='" + Globals.curSiteID + "'";
                sql += " ORDER BY DATETIMEPROD";
                using (SqlConnection conn = new SqlConnection(Globals.myDBUtils.myConnString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    conn.Open();
                    da.MissingSchemaAction = MissingSchemaAction.Add;
                    da.Fill(dsLicenses, "LICENSES");

                    // Write results of query as XML to a string and return
                    StringWriter sw = new StringWriter();
                    dsLicenses.WriteXml(sw);

                    return "OK" + sw.ToString();
                }
            }
            catch (Exception e)
            {
                return "EX" + e.ToString();
            }

            //return (retval);
        }

    }
}
