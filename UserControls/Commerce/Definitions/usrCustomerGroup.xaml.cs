using Syncfusion.XlsIO.Parser.Biff_Records;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using WpfCol.Windows.toolWindows;

namespace WpfCol
{
    /// <summary>
    /// Interaction logic for winCol.xaml
    /// </summary>
    public partial class usrCustomerGroup : UserControl,ITabForm
    {
        public usrCustomerGroup()
        {
            CustomerGroups = new ObservableCollection<CustomerGroup>();
            InitializeComponent();
            isCancel = true;
        }
        Brush brush = null;
        public ObservableCollection<CustomerGroup> CustomerGroups { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                if ((sender as TextBox).Name == "txtCustomerGroupName")
                {
                    btnConfirm.Focus();
                }
                else
                {
                    TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                    request.Wrapped = true;
                    (sender as TextBox).MoveFocus(request);
                }
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (btnConfirm.IsFocused)
                    {
                        btnConfirm_Click(null, null);
                    }
                }));
                return;
            }            
            if ((sender as TextBox).Name != "txtCustomerGroupName"&& (sender as TextBox).Name != "txtWebSite" && (sender as TextBox).Name != "txtEmail" && (sender as TextBox).Name != "txtAddress" && (sender as TextBox).Name != "txtDescription")
                e.Handled = !IsTextAllowed(e.Text);            
        }
        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CustomerGroups = new ObservableCollection<CustomerGroup>();
            var db = new ColDbEntities1();
            var count = db.CustomerGroup.Count();
            var h = db.CustomerGroup.AsNoTracking().ToList();
            //var h = db.CustomerGroup.Take(10).ToList();
            //if(count>10)
            //{
            //    for (int i = 0; i < count-10; i++)
            //    {
            //        h.Add(null);
            //    }
            //}
            h.ForEach(u => CustomerGroups.Add(u));
            datagrid.SearchHelper.AllowFiltering = true;
            txtGroup.Focus();
            dataPager.Source = null;
            dataPager.Source = CustomerGroups;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = false;
            haserror = GetError();

            if (haserror)
                return;
            var db = new ColDbEntities1();
            var c = int.Parse(txtGroup.Text);
            var col = db.PriceGroup.FirstOrDefault(g => g.GroupCode == c);
            if (col == null)
            {
                Sf_txtGroup.ErrorText = "این کد گروه وجود ندارد";
                Sf_txtGroup.HasError = true;
                return;
            }
            var i = int.Parse(txtCodeCustomerGroup.Text);
            var CustomerGroup = db.CustomerGroup.Find(id);
            //var mCustomerGroup = db.CustomerGroup.FirstOrDefault(g => g.fk_GroupId == col.Id && g.CustomerGroupCode == i);
            //if (CustomerGroup?.Id != mCustomerGroup?.Id && mCustomerGroup != null)
            //{
            //    Xceed.Wpf.Toolkit.MessageBox.Show("این کد گروه مشتریان و کد گروه از قبل وجود داشته است!");
            //    return;
            //}    
            var nCustomerGroup = db.CustomerGroup.FirstOrDefault(g => g.fk_GroupId == col.Id && g.CustomerGroupName == txtCustomerGroupName.Text);
            if (CustomerGroup?.Id != nCustomerGroup?.Id && nCustomerGroup != null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("این نام گروه مشتریان و کد گروه از قبل وجود داشته است!");
                return;
            }

            CustomerGroup e_add = null;
            if (id == Guid.Empty)
            {
                datagrid.SortColumnDescriptions.Clear();
                e_add = new CustomerGroup()
                {
                    Id = Guid.NewGuid(),
                    CustomerGroupCode = i,
                    fk_GroupId = col.Id,
                    CustomerGroupName = txtCustomerGroupName.Text  
                };                
                db.CustomerGroup.Add(e_add);
                CustomerGroups.Add(e_add);
            }
            else
            {                
                var e_Edidet = CustomerGroups.FirstOrDefault(a => a.Id == id);
                e_Edidet.fk_GroupId = CustomerGroup.fk_GroupId = col.Id;
                e_Edidet.CustomerGroupCode = CustomerGroup.CustomerGroupCode = i;
                e_Edidet.CustomerGroupName = CustomerGroup.CustomerGroupName = txtCustomerGroupName.Text;
                e_Edidet.PriceGroup.GroupName = txtGroupName.Text;
            }
            if (!db.SafeSaveChanges())  return;
            if (id == Guid.Empty)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت گروه مشتریان");
                txtCodeCustomerGroup.Text = (e_add.CustomerGroupCode + 1).ToString();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش گروه مشتریان");
                btnCancel_Click(null, null);
            }
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";            
            txtCodeCustomerGroup.IsReadOnly = false;
            txtGroup.IsReadOnly = false;
            txtCustomerGroupName.Text = "";
            isCancel = true;            
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            if (id != Guid.Empty)
                txtGroup.Focus();
            else
            {
                txtCustomerGroupName.Focus();
                dataPager.MoveToLastPage();
            }

            id = Guid.Empty;
        }
        Guid id = Guid.Empty;
        private bool GetError()
        {
            var haserror = false;
            if (txtCodeCustomerGroup.Text.Trim() == "")
            {
                Sf_txtCodeCustomerGroup.HasError = true;
                haserror = true;
            }
            else
                Sf_txtCodeCustomerGroup.HasError = false;
            if (txtCustomerGroupName.Text.Trim() == "")
            {
                Sf_txtCustomerGroupName.HasError = true;
                haserror = true;
            }
            else
                Sf_txtCustomerGroupName.HasError = false;
            if (txtGroup.Text.Trim() == "")
            {
                Sf_txtGroup.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtGroup.HasError = false;
                Sf_txtGroup.ErrorText = "";
            }
            return haserror;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این فرم خارج شوید؟", "خروج", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void btnExcelPattern_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ExcelPattern", "Commodity.xlsx"));
        }

        private void txtProductID_KeyDown(object sender, KeyEventArgs e)
        {
            /*if (e.Key == Key.F1)
            {
                Border_MouseDown("id",null);
            }*/
        }

        private void txtMu_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
