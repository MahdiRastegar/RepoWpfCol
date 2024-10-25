using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Office2019.Excel.RichData2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
using WpfCol.Windows.toolWindows;
using Point = System.Windows.Point;

namespace WpfCol
{
    /// <summary>
    /// Interaction logic for winCol.xaml
    /// </summary>
    public partial class winMoein : UserControl,ITabForm
    {
        public winMoein()
        {
            Moeins = new ObservableCollection<Moein>();
            InitializeComponent();
        }
        Brush brush = null;
        public ObservableCollection<Moein> Moeins { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                if ((sender as TextBox).Name == "txtMoeinName")
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
            if ((sender as TextBox).Name != "txtMoeinName")
                e.Handled = !IsTextAllowed(e.Text);            
        }
        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Moeins = new ObservableCollection<Moein>();
            var db = new ColDbEntities1();
            var count = db.Moein.Count();
            var h = db.Moein.AsNoTracking().ToList();
            //var h = db.Moein.Take(10).ToList();
            //if(count>10)
            //{
            //    for (int i = 0; i < count-10; i++)
            //    {
            //        h.Add(null);
            //    }
            //}
            h.ForEach(u => Moeins.Add(u));
            datagrid.SearchHelper.AllowFiltering = true;
            //checkListBox.ItemsSource = db.tGroup.ToList();
            txtCol.Focus();
            dataPager.Source = null;
            dataPager.Source = Moeins;
            isCancel = true;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = false;
            haserror = GetError();

