using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASCTracRestService.Controllers.CustOrder
{
    class CustOrderFunction
    {
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetScheduleOrderInfo(string aOrderNum, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                var mySchedCO = new ASCTracFunctionsData.CustOrder.COSched(); // ASCTracFunctions.CustOrder.SchedCO();
                ASCTracFunctionStruct.CustOrder.COSchedInfoType myOrder = mySchedCO.getCOSchedInfo(aOrderNum, ref errmsg);
                if (!String.IsNullOrEmpty(errmsg))
                {
                    retval.successful = false;
                    retval.ErrorMessage = errmsg;
                }
                else
                {
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myOrder);
                }
            }
            catch (Exception ex)
            {
                Globals.myASCLog.fErrorData = ex.ToString();
                Globals.myASCLog.ProcessTran(ex.Message, "X");
                retval.successful = false;
                retval.ErrorMessage = ex.Message;
            }

            return (retval);
        }


        public string ScheduleOrder(ASCTracFunctionStruct.CustOrder.CustOrderInfoType aCOInfo, ASCTracFunctionStruct.CustOrder.COSchedInfoType aCOSchedInfo, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;

            try
            {
                List<string> orderlist = new List<string>();
                orderlist.Add(aCOInfo.OrderNumber);
                var mySchedCO = new ASCTracFunctionsData.CustOrder.COSched(); // ASCTracFunctions.CustOrder.SchedCO();
                retval = mySchedCO.doScheduleCO(orderlist, aCOSchedInfo);
            }
            catch (Exception ex)
            {
                Globals.myASCLog.fErrorData = ex.ToString();
                Globals.myASCLog.ProcessTran(ex.Message, "X");
                retval = "EX" + ex.Message;
            }
            
            return (retval);
        }
        
        public ASCTracFunctionStruct.ascBasicReturnMessageType GetCOShipInfo(string aOrderNum, ParseNet.GlobalClass Globals)
        {
            ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                string errmsg = string.Empty;
                var myShipCO = new ASCTracFunctionsData.CustOrder.COShip();
                var myData = myShipCO.getCOShipInfo( "C", aOrderNum, "", false, ref errmsg);
                //ASCTracFunctionStruct.CustOrder.COShipType myOrder = myShipCO.getCOShipInfo( aOrderNum, ref errmsg);
                if (!String.IsNullOrEmpty(errmsg))
                {
                    retval.successful = false;
                    retval.ErrorMessage = errmsg;
                }
                else
                {
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myData);
                }
            }
            catch (Exception ex)
            {
                Globals.myASCLog.fErrorData = ex.ToString();
                Globals.myASCLog.ProcessTran(ex.Message, "X");
                retval.successful = false;
                retval.ErrorMessage = ex.Message;
            }

            return (retval);
        }
        
        public string COConfirmShip(ASCTracFunctionStruct.CustOrder.CustOrderInfoType aCOInfo, ASCTracFunctionStruct.CustOrder.COShipType aCOShipInfo, ParseNet.GlobalClass Globals)
        {
            string retval = string.Empty;

            try
            {
                List<string> orderlist = new List<string>();
                orderlist.Add(aCOInfo.OrderNumber);
                var myShipCO = new ASCTracFunctionsData.CustOrder.COShip();

                //retval = myShipCO.doShipCO(orderlist, aCOShipInfo);
            }
            catch (Exception ex)
            {
                Globals.myASCLog.fErrorData = ex.ToString();
                Globals.myASCLog.ProcessTran(ex.Message, "X");
                retval = "EX" + ex.Message;
            }
            
            return (retval);
        }
        
    }
}
