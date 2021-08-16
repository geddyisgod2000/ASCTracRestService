using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASCTracRestService.Filters
{
    public class StandardUserServices : IUserServices
    {
        public int Authenticate(string userName, string password)
        {
            int retval = 0;
            string aerrmsg = string.Empty;
            if (iParse.InitParse(ascLibrary.dbConst.cmdSIGN_ON, userName, string.Empty, string.Empty, ref aerrmsg))
            {
                string tmp = string.Empty;
                if( iParse.myParseNet.Globals.myDBUtils.ReadFieldFromDB("SELECT START_DATE, END_DATE FROM ASCREST_AUTH WHERE TOKEN_VALUE='" + password + "'", "", ref tmp))
                {
                    retval = 1;
                }              
                else
                {

                }
                /*

                string amsg = ascLibrary.dbConst.cmdSIGN_ON;
                amsg += ascLibrary.dbConst.HHDELIM + userName;
                amsg += ascLibrary.dbConst.HHDELIM + "WEB";
                amsg += ascLibrary.dbConst.HHDELIM + password;

                string rtnmsg = iParse.myParseNet.ParseMessage(amsg);

                if (rtnmsg.StartsWith(ascLibrary.dbConst.stOK))
                {
                    retval = 1;
                }
                */
            }
            return (retval);
        }
    }
}