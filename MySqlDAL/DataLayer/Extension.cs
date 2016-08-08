using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Common.DataLayer
{
    public static class Extension
    {
       
       

        public static string DistinctFromMany(this IEnumerable<string> source)
        {
            var str = String.Join(",", source);
            return String.Join(", ", str.Split(',').Select(x => x.Trim()).Distinct().OrderBy(y => y).ToList());
        }

        public static string ToJsonFromXml(this XmlNode xmlNode)
        {
            var nodesNull = xmlNode.SelectNodes("//*[(@type='boolean' or @type='number' or @type='string') and not(text())]");
            foreach (XmlNode nodeNull in nodesNull) nodeNull.Attributes["type"].Value = "null";

            string json;
            using (var mS = new MemoryStream())
            {
                using (var jsonReaderWriterFactory = JsonReaderWriterFactory.CreateJsonWriter(mS))
                {
                    xmlNode.WriteTo(jsonReaderWriterFactory);
                    jsonReaderWriterFactory.Flush();
                    mS.Position = 0;
                    using (var sR = new StreamReader(mS)) { json = sR.ReadToEnd(); }
                }
            }

            var jObject = JObject.Parse(json);
            json = JsonConvert.SerializeObject(jObject);
            var expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var jsonSerializerSettings = new JsonSerializerSettings() { ContractResolver = new LowerCaseFirstPropertyNamesContractResolver() };
            json = JsonConvert.SerializeObject(expandoObject, jsonSerializerSettings);

            return json;
        }

        public static JObject ToJObjectFromXml(this XmlNode xmlNode)
        {
            return JObject.Parse(xmlNode.ToJsonFromXml());
        }

        public static XmlDocument ToXmlFromJson(this string json)
        {
            var jObject = JObject.Parse(json);
            json = JsonConvert.SerializeObject(jObject);
            var expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            var jsonSerializerSettings = new JsonSerializerSettings() { ContractResolver = new UpperCaseFirstPropertyNamesContractResolver() };
            json = JsonConvert.SerializeObject(expandoObject, jsonSerializerSettings);

            XmlDocument xmlDocument = new XmlDocument();
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), XmlDictionaryReaderQuotas.Max))
            {
                XElement xml = XElement.Load(reader);
                xmlDocument.LoadXml(xml.ToString());
            }
            return xmlDocument;
        }

        public static XmlDocument ToXmlFromJObject(this JObject jObject)
        {
            return JsonConvert.SerializeObject(jObject).ToXmlFromJson();

            //var json = JsonConvert.SerializeObject(jObject);
            //var expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(json);
            //var jsonSerializerSettings = new JsonSerializerSettings() { ContractResolver = new UpperCaseFirstPropertyNamesContractResolver() };
            //json = JsonConvert.SerializeObject(expandoObject, jsonSerializerSettings);
            //XmlDocument xmlDocument = new XmlDocument();
            //using (var reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(json), XmlDictionaryReaderQuotas.Max))
            //{
            //    XElement xml = XElement.Load(reader);
            //    xmlDocument.LoadXml(xml.ToString());
            //}
            //return xmlDocument;

            //var xmlDocument = JsonConvert.DeserializeXmlNode(jObject.ToString(), "root");
            //var stringBuilder = new StringBuilder();
            //using (var stringWriter = new StringWriter())
            //{
            //    using (var xmlTextWriter = new XmlTextWriter(stringWriter))
            //    {
            //        xmlTextWriter.Formatting = System.Xml.Formatting.Indented;
            //        xmlDocument.WriteTo(xmlTextWriter);
            //    }
            //}
            //return xmlDocument;
        }

        //public static HttpPostedFile ToHttpPostedFile(this byte[] data, string filename, string contentType)
        //{
        //    // Get the System.Web assembly reference
        //    Assembly systemWebAssembly = typeof(HttpPostedFileBase).Assembly;
        //    // Get the types of the two internal types we need
        //    Type typeHttpRawUploadedContent = systemWebAssembly.GetType("System.Web.HttpRawUploadedContent");
        //    Type typeHttpInputStream = systemWebAssembly.GetType("System.Web.HttpInputStream");

        //    // Prepare the signatures of the constructors we want.
        //    Type[] uploadedParams = { typeof(int), typeof(int) };
        //    Type[] streamParams = { typeHttpRawUploadedContent, typeof(int), typeof(int) };
        //    Type[] parameters = { typeof(string), typeof(string), typeHttpInputStream };

        //    // Create an HttpRawUploadedContent instance
        //    object uploadedContent = typeHttpRawUploadedContent
        //      .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, uploadedParams, null)
        //      .Invoke(new object[] { data.Length, data.Length });

        //    // Call the AddBytes method
        //    typeHttpRawUploadedContent
        //      .GetMethod("AddBytes", BindingFlags.NonPublic | BindingFlags.Instance)
        //      .Invoke(uploadedContent, new object[] { data, 0, data.Length });

        //    // This is necessary if you will be using the returned content (ie to Save)
        //    typeHttpRawUploadedContent
        //      .GetMethod("DoneAddingBytes", BindingFlags.NonPublic | BindingFlags.Instance)
        //      .Invoke(uploadedContent, null);

        //    // Create an HttpInputStream instance
        //    object stream = (Stream)typeHttpInputStream
        //      .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, streamParams, null)
        //      .Invoke(new object[] { uploadedContent, 0, data.Length });

        //    // Create an HttpPostedFile instance
        //    HttpPostedFile postedFile = (HttpPostedFile)typeof(HttpPostedFile)
        //      .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null)
        //      .Invoke(new object[] { filename, contentType, stream });

        //    return postedFile;
        //}

        #region Private Methods
        private static string FormatPatentNumber(string patentNumber)
        {
            string retValue = patentNumber;
            int iPatentNumber;
            string tempDigits = String.Empty;
            string tempString = String.Empty;
            for (int i = 0; i < patentNumber.Length; i++)
            {
                if (Char.IsDigit(patentNumber[i]))
                {
                    tempDigits = patentNumber.Substring(i);
                    tempString = patentNumber.Substring(0, i);
                    break;
                }
            }
            if (tempDigits.Length > 0)
            {
                if (int.TryParse(tempDigits, out iPatentNumber))
                {
                    retValue = tempString + iPatentNumber.ToString("#,#");
                }
            }
            return retValue;
        }
       
    
        #endregion
    }



    public class UpperCaseFirstPropertyNamesContractResolver : DefaultContractResolver
    {
        //public UpperCaseFirstPropertyNamesContractResolver()
        //    : base(true)
        //{
        //}

        protected override string ResolvePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }
            propertyName = char.ToUpper(propertyName[0]) + propertyName.Substring(1);
            return propertyName;
        }
    }
    public class LowerCaseFirstPropertyNamesContractResolver : DefaultContractResolver
    {
        //public LowerCaseFirstPropertyNamesContractResolver()
        //    : base(true)
        //{
        //}

        protected override string ResolvePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }
            propertyName = char.ToLower(propertyName[0]) + propertyName.Substring(1);
            return propertyName;
        }
    }
}
