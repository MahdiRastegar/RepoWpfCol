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
    public partial class usrAcType : UserControl,ITabForm
    {
        public usrAcType()
        {
            InitializeComponent();            
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
            var M = db.DocumentType.AsNoTracking().ToList();
            datagrid.ItemsSource = M;
            datagrid.SearchHelper.AllowFiltering = true;
            txtAcType.Focus();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = GetError();

            if (haserror)
                return;
            var db = new ColDbEntities1();
            //var g = (datagrid.SelectedItem.GetType().GetProperty("Name").GetValue(datagrid.SelectedItem));
            var group = db.DocumentType.Find(txtAcType.Tag as Guid?);
            if (group == null)
            {
                if (db.DocumentType.Any(h => h.IsManual && h.Name == txtAcType.Text))
                {
                    Sf_txtVra.HasError = true;
                    Sf_txtVra.ErrorText = "نوع سند تکراریست!";
                    return;
                }
                db.DocumentType.Add(new DocumentType()
                {
                    Id = Guid.NewGuid(),
                    Name = txtAcType.Text,
                    IsManual = true
                });
            }
            else
            {
                var f = db.DocumentType.FirstOrDefault(h => h.IsManual && h.Name == txtAcType.Text);
                if (f!=null&& (txtAcType.Tag as Guid?) != f.Id)
                {
                    Sf_txtVra.HasError = true;
                    Sf_txtVra.ErrorText = "این نام قبلا ثبت شده";
                    return;
                }
                group.Name = txtAcType.Text;
            }
            if (!db.SafeSaveChanges())  return;
            var M = db.DocumentType.ToList();
            datagrid.ItemsSource = M;
            if (group == null)
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت گروه");
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش گروه");
            }
            btnCancel_Click(null, null);

            txtAcType.Text = "";
            txtAcType.Tag = Guid.Empty;
            isCancel = true;
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            if (group == null)                
                txtAcType.Focus();
        }

        private bool GetError()
        {
            bool haserror = false;
            if (txtAcType.Text.Trim() == "")
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
    
        bool isCancel = true;
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if(txtAcType.Text.Trim()=="")
            {
                return;
            }
            if (sender != null && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            txtAcType.Text = "";
            txtAcType.Tag = Guid.Empty;
            Sf_txtVra.HasError = false;
            isCancel = true;
            var db = new ColDbEntities1();
            txtAcType.Focus();
            datagrid.SelectedIndex = -1;
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

        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (isCancel&&datagrid.SelectedItem!=null) 
            {
                var group = datagrid.SelectedItem as DocumentType;
                txtAcType.Text = group.Name;
                txtAcType.Tag = group.Id;
                gridDelete.Visibility = Visibility.Visible;
                isCancel = true;
                borderEdit.Visibility = Visibility.Visible;
                GetError();
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
            db.DocumentType.Remove(db.DocumentType.Find((datagrid.SelectedItem as DocumentType).Id));
            if (!db.SafeSaveChanges())  return;
            (datagrid.ItemsSource as List<DocumentType>).Remove((datagrid.SelectedItem as DocumentType));
            var u = datagrid.ItemsSource;
            datagrid.ItemsSource = null;
            datagrid.ItemsSource = u;
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
            var item = list.FirstOrDefault(u => u.Header == "گروه تفضیلی");
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
    }
}
