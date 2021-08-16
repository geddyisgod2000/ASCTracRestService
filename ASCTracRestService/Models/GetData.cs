using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ASCTracRestService.Models
{
    internal class GetData
    {

        private static Dictionary<string, string> GetDictionaryList(string aConnStr, string aTblName, string aFields, string aWhereStr, string aIDField, string aValueField, ref string errmsg)
        {
            string fields = aFields;
            if (String.IsNullOrEmpty(fields))
                fields = aIDField + "," + aValueField;
            Dictionary<string, string> retval = new Dictionary<string, string>();
            errmsg = string.Empty;
            string sql = "SELECT " + fields + " FROM " + aTblName;
            if (!String.IsNullOrEmpty(aWhereStr))
                sql += " WHERE " + aWhereStr;
            try
            {
                SqlConnection myConnection = new SqlConnection(aConnStr);
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        retval.Add(myReader[aIDField].ToString(), myReader[aValueField].ToString());
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                retval.Clear();
            }
            return (retval);
        }


        internal static Dictionary<string, string> GetDictionaryList(string aTblName, string aFields, string aWhereStr, string aIDField, string aValueField, string aUserID)
        {
            Dictionary<string, string> retval = new Dictionary<string, string>();
            string errmsg = string.Empty;
            try
            {
                retval = GetDictionaryList(iParse.myParseNet.Globals.myDBUtils.myConnString, aTblName, aFields, aWhereStr, aIDField, aValueField, ref errmsg);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
            }
            if (!String.IsNullOrEmpty(errmsg))
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = "Get Dictionary List Exception for " + aTblName + "." + aFields + "\r\n" + errmsg;
                retval.Clear();
                retval.Add("Error", errmsg);
            }

            return (retval);
        }

        internal static Dictionary<string, string> GetReasonCodes()
        {
            string selectStr = "REASON_CODE" +
                ", ISNULL( REASON_TYPE, '') + '|' + ISNULL( DESCRIPTION, '') + '|' + ISNULL( MAF_FLAG, 'F')" +
                " + '|' + ISNULL( COST_CENTER_ID, '') + '|' + ISNULL( ASK_COST_CENTER, 'F') + '|' + ISNULL( ASK_RESP_SITE, 'F') " +
                " + '|' + ISNULL( ASK_COMMENT, 'F') + '|' + ISNULL( ASK_PROJECT_NUMBER, 'F') AS REASON_DATA";
            return (GetDictionaryList("REASNCDS", selectStr, string.Empty, "REASON_CODE", "REASON_DATA", string.Empty));
        }

        internal static List<ASCTracFunctionStruct.LookupItemType> GetLookupTypeListJSon(string aTblName, string aIDField, string aDescField, string aWhereStr)
        {
            List<ASCTracFunctionStruct.LookupItemType> retval = new List<ASCTracFunctionStruct.LookupItemType>();
            string sql = "SELECT " + aIDField + " as MYID," + aDescField + " as MYDESCRIPTION FROM " + aTblName;
            if (!String.IsNullOrEmpty(aWhereStr))
                sql += " WHERE " + aWhereStr;
            try
            {
                SqlConnection myConnection = new SqlConnection(iParse.myParseNet.Globals.myDBUtils.myConnString);
                SqlCommand myCommand = new SqlCommand(sql, myConnection);
                myConnection.Open();
                try
                {
                    SqlDataReader myReader = myCommand.ExecuteReader();
                    while (myReader.Read())
                    {
                        retval.Add(new ASCTracFunctionStruct.LookupItemType(myReader["MYID"].ToString(), myReader["MYDESCRIPTION"].ToString()));
                    }
                }
                finally
                {
                    myConnection.Close();
                }
            }
            catch (Exception e)
            {
                iParse.myParseNet.Globals.myASCLog.fErrorData = "GetList for " + aTblName + "\r\n" + e.ToString() + "\r\n" + sql;
            }
            return (retval);
        }


    }
}