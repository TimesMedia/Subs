using CPD.Data;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace CPD.Business
{
    /// <summary>
    /// Interaction logic for CPDCertificate.xaml
    /// </summary>
    public partial class CPDCertificate : UserControl
    {
        private string gCertificatePrefix = "c:\\Subs\\CPDCertificate";
        private MemoryStream gPdfMemoryStream = new MemoryStream();


        public CPDCertificate()
        {
            InitializeComponent();
        }

        public string Render(int pResultId)
        {
            string lCertificateFile = gCertificatePrefix + pResultId.ToString() + ".pdf";
            Certificate lCertificate = CertificateData.GetCertificate(pResultId);

            try
            {
                // Assign values to the variable parts

                Naam.Content = lCertificate.Customer;
               
                CouncilNumber.Content = lCertificate.CouncilNumber;

                Module.Text = null;
                Module.Text = lCertificate.Module;

                Publication.Content = lCertificate.Publication;
                Date.Content = lCertificate.Datum.ToString("dd MMMM yyyy");
                Accreditation.Content = lCertificate.AccreditationNumber;

                if (lCertificate.NormalPoints > 0)
                {
                    CPDPoints.Content = "CPD Points: " + lCertificate.NormalPoints.ToString();
                }

                if (lCertificate.EthicsPoints > 0)
                {
                    EthicsPoints.Content = "Clinical Points: " + lCertificate.EthicsPoints.ToString();
                }
  
                byte[] lXps = FlowDocumentConverter.XpsConverter.ConverterDoc(this.gFlowDocument);
                MemoryStream lXpsStream = new MemoryStream(lXps);

                if (Directory.Exists("c:\\Subs"))
                {
                    if (File.Exists(lCertificateFile))
                    {
                        File.Delete(lCertificateFile);
                    }

                    FileStream lPdfStream = File.OpenWrite(lCertificateFile);
                    PdfSharp.Xps.XpsConverter.Convert(lXpsStream, lPdfStream, false);

                    lPdfStream.Position = 0;
                    lPdfStream.Flush();
                    lPdfStream.Close();
                }

                PdfSharp.Xps.XpsConverter.Convert(lXpsStream, gPdfMemoryStream, false);
                return "OK";

            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Render", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
        }


        public string EmailCertificate(int pResultId)
        {

            CPD.Data.Certificate lCertificate = CPD.Data.CertificateData.GetCertificate(pResultId);

            try
            {
                string lBody = "Dear " + lCertificate.Customer + "\n\n"
                 + "Congratulations! You have passed the test."
                 + " Your certificate is attached.\n\n"
                 + "Best\n\n"
                 + "Riette van der Merwe\n"
                 + "ARENA Holdings(Pty) Ltd\n"
                 + "MIMS\n"
                 + "Tel: (011) 280 5856\n"
                 + "Fax: (086) 675-7910\n"
                 + "E-mail: vandermerwer@mims.co.za";

                string lResult = CustomerBiz.SendEmail(gCertificatePrefix + pResultId.ToString() + ".pdf",
                                      lCertificate.EMailAddress,
                                      "MIMS CPD Certificate",
                                      lBody);

                if (lResult != "OK")
                { 
                    return lResult;
                }
               
                CertificateData.IssueOfCertificate(pResultId, DateTime.Now);

                return "OK";
            }
            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "EmailCertificate", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return ex.Message;
            }
        }




        //public string EmailCertificate(int pResultId)
        //{

        //    //string lCertificateFile = gCertificatePrefix + pResultId.ToString() + ".pdf";
        //    CPD.Data.CertificateDoc.CertificateDataTable lCertificateTable = CPD.Data.CertificateData.GetCertificate(pResultId);

        //    try
        //    {
        //        SmtpClient smtpClient = new SmtpClient("172.15.83.191", 25);
        //        smtpClient.Credentials = new System.Net.NetworkCredential("vandermerwer@mims.co.za", "");
        //        smtpClient.EnableSsl = true;

        //        smtpClient.Timeout = 30000;
        //        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        //        MailMessage mail = new MailMessage();
        //        mail.From = new MailAddress("vandermerwer@mims.co.za", "Mims");

        //        mail.To.Add(new MailAddress(lCertificateTable[0].EMailAddress));
        //        //mail.To.Add(new MailAddress("heinreitmann@gmail.com"));


        //        mail.Subject = "MIMS CPD Certificate";
        //        Attachment lAttachment = new Attachment(gPdfMemoryStream, "CPDCertificate.pdf", System.Net.Mime.MediaTypeNames.Application.Pdf);
        //        mail.Attachments.Add(lAttachment);
        //        string myBody = "Dear " + lCertificateTable[0].Customer + "\n\n"
        //         + "Congratulations! You have passed the test."
        //         + " Your certificate is attached.\n\n"
        //         + "Best\n\n"
        //         + "Riette van der Merwe\n"
        //         + "ARENA Holdings(Pty) Ltd\n"
        //         + "MIMS\n"
        //         + "Tel: (011) 280 5856\n"
        //         + "Fax: (086) 675-7910\n"
        //         + "E-mail: vandermerwer@mims.co.za";
        //        mail.Body = myBody;
        //        smtpClient.Send(mail);

        //        CertificateData.RecordEmailSuccess(pResultId);

        //        return "OK";
        //    }
        //    catch (Exception ex)
        //    {
        //        //Display all the exceptions

        //        Exception CurrentException = ex;
        //        int ExceptionLevel = 0;
        //        do
        //        {
        //            ExceptionLevel++;
        //            ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "EmailCertificate", "");
        //            CurrentException = CurrentException.InnerException;
        //        } while (CurrentException != null);

        //        return ex.Message;
        //    }
        //}
    }
}
