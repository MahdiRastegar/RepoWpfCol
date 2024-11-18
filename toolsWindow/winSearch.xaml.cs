
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using WpfCol.Interfaces;

namespace WpfCol.Windows.toolWindows
{
    public class Mu
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public object AdditionalEntity { get; set; }
        public string Name2 { get; set; }
    }
    /// <summary>
    /// Interaction logic for winSearch.xaml
    /// </summary>
    public partial class winSearch : Window
    {
        public dynamic ParentTextBox;
        public winSearch(List<Mu> mus)
        {
            InitializeComponent();
            this.FontFamily = MainWindow.Current.FontFamily;
            SearchTermTextBox.Focus();
            Mus = new ObservableCollection<Mu>();
            foreach (var item in mus)
            {
                Mus.Add(item);
            }
            datagrid.ItemsSource = Mus;
        }
        public ObservableCollection<Mu> Mus { set; get; }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectionController.CurrentCellManager.CurrentCell != null)
            {
                MuText = ((datagrid.SelectionController.CurrentCellManager.CurrentCell?.Element as GridCell)?.DataContext as Mu);
                Close();
            }
        }        
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void datagrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(datagrid.SelectionController.CurrentCellManager.CurrentCell!=null)
            {
                MuText = ((datagrid.SelectionController.CurrentCellManager.CurrentCell?.Element as GridCell)?.DataContext as Mu);
                if (!(ParentTextBox is TextBox))
                {
                    var db = new ColDbEntities1();
                    if (MuText.AdditionalEntity is AccountSearchClass accountSearchClass)
                    {
                        ParentTextBox.Moein = db.Moein.Find(accountSearchClass.Id);
                    }
                    else
                        ParentTextBox.Preferential = db.Preferential.Find(MuText.Id);
                    //ParentTextBox.Text = $"{MuText.Name}-{(MuText.AdditionalEntity as AccountSearchClass).Tafzili}";
                    //ParentTextBox.AccountName = $"{MuText.Value}-{(MuText.AdditionalEntity as AccountSearchClass).T_Name}";
                    //ParentTextBox.AccountName = $"{(MuText.AdditionalEntity as AccountSearchClass).T_Name}-{MuText.Value.Split('-')[1]}";
                }
                else if (datagrid.Columns.Count == 1)
                    ParentTextBox.Text = MuText.Name;
                else if (ParentTextBox.Tag != null)
                {
                    if (MuText.AdditionalEntity != null)
                        ParentTextBox.Text = (MuText.AdditionalEntity as ISearch).Result;
                    else
                        ParentTextBox.Text = MuText.Value;
                    ParentTextBox.Tag = MuText;
                }
                else
                {
                    ParentTextBox.Text = MuText.Value;
                }
                Close();
            }
        }
        public Mu MuText {  get; set; }
        bool IsLoaded = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SearchTermTextBox.Text = "";
            IsLoaded = true;
            datagrid.SearchHelper.AllowFiltering = true;
            datagrid.SearchHelper.SearchBrush = Brushes.LightBlue;
            if (datagrid.Columns[0].HeaderText=="کل")
            {
                datagrid.Columns[0].Width = 80;
                datagrid.Columns[1].Width = 200;
                datagrid.Columns[2].Width = 80;
                datagrid.ItemsSource = new ObservableCollection<Mu>((datagrid.ItemsSource as ObservableCollection<Mu>).OrderBy(y => y.Value).ThenBy(y => (y.AdditionalEntity as AccountSearchClass).Moein).ToList());
            }
            else if (datagrid.Columns[0].HeaderText == "نام")
            {
                datagrid.ItemsSource = new ObservableCollection<Mu>((datagrid.ItemsSource as ObservableCollection<Mu>).OrderBy(y => y.Name2).ThenBy(y => y.Value).ToList());
            }
            /*Dispatcher.BeginInvoke(new Action(() =>
            {
                SearchTermTextBox.Focus();
            }));                */
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() == "")
                datagrid.SearchHelper.ClearSearch();
            else
                datagrid.SearchHelper.Search(SearchTermTextBox.Text);
            if (!(ParentTextBox is TextBox))
            {
                if (MuText != null)
                {
                    var db = new ColDbEntities1();
                    if (MuText.AdditionalEntity is AccountSearchClass accountSearchClass)
                    {
                        ParentTextBox.Moein = db.Moein.Find(accountSearchClass.Id);
                    }
                    else
                        ParentTextBox.Preferential = db.Preferential.Find(MuText.Id);
                    //ParentTextBox.Text = $"{MuText.Name}-{(MuText.AdditionalEntity as AccountSearchClass).Tafzili}";
                    //ParentTextBox.AccountName = $"{MuText.Value}-{(MuText.AdditionalEntity as AccountSearchClass).T_Name}";
                    //ParentTextBox.AccountName = $"{(MuText.AdditionalEntity as AccountSearchClass).T_Name}-{MuText.Value.Split('-')[1]}";
                }
            }
            else
                ParentTextBox.Text = SearchTermTextBox.Text;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                return;
            }
            if (e.Key== Key.Enter&&(datagrid.SelectedIndex!=-1||(SearchTermTextBox.Text.Trim()!=""&&datagrid.View.Records.Count==1)))
            {
                if (datagrid.SelectedItem != null)
                {
                    MuText = datagrid.SelectedItem as Mu;
                    if (!(ParentTextBox is TextBox))
                    {
                        var db = new ColDbEntities1();
                        if (MuText.AdditionalEntity is AccountSearchClass accountSearchClass)
                        {
                            ParentTextBox.Moein = db.Moein.Find(accountSearchClass.Id);
                        }
                        else
                            ParentTextBox.Preferential = db.Preferential.Find(MuText.Id);
                        //ParentTextBox.Text = $"{MuText.Name}-{(MuText.AdditionalEntity as AccountSearchClass).Tafzili}";
                        //ParentTextBox.AccountName = $"{MuText.Value}-{(MuText.AdditionalEntity as AccountSearchClass).T_Name}";
                        //ParentTextBox.AccountName = $"{(MuText.AdditionalEntity as AccountSearchClass).T_Name}-{MuText.Value.Split('-')[1]}";
                    }
                    else if (datagrid.Columns.Count == 1)
                        ParentTextBox.Text = MuText.Name;
                    else if (ParentTextBox.Tag != null)
                    {
                        if (MuText.AdditionalEntity != null)
                            ParentTextBox.Text = (MuText.AdditionalEntity as ISearch).Result;
                        else
                            ParentTextBox.Text = MuText.Value;
                        ParentTextBox.Tag = MuText;
                    }
                    else
                    {
                        ParentTextBox.Text = MuText.Value;
                    }
                    Close();
                }
                else
                {
                    datagrid.SelectedIndex = 0;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        MuText = datagrid.SelectedItem as Mu;
                        if (!(ParentTextBox is TextBox))
                        {
                            ParentTextBox.Text = $"{MuText.Name}-{MuText.Value}";
                        }
                        else if (datagrid.Columns.Count == 1)
                            ParentTextBox.Text = MuText.Name;
                        else if (ParentTextBox.Tag != null)
                        {
                            if (MuText.AdditionalEntity != null)
                                ParentTextBox.Text = (MuText.AdditionalEntity as ISearch).Result;
                            else
                                ParentTextBox.Text = MuText.Value;
                            ParentTextBox.Tag = MuText;
                        }
                        else
                        {
                            ParentTextBox.Text = MuText.Value;
                        }
                        Close();
                    }));
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
        MainWindow.Current.Activate()));
            (Tag as ITabForm).SetNull();
            //    Dispatcher.BeginInvoke(new Action(() =>
            //    (Tag as ITabForm).SetNull()));
        }
        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //Close();
            }
            catch { }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (IsLoaded)
                try
                {
                    Close();
                }
                catch { }
        }

        private void ClearSearch_MouseLeave(object sender, MouseEventArgs e)
        {
            ClearSearch.Opacity = .65;
        }

        private void ClearSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchTermTextBox.Clear();
        }

        private void ClearSearch_MouseEnter(object sender, MouseEventArgs e)
        {
            ClearSearch.Opacity = 1;
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