            if (haserror)
                return;
            var db = new ColDbEntities1();
            var c = int.Parse(txtCol.Text);
            var col = db.Col.FirstOrDefault(g => g.ColCode == c);
            if (col == null)
            {
                Sf_txtCol.ErrorText = "این کد کل وجود ندارد";
                Sf_txtCol.HasError = true;
                return;
            }
            var i = int.Parse(txtCodeMoein.Text);
            var moein = db.Moein.Find(id);
            //var mmoein = db.Moein.FirstOrDefault(g => g.fk_ColId == col.Id && g.MoeinCode == i);
            //if (moein?.Id != mmoein?.Id && mmoein != null)
            //{
            //    Xceed.Wpf.Toolkit.MessageBox.Show("این کد معین و کد کل از قبل وجود داشته است!");
            //    return;
            //}    
            var nmoein = db.Moein.FirstOrDefault(g => g.fk_ColId == col.Id && g.MoeinName == txtMoeinName.Text);
            if (moein?.Id != nmoein?.Id && nmoein != null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("این نام معین و کد کل از قبل وجود داشته است!");
                return;
            }
            Moein e_add = null;
            if (id == Guid.Empty)
            {
                datagrid.SortColumnDescriptions.Clear();
                e_add = new Moein()
                {
                    Id = Guid.NewGuid(),
                    MoeinCode = i,
                    fk_ColId = col.Id,
                    MoeinName = txtMoeinName.Text
                };
                db.Moein.Add(e_add);
                Moeins.Add(e_add);
                //foreach (var item in checkListBox.SelectedItems)//GroupDeleted
                //{
                //    var group = item as tGroup;
                //    db.MoeinGroup.Add(new MoeinGroup()
                //    {
                //        Id = Guid.NewGuid(),
                //        Moein = e_add,
                //        fk_GroupId = group.Id
                //    });
                //}                
            }
            else
            {                
                var e_Edidet = Moeins.FirstOrDefault(a => a.Id == id);
                e_Edidet.fk_ColId = moein.fk_ColId = col.Id;
                e_Edidet.MoeinName = moein.MoeinName = txtMoeinName.Text;
                //db.MoeinGroup.RemoveRange(db.MoeinGroup.Where(j => j.fk_MoeinId == id));//GroupDeleted
                //foreach (var item in checkListBox.SelectedItems)
                //{
                //    var group = item as tGroup;
                //    db.MoeinGroup.Add(new MoeinGroup()
                //    {
                //        Id = Guid.NewGuid(),
                //        fk_MoeinId = id,
                //        fk_GroupId = group.Id
                //    });
                //}
            }
            db.SaveChanges();
            if (id == Guid.Empty)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت معین");
                txtCodeMoein.Text = (e_add.MoeinCode + 1).ToString();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش معین");
                btnCancel_Click(null, null);
            }
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";                            
            txtCodeMoein.IsReadOnly = false;
            txtCol.IsReadOnly = false;
            txtMoeinName.Text = "";
            isCancel = true;            
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            if (id != Guid.Empty)
                txtCol.Focus();
            else
            {
                txtMoeinName.Focus();
                dataPager.MoveToLastPage();
            }
            id = Guid.Empty;
        }
        Guid id = Guid.Empty;
        private bool GetError()
        {
            var haserror = false;
            if (txtCodeMoein.Text.Trim() == "")
            {
                Sf_txtCodeMoein.HasError = true;
                haserror = true;
            }
            else
                Sf_txtCodeMoein.HasError = false;
            if (txtMoeinName.Text.Trim() == "")
            {
                Sf_txtMoeinName.HasError = true;
                haserror = true;
            }
            else
                Sf_txtMoeinName.HasError = false;
            if (txtCol.Text.Trim() == "")
            {
                Sf_txtCol.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtCol.HasError = false;
                Sf_txtCol.ErrorText = "";
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
            else if(e.Key == Key.F1&&txtCol.IsFocused)
            {
                if (window != null)
                    return;
                /*Point relativePoint = y.TransformToAncestor(this)
                          .Transform(new Point(this.Left+Width, this.Top-Height));*/
                isCancel = false;
                Point relativePoint = new Point(MainWindow.Current.Left + Width - 500, MainWindow.Current.Top + 50);
                if (MainWindow.Current.WindowState == WindowState.Maximized)
                    relativePoint = txtCol.TransformToAncestor(this)
                          .Transform(new Point(530, 0));
                var db = new ColDbEntities1();
                var list = db.Col.ToList().Select(r => new Mu() { Name = r.ColName, Value = r.ColCode.ToString() }).ToList();
                var win = new winSearch(list);
                win.Tag = this;
                win.ParentTextBox = txtCol;
                win.SearchTermTextBox.Text = txtCol.Text;
                win.SearchTermTextBox.Select(1, 0);
                win.Owner = MainWindow.Current;
                //win.Left = relativePoint.X - 60;
                //win.Top = relativePoint.Y + 95;
                window = win;
                win.Show();
                win.Focus();
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
        bool isCancel = true;
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (isCancel && id == Guid.Empty)
            {
                return;
            }
            if (sender != null && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            txtCodeMoein.IsReadOnly = false;
            txtCol.IsReadOnly = false;
            txtMoeinName.Text = "";
            Sf_txtMoeinName.HasError = false;
            Sf_txtCodeMoein.HasError = false;
            Sf_txtCol.HasError = false;
            Sf_txtCol.ErrorText = "";
            //txtCodeMoein.Text = (en.MoeinCode + 1).ToString();

            txtCol.Focus();
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            txtCol.TextChanged -= TxtCol_TextChanged;
            txtColName.Text = txtCol.Text = txtCodeMoein.Text = "";
            txtCol.TextChanged += TxtCol_TextChanged;
            isCancel = true;
            id = Guid.Empty;
        }

        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (isCancel&&datagrid.SelectedItem!=null) 
            {
                var moein = datagrid.SelectedItem as Moein;
                id = moein.Id;
                txtCol.TextChanged -= TxtCol_TextChanged;
                txtCol.Text = moein.Col.ColCode.ToString();
                txtCol.TextChanged += TxtCol_TextChanged;
                txtColName.Text = moein.Col.ColName;
                txtMoeinName.Text = moein.MoeinName;
                txtCodeMoein.Text = moein.MoeinCode.ToString();
                /*checkListBox.SelectedItems.Clear();//GroupDeleted
                var db = new ColDbEntities1();
                foreach (MoeinGroup mo in db.MoeinGroup.Where(t => t.fk_MoeinId == id).ToList())
                    checkListBox.SelectedItems.Add((checkListBox.ItemsSource as List<tGroup>).Find(u => u.Id == mo.fk_GroupId));*/
                gridDelete.Visibility = Visibility.Visible;
                borderEdit.Visibility = Visibility.Visible;
                txtCodeMoein.IsReadOnly = true;
                txtCol.IsReadOnly = true;
                isCancel = true;
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
            var moein = db.Moein.Find(id);
            if (db.AcDocument_Detail.Any(y => y.fk_MoeinId == moein.Id))
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("قبلا با این معین سند حسابداری زده شده است و قابل حذف نیست!");
                return;
            }
            db.Moein.Remove(db.Moein.Find(id));
            db.SaveChanges();
            Moeins.Remove((datagrid.SelectedItem as Moein));            
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

        private void txtMoeinName_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtCodeMoein_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }
        public static Window window;
        private void TxtCol_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtCol_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!isCancel)
                try
                {
                    var db = new ColDbEntities1();
                    var g = int.Parse(txtCol.Text);

                    txtColName.Text = db.Col.FirstOrDefault(gs => gs.ColCode == g).ColName;
                    txtCodeMoein.Text = (db.Moein.Include("Col").Where(u => u.Col.ColCode == g).Max(y => y.MoeinCode) + 1).ToString();
                }
                catch
                {
                    txtCodeMoein.Text = "1";
                }
        }

        private void DataPager_PageIndexChanging(object sender, Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangingEventArgs e)
        {
            var ex = datagrid.View.FilterPredicates;
            
            var db = new ColDbEntities1();
            //db.Moein.Where(ex)
            var count = db.Moein.Count();
            var F = db.Moein.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                Moeins[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "حساب معین");
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
                var g = int.Parse(txtCol.Text);

                var y = db.Col.FirstOrDefault(gs => gs.ColCode == g);
                if (y != null)
                    Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(50);
                        txtMoeinName.Focus();
                    }));
            }
            catch { }
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
                dataPager.PageSize = visibleRows - 2;
                var g = dataPager.Source;
                dataPager.Source = null;
                dataPager.Source = g;
            }
        }
    }
}
