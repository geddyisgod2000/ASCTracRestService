using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;

namespace ASCTracRestService
{
    public class WCFUtils
    {
        public static Dictionary<string, string> GetDictionaryList(string aConnStr, string aTblName, string aFields, string aWhereStr, string aIDField, string aValueField, ref string errmsg)
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


        public static string GetInvList(string aWhereStr, ParseNet.GlobalClass Globals)
        {
            DataSet dsLicenses = new DataSet();
            string sql = "SELECT LI.SKIDID, LI.QTYTOTAL, LI.QTYONHOLD, LI.LOCATIONID, LI.EXPDATE, LI.ITEMID, LI.LOTID, LI.REASONFORHOLD, LI.DATETIMEPROD";
            sql += ", LI.PREALLOC_ORDERNUMBER, LI.PREALLOC_WORKORDER_ID, LI.PROMO_CODE, LI.REBLEND_FLAG";
            sql += ", L.TYPE, L.PICKABLE_FLAG, L.PICK_ASSIGNMENT_FLAG, I.DESCRIPTION";
            sql += " FROM LOCITEMS LI";
            sql += " JOIN LOC L ON L.LOCATIONID=LI.LOCATIONID AND L.SITE_ID=LI.SITE_ID";
            sql += " JOIN ITEMMSTR I ON I.ASCITEMID=LI.ASCITEMID";
            sql += " WHERE " + aWhereStr;
            sql += " ORDER BY LI.QAHOLD, LI.EXPDATE, LI.SKIDID";

            using (SqlConnection conn = new SqlConnection(Globals.myDBUtils.myConnString))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
            {
                conn.Open();
                da.MissingSchemaAction = MissingSchemaAction.Add;
                da.Fill(dsLicenses, "LOCITEMS");

                // Write results of query as XML to a string and return
                StringWriter sw = new StringWriter();
                dsLicenses.WriteXml(sw);

                return sw.ToString();

            }
        }
    }
}
