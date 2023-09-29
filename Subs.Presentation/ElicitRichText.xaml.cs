using Subs.Data;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

namespace Subs.Presentation
{
    /// <summary>
    /// Interaction logic for ElicitString.xaml
    /// </summary>
    /// 
    public class TupleOfIntegers
    { 
          public int Integer1 { get; set; }
          public int Integer2 { get; set; }
          public int Integer3 { get; set; }
    }



    public partial class ElicitRichText : Window
    {
        private string gStringAnswer = "";

        public ElicitRichText(string pQuestion)
        {
            InitializeComponent();
            gQuestion.Content = pQuestion;
            gAnswer.Focus();
            this.Title = "Request for information";

        }

        public string Answer
        {
            get
            {
                if (string.IsNullOrWhiteSpace(gStringAnswer))
                {
                    return "";
                }
                else
                {
                    return (string)gStringAnswer;
                }
            }
        }

        public List<int> ListOfIntegers
        {
            get
            {
                List<int> lIntegers = new List<int>();
               
                if (!string.IsNullOrWhiteSpace(gStringAnswer))
                {
                    char[] lSeperator = { '\r', '\n' };
                    string[] lSubscriptions = Answer.Split(lSeperator, System.StringSplitOptions.RemoveEmptyEntries);
                   
                    foreach (string lItem in lSubscriptions)
                    {
                        lIntegers.Add(Int32.Parse(lItem));
                    }
                }
                return lIntegers;
            }
        }

        public List<TupleOfIntegers> ListOfTupleOfIntegers
        {
            get
            {
                string lCurrentString = "";
                try
                { 
                    List<TupleOfIntegers> lTuple = new List<TupleOfIntegers>();

                    if (!string.IsNullOrWhiteSpace(gStringAnswer))
                    {
                        char[] lSeperator = { '\r', '\n' };
                        string[] lPairString = Answer.Split(lSeperator, System.StringSplitOptions.RemoveEmptyEntries);


                        char[] lSeperator2 = { ' ' };

                        foreach (string lItem in lPairString)
                        {
                            lCurrentString = lItem;
                            TupleOfIntegers lTupleOfIntegers = new TupleOfIntegers();

                            string[] lTupleOfStrings = lItem.Split(lSeperator2, System.StringSplitOptions.RemoveEmptyEntries);
                      
                            lTupleOfIntegers.Integer1 = Int32.Parse(lTupleOfStrings[0]);
                            lTupleOfIntegers.Integer2 = Int32.Parse(lTupleOfStrings[1]);
                            lTupleOfIntegers.Integer3 = Int32.Parse(lTupleOfStrings[2]);

                            lTuple.Add(lTupleOfIntegers);
                        }
                    }
                    return lTuple;
                }

                catch (Exception ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ListOfPairOfIntegers", lCurrentString);
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    throw ex;;
                }
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = "Request for information";
        }

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            gAnswer.SelectAll();
            TextSelection lSelection = gAnswer.Selection;
            gStringAnswer = lSelection.Text;
            this.Close();
            return;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            gStringAnswer = "";
            this.Close();
            return;
        }
    }
}
