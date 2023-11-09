using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CPD.Data;

namespace CPD.Business
{
    public class MailRequesttype
    {
        public OrderedDictionary headers { get; set; }
        public OrderedDictionary body { get; set; }
        public OrderedDictionary options { get; set; }
        public OrderedDictionary[] attachments { get; set; }
    }


    static class CustomerBiz
    {
        public static string SendEmail(string pFileName, string pDestination, string pSubject, string pBody)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var lClient = new HttpClient();

            try
            {
                MailRequesttype lRequest = new MailRequesttype();
                lRequest.headers = new OrderedDictionary();
                lRequest.body = new OrderedDictionary();
                lRequest.options = new OrderedDictionary();

                lRequest.headers.Add("from", "vandermerwer@mims.co.za");
                lRequest.headers.Add("to", pDestination);

                lRequest.headers.Add("subject", pSubject);
                lRequest.headers.Add("reply_to", "vandermerwer@mims.co.za");

                lRequest.body.Add("text", pBody);

                if (!String.IsNullOrEmpty(pFileName))
                {
                    FileStream lFileStream = File.OpenRead(pFileName);
                    byte[] lBytes = (byte[])Array.CreateInstance(typeof(byte), (int)lFileStream.Length);
                    lFileStream.Read(lBytes, 0, (int)lFileStream.Length);
                    lFileStream.Close();

                    var attachments = new OrderedDictionary();
                    attachments.Add("filename", pFileName);
                    Byte[] bytes = File.ReadAllBytes(pFileName);
                    String file = Convert.ToBase64String(bytes);
                    attachments.Add("data", file);

                    lRequest.attachments = new OrderedDictionary[1];
                    lRequest.attachments[0] = attachments;
                }

                // Setting content type.                   
                lClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var byteArray = Encoding.ASCII.GetBytes("vanderMerweR@mims.co.za:VTzyC0BG7PXZLB2mROmdQ1y27jTM9cdM_8");

                // Setting Authorization.  
                lClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));


                // Process HTTP GET/POST REST Web API  

                string jsonString = JsonSerializer.Serialize(lRequest);
                // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                string UriString = "https://arena.everlytic.net/api/2.0/trans_mails";
                var lResponse = lClient.PostAsync(UriString, content);

                if (lResponse.Result.ReasonPhrase == "Created")
                {
                    return "OK";
                }
                else
                {
                    ExceptionData.WriteException(1, "1" + " " + lResponse.Result.ReasonPhrase, "CustomerBiz static", "SendEmail", pDestination);
                    return lResponse.Result.ReasonPhrase;
                }
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, "CustomerBiz static", "SendEmail", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
        }
    }
}
