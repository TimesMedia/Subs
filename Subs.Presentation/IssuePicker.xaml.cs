using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Subs.Data;

namespace Subs.Presentation
{
       public partial class IssuePicker : Window
    {
        #region Globals
            private readonly CollectionViewSource gIssueView = new CollectionViewSource();
            private int gInitialProductId = 0;
            private int gInitialIssueId = 0;
            private static MIMSDataContext gMimsDataContext = new MIMSDataContext(Settings.ConnectionString);
            private List<Issue> gIssues = new List<Issue>();

        #endregion


        public IssuePicker(int pProductId)
        {
            InitializeComponent();

            try
            {
                gInitialProductId = pProductId;
 
                gIssues = gMimsDataContext.Issues.Where(p => p.ProductId == pProductId && p.Year > (DateTime.Now.Year - 5)).OrderBy(p => p.Sequence).ToList();


                gIssueView.Source = gIssues;
                IssueDataGrid.ItemsSource = gIssueView.View;
            }

            catch (Exception ex)
            {
                //Display all the exceptions

                Exception CurrentException = ex;
                int ExceptionLevel = 0;
                do
                {
                    ExceptionLevel++;
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "IssuePicker constructor", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
               // Check to see if there is a preferred initial issue.

                gIssueView.View.MoveCurrentToFirst();

                bool lIssueFound = false;
                if (gInitialIssueId != 0)
                {
                    do
                    {
                        Issue lIssue = (Issue)gIssueView.View.CurrentItem;
                        if (lIssue.IssueId == gInitialIssueId)
                        {
                            lIssueFound = true;
                            break;
                        }
                    } while (gIssueView.View.MoveCurrentToNext());

                    if (!lIssueFound)
                    {
                        throw new Exception("There does not seem to be an active Issue with ID = " + gInitialIssueId.ToString());
                    }

                    DependencyObject CurrentNode = VisualTreeHelper.GetChild(IssueDataGrid, 0);
                    CurrentNode = VisualTreeHelper.GetChild(CurrentNode, 0);
                    ScrollViewer lScrollViewer = (ScrollViewer)CurrentNode;


                    lScrollViewer = (ScrollViewer)CurrentNode;

                    int lScrollPosition = 0;

                    if (gIssueView.View.CurrentPosition <= (lScrollViewer.ViewportHeight / 2))
                    {
                        lScrollPosition = 0;
                    }
                    else
                    {
                        lScrollPosition = gIssueView.View.CurrentPosition - ((int)Math.Floor(lScrollViewer.ViewportHeight) / 2);
                    }

                    lScrollViewer.ScrollToVerticalOffset(lScrollPosition);
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
                    ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "Window_Loaded", "");
                    CurrentException = CurrentException.InnerException;
                } while (CurrentException != null);

                MessageBox.Show("Error in Window_Loaded " + ex.Message);
            }

        }


        private void IssueDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        public void IssueSelect(int pIssueId)
        {
            gInitialIssueId = pIssueId;

        }

        public bool IssueWasSelected
        {
            get
            {
                if (IssueDataGrid.SelectedItems.Count != 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
        }

        public int IssueId
        {
            get
            {
                try
                {
                    Issue lIssue = (Issue)gIssueView.View.CurrentItem;
                    
                    return (int)lIssue.IssueId;
                }

                catch (Exception ex)
                {
                    //Display all the exceptions

                    Exception CurrentException = ex;
                    int ExceptionLevel = 0;
                    do
                    {
                        ExceptionLevel++;
                        ExceptionData.WriteException(1, ExceptionLevel.ToString() + " " + CurrentException.Message, this.ToString(), "IssueId", "");
                        CurrentException = CurrentException.InnerException;
                    } while (CurrentException != null);

                    MessageBox.Show("Failed on IssueId " + ex.Message);
                    return 0;
                }
            }
        }

        public string IssueName
        {
            get
            {
                Issue lIssue = (Issue)gIssueView.View.CurrentItem;
                return lIssue.IssueDescription;
            }
        }

        public int Sequence
        {
            get
            {
                Issue lIssue = (Issue)gIssueView.View.CurrentItem;
                return lIssue.Sequence;
            }
        }

      
    }
}
