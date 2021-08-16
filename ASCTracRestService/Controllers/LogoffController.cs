using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ASCTracRestService.Controllers
{
    public class LogoffController : ApiController
    {
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
                    iParse.myParseNet.Globals.myDBUtils.RunSqlCommand("DELETE ASCREST_AUTH WHERE TOKEN_VALUE='" + aInboundMsg.DataMessage + "'");
                    iParse.myParseNet.Globals.CheckActiveConnection(ascLibrary.dbConst.cmdSIGN_OFF, ref aerrmsg);
                }
                aerrmsg = string.Empty;
            }
            catch //(Exception e)
            {
            }
            return (retmsg);
        }

    }
}
