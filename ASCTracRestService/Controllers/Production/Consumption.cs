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
    public class Consumption
    {
        //=======================================================================
        public string GetWOComponents(string aWO, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;

            try
            {
                DataSet dsLicenses = new DataSet();

                // ConfigurationManager.AppSettings["dbconnstring"];
                string sql = "SELECT D.SEQ_NUM, D.COMP_ITEMID, I.DESCRIPTION, D.QTY, D.QTY_PICKED, D.QTY_USED, MAX( L.LOCATIONID) AS KANBAN_LOCATION";
                sql += " FROM WO_DET D ";
                sql += " JOIN ITEMMSTR I ON I.ASCITEMID=D.COMP_ASCITEMID";
                sql += " LEFT JOIN LOC L ON L.ASCITEMID=D.COMP_ASCITEMID AND L.TYPE='K'";
                sql += " WHERE D.WORKORDER_ID='" + aWO + "'";
                sql += " GROUP BY D.SEQ_NUM, D.COMP_ITEMID, I.DESCRIPTION, D.QTY, D.QTY_PICKED, D.QTY_USED";
                sql += " ORDER BY D.SEQ_NUM";

                using (SqlConnection conn = new SqlConnection(Globals.myDBUtils.myConnString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    conn.Open();
                    da.MissingSchemaAction = MissingSchemaAction.Add;
                    da.Fill(dsLicenses, "WO_DET");

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
        }

        //=======================================================================
        public string GetWOComponentLicenses(string aWO, long seqnum, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;

            try
            {
                DataSet dsLicenses = new DataSet();

                // ConfigurationManager.AppSettings["dbconnstring"];
                string sql = "SELECT SKIDID, QTYTOTAL, LOCATIONID, DATETIMEPROD, LOTID, EXPDATE, PUTDOWN_DATETIME FROM LOCITEMS";
                sql += " WHERE PICKORDERNUM='" + aWO + "' AND PICKLINENUM='" + seqnum.ToString() + "'";
                sql += " ORDER BY PUTDOWN_DATETIME";

                using (SqlConnection conn = new SqlConnection(Globals.myDBUtils.myConnString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    conn.Open();
                    da.MissingSchemaAction = MissingSchemaAction.Add;
                    da.Fill(dsLicenses, "LOCITEMS");

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
        }

        //========================================================================
        public string WOIssueComponent(string aWO, long seqnum, string aSkidID, string aFGSkidID, string aItemID, string aLocID, double aQtyIssued, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;
            try
            {
                ascLibrary.TDBReturnType tmpRetVal = ascLibrary.TDBReturnType.dbrtUNKNOWN_ERR;
                if (string.IsNullOrEmpty(aSkidID) || (aSkidID.StartsWith("-")))
                {
                    //ascLibrary.TDBReturnType retval = myParseNet.Globals.dmProd.issueitem( aWO, "", )
                }
                else
                    tmpRetVal = Globals.dmProd.issueskid(aWO, string.Empty, aSkidID, aFGSkidID, seqnum.ToString(), string.Empty, aQtyIssued);
                if (tmpRetVal.Equals(ascLibrary.TDBReturnType.dbrtOK))
                    Globals.mydmupdate.ProcessUpdates();
                retval = ParseNet.dmascmessages.GetErrorMsg(tmpRetVal);
            }
            catch (Exception e)
            {
                retval = "EX" + e.ToString();
            }
            return (retval);
        }


    }
}
