using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using ApClient.Soap.Messages;

namespace ApClient.Soap
{
    [ExcludeFromCodeCoverage]
    public static class ApSoap
    {
    
        public static object ToObject(string value, string type, out Type theType)
        {
            switch (type)
            {
                case "string":
                    theType = typeof(string);
                    return value;
                case "int":
                    theType = typeof(int);
                    return XmlConvert.ToInt32(value);
                case "unsignedInt":
                    theType = typeof(uint);
                    return XmlConvert.ToUInt32(value);
                case "integer":
                    //THIS IS INSANE!!!
                    theType = typeof(decimal);
                    return XmlConvert.ToDecimal(value);
                case "float":
                    theType = typeof(float);
                    return XmlConvert.ToSingle(value);
                case "dateTime":
                    theType = typeof(DateTime);
                    return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
                case "duration":
                    theType = typeof(TimeSpan);
                    return XmlConvert.ToTimeSpan(value);
                case "boolean":
                    theType = typeof(bool);
                    return XmlConvert.ToBoolean(value);
                case "unsignedByte":
                    theType = typeof(byte);
                    return XmlConvert.ToByte(value);
                case "unsignedShort":
                    theType = typeof(ushort);
                    return XmlConvert.ToUInt16(value);
                case "unsignedLong":
                    theType = typeof(ulong);
                    return XmlConvert.ToUInt64(value);
                case "byte":
                    theType = typeof(byte);
                    return XmlConvert.ToSByte(value);
                case "short":
                    theType = typeof(short);
                    return XmlConvert.ToInt16(value);
                case "long":
                    theType = typeof(long);
                    return XmlConvert.ToInt64(value);
                case "double":
                    theType = typeof(double);
                    return XmlConvert.ToDouble(value);
                default:
                    theType = typeof(string);
                    return value;
            }
        }

        public static LogonRequest CreateLogonRequest(string requestHandle, string userName, string password)
        {
            return new LogonRequest()
            {
                ClientRequestHandle = requestHandle,
                Language = null,
                Password = password,
                UserName = userName,
            };
        }

        public static ReadRequest CreateReadRequest(string reqId, string[] itemNames)
        {
            var rrils = new ReadRequestItemList[1];
            for (int i = 0; i < rrils.Length; i++)
            {
                var ris = new RequestItem[itemNames.Length];
                for (int j = 0; j < ris.Length; j++)
                {
                    var ri = new RequestItem()
                    {
                        ItemRequestSpec = null,
                        ClientItemHandle = j.ToString(),
                        ItemName = itemNames[j],
                        Properties = null,
                    };
                    ris[j] = ri;
                }
                var rril = new ReadRequestItemList()
                {
                    ItemRequestSpec = null,
                    Item = ris,
                };
                rrils[i] = rril;
            }
            var rrrb = new ReadRequestRequestBase()
            {
                ClientRequestHandle = reqId,
                RequestTimeout = XmlConvert.ToString(new TimeSpan(0, 0, 0, 60, 0)),
                ReturnItemName = true,
                ReturnItemPath = true,
                ReturnItemTime = true,
                ScanTimeHint = 1000,
                ScanTimeHintSpecified = true,
            };
            var irs = new ItemRequestSpec()
            {
                //DeadBand = 50.1,
                //DeadBandSpecified = true,
                FromIndex = 0,
                FromIndexSpecified = true,
                //FromTime = DateTime.Now - new TimeSpan(1, 0, 0, 0),
                //FromTimeSpecified = true,
                //ItemPath = null,
                //MaxAge = XmlConvert.ToString(new TimeSpan(0, 0, 0, 60, 0)),
                //MaxSpan = 100,
                //MaxSpanSpecified = true,
                //ReqType = null,
                ToIndex = 1000,
                ToIndexSpecified = true,
                //ToTime = DateTime.Now,
                //ToTimeSpecified = true,
            };
            return new ReadRequest()
            {
                RequestBase = rrrb,
                ItemRequestSpec = irs,
                ItemList = rrils,
            };
        }


        public static object CreateBrowseRequest(string reqId)
        {
            var browseRequest = new BrowseRequest()
            {
                RequestBase = new RequestBase()
                {
                    ClientRequestHandle = reqId,
                    ReturnItemName = true,
                    //RequestTimeout = ToXmlString(TimeSpan.FromSeconds(10), out type),
                    //ReturnItemPath = true,
                    //ReturnItemTime = true
                },                
                LeafFilter = "*",
                BranchFilter = "*",
                MaxItemsReturnedSpecified = true,
                MaxItemsReturned = 300,
                ReturnProperties = true,
                ReturnBranches = false,
                StartPoint = new BrowseRequestStartPoint()
                {
                    Value = "Turbine",
                    //ItemPath = ""
                },
                //ContinuationPoint = ""
            };

            return browseRequest;
        }
    }
}
