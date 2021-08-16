using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers
{
    public class ImageCaptureController : ApiController
    {
        public ASCTracFunctionStruct.ascBasicReturnMessageType doImageCapture(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                if (iParse.InitParse(ascLibrary.dbConst.cmdGET_ITEM_IMAGE, "doImageCapture", aInboundMsg.UserID, aInboundMsg.SiteID, ref errmsg))
                {

                    ASCTracFunctionStruct.MaintType myMaintType = Newtonsoft.Json.JsonConvert.DeserializeObject<ASCTracFunctionStruct.MaintType>(aInboundMsg.DataMessage);
                    errmsg = doImageCapture(myMaintType.recType, myMaintType.ID, myMaintType.DisplayID, myMaintType.anImage);

                    if (!String.IsNullOrEmpty(errmsg))
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

        public string doImageCapture(string aRecType, string aID, string aDocType, byte[] aImage)
        {
            string retval = string.Empty;
            string tmp = string.Empty;
            if (aRecType.Equals("C") && !iParse.myParseNet.Globals.myGetInfo.GetOrderInfo(aID, "SITE_ID", ref tmp))
                retval = ParseNet.dmascmessages.GetErrorMsg(ascLibrary.TDBReturnType.dbrtNO_ORDERNUM);

            if (String.IsNullOrEmpty(retval))
            {
                var compressType = "JPG";
                var pictureDir = iParse.myParseNet.Globals.myConfig.iniGNFilePathImageCapture.Value;

                var fname = Path.Combine(pictureDir, aRecType + "." + aID + "." + DateTime.Now.ToString("MMddyy-HHmmss") + "." + compressType);

                Image newImage;
                using (MemoryStream ms = new MemoryStream(aImage, 0, aImage.Length))
                {

                    ms.Write(aImage, 0, aImage.Length);

                    newImage = Image.FromStream(ms, true);
                    newImage.Save(fname);
                }
                //ascLibrary.ascUtils.ascWriteLog( .WriteLogFile(myipaddr, "Save Picture " + aImageDataPacket.imageID + ", size " + aImageDataPacket.memStream.ToArray().Length.ToString() + " to " + fname, false);
                //File.WriteAllBytes(fname, aImageDataPacket.memStream.ToArray());

                string sql = "INSERT INTO RECVRPICS ";
                sql += " ( RECTYPE, RECEIVER_ID, PICTURE, SITE_ID, CREATE_DATE, PRINTED_DATETIME, CREATE_USERID, CONTAINER_ID, DOCUMENT_TYPE)";
                sql += " VALUES( @RECTYPE, @RECEIVER_ID, @filename, @siteID, GetDate(), GetDate(), @userid, @containerID, @Document_Type ) ";

                SqlConnection conn = new SqlConnection(iParse.myParseNet.Globals.myDBUtils.myConnString);
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@rectype", ascLibrary.ascStrUtils.ascSubString(aRecType, 0, 1));
                    cmd.Parameters.AddWithValue("@receiver_id", ascLibrary.ascStrUtils.ascSubString(aID, 0, 60));
                    cmd.Parameters.AddWithValue("@Document_Type", aDocType);
                    cmd.Parameters.AddWithValue("@filename", fname);
                    cmd.Parameters.AddWithValue("@siteID", iParse.myParseNet.Globals.curSiteID);
                    cmd.Parameters.AddWithValue("@userid", iParse.myParseNet.Globals.curUserID);
                    cmd.Parameters.AddWithValue("@containerID", "");
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            return (retval);
        }

    }
}
