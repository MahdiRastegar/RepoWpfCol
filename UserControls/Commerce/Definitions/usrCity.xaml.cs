using DocumentFormat.OpenXml.Office2010.Excel;
using Syncfusion.XlsIO.Parser.Biff_Records;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WpfCol
{
    /// <summary>
    /// Interaction logic for winCol.xaml
    /// </summary>
    public partial class usrCity : UserControl,ITabForm
    {
        public usrCity()
        {
            InitializeComponent();            
            isCancel = true;
        }
        Brush brush = null;

        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                /*
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                request.Wrapped = true;
                (sender as TextBox).MoveFocus(request);*/
                btnConfirm.Focus();
                btnConfirm_Click(null, null);
                return;
            }
            /*
            if ((sender as TextBox).Name == "txtVra" || (sender as TextBox).Name == "txtDis")
                e.Handled = !IsTextAllowed(e.Text);*/
        }
        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        int rowsCount;
        List<long> Ids;
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var db=new ColDbEntities1();
            cmbProvince.ItemsSource = db.Province.AsNoTracking().ToList();

            var M = db.City.AsNoTracking().ToList();
            datagrid.ItemsSource = M;
            datagrid.SearchHelper.AllowFiltering = true;
            txtGroupName.Focus();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = GetError();

            if (haserror)
                return;
            var db = new ColDbEntities1();
            var group = db.City.Find(id);
            if (group == null)
                db.City.Add(new City()
                {
                    Id = Guid.NewGuid(),
                    Name = txtGroupName.Text,
                    fk_ProvinceId = (cmbProvince.SelectedItem as Province).Id
                });
            else
            {
                group.Name = txtGroupName.Text;
                group.fk_ProvinceId = (cmbProvince.SelectedItem as Province).Id;
            }
            if (!db.SafeSaveChanges())  return;
            var cities = db.City.Include("Province");
            var M = cities.AsNoTracking().ToList();
            datagrid.ItemsSource = M;
            if (group == null)
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت شهر");
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش شهر");
            }
            btnCancel_Click(null, null);

            txtGroupName.Text = "";
            isCancel = true;
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            if (group == null)
                txtGroupName.Focus();
        }

        private bool GetError()
        {
            bool haserror = false;
            if (cmbProvince.SelectedIndex == -1)
            {
                Sf_txtProvince.HasError = true;
                haserror = true;
            }
            else
                Sf_txtProvince.HasError = false;
            if (txtGroupName.Text.Trim() == "")
            {
                Sf_txtVra.HasError = true;
                haserror = true;
            }
            else
                Sf_txtVra.HasError = false;
           
            return haserror;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این فرم خارج شوید؟", "خروج", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }


        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            var gr = brush as LinearGradientBrush;
            if (gr != null)
            {
                var gr2 = new LinearGradientBrush();
                foreach (var item in gr.GradientStops)
                {
                    gr2.GradientStops.Add(new GradientStop(item.Color, item.Offset));
                }
                for (var i = 1; i < gr2.GradientStops.Count; i++)
                {
                    gr2.GradientStops[i].Color = ColorToBrushConverter.GetLightOfColor(gr.GradientStops[i].Color, .15f);
                }
                gr2.EndPoint = gr.EndPoint;
                gr2.StartPoint = gr.StartPoint;
                border.Background = gr2;
            }
            else
            {
                border.Background = new SolidColorBrush(ColorToBrushConverter.GetLightOfColor((brush as SolidColorBrush).Color, .15f));
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void border_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Border).Background = brush;
        }

        private void border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            if (border.IsMouseOver)
            {
                Border_MouseEnter(sender, e);
            }
            else
            {
                border.Background = brush;
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
                Border_MouseDown("mu", null);
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
            if(e.Key == Key.Escape)
            {
                CloseForm();
            }
        }

        private bool _iscancel = false;

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
            if (isCancel && sender != null && datagrid.SelectedIndex == -1)
            {
                gridContainer.Opacity = 1;
                gridContainer.IsEnabled = true;
                return;
            }
            if (!isCancel && sender != null && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            txtGroupName.Text = "";
            Sf_txtVra.HasError = false;
            Sf_txtProvince.HasError = false;
            isCancel = true;            
            txtGroupName.Focus();

            if (sender != null)
            {
                if (datagrid.SelectedIndex == -1)
                {
                    gridContainer.Opacity = 1;
                    gridContainer.IsEnabled = true;
                }
            }

            datagrid.SelectedIndex = -1;
            id = Guid.Empty;
            datagrid.ClearFilters();
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
        }

        private void cmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isCancel = false;
        }

        private void txtGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }
        Guid id = Guid.Empty;
        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (isCancel && datagrid.SelectedItem != null)
            {
                var group = datagrid.SelectedItem as City;
                txtGroupName.Text = group.Name;
                cmbProvince.SelectedItem = (cmbProvince.ItemsSource as List<Province>).First(t => t.Id == group.Province.Id);
                gridDelete.Visibility = Visibility.Visible;
                isCancel = true;
                borderEdit.Visibility = Visibility.Visible;
                id = group.Id;
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
            db.City.Remove(db.City.Find((datagrid.SelectedItem as City).Id));
            if (!db.SafeSaveChanges())  return;
            (datagrid.ItemsSource as List<City>).Remove((datagrid.SelectedItem as City));
            var u = datagrid.ItemsSource;
            datagrid.ItemsSource = null;
            datagrid.ItemsSource = u;
            id = Guid.Empty;
            btnCancel_Click(null, null);
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() == "")
                datagrid.SearchHelper.ClearSearch();
            else
                datagrid.SearchHelper.Search(SearchTermTextBox.Text);
        }

        public bool CloseForm()
        {
            if (Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این فرم خارج شوید؟", "خروج", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return false;
            }
            forceClose = true;
            var list = MainWindow.Current.GetTabControlItems;
            var item = list.FirstOrDefault(u => u.Header == "شهر");
            MainWindow.Current.tabcontrol.Items.Remove(item);
            return true;
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            datagrid.AllowFiltering = !datagrid.AllowFiltering;
            if (!datagrid.AllowFiltering)
                datagrid.ClearFilters();
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {

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

        public void SetNull()
        {

        }

        private void cmbProvince_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            isCancel = false;
            if (cmbProvince.SelectedIndex != -1)
                txtGroupName.Focus();
        }

        private void cmbProvince_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(50);
                    txtGroupName.Focus();
                }));
                return;
            }
            cmbProvince.SelectedIndex = -1;
        }

        private void cmbProvince_LostFocus(object sender, RoutedEventArgs e)
        {
          
                if (cmbProvince.SelectedIndex == -1)
                    cmbProvince.Text = "";
        }
    }
}
