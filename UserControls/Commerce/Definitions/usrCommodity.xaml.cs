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
    public partial class usrCommodity : UserControl,ITabForm
    {
        public usrCommodity()
        {
            Commodities = new ObservableCollection<Commodity>();
            InitializeComponent();
            isCancel = true;
        }
        Brush brush = null;
        public ObservableCollection<Commodity> Commodities { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                (sender as TextBox).MoveFocus(request);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (btnConfirm.IsFocused)
                    {
                        btnConfirm_Click(null, null);
                    }
                }));
                return;
            }        
            if ((sender as TextBox).Name != "txtCommodityName"&& (sender as TextBox).Name != "txtWebSite" && (sender as TextBox).Name != "txtEmail" && (sender as TextBox).Name != "txtAddress" && (sender as TextBox).Name != "txtDescription")
                e.Handled = !IsTextAllowed(e.Text);            
        }
        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Commodities = new ObservableCollection<Commodity>();
            var db = new ColDbEntities1();
            var count = db.Commodity.Count();
            var h = db.Commodity.AsNoTracking().ToList();
            //var h = db.Commodity.Take(10).ToList();
            //if(count>10)
            //{
            //    for (int i = 0; i < count-10; i++)
            //    {
            //        h.Add(null);
            //    }
            //}
            h.ForEach(u => Commodities.Add(u));
            datagrid.SearchHelper.AllowFiltering = true;
            txtGroup.Focus();
            dataPager.Source = null;
            dataPager.Source = Commodities;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = false;
            haserror = GetError();

            if (haserror)
                return;
            var db = new ColDbEntities1();
            var c = int.Parse(txtGroup.Text);
            var col = db.GroupCommodity.FirstOrDefault(g => g.GroupCode == c);
            if (col == null)
            {
                Sf_txtGroup.ErrorText = "این کد گروه وجود ندارد";
                Sf_txtGroup.HasError = true;
                return;
            }
            var i = int.Parse(txtCodeCommodity.Text);
            var Commodity = db.Commodity.Find(id);
            //var mCommodity = db.Commodity.FirstOrDefault(g => g.fk_GroupId == col.Id && g.Code == i);
            //if (Commodity?.Id != mCommodity?.Id && mCommodity != null)
            //{
            //    Xceed.Wpf.Toolkit.MessageBox.Show("این کد کالا و کد گروه از قبل وجود داشته است!");
            //    return;
            //}    
            var nCommodity = db.Commodity.FirstOrDefault(g => g.fk_GroupId == col.Id && g.Name == txtCommodityName.Text);
            if (Commodity?.Id != nCommodity?.Id && nCommodity != null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("این نام کالا و کد گروه از قبل وجود داشته است!");
                return;
            }

            Commodity e_add = null;
            var Unitcode = int.Parse(txtUnit.Text);
            var mu = db.Unit.First(t => t.Code == Unitcode);
            if (id == Guid.Empty)
            {
                datagrid.SortColumnDescriptions.Clear();
                e_add = new Commodity()
                {
                    Id = Guid.NewGuid(),
                    Code = i,
                    fk_GroupId = col.Id,
                    Name = txtCommodityName.Text,
                    Unit = mu
                };
                db.Commodity.Add(e_add);
                Commodities.Add(e_add);
            }
            else
            {                
                var e_Edidet = Commodities.FirstOrDefault(a => a.Id == id);
                e_Edidet.fk_GroupId = Commodity.fk_GroupId = col.Id;
                e_Edidet.Code = Commodity.Code = i;
                e_Edidet.Name = Commodity.Name = txtCommodityName.Text;
                e_Edidet.GroupCommodity.GroupName = txtGroupName.Text;
                e_Edidet.Unit = mu;
            }
            if (!db.SafeSaveChanges())  return;
            if (id == Guid.Empty)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت کالا");
                txtCodeCommodity.Text = (e_add.Code + 1).ToString();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش کالا");
                btnCancel_Click(null, null);
            }
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";            
            txtCodeCommodity.IsReadOnly = false;
            txtGroup.IsReadOnly = false;
            txtCommodityName.Text = "";
            isCancel = true;            
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            if (id != Guid.Empty)
                txtGroup.Focus();
            else
            {
                txtCommodityName.Focus();
                dataPager.MoveToLastPage();
            }

            id = Guid.Empty;
        }
        Guid id = Guid.Empty;
        private bool GetError()
        {
            var haserror = false;
            if (txtCodeCommodity.Text.Trim() == "")
            {
                Sf_txtCodeCommodity.HasError = true;
                haserror = true;
            }
            else
                Sf_txtCodeCommodity.HasError = false;
            if (txtCommodityName.Text.Trim() == "")
            {
                Sf_txtCommodityName.HasError = true;
                haserror = true;
            }
            else
                Sf_txtCommodityName.HasError = false;
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
            if (txtUnit.Text.Trim() == "")
            {
                Sf_txtUnit.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtUnit.HasError = false;
                Sf_txtUnit.ErrorText = "";
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
            else if (e.Key == Key.F1)
            {
                if (txtGroup.IsFocused && !txtGroup.IsReadOnly)
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
                    var list = db.GroupCommodity.ToList().Select(r => new Mu() { Name = r.GroupName, Value = r.GroupCode.ToString() }).ToList();
                    var win = new winSearch(list);
                    win.Tag = this;
                    win.ParentTextBox = txtGroup;
                    win.SearchTermTextBox.Text = txtGroup.Text;
                    win.SearchTermTextBox.Select(1, 0);
                    win.Owner = MainWindow.Current;
                    //win.Left = relativePoint.X - 60;
                    //win.Top = relativePoint.Y + 95;
                    window = win;
                    win.Show(); 
                    win.Focus();
                }
                else if (txtUnit.IsFocused && !txtUnit.IsReadOnly)
                {
                    if (window != null)
                        return;
                    /*Point relativePoint = y.TransformToAncestor(this)
                              .Transform(new Point(this.Left+Width, this.Top-Height));*/
                    isCancel = false;
                    Point relativePoint = new Point(MainWindow.Current.Left + Width - 500, MainWindow.Current.Top + 50);
                    if (MainWindow.Current.WindowState == WindowState.Maximized)
                        relativePoint = txtUnit.TransformToAncestor(this)
                              .Transform(new Point(530, 0));
                    var db = new ColDbEntities1();
                    var list = db.Unit.ToList().Select(r => new Mu() { Name = r.Name, Value = r.Code.ToString() }).ToList();
                    var win = new winSearch(list);
                    win.Tag = this;
                    win.ParentTextBox = txtUnit;
                    win.SearchTermTextBox.Text = txtUnit.Text;
                    win.SearchTermTextBox.Select(1, 0);
                    win.Owner = MainWindow.Current;
                    //win.Left = relativePoint.X - 60;
                    //win.Top = relativePoint.Y + 95;
                    window = win;
                    win.Show();
                    win.Focus();
                }
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
            txtCodeCommodity.IsReadOnly = false;
            txtGroup.IsReadOnly = false;
            txtCommodityName.Text = "";
            Sf_txtCommodityName.HasError = false;
            Sf_txtCodeCommodity.HasError = false;
            Sf_txtGroup.HasError = false;
            Sf_txtGroup.ErrorText = "";
            //txtCodeCommodity.Text = (en.Code + 1).ToString();
            Sf_txtUnit.HasError = false;
            txtUnit.Text = "";
            txtUnitName.Text = "";

            txtGroup.Focus();
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            txtGroup.TextChanged -= TxtGroup_TextChanged;
            txtGroupName.Text = txtGroup.Text = txtCodeCommodity.Text = "";
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
                var Commodity = datagrid.SelectedItem as Commodity;
                id = Commodity.Id;
                txtGroup.TextChanged -= TxtGroup_TextChanged;
                txtGroup.Text = Commodity.GroupCommodity.GroupCode.ToString();
                txtGroup.TextChanged += TxtGroup_TextChanged;
                txtGroupName.Text = Commodity.GroupCommodity.GroupName;
                txtCommodityName.Text = Commodity.Name;
                txtCodeCommodity.Text = Commodity.Code.ToString();
                txtUnit.Text = Commodity.Unit.Code.ToString();
                txtUnitName.Text = Commodity.Unit.Name.ToString();

                gridDelete.Visibility = Visibility.Visible;
                borderEdit.Visibility = Visibility.Visible;
                txtCodeCommodity.IsReadOnly = true;
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
            var Commodity = db.Commodity.Find(id);
            //if (db.AcDocument_Detail.Any(y => y.fk_CommodityId == Commodity.Id))
            //{
            //    Xceed.Wpf.Toolkit.MessageBox.Show("قبلا با این کالا سند حسابداری زده شده است و قابل حذف نیست!");
            //    return;
            //}            
            db.Commodity.Remove(Commodity);
            if (!db.SafeSaveChanges())  return;
            id = Guid.Empty;
            Commodities.Remove((datagrid.SelectedItem as Commodity));            
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

        private void txtCommodityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtCodeCommodity_TextChanged(object sender, TextChangedEventArgs e)
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
                    var group = db.GroupCommodity.FirstOrDefault(gs => gs.GroupCode == g);
                    txtGroupName.Text = group.GroupName;

                    try
                    {
                        txtCodeCommodity.Text = (db.Commodity.Where(u => u.GroupCommodity.GroupCode == g).Max(y => y.Code) + 1).ToString();
                    }
                    catch
                    {
                        txtCodeCommodity.Text = ((group.GroupCode * 10000) + 1).ToString();
                    }                    
                }
                catch
                {
                    txtCodeCommodity.Text = "1";
                    txtGroupName.Text = "";
                }
        }

        private void DataPager_PageIndexChanging(object sender, Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangingEventArgs e)
        {
            var ex = datagrid.View.FilterPredicates;
            
            var db = new ColDbEntities1();
            //db.Commodity.Where(ex)
            var count = db.Commodity.Count();
            var F = db.Commodity.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                Commodities[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "کالا");
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
            if ((window as winSearch).ParentTextBox == txtUnit)
            {
                window = null;
                return;
            }
            window = null;

            try
            {
                var db = new ColDbEntities1();
                var g = int.Parse(txtGroup.Text);

                var y = db.GroupCommodity.FirstOrDefault(gs => gs.GroupCode == g);
                if (y != null)
                    Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(50);
                        txtCommodityName.Focus();
                    }));
            }
            catch { }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {/*
            var db = new ColDbEntities1();    
            foreach (var item in db.Commodity.ToList())
            {
                foreach (var item2 in db.MoeinGroup.Where(t => t.fk_GroupId == item.fk_GroupId))
                {
                    db.Account.Add(new Account()
                    {
                        Id = Guid.NewGuid(),
                        fk_ColId = item2.Moein.fk_ColId,
                        fk_CommodityId = item.Id,
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
                txtCommodityName.Focus();

                return;
            }
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void txtUnit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                btnConfirm.Focus();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (btnConfirm.IsFocused)
                    {
                        btnConfirm_Click(null, null);
                    }
                }));
                return;
            }
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void txtUnit_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtUnit.Text == "")
            {
                txtUnit.Text = string.Empty;
                txtUnitName.Text = string.Empty;
                return;
            }
            var db = new ColDbEntities1();
            var code=int.Parse(txtUnit.Text);
            var mu = db.Unit.FirstOrDefault(t =>  t.Code == code);
            if (mu == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("چنین واحد اندازه گیری وجود ندارد!");
                txtUnit.Text = txtUnitName.Text = string.Empty;
            }
            else
            {
                txtUnitName.Text = mu.Name;
            }
        }
    }
}
