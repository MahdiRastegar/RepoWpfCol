using Syncfusion.Windows.Tools.Controls;
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
    public partial class usrPreferential : UserControl,ITabForm
    {
        public usrPreferential()
        {
            Preferentials = new ObservableCollection<Preferential>();
            InitializeComponent();
            isCancel = true;
        }
        Brush brush = null;
        public ObservableCollection<Preferential> Preferentials { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                if ((sender as TextBox).Name == "txtPreferentialName")
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
            if ((sender as TextBox).Name != "txtPreferentialName"&& (sender as TextBox).Name != "txtWebSite" && (sender as TextBox).Name != "txtEmail" && (sender as TextBox).Name != "txtAddress" && (sender as TextBox).Name != "txtDescription")
                e.Handled = !IsTextAllowed(e.Text);            
        }
        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Preferentials = new ObservableCollection<Preferential>();
            var db = new ColDbEntities1();
            cmbProvince.ItemsSource = db.Province.AsNoTracking().ToList();            

            var count = db.Preferential.Count();
            var h = db.Preferential.Include("City").AsNoTracking().ToList();
            //var h = db.Preferential.Take(10).ToList();
            //if(count>10)
            //{
            //    for (int i = 0; i < count-10; i++)
            //    {
            //        h.Add(null);
            //    }
            //}
            h.ForEach(u => Preferentials.Add(u));
            datagrid.SearchHelper.AllowFiltering = true;
            txtGroup.Focus();
            dataPager.Source = null;
            dataPager.Source = Preferentials;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = false;
            haserror = GetError();

            if (haserror)
                return;
            var db = new ColDbEntities1();
            var c = int.Parse(txtGroup.Text);
            var col = db.tGroup.FirstOrDefault(g => g.GroupCode == c);
            if (col == null)
            {
                Sf_txtGroup.ErrorText = "این کد گروه وجود ندارد";
                Sf_txtGroup.HasError = true;
                return;
            }
            var i = int.Parse(txtCodePreferential.Text);
            var preferential = db.Preferential.Find(id);
            //var mpreferential = db.Preferential.FirstOrDefault(g => g.fk_GroupId == col.Id && g.PreferentialCode == i);
            //if (preferential?.Id != mpreferential?.Id && mpreferential != null)
            //{
            //    Xceed.Wpf.Toolkit.MessageBox.Show("این کد تفضیلی و کد گروه از قبل وجود داشته است!");
            //    return;
            //}    
            var npreferential = db.Preferential.FirstOrDefault(g => g.fk_GroupId == col.Id && g.PreferentialName == txtPreferentialName.Text);
            if (preferential?.Id != npreferential?.Id && npreferential != null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("این نام تفضیلی و کد گروه از قبل وجود داشته است!");
                return;
            }

            Preferential e_add = null;
            if (id == Guid.Empty)
            {
                datagrid.SortColumnDescriptions.Clear();
                e_add = new Preferential()
                {
                    Id = Guid.NewGuid(),
                    PreferentialCode = i,
                    fk_GroupId = col.Id,
                    PreferentialName = txtPreferentialName.Text,
                    Mobile = txtMobile.Text,
                    Phone1 = txtPhone1.Text,
                    Phone2 = txtPhone2.Text,
                    Phone3 = txtPhone3.Text,
                    WebSite = txtWebSite.Text,
                    Email = txtEmail.Text,
                    Address = txtAddress.Text,
                    Description = txtDescription.Text,
                    fk_CityId = (cmbCity.SelectedItem as City)?.Id
                };
                db.Preferential.Add(e_add);
                Preferentials.Add(e_add);
            }
            else
            {                
                var e_Edidet = Preferentials.FirstOrDefault(a => a.Id == id);
                e_Edidet.fk_GroupId = preferential.fk_GroupId = col.Id;
                e_Edidet.PreferentialCode = preferential.PreferentialCode = i;
                e_Edidet.PreferentialName = preferential.PreferentialName = txtPreferentialName.Text;
                e_Edidet.tGroup.GroupName = txtGroupName.Text;

                e_Edidet.Mobile = preferential.Mobile = txtMobile.Text;
                e_Edidet.Phone1 = preferential.Phone1 = txtPhone1.Text;
                e_Edidet.Phone2 = preferential.Phone2 = txtPhone2.Text;
                e_Edidet.Phone3 = preferential.Phone3 = txtPhone3.Text;
                e_Edidet.WebSite = preferential.WebSite = txtWebSite.Text;
                e_Edidet.Email = preferential.Email = txtEmail.Text;
                e_Edidet.Address = preferential.Address = txtAddress.Text;
                e_Edidet.Description = preferential.Description = txtDescription.Text;
                preferential.fk_CityId = (cmbCity.SelectedItem as City)?.Id;
                e_Edidet.City = cmbCity.SelectedItem as City;
                if (e_Edidet.City != null)
                    e_Edidet.City.Province = cmbProvince.SelectedItem as Province;
            }
            if (!db.SafeSaveChanges())  return;
            if (id == Guid.Empty)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت تفضیلی");
                txtCodePreferential.Text = (e_add.PreferentialCode + 1).ToString();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش تفضیلی");
                btnCancel_Click(null, null);
            }
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";            
            ClearMore();
            txtCodePreferential.IsReadOnly = false;
            txtGroup.IsReadOnly = false;
            txtPreferentialName.Text = "";
            isCancel = true;            
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            if (id != Guid.Empty)
                txtGroup.Focus();
            else
            {
                txtPreferentialName.Focus();
                dataPager.MoveToLastPage();
            }

            id = Guid.Empty;
        }
        Guid id = Guid.Empty;
        private bool GetError()
        {
            var haserror = false;
            if (txtCodePreferential.Text.Trim() == "")
            {
                Sf_txtCodePreferential.HasError = true;
                haserror = true;
            }
            else
                Sf_txtCodePreferential.HasError = false;
            if (txtPreferentialName.Text.Trim() == "")
            {
                Sf_txtPreferentialName.HasError = true;
                haserror = true;
            }
            else
                Sf_txtPreferentialName.HasError = false;
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
                var list = db.tGroup.ToList().Select(r => new Mu() { Name = r.GroupName, Value = r.GroupCode.ToString() }).ToList();
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
            txtCodePreferential.IsReadOnly = false;
            txtGroup.IsReadOnly = false;
            txtPreferentialName.Text = "";
            Sf_txtPreferentialName.HasError = false;
            Sf_txtCodePreferential.HasError = false;
            Sf_txtGroup.HasError = false;
            Sf_txtGroup.ErrorText = "";
            ClearMore();
            //txtCodePreferential.Text = (en.PreferentialCode + 1).ToString();

            txtGroup.Focus();
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            txtGroup.TextChanged -= TxtGroup_TextChanged;
            txtGroupName.Text = txtGroup.Text = txtCodePreferential.Text = "";
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

        private void ClearMore()
        {
            txtMobile.Text = "";
            txtPhone1.Text = "";
            txtPhone2.Text = "";
            txtPhone3.Text = "";
            txtWebSite.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";
            txtDescription.Text = "";
            cmbCity.SelectedItem = null;
            cmbProvince.SelectedItem = null;
        }

        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (isCancel&&datagrid.SelectedItem!=null) 
            {
                var preferential = datagrid.SelectedItem as Preferential;
                id = preferential.Id;
                txtGroup.TextChanged -= TxtGroup_TextChanged;
                txtGroup.Text = preferential.tGroup.GroupCode.ToString();
                txtGroup.TextChanged += TxtGroup_TextChanged;
                txtGroupName.Text = preferential.tGroup.GroupName;
                txtPreferentialName.Text = preferential.PreferentialName;
                txtCodePreferential.Text = preferential.PreferentialCode.ToString();

                txtMobile.Text = preferential.Mobile;
                txtPhone1.Text = preferential.Phone1;
                txtPhone2.Text = preferential.Phone2;
                txtPhone3.Text = preferential.Phone3;
                txtWebSite.Text = preferential.WebSite;
                txtEmail.Text = preferential.Email;
                txtAddress.Text = preferential.Address;
                txtDescription.Text = preferential.Description;
                if (preferential.City != null)
                {
                    cmbCity.SelectionChanged -= cmbProvince_SelectionChanged;
                    cmbProvince.SelectionChanged -= cmbProvince_SelectionChanged;
                    cmbProvince.SelectedItem = (cmbProvince.ItemsSource as List<Province>).First(u => u.Id == preferential.City?.Province.Id);
                    var id = (cmbProvince.SelectedItem as Province).Id;
                    var db = new ColDbEntities1();
                    cmbCity.ItemsSource = db.City.AsNoTracking().Where(y => y.fk_ProvinceId == id).ToList();
                    cmbCity.SelectedItem = (cmbCity.ItemsSource as List<City>).First(u => u.Id == preferential.City.Id);
                    cmbCity.SelectionChanged += cmbProvince_SelectionChanged;
                    cmbProvince.SelectionChanged += cmbProvince_SelectionChanged;
                }
                else
                {
                    cmbCity.SelectedItem = null;
                    cmbProvince.SelectedItem = null;
                }
                gridDelete.Visibility = Visibility.Visible;
                borderEdit.Visibility = Visibility.Visible;
                txtCodePreferential.IsReadOnly = true;
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
            var preferential = db.Preferential.Find(id);
            if (db.AcDocument_Detail.Any(y => y.fk_PreferentialId == preferential.Id))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("قبلا با این تفضیلی سند حسابداری زده شده است و قابل حذف نیست!");
                return;
            }
            var listPreferentials = new List<string>();
            foreach (var item in db.CodeSetting.Where(s => s.Name.Contains("PreferentialCode")))
            {
                var code = db.CodeSetting.FirstOrDefault(j => j.Name == item.Name);
                if (code != null)
                {
                    listPreferentials.Add(item.Value);
                }
            }
            if (listPreferentials.Contains(preferential.PreferentialCode.ToString()))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("در تنظیمات پیکربندی از این تفضیل استفاده شده است و قابل حذف نیست!");
                return;
            }
            db.Preferential.Remove(preferential);
            if (!db.SafeSaveChanges())  return;
            Preferentials.Remove((datagrid.SelectedItem as Preferential));            
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

        private void txtPreferentialName_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtCodePreferential_TextChanged(object sender, TextChangedEventArgs e)
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
                    var group = db.tGroup.FirstOrDefault(gs => gs.GroupCode == g);
                    txtGroupName.Text = group.GroupName;
                    try
                    {
                        txtCodePreferential.Text = (db.Preferential.Include("Col").Where(u => u.tGroup.GroupCode == g).Max(y => y.PreferentialCode) + 1).ToString();
                    }
                    catch
                    {
                        txtCodePreferential.Text = ((group.GroupCode * 10000) + 1).ToString();
                    }
                }
                catch
                {
                    txtCodePreferential.Text = "1";
                    txtGroupName.Text = "";
                }
        }

        private void DataPager_PageIndexChanging(object sender, Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangingEventArgs e)
        {
            var ex = datagrid.View.FilterPredicates;
            
            var db = new ColDbEntities1();
            //db.Preferential.Where(ex)
            var count = db.Preferential.Count();
            var F = db.Preferential.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                Preferentials[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "حساب تفضیلی");
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

                var y = db.tGroup.FirstOrDefault(gs => gs.GroupCode == g);
                if (y != null)
                    Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(50);
                        txtPreferentialName.Focus();
                    }));
            }
            catch { }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {/*
            var db = new ColDbEntities1();    
            foreach (var item in db.Preferential.ToList())
            {
                foreach (var item2 in db.MoeinGroup.Where(t => t.fk_GroupId == item.fk_GroupId))
                {
                    db.Account.Add(new Account()
                    {
                        Id = Guid.NewGuid(),
                        fk_ColId = item2.Moein.fk_ColId,
                        fk_PreferentialId = item.Id,
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
                txtPreferentialName.Focus();

                return;
            }
            e.Handled = !IsTextAllowed(e.Text);
        }
        private void cmbProvince_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmb = sender as ComboBoxAdv;
            isCancel = false;
            switch (cmb.Name)
            {
                case "cmbProvince":
                    if (cmb.SelectedIndex != -1)
                    {
                        cmbCity.Focus();
                        var db = new ColDbEntities1();
                        var id = (cmbProvince.SelectedItem as Province).Id;
                        cmbCity.ItemsSource = db.City.AsNoTracking().Where(y=>y.fk_ProvinceId==id).ToList();
                    }
                    break;
                case "cmbCity":
                    if (cmb.SelectedIndex != -1)
                    {
                        txtPhone1.Focus();
                    }
                    break;

            }
        }

        private void cmbProvince_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var cmb = sender as ComboBoxAdv;
            if (e.Key == Key.Enter)
            {
                switch (cmb.Name)
                {
                    case "cmbProvince":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            cmbCity.Focus();
                            var db = new ColDbEntities1();
                            var id = (cmbProvince.SelectedItem as Province).Id;
                            cmbCity.ItemsSource = db.City.AsNoTracking().Where(y => y.fk_ProvinceId == id).ToList();
                        }));
                        break;
                    case "cmbCity":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtPhone1.Focus();
                        }));
                        break;

                }
                return;
            }
            cmb.SelectedIndex = -1;
        }

        private void cmbProvince_LostFocus(object sender, RoutedEventArgs e)
        {
            var cmb=sender as ComboBoxAdv;
            if (cmb.SelectedIndex == -1)
                cmb.Text = "";
        }
    }
}
