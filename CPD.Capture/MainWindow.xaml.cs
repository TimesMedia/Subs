using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Data;
using CPD.Data;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Linq;

namespace CPD.Capture
{
      public partial class MainWindow : Window
      {
            public ModuleDataContext gModuleContext;
            CollectionViewSource gSurveyViewSource = new CollectionViewSource();
            string gConnectionString = string.Empty;

            CollectionViewSource gModuleViewSource;

            List<string> gFacility = new List<string>() { "CPD", "Read only" };

            public MainWindow()
            {
            InitializeComponent();

            Settings.MIMSConnectionString = global::CPD.Capture.Properties.Settings.Default.MIMSConnectionString;

            Settings.CPDConnectionString = global::CPD.Capture.Properties.Settings.Default.CPDConnectionString;

            gModuleContext = new ModuleDataContext(Settings.CPDConnectionString);
            gModuleContext.Log = Console.Out;

            gSurveyViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("SurveyViewSource")));
            gModuleViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("ModuleViewSource")));

            string lStage = "Before try";


            try
            {
                // Set the Status-strip
                string[] myStatusMessages;
                char[] charSeparators = new char[] { ';' };
                myStatusMessages = Settings.CPDConnectionString.Split(charSeparators, 10, StringSplitOptions.RemoveEmptyEntries);
                string myServer = "";
                string myDataBase = "";
                //string myVersion = "";

                lStage = "Before foreach";

                foreach (string myMember in myStatusMessages)
                {
                    if (myMember.StartsWith("Data Source"))
                    {
                        myServer = myMember.Substring(12);
                    }

                    if (myMember.StartsWith("Initial Catalog"))
                    {
                        myDataBase = myMember.Substring(16);
                    }
                }

                lStage = "Before Currentversion";
                //myVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();

                lStage = "Before Title";

                this.Title = "CPD question capture facility " + myServer + " on database " + myDataBase + " Version "; // + myVersion;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "MainWindow", lStage);
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

            }

            }

            private void Window_Loaded(object sender, RoutedEventArgs e)
            {
            try { 
            gSurveyViewSource = (CollectionViewSource)this.FindResource("SurveyViewSource");
            StringWriter lStringWriter = new StringWriter();
            gModuleContext.Log = lStringWriter;
            
            gSurveyViewSource.Source = gModuleContext.Survey2s.ToList();

            MessageBox.Show(lStringWriter.ToString());
            }       

            catch (Exception ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Window_Loaded", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    MessageBox.Show("Window_Loaded; " + ex.Message);
                }

            }

     
            private void ButtonExit_Click(object sender, RoutedEventArgs e)
            {
                this.Close();
            }

            private void ButtonAddSurvey_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    //DataSet1.Survey2Row lNewRow = gDataSet1.Survey2.NewSurvey2Row();
                    //lNewRow.Naam = "ZZZ New Survey";
                    //lNewRow.IssueId = 0;
                    //gDataSet1.Survey2.AddSurvey2Row(lNewRow);
                    MessageBox.Show("OK a row has been added with the name ZZZ New Survey . Look at the bottom of the listing on the left.");
                }

                catch (Exception ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonAddSurvey_Click", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    MessageBox.Show("Error in ButtonAddSurvey_Click: " + ex.Message);
                }

            }
            private void ButtonUpdateSurvey_Click(object sender, RoutedEventArgs e)
            {
            try
            {
                System.Data.Linq.ITable lTable = (System.Data.Linq.ITable)gModuleContext.GetTable(typeof(Module));

                //System.Data.Linq.ModifiedMemberInfo[] lModified =  gModuleContext.Modules.GetModifiedMembers();
              

                Survey2 lCurrentSurvey = (Survey2)gSurveyViewSource.View.CurrentItem;

                if (!Consolidate(lCurrentSurvey.SurveyId))
                {
                    MessageBox.Show("There was a problem consolidating the new answers");
                    return;
                }

                gModuleContext.SubmitChanges();

                MessageBox.Show(lCurrentSurvey.Naam + "Successfully updated.");
            }
           
            catch (Exception ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "ButtonUpdateSurvey_Click", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    MessageBox.Show("Update of Survey failed: " + ex.Message);
                }
            }

            private void SurveyDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                try
                {
                    Survey2 lCurrentSurvey = (Survey2)gSurveyViewSource.View.CurrentItem;

                   
                    TextRange lTextRange;
                    System.IO.FileStream lFileStream;

                    if (!System.IO.File.Exists(lCurrentSurvey.RTFFileName))
                    {
                        return;
                    }

                    lTextRange = new TextRange(gRichTextBox.Document.ContentStart, gRichTextBox.Document.ContentEnd);
                    using (lFileStream = new System.IO.FileStream(lCurrentSurvey.RTFFileName, System.IO.FileMode.OpenOrCreate))
                    {
                        lTextRange.Load(lFileStream, System.Windows.DataFormats.Rtf);
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
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "SurveyDataGrid_SelectionChanged", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    MessageBox.Show("Error in SurveyDataGrid_SelectionChanged: " + ex.Message);
                }

            }
       

            private bool Consolidate(int pSurveyId)
            {
                this.Cursor = Cursors.Wait;

                try
                {
                    IEnumerable<int> lModuleIds = (IEnumerable<int>)gModuleContext.Modules.Where(a => a.SurveyId == pSurveyId).Select(b => b.ModuleId).ToList();
                    IEnumerable<int> lArticleIds = (IEnumerable<int>)gModuleContext.Articles.Where(a => lModuleIds.Contains(a.ModuleId)).Select(b => b.ArticleId).ToList();
                    IEnumerable<Question> lQuestions = (IEnumerable<Question>)gModuleContext.Questions.Where(a => lArticleIds.Contains(a.ArticleId)).ToList();

                    foreach (Question lQuestion in lQuestions)
                    {
                        IEnumerable<Answer> lHits = (IEnumerable<Answer>)gModuleContext.Answers.Where(a => a.QuestionId == lQuestion.QuestionId);

                        if (lHits.Count() == 0)
                        {
                            // No answer was changed
                            continue;
                        }

                        // Encode the answer
                        int lEncodedAnswer = 0;
                        int i = 0;
                        IEnumerable<Answer> lAnswers = (IEnumerable<Answer>)gModuleContext.Answers.Where(a => a.QuestionId == lQuestion.QuestionId);

                        foreach (Answer lAnswer in lAnswers)
                        {
                            if (lAnswer.Correct)
                            {
                                lEncodedAnswer += (int)Math.Pow(2, i);
                            }
                            i++;
                        }

                        lQuestion.CorrectAnswer = lEncodedAnswer;
                    }
                    return true;
                }

                catch (Exception ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Consolidate", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    return false;
                }
                finally
                {
                    this.Cursor = Cursors.Arrow;
                }
            }

        private void ButtonTest_Click(object sender, RoutedEventArgs e)
        {
            try {

                List<AvailableModule> lModules = ModuleData.GetAvailableModules(2);
                MessageBox.Show(lModules.Count.ToString());



                List<AvailableSurvey> lSurveys = ModuleData.GetAvailableTest(120072);
            MessageBox.Show(lSurveys.Count.ToString()); 
            }
            catch(Exception ex)
            {
            MessageBox.Show(ex.Message);
            }
        }
    }
}
