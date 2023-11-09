using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.IO;
using Microsoft.Reporting.WinForms;
using FlowDocumentConverter;
using CPD.Data;

namespace CPD.Business
{
    public static class ResultBiz
    {
       
        public static int InitialiseTest(int CustomerId, int ModuleId, out string ErrorMessage)
        {
            ErrorMessage = "OK";

            if (ResultData.BuzyWithTest(CustomerId))
            {
                ErrorMessage = "You cannot initialise a test if you have not completed the existing one.";
                return 0;
            }


            if (ResultData.GetAttempt(CustomerId, ModuleId) > 1)
            {
                ErrorMessage = "You cannot take this test more than two times.";
                return 0;
            }

            return ResultData.Initialise(CustomerId, ModuleId);
        }
    }
}