//                Border_MouseDown("mu", null);
            }
        }

        private void datagrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {


        }

        private void datagrid_CurrentCellEndEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs e)
        {

        }

        private void btnTransferOfExcel_Click(object sender, RoutedEventArgs e)
        {

        }
        bool forceClose = false;
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseForm();
            }
            else if (e.Key == Key.F1 && txtGroup.IsFocused && !txtGroup.IsReadOnly)
            {
                if (window != null)
                    return;
                /*Point relativePoint = y.TransformToAncestor(this)
                          .Transform(new Point(this.Left+Width, this.Top-Height));*/
                isCancel = false;
                Point relativePoint = new Point(MainWindow.Current.Left + Width - 500, MainWindow.Current.Top + 50);
                if (MainWindow.Current.WindowState == WindowState.Maximized)
                    relativePoint = txtGroup.TransformToAncestor(this)
                          .Transform(new Point(530, 0));
                var db = new ColDbEntities1();
                var list = db.PriceGroup.ToList().Select(r => new Mu() { Name = r.GroupName, Value = r.GroupCode.ToString() }).ToList();
                var win = new winSearch(list);
                win.Tag = this;
                win.ParentTextBox = txtGroup;
                win.SearchTermTextBox.Text = txtGroup.Text;
                win.SearchTermTextBox.Select(1, 0);
                win.Owner = MainWindow.Current;
                //win.Left = relativePoint.X - 60;
                //win.Top = relativePoint.Y + 95;
                window = win;
                win.Show(); win.Focus();
            }
        }

        private void cmbType_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key ==  Key.Enter)
            {                
                btnConfirm.Focus();
                return;
            }
        }
        private bool _iscancel=false;

        public bool isCancel
        {
            get 
            {
                return _iscancel; 
            }
            set
            {
                _iscancel = value;

                gridContainer.Opacity = .6;
                gridContainer.IsEnabled = false;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (isCancel&& sender != null&&id== Guid.Empty)
            {
                gridContainer.Opacity = 1;
                gridContainer.IsEnabled = true;
                return;
            }
            if (!isCancel && sender != null && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            txtCodeCustomerGroup.IsReadOnly = false;
            txtGroup.IsReadOnly = false;
            txtCustomerGroupName.Text = "";
            Sf_txtCustomerGroupName.HasError = false;
            Sf_txtCodeCustomerGroup.HasError = false;
            Sf_txtGroup.HasError = false;
            Sf_txtGroup.ErrorText = "";
            //txtCodeCustomerGroup.Text = (en.CustomerGroupCode + 1).ToString();

            txtGroup.Focus();
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            txtGroup.TextChanged -= TxtGroup_TextChanged;
            txtGroupName.Text = txtGroup.Text = txtCodeCustomerGroup.Text = "";
            txtGroup.TextChanged += TxtGroup_TextChanged;
            isCancel = true;
            if (sender != null)
            {
                if (id == Guid.Empty)
                {
                    gridContainer.Opacity = 1;
                    gridContainer.IsEnabled = true;
                }
                id = Guid.Empty;
            }
        }

        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (isCancel&&datagrid.SelectedItem!=null) 
            {
                var CustomerGroup = datagrid.SelectedItem as CustomerGroup;
                id = CustomerGroup.Id;
                txtGroup.TextChanged -= TxtGroup_TextChanged;
                txtGroup.Text = CustomerGroup.PriceGroup.GroupCode.ToString();
                txtGroup.TextChanged += TxtGroup_TextChanged;
                txtGroupName.Text = CustomerGroup.PriceGroup.GroupName;
                txtCustomerGroupName.Text = CustomerGroup.CustomerGroupName;
                txtCodeCustomerGroup.Text = CustomerGroup.CustomerGroupCode.ToString();

                gridDelete.Visibility = Visibility.Visible;
                borderEdit.Visibility = Visibility.Visible;
                txtCodeCustomerGroup.IsReadOnly = true;
                txtGroup.IsReadOnly = true;
                isCancel = true;
                GetError();
                gridContainer.Opacity = 1;
                gridContainer.IsEnabled = true;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid.SelectedItem == null)
                return;
            if (Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید این اطلاعات پاک شود؟", "حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            var db = new ColDbEntities1();
            var CustomerGroup = db.CustomerGroup.Find(id);
            //if (db.AcDocument_Detail.Any(y => y.fk_CustomerGroupId == CustomerGroup.Id))
            //{
            //    Xceed.Wpf.Toolkit.MessageBox.Show("قبلا با این گروه مشتریان سند حسابداری زده شده است و قابل حذف نیست!");
            //    return;
            //}            
            db.CustomerGroup.Remove(CustomerGroup);
            if (!db.SafeSaveChanges())  return;
            id = Guid.Empty;
            CustomerGroups.Remove((datagrid.SelectedItem as CustomerGroup));            
            btnCancel_Click(null, null);
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (SearchTermTextBox.Text.Trim() == "")
                    datagrid.SearchHelper.ClearSearch();
                else
                    datagrid.SearchHelper.Search(SearchTermTextBox.Text);
            }
            catch(Exception ex)
            {
            }
        }

        private void txtCustomerGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtCodeCustomerGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }
        public static Window window;
        private void TxtGroup_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtGroup_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!isCancel)
                try
                {
                    var db = new ColDbEntities1();
                    var g = int.Parse(txtGroup.Text);
                    var group = db.PriceGroup.FirstOrDefault(gs => gs.GroupCode == g);
                    txtGroupName.Text = group.GroupName;
                    try
                    {
                        txtCodeCustomerGroup.Text = (db.CustomerGroup.Where(u => u.PriceGroup.GroupCode == g).Max(y => y.CustomerGroupCode) + 1).ToString();
                    }
                    catch
                    {
                        txtCodeCustomerGroup.Text = ((group.GroupCode * 10000) + 1).ToString();
                    }
                }
                catch
                {
                    txtCodeCustomerGroup.Text = "1";
                    txtGroupName.Text = "";
                }
        }

        private void DataPager_PageIndexChanging(object sender, Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangingEventArgs e)
        {
            var ex = datagrid.View.FilterPredicates;
            
            var db = new ColDbEntities1();
            //db.CustomerGroup.Where(ex)
            var count = db.CustomerGroup.Count();
            var F = db.CustomerGroup.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                CustomerGroups[i] = F[j];
                j++;
            }
        }
        public bool CloseForm()
        {
            if (Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این فرم خارج شوید؟", "خروج", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return false;
            }
            forceClose = true;
            var list = MainWindow.Current.GetTabControlItems;
            var item = list.FirstOrDefault(u => u.Header == "گروه مشتریان");
            MainWindow.Current.tabcontrol.Items.Remove(item);
            return true;
        }

        private void ClearSearch_MouseEnter(object sender, MouseEventArgs e)
        {
            ClearSearch.Opacity = 1;
        }

        private void ClearSearch_MouseLeave(object sender, MouseEventArgs e)
        {
            ClearSearch.Opacity = .65;
        }

        private void ClearSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchTermTextBox.Clear();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            datagrid.AllowFiltering = !datagrid.AllowFiltering;
            if (!datagrid.AllowFiltering)
                datagrid.ClearFilters();
        }

        public void SetNull()
        {
            window = null;

            try
            {
                var db = new ColDbEntities1();
                var g = int.Parse(txtGroup.Text);

                var y = db.PriceGroup.FirstOrDefault(gs => gs.GroupCode == g);
                if (y != null)
                    Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(50);
                        txtCustomerGroupName.Focus();
                    }));
            }
            catch { }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {/*
            var db = new ColDbEntities1();    
            foreach (var item in db.CustomerGroup.ToList())
            {
                foreach (var item2 in db.MoeinGroup.Where(t => t.fk_GroupId == item.fk_GroupId))
                {
                    db.Account.Add(new Account()
                    {
                        Id = Guid.NewGuid(),
                        fk_ColId = item2.Moein.fk_ColId,
                        fk_CustomerGroupId = item.Id,
                        fk_MoeinId = item2.fk_MoeinId
                    });
                }
            }
            if (!db.SafeSaveChanges())  return;*/
        }

        private void txtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
        }

        private void txtWebSite_GotFocus(object sender, RoutedEventArgs e)
        {
            InputLanguageManager.Current.CurrentInputLanguage = new CultureInfo("en-US");
        }

        private void datagrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // ارتفاع سطرهای grid را محاسبه کنید (می‌توانید ارتفاع سطر ثابت فرض کنید)
            double rowHeight = 30; // ارتفاع هر سطر (این مقدار ممکن است بسته به طراحی تغییر کند)

            // ارتفاع موجود در grid را محاسبه کنید
            double availableHeight = datagrid.ActualHeight;

            // محاسبه تعداد سطرهایی که در صفحه جا می‌شوند
            int visibleRows = (int)(availableHeight / rowHeight);

            // تنظیم PageSize بر اساس تعداد سطرهای محاسبه شده
            if (visibleRows > 0)
            {
                dataPager.PageSize = visibleRows-2;
                var g = dataPager.Source;
                dataPager.Source = null;
                dataPager.Source = g;
            }
        }

        private void txtGroup_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                txtCustomerGroupName.Focus();

                return;
            }
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}
