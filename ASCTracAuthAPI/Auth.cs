using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ASCTracAuthAPI
{
    public class Auth : IJWTAuth
    {

        private readonly string username = "kirtesh";
        private readonly string password = "Demo1";
        private readonly string key;
        public Auth(string key)
        {
            this.key = key;
        }

        public string Authentication(string username, string password)
        {
            if (!(username.Equals(username) || password.Equals(password)))
            {
                return "ERUsername and Password are required";
            }

            ParseNet.ParseNetMain myParseNet = new ParseNet.ParseNetMain();
            myParseNet.InitParse("AliasASCTrac");
            string amsg = ascLibrary.dbConst.cmdSIGN_ON;
            amsg += ascLibrary.dbConst.HHDELIM + username;
            amsg += ascLibrary.dbConst.HHDELIM + "WEB";
            amsg += ascLibrary.dbConst.HHDELIM + password;

            string rtnmsg = myParseNet.ParseMessage(amsg);

            if (!rtnmsg.StartsWith(ascLibrary.dbConst.stOK))
            {
                if (rtnmsg.StartsWith("ER"))
                    return (rtnmsg);
                return ("ER" + rtnmsg);
            }
            else
            {

                // 1. Create Security Token Handler
                var tokenHandler = new JwtSecurityTokenHandler();

                // 2. Create Private Key to Encrypted
                var tokenKey = Encoding.ASCII.GetBytes(key);

                //3. Create JETdescriptor
                var tokenDescriptor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                        new Claim(ClaimTypes.Name, username)
                        }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
                };
                //4. Create Token
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var retval = tokenHandler.WriteToken(token);
                string numMinutes = "3";
                myParseNet.Globals.myDBUtils.RunSqlCommand("INSERT INTO ASCREST_AUTH (TOKEN_VALUE, START_DATE, END_DATE) VALUES ('" + retval + "', GetDate(), dateadd( n, " + numMinutes + ", GETDATE()))");

                // 5. Return Token from method
                return retval;
            }
        }

    }
}
