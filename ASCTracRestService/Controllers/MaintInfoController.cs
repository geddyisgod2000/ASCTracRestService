using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers
{
    public class MaintInfoController : ApiController
    {

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetMaintInfo( string aID)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string data = aID;
                string userid = ascLibrary.ascStrUtils.GetNextWord(ref data);
                string siteid = ascLibrary.ascStrUtils.GetNextWord(ref data);
                string RecType = ascLibrary.ascStrUtils.GetNextWord(ref data);
                string RecID = ascLibrary.ascStrUtils.GetNextWord(ref data);

                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdASSET_MAINT, "GetMaintInfo ", userid, siteid, ref errmsg))
                {
                    ASCTracFunctionStruct.MaintType myMaintType = new ASCTracFunctionStruct.MaintType( RecType, RecID, "", "");
                    if (RecType.Equals("I"))
                        errmsg = GetItemMaintInfo(RecID, ref myMaintType);
                    else if (RecType.Equals("L"))
                        errmsg = GetLocMaintInfo(RecID, ref myMaintType);
                    else
                        errmsg = "Invalid Record Type " + RecType;
                    if (String.IsNullOrEmpty(errmsg) && (myMaintType == null))
                    {
                        errmsg = "No Maintenence Record Created";
                    }
                    if (!String.IsNullOrEmpty(errmsg))
                    {
                        retval.ErrorMessage = errmsg;
                        retval.successful = false;
                    }
                    else
                        retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myMaintType);
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
        public ASCTracFunctionStruct.ascBasicReturnMessageType doGetMaintInfo(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdASSET_MAINT, "doMaintInfo ", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {
                    ASCTracFunctionStruct.MaintType myMaintType = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.MaintType>(aInboundMsg.DataMessage);
                    if (myMaintType.recType.Equals("I"))
                        errmsg = doItemMaintInfo(myMaintType);
                    else if (myMaintType.recType.Equals("L"))
                        errmsg = doLocMaintInfo(myMaintType);
                    else
                        errmsg = "Invalid Record Type " + myMaintType.recType;

                    if( !String.IsNullOrEmpty( errmsg))
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


        private string GetItemMaintInfo(string aItemID, ref ASCTracFunctionStruct.MaintType aMaintType)
        {
            string retval = string.Empty;
            string ItemInfo = string.Empty;

            string sizefields = "UNITWIDTH,UNITLENGTH,UNITHEIGHT, ";
            // if dimension check set to label sizes, then check against item size fields.
            if (iParse.myParseNet.Globals.myConfig.iniRXAskDimensions.Value == "L")
                sizefields = "SKIDWIDTH, SKIDLENGTH, SKID_HEIGHT,";
            sizefields += "UnitWeight,BOL_UNITWEIGHT";
            var ascItemID = iParse.myParseNet.Globals.dmMiscItem.getascitemorupc(iParse.myParseNet.Globals.curSiteID, aItemID, string.Empty);
            if (!iParse.myParseNet.Globals.myGetInfo.GetASCItemInfo(ascItemID, "DESCRIPTION, VMI_CUSTID, " + sizefields, ref ItemInfo))
                retval = "Item " + aItemID + " does not exist";
            else
            {
                aMaintType = new ASCTracFunctionStruct.MaintType("I", ascItemID, aItemID, ascLibrary.ascStrUtils.GetNextWord(ref ItemInfo));
                var vmiCustID = ascLibrary.ascStrUtils.GetNextWord(ref ItemInfo);

                sizefields = sizefields.Replace(",", "|");
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_WIDTH), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_LENGTH), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_HEIGHT), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_NETWEIGHT), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_BOLWEIGHT), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
            }
            return (retval);
        }

        private string GetLocMaintInfo(string aLocationID,  ref ASCTracFunctionStruct.MaintType aMaintType)
        {
            string retval = string.Empty;
            string ItemInfo = string.Empty;

            string sizefields = "MAXSKIDSFIT,WIDTH,LOCLEN,HEIGHT,REPL_MIN_QTY,REPL_MAX_QTY ";
            string locfields = "RANDOM,TYPE,ITEMID,CALC_MAXSKIDS_FLAG," + sizefields;

            if (!iParse.myParseNet.Globals.myGetInfo.GetLocInfo(aLocationID, "LOCATIONDESCRIPTION, " + locfields, ref ItemInfo))
                retval = "Location " + aLocationID + " does not exist";
            else
            {
                string locDesc = ascLibrary.ascStrUtils.GetNextWord(ref ItemInfo);
                var fRandom = ascLibrary.ascStrUtils.GetNextWord(ref ItemInfo);
                var fLocType = ascLibrary.ascStrUtils.GetNextWord(ref ItemInfo);
                var itemid = ascLibrary.ascStrUtils.GetNextWord(ref ItemInfo);
                var fCalcMaxSkids = ascLibrary.ascStrUtils.GetNextWord(ref ItemInfo);

                string locTypeDesc = string.Empty;
                iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT TYPE FROM LOCTYPE WHERE LOCTYPEID='" + fLocType + "'", "", ref locTypeDesc);


                aMaintType = new ASCTracFunctionStruct.MaintType("L", aLocationID, aLocationID, locDesc + " (" + locTypeDesc + ")");

                sizefields = sizefields.Replace(",", "|");
                //aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("I", "Location Type","TYPE", locTypeDesc ));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_NUMSKIDS), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_WIDTH), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_LENGTH), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", ParseNet.dmascmessages.getmessagebyid(ParseNet.TASCMessageType.PMSG_PROMPT_HEIGHT), ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", "Restock Point", ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
                aMaintType.myMaintFields.Add(new ASCTracFunctionStruct.FieldEntry<string>("R", "Restock Qty", ascLibrary.ascStrUtils.GetNextWord(ref sizefields), ascLibrary.ascStrUtils.ascGetNextWord(ref ItemInfo, ascLibrary.dbConst.HHDELIM)));
            }
            return (retval);
        }


        private string doItemMaintInfo(ASCTracFunctionStruct.MaintType aMaintType)
        {
            string retval = string.Empty;

            string tmp = string.Empty;
            if (!iParse.myParseNet.Globals.myGetInfo.GetASCItemInfo(aMaintType.ID, "SITE_ID", ref tmp))
                retval = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ITEM);
            else
            {
                string updstr = string.Empty;
                foreach (var rec in aMaintType.myMaintFields)
                {
                    if (!rec.OrigValue.Equals(rec.newValue))
                    {
                        ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, rec.FieldName, rec.newValue.ToString());
                        //retval = updstr;
                    }
                }
                iParse.myParseNet.Globals.mydmupdate.UpdateFields("ITEMMSTR", updstr, "ASCITEMID='" + aMaintType.ID + "'");
                iParse.myParseNet.Globals.mydmupdate.ProcessUpdates();
            }

            return (retval);
        }

        private string doLocMaintInfo(ASCTracFunctionStruct.MaintType aMaintType)
        {
            string retval = string.Empty;
            string tmp = string.Empty;
            if (!iParse.myParseNet.Globals.myGetInfo.GetLocInfo(aMaintType.ID, "TYPE", ref tmp))
                retval = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_LOCATION);
            else
            {
                string updstr = string.Empty;
                foreach (var rec in aMaintType.myMaintFields)
                {
                    if (!rec.OrigValue.Equals(rec.newValue))
                    {
                        ascLibrary.ascStrUtils.ascAppendSetQty(ref updstr, rec.FieldName, rec.newValue.ToString());
                        //retval = updstr;
                    }
                }
                iParse.myParseNet.Globals.mydmupdate.UpdateFields("LOC", updstr, "LOCATIONID='" + aMaintType.ID + "' AND SITE_ID='" + iParse.myParseNet.Globals.curSiteID + "'");
                iParse.myParseNet.Globals.mydmupdate.ProcessUpdates();
            }
            return (retval);
        }
    }
}
