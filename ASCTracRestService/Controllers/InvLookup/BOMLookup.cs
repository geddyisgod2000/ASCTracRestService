using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ASCTracRestService.Controllers.InvLookup
{
    class BOMLookup
    {
        internal ASCTracFunctionStruct.ascBasicReturnMessageType GetBOMAvailList(string aItemID, double aQty, ParseNet.GlobalClass Globals)
        {
        ASCTracFunctionStruct.ascBasicReturnMessageType retval = new ASCTracFunctionStruct.ascBasicReturnMessageType();
            try
            {
                // Get Items Info
                string ascItemID = Globals.dmMiscItem.GetASCItem(Globals.curSiteID, aItemID, "");
                string iteminfo = string.Empty;
                if (!Globals.myGetInfo.GetASCItemInfo(ascItemID, "DESCRIPTION, STOCK_UOM, PUR_MFG_FLAG, QTYTOTAL, QTYONHOLD, QTYALLOC, QTYSCHEDULED, QTYREQUIRED, QTYHARDALLOC", ref iteminfo))
                {
                    retval.successful = false;
                    retval.ErrorMessage = "Item " + aItemID + " does not exist in site " + Globals.curSiteID;
                }
                else
                {
                    // Get Components of Item Info
                    List<ASCTracFunctionStruct.Inventory.BOMAvailType> myList = new List<ASCTracFunctionStruct.Inventory.BOMAvailType>();
                    myList.Add(new ASCTracFunctionStruct.Inventory.BOMAvailType());
                    myList[0].ItemID = aItemID;
                    myList[0].Description = ascLibrary.ascStrUtils.GetNextWord(ref iteminfo);
                    myList[0].StockUOM = ascLibrary.ascStrUtils.GetNextWord(ref iteminfo);
                    myList[0].PurOrMfgFlag = ascLibrary.ascStrUtils.GetNextWord(ref iteminfo);
                    myList[0].QtyNeeded = aQty;
                    myList[0].QtyTotal = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref iteminfo), 0);
                    myList[0].QtyOnHold = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref iteminfo), 0);
                    myList[0].QtyPicked = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref iteminfo), 0);
                    myList[0].QtyScheduled = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref iteminfo), 0);
                    myList[0].QtyRequired = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref iteminfo), 0);
                    myList[0].QtyHardAlloc = ascLibrary.ascUtils.ascStrToDouble(ascLibrary.ascStrUtils.GetNextWord(ref iteminfo), 0);

                    string sql = "SELECT B.BOMASCITEMID AS COMP_ASCITEMID, B.BOMITEMID as COMP_ITEMID, I.DESCRIPTION, I.DESCRIPTION2, I.STOCK_UOM, ( IQ.QTYAVAIL / SUM(B.QTY)) AS QTYCANBUILD, I.PUR_MFG_FLAG," +
                        " ( SUM( b.qty)*" + aQty.ToString() + ") AS QTY_NEEDED, " +
                        " 0.0 AS QTY_PICKED, IQ.QTYTOTAL, IQ.QTYALLOC, IQ.QTYONHOLD, IQ.QTYSCHEDULED,IQ.QTYREQUIRED," +
                        " IQ.QTYHARDALLOC, IQ.QTYTOSCHEDULE" +
                        " FROM BOM B (NOLOCK)" +
                        " LEFT JOIN ITEMMSTR I (NOLOCK) ON I.ASCITEMID=B.BOMASCITEMID" +
                        " LEFT JOIN ITEMQTY IQ (NOLOCK) ON IQ.ASCITEMID=B.BOMASCITEMID" +
                        " WHERE B.ASCITEMID='" + ascItemID + "'" +
                        " GROUP BY B.BOMASCITEMID, B.BOMITEMID, I.DESCRIPTION, I.DESCRIPTION2, I.STOCK_UOM, I.PUR_MFG_FLAG," +
                        " IQ.QTYTOTAL, IQ.QTYALLOC, IQ.QTYONHOLD, IQ.QTYAVAIL," +
                        " IQ.QTYSCHEDULED,IQ.QTYREQUIRED,IQ.QTYHARDALLOC, IQ.QTYTOSCHEDULE";
                    SqlConnection myConnection = new SqlConnection(Globals.myDBUtils.myConnString);
                    SqlCommand myCommand = new SqlCommand(sql, myConnection);
                    myConnection.Open();
                    try
                    {
                        SqlDataReader myReader = myCommand.ExecuteReader();
                        int idx = 1;
                        while (myReader.Read())
                        {
                            myList.Add(new ASCTracFunctionStruct.Inventory.BOMAvailType());
                            myList[idx].ItemID = myReader["COMP_ITEMID"].ToString();
                            myList[idx].Description = myReader["COMP_ITEMID"].ToString();
                            myList[idx].StockUOM = myReader["STOCK_UOM"].ToString();
                            myList[idx].PurOrMfgFlag = myReader["PUR_MFG_FLAG"].ToString();
                            myList[idx].QtyCanBuild = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYCANBUILD"].ToString(), 0);
                            myList[idx].QtyNeeded = ascLibrary.ascUtils.ascStrToDouble(myReader["QTY_NEEDED"].ToString(), 0);
                            myList[idx].QtyTotal = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYTOTAL"].ToString(), 0);
                            myList[idx].QtyOnHold = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYONHOLD"].ToString(), 0);
                            myList[idx].QtyPicked = ascLibrary.ascUtils.ascStrToDouble(myReader["QTY_PICKED"].ToString(), 0);
                            myList[idx].QtyScheduled = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYSCHEDULED"].ToString(), 0);
                            myList[idx].QtyRequired = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYREQUIRED"].ToString(), 0);
                            myList[idx].QtyHardAlloc = ascLibrary.ascUtils.ascStrToDouble(myReader["QTYHARDALLOC"].ToString(), 0);

                            myList[idx].QtyAvail = myList[idx].QtyTotal - myList[idx].QtyOnHold - myList[idx].QtyPicked;
                            if (myList[idx].QtyAvail < 0)
                                myList[idx].QtyAvail = 0;

                            myList[idx].QtyNotReq = myList[idx].QtyAvail - myList[idx].QtyScheduled - myList[idx].QtyRequired;
                            if (myList[idx].QtyNotReq < 0)
                                myList[idx].QtyNotReq = 0;

                            myList[idx].QtyNotSched = myList[idx].QtyAvail - myList[idx].QtyScheduled;
                            if (myList[idx].QtyNotSched < 0)
                                myList[idx].QtyNotSched = 0;

                            myList[idx].QtyOnOrder = myList[idx].QtyRequired + myList[idx].QtyScheduled;

                            if (myList[idx].QtyAvail < myList[idx].QtyNeeded)
                                myList[idx].AvailStatus = "Not Available";
                            else if (myList[idx].QtyNotReq < myList[idx].QtyNeeded)
                                myList[idx].AvailStatus = "Not Required";
                            else if (myList[idx].QtyNotSched < myList[idx].QtyNeeded)
                                myList[idx].AvailStatus = "Not Scheduled";
                            else
                                myList[idx].AvailStatus = "Available";

                            idx += 1;
                        }
                    }
                    finally
                    {
                        myConnection.Close();
                    }
                    retval.DataMessage = Newtonsoft.Json.JsonConvert.SerializeObject(myList);
                }
            }
            catch (Exception e)
            {
                retval.successful = false;
                retval.ErrorMessage = e.Message;
                Globals.myASCLog.fErrorData = e.ToString();
                Globals.myASCLog.ProcessTran(e.Message, "X");
            }
            return (retval);
        }
    }
}
