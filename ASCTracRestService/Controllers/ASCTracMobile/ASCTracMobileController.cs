using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace ASCTracRestService.Controllers.ASCTracMobile
{
    public class ASCTracMobileController : ApiController
    {
        private string pictureDir = string.Empty;
        private int pictureIdx = 1;
        /*
        [HttpPut]
        public string doASCTracMobile(String aData)
        {
            string retval = string.Empty;
            try
            {
                retval = iParse.myParseNet.ParseMessage(aData);
            }
            catch (Exception ex)
            {
                retval = "EX" + ex.Message;
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.fErrorData = ex.ToString();
            }
            try
            {
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.ProcessTran(retval, "E");
            }
            catch { }
            return (retval);
        }
        */
        [HttpPut]
        public string doASCTracMobile(ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            string retval = string.Empty;
            string msgNum = string.Empty;
            try
            {
                msgNum = aInboundMsg.DataMessage.Substring(0, 1);
                string data = aInboundMsg.DataMessage.Substring(1);
                string data2 = aInboundMsg.DataMessage.Substring(1);
                string msgtype = ascLibrary.ascStrUtils.GetNextWord(ref data2);
                string userid = ascLibrary.ascStrUtils.GetNextWord(ref data2);
                if ( iParse.InitParse( msgtype, data, userid, "", ref retval))
                    retval = iParse.myParseNet.ParseMessage(data);
            }
            catch (Exception ex)
            {
                retval = "EX" + ex.ToString();
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.fErrorData = ex.ToString();
            }
            try
            {
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.ProcessTran(retval, "E");
            }
            catch { }
            return (msgNum + retval);
        }

        [HttpGet]
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetFilename(string aFilename)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            //const int bufsize = 36000;
            byte[] buffer = null; // new byte[bufsize];
            string errmsg = string.Empty;
            try
            {
                if (iParse.InitParse(ascLibrary.dbConst.cmdGET_PICTURN, aFilename, "", "", ref errmsg))
                {
                    buffer = File.ReadAllBytes(aFilename);
                    //retval.anImage = aImageData; //.DataMessage = System.Text.Encoding.Default.GetString(aImageData);
                    retval.anImage = new byte[buffer.Length + buffer.Length];
                    System.Buffer.BlockCopy(buffer, 0, retval.anImage, 0, buffer.Length);

                    //retval = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                errmsg = ex.Message;
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.fErrorData = ex.ToString();
            }
            try
            {
                retval.ErrorMessage = errmsg;
                if (!String.IsNullOrEmpty(errmsg))
                    retval.successful = false;
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.ProcessTran(errmsg, "E");
            }
            catch { }
            return (retval);
        }


        [HttpPost]
        public string doASCTracImageCapture( string signatureID, ASCTracFunctionStruct.ascBasicInboundMessageType aInboundMsg)
        {
            string retval = string.Empty;
            try
            {
                if (iParse.InitParse(ascLibrary.dbConst.cmdGET_PICTURN, signatureID, "", "", ref retval))
                {
                    string compressType = signatureID;
                    string transferType = ascLibrary.ascStrUtils.GetNextWord(ref compressType); // signature or image
                    string imageType = ascLibrary.ascStrUtils.GetNextWord(ref compressType); // V=Vendor, C=Pick Container, D=Dockschd, T=Test...
                    string imageID = ascLibrary.ascStrUtils.GetNextWord(ref compressType);
                    string containerID = string.Empty;

                    byte[] aImageInBytes = aInboundMsg.anImage; // System.Text.Encoding.UTF8.GetBytes(aInboundMsg.DataMessage);

                    if (imageType.Equals("O"))
                    {
                        if (!iParse.myParseNet.Globals.myDBUtils.ifRecExists("SELECT ORDERNUMBER FROM ORDRHDR WHERE ORDERNUMBER='" + imageID + "'"))
                        {
                            containerID = imageID;
                            if (!iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT ORDERNUM FROM CONTAINR WHERE CONTAINER_ID='" + containerID + "' ORDER BY PICK_DATETIME DESC", "", ref imageID))
                                iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT ORDERNUM FROM CONTAINR WHERE ASN_CONTAINER_ID='" + containerID + "' ORDER BY PICK_DATETIME DESC", "", ref imageID);
                        }
                    }
                    string siteID = "-";
                    string userid = "RF";
                    if (compressType.Contains("|"))
                        siteID = ascLibrary.ascStrUtils.GetNextWord(ref compressType);
                    if (compressType.Contains("|"))
                        userid = ascLibrary.ascStrUtils.GetNextWord(ref compressType);

                    var fSignature = transferType.Equals("SIGNATURE", StringComparison.CurrentCultureIgnoreCase);
                    if (fSignature)
                    {
                        //WriteLogFile(myipaddr, "Save Signature " + aImageDataPacket.imageID, false);
                        if (compressType.Length == 1)
                        {
                            imageID = imageType;
                            imageType = compressType;
                            compressType = "BMP";
                        }

                        string sql = "INSERT INTO IMAGES ";
                        sql += " ( IMAGE_TYPE, IMAGE_ID, DATE_SIGNED, IMAGE, COMPRESS_TYPE )";
                        sql += " VALUES( @IMAGE_TYPE, @image_id, GetDate(), @image, @compress_type ) ";

                        SqlConnection conn = new SqlConnection(iParse.myParseNet.Globals.myDBUtils.myConnString);
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@image_type", ascLibrary.ascStrUtils.ascSubString(imageType, 0, 2));
                            cmd.Parameters.AddWithValue("@image", aImageInBytes);
                            cmd.Parameters.AddWithValue("@image_id", ascLibrary.ascStrUtils.ascSubString(imageID, 0, 30));
                            cmd.Parameters.AddWithValue("@compress_type", compressType);
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(pictureDir))
                        {
                            iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT CFGDATA FROM CFGSETTINGS WHERE CFGFIELD='GNFilePathImageCapture' AND CFGDATA<>'NIL'", "", ref pictureDir);
                            if (string.IsNullOrEmpty(pictureDir))
                                pictureDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\IMAGES\\";
                            else if (!pictureDir.EndsWith("\\"))
                                pictureDir += "\\";
                        }

                        if (compressType.Length <= 1)
                        {
                            //imageID = imageType;
                            //imageType = compressType;
                            compressType = "BMP";
                        }

                        //myDBUtils.RunSqlCommand("DELETE FROM IMAGES WHERE IMAGE_TYPE='" + imageType + "' AND IMAGE_ID='" + imageID + "'");
                        string fname = pictureDir + imageType + "-" + imageID + "-" + pictureIdx.ToString() + "." + compressType;
                        if (imageType == "V")
                            fname = pictureDir + imageID + "." + siteID + "." + DateTime.Now.ToString("MMddyy-HHmmss") + "." + compressType;
                        while (File.Exists(fname))
                        {
                            pictureIdx += 1;
                            fname = pictureDir + imageType + "-" + imageID + "-" + pictureIdx.ToString() + "." + compressType;
                        }
                        pictureIdx += 1;
                        if (pictureIdx >= 1000)
                            pictureIdx = 1;

                        // WriteLogFile(myipaddr, "Save Picture " + aImageDataPacket.imageID + ", size " + aImageDataPacket.memStream.ToArray().Length.ToString() + " to " + fname, false);
                        File.WriteAllBytes(fname, aImageInBytes); // aImageDataPacket.memStream.ToArray());

                        //using (FileStream pfile = new FileStream(fname, FileMode.Create, FileAccess.Write))
                        //{
                        //    aImageDataPacket.memStream.WriteTo(pfile);
                        //pfile.Flush();
                        //pfile.Close();
                        //}
                        string sql = "INSERT INTO RECVRPICS ";
                        sql += " ( RECTYPE, RECEIVER_ID, PICTURE, SITE_ID, CREATE_DATE, CREATE_USERID, CONTAINER_ID)";
                        sql += " VALUES( @RECTYPE, @RECEIVER_ID, @filename, @siteID, GetDate(), @userid, @containerID ) ";

                        SqlConnection conn = new SqlConnection(iParse.myParseNet.Globals.myDBUtils.myConnString);
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            conn.Open();
                            cmd.Parameters.AddWithValue("@rectype", ascLibrary.ascStrUtils.ascSubString(imageType, 0, 1));
                            cmd.Parameters.AddWithValue("@receiver_id", ascLibrary.ascStrUtils.ascSubString(imageID, 0, 60));
                            cmd.Parameters.AddWithValue("@filename", fname);
                            cmd.Parameters.AddWithValue("@siteID", siteID);
                            cmd.Parameters.AddWithValue("@userid", userid);
                            cmd.Parameters.AddWithValue("@containerID", containerID);
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                retval = "EX" + ex.Message;
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.fErrorData = ex.ToString();
            }
            try
            {
                if (iParse.myParseNet.Globals.myASCLog != null)
                    iParse.myParseNet.Globals.myASCLog.ProcessTran(retval, "E");
            }
            catch { }
            return (retval);

        }
    }
}
