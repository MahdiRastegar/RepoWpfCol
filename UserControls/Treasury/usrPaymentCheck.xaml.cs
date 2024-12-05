using DocumentFormat.OpenXml.Office2010.Excel;
using Mahdi.PersianDateControls;
using PersianCalendarWPF;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TextInputLayout;
using Syncfusion.Windows.Controls.Input;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.XlsIO.Parser.Biff_Records;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfCol.Interfaces;
using WpfCol.Windows.toolWindows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using static ClosedXML.Excel.XLPredefinedFormat;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace WpfCol
{
    /// <summary>
    /// Interaction logic for winCol.xaml
    /// </summary>
    public partial class usrPaymentCheck : UserControl,ITabForm,IDisposable
    {
        public bool DataGridIsFocused
        {
            get
            {
                return datagrid.IsFocused;
            }
        }
        List<Mu> mus1 = new List<Mu>();
        List<Mu> mus2 = new List<Mu>();
        public usrPaymentCheck()
        {
            temp_checkPaymentEvents = new ObservableCollection<CheckPaymentEvent>();
            checkPaymentEvents = new ObservableCollection<CheckPaymentEvent>();
            mini_checkPaymentEvents = new ObservableCollection<CheckPaymentEvent>();
            InitializeComponent();
            Sf_txtMoein.IsEnabled = false;
            txbMoein.Text = txtMoein.Text = "";
            Sf_txtPreferential.IsEnabled = false;
            txbPreferential.Text = txtPreferential.Text = "";            
            txbCalender.Text = pcw1.SelectedDate.ToString();
        }

        public void Dispose()
        {
            if (DataContext == null)
                return;
            mus1.Clear();
            mus2.Clear();
            mini_checkPaymentEvents.Clear();
            checkPaymentEvents.Clear();
            datagrid.Dispose();
            dataPager.Dispose();
            DataContext = null;
            GC.Collect();
        }

        Brush brush = null;
        public ObservableCollection<CheckPaymentEvent> temp_checkPaymentEvents { get; set; }
        public ObservableCollection<CheckPaymentEvent> mini_checkPaymentEvents { get; set; }
        public ObservableCollection<CheckPaymentEvent> checkPaymentEvents { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                if ((sender as TextBox).Name == "txtDescription")
                {
                    datagrid.Focus();                    
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
            if ((sender as TextBox).Name != "txtDescription")
                e.Handled = !IsTextAllowed(e.Text);            
        }
        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            mini_checkPaymentEvents.Clear();
            checkPaymentEvents.Clear();
            GC.Collect();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var db = new ColDbEntities1();
            mus1.Clear();
            mus2.Clear();

            var moeins = db.Moein.ToList();
            var preferentials = db.Preferential.ToList();
            foreach (var item in moeins)
            {
                AccountSearchClass accountSearchClass = new AccountSearchClass();
                accountSearchClass.Id = item.Id;
                accountSearchClass.Moein = item.MoeinCode.ToString();
                accountSearchClass.MoeinName = item.MoeinName;
                accountSearchClass.ColMoein = $"{item.Col.ColCode}{item.MoeinCode}";
                mus1.Add(new Mu()
                {
                    Id = item.Id,
                    Value = $"{item.Col.ColCode}",
                    Name = $"{item.Col.ColName}",
                    AdditionalEntity = accountSearchClass
                });
            }
            foreach (var item in preferentials)
            {
                mus2.Add(new Mu()
                {
                    Id = item.Id,
                    Name = $"{item.PreferentialName}",
                    Value = $"{item.PreferentialCode}",
                    Name2 = item.tGroup.GroupName
                });
            }
            Fill();     
            dataPager.Source = null;
            dataPager.Source = checkPaymentEvents;
            datagrid.Focus();
            datagrid.SearchHelper.AllowFiltering = true;
            isCancel = true;
            TabControlExt_SelectionChanged(null, null);
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = false;
            haserror = GetError();

            if (haserror)
                return;
            var db=new ColDbEntities1();
            var x = new List<CheckPaymentEvent>();
            foreach (CheckPaymentEvent item in datagrid.SelectedItems)
            {
                var en = new CheckPaymentEvent()
                {
                    ChEvent = db.ChEvent.First(t => t.ChEventCode == cmbChangeState.SelectedIndex + 5),
                    fk_AcId = item.AcDocument_Header?.Id,
                    EventDate = pcw1.SelectedDate.ToDateTime(),
                    fk_DetaiId = item.fk_DetaiId,
                    Description = txtDescription.Text,
                    Id = Guid.NewGuid()
                };
                var checkRecieve_Not = db.CheckPaymentEvent.First(t => t.fk_DetaiId == item.fk_DetaiId && t.ChEvent.ChEventCode == 6);
                switch (control.SelectedIndex)
                {
                    case 1:
                        if (cmbChangeState.SelectedIndex == 0)
                        {
                            en.fk_PreferentialId = item.fk_PreferentialId;
                            en.fk_MoeinId = item.fk_MoeinId;
                        }
                        else if (cmbChangeState.SelectedIndex == 2)
                        {
                            en.fk_PreferentialId = item.fk_PreferentialId;
                            en.fk_MoeinId = mus1.Find(t => (t.AdditionalEntity as AccountSearchClass).ColMoein == txtMoein.Text).Id;
                        }                      
                        break;
                    case 2:
                        if (cmbChangeState.SelectedIndex == 1)
                        {
                            en.fk_PreferentialId = checkRecieve_Not.fk_PreferentialId;
                            en.fk_MoeinId = checkRecieve_Not.fk_MoeinId;
                        }                        
                        break;                                    
                }
                db.CheckPaymentEvent.Add(en);
                x.Add(item);
                en.PaymentMoney_Detail = db.PaymentMoney_Detail.Find(en.fk_DetaiId);
                en.Preferential = db.Preferential.Find(en.fk_PreferentialId);
                en.Moein = db.Moein.Find(en.fk_MoeinId);
                checkPaymentEvents.Add(en);
            }
            foreach (CheckPaymentEvent item in x)
                checkPaymentEvents.Remove(item);
            if (!db.SafeSaveChanges())  return;

            Xceed.Wpf.Toolkit.MessageBox.Show("عملیات با موفقیت انجام شد.", "تغییر وضعیت");
            TabControlExt_SelectionChanged(null, null);
            btnCancel_Click(null, null);                            
            isCancel = true;                        
        }
        private bool GetError()
        {
            var haserror = false;
            datagrid.BorderBrush = new  System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));
            
            if (cmbChangeState.SelectedIndex == -1)//AcDocument_Details.Any(g => !viewModel.AllCommodities.Any(y => y.CommodityCode == g.CommodityCode)))
            {
                Sf_cmbChangeState.HasError = true;
                haserror = true;
            }
            if (datagrid.SelectedItems.Count == 0)//AcDocument_Details.Any(g => !viewModel.AllCommodities.Any(y => y.CommodityCode == g.CommodityCode)))
            {
                datagrid.BorderBrush = Brushes.Red;
                haserror = true;
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
        // تعریف توابع مورد نیاز از user32.dll
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        // کلیدهای مجازی
        const byte VK_F2 = 0x71; // کد کلید F2
        const byte VK_Down = 0x28; // کد مجازی برای کلید جهت پایین
        const uint KEYEVENTF_KEYUP = 0x0002; // نشان دهنده آزاد کردن کلید
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);
        private void datagrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }
        int tempSelectedIndex = -1;

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
            else if (!txtMoein.IsFocused && !txtPreferential.IsFocused)
            {
                switch (e.Key) 
                {
                    case Key.F1:
                        control.SelectedIndex = 0;
                        break;
                    case Key.F2:
                        control.SelectedIndex = 1;
                        break;
                    case Key.F3:
                        control.SelectedIndex = 2;
                        break;
                    case Key.F4:
                        control.SelectedIndex = 3;
                        break;
                    case Key.F5:
                        control.SelectedIndex = 4;
                        break;
                    case Key.F6:
                        control.SelectedIndex = 5;
                        break;
                    case Key.F7:
                        control.SelectedIndex = 6;
                        break;
                }
            }
        }

        bool isCancel = true;
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (isCancel && SearchTermTextBox.Text.Trim()=="")
            {
                return;
            }
            if (sender != null && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            GStop1.Color = new System.Windows.Media.Color()
            {
                R = 244,
                G = 248,
                B = 255,
                A = 255
            };            
            
            datagrid.Visibility = Visibility.Visible;
            gridSetting.Visibility = gridConfirm.Visibility = Visibility.Visible;
            Sf_txtMoein.HasError = Sf_txtPreferential.HasError= Sf_cmbChangeState.HasError = false;
            datagrid.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));
            txtMoein.Text= string.Empty;
            txtPreferential.Text= string.Empty;
            txtDescription.Text= string.Empty;
            txbMoein.Text= string.Empty;
            txbPreferential.Text= string.Empty;
            //txtCodeAcDocument_Detail.Text = (en.AcDocument_DetailCode + 1).ToString();

            txtMoein.Focus();
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            //datagrid.TableSummaryRows.Clear();
            //datagrid.SearchHelper.SearchText = string.Empty;
            testsearch.Text = "جستجو...";
            SearchTermTextBox.Text = "";
            txtCount.Text = "";
            txtSumPrice.Text = "";
            datagrid.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));

            isCancel = true;
        }

        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if(datagrid.SelectedItems.Count!=0)
            {
                txtCount.Text = datagrid.SelectedItems.Count.ToString();
                txtSumPrice.Text = datagrid.SelectedItems.Sum(t => (t as CheckPaymentEvent).PaymentMoney_Detail.Price).ToString();
            }
            else
            {
                txtCount.Text = txtSumPrice.Text = "";
            }
            return;
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() == string.Empty)
            {
                if (datagrid.SearchHelper.SearchText.Trim() != "")
                {

                }
                else
                    return;
            }
            try
            {
                if (datagrid.Visibility == Visibility.Visible)
                {
                    datagrid.SelectedIndex = -1;
                    datagrid.SearchHelper.Search(SearchTermTextBox.Text);
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    if (InputLanguageManager.Current.CurrentInputLanguage.Name != "fa-IR")
                    {
                        decimal ds = 0;
                        if (decimal.TryParse(SearchTermTextBox.Text.Trim().Replace(",", ""), out ds) && ds >= 0)
                        {

                            int temp = SearchTermTextBox.SelectionStart;
                            SearchTermTextBox.TextChanged -= SearchTermTextBox_TextChanged;
                            SearchTermTextBox.Text = string.Format("{0:#,###}", ds);
                            if (SearchTermTextBox.SelectionStart != temp)
                                SearchTermTextBox.SelectionStart = temp + 1;
                            SearchTermTextBox.TextChanged += SearchTermTextBox_TextChanged;
                        }
                    }
                    datagrid.SelectedIndex = -1;
                    if (SearchTermTextBox.Text.Trim() == "")
                    {
                        dataPager.Visibility = Visibility.Visible;
                        datagrid.SearchHelper.ClearSearch();
                        var g = dataPager.Source;
                        dataPager.Source = null;
                        dataPager.Source = g;
                    }
                    else
                    {
                        //dataPager.Visibility = Visibility.Collapsed;
                        datagrid.SearchHelper.Search(SearchTermTextBox.Text);
                        //datagridSearch.View.Refresh();

                        //var h2 = FirstLevelNestedGrid.SearchHelper.GetSearchRecords();
                        //var h1 = datagridSearch.SearchHelper.GetSearchRecords();

                        /*foreach (AcDocument_Header item in datagridSearch.DetailsViewDefinition)
                        {
                            if(item.AcDocument_Detail.Count!=0)
                            {

                            }
                            else
                            {

                            }
                        }*/
                        //datagridSearch.SearchHelper.Search(SearchTermTextBox.Text);
                    }
                }
                //if (SearchTermTextBox.Text == "")
                //    RefreshDataGridForSetPersianNumber();                
            }
            catch(Exception ex)
            {
            }
            Mouse.OverrideCursor = null;
        }

        private void txtNoDocumen_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtCodeAcDocument_Detail_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }
        public static Window window;
        private void txtDoumentType_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void txtDoumentType_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void DataPager_PageIndexChanging(object sender, Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangingEventArgs e)
        {
            var ex = datagrid.View.FilterPredicates;
            
            var db = new ColDbEntities1();
            //db.AcDocument_Detail.Where(ex)
            var count = db.CheckPaymentEvent.Count();
            var F = db.CheckPaymentEvent.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                checkPaymentEvents[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "چک های پرداختی");
            MainWindow.Current.tabcontrol.Items.Remove(item);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Dispose();
            }));
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
            if(window!=null)
            {
                if ((window as winSearch).ParentTextBox is CheckPaymentEvent)
                {
                    var y = (window as winSearch).ParentTextBox as CheckPaymentEvent;
                    //((datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell).Content as FrameworkElement).DataContext = null;
                    //((datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell).Content as FrameworkElement).DataContext = y;
                    var detail = y;
                    var v = datagrid.SelectionController.CurrentCellManager.CurrentCell;
                    if ((window as winSearch)?.MuText != null)
                    {
                        datagrid.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //MMM
                            var th = new Thread(() =>
                            {
                                Thread.Sleep(100);
                                Dispatcher.Invoke(() =>
                                {
                                    var i = 1;
                                    if (v.ColumnIndex == 2)
                                        i++;
                                    if (datagrid.SelectedIndex == -1)
                                    {
                                        datagrid.GetAddNewRowController().CommitAddNew();
                                        datagrid.View.Refresh();
                                        datagrid.SelectedIndex = datagrid.GetLastRowIndex() - 1;
                                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(v.RowIndex - 1, v.ColumnIndex + i));
                                    }
                                    else
                                    {
                                        datagrid.View.Refresh();
                                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(v.RowIndex, v.ColumnIndex + i));
                                    }
                                    //MMM
                                    datagrid.IsHitTestVisible = true;
                                });
                            });
                            th.Start();
                            //datagrid.SelectCells(datagrid.GetRecordAtRowIndex(datagrid.SelectedIndex-1), datagrid.Columns[1], datagrid.GetRecordAtRowIndex(datagrid.SelectedIndex), datagrid.Columns[2]);
                        }));
                    }
                }
                else if ((window as winSearch).ParentTextBox is TextBox textBox && textBox.Tag != null && textBox.Tag.ToString() != "True")
                {
                    if (textBox.Name == "txtMoein")
                    {
                        txbMoein.Text = ((textBox.Tag as Mu).AdditionalEntity as AccountSearchClass).MoeinName;
                    }
                    else
                    {
                        txbPreferential.Text = (textBox.Tag as Mu).Name;
                    }
                    datagrid.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(100);
                        TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                        request.Wrapped = true;
                        textBox.MoveFocus(request);
                    }), DispatcherPriority.Background);
                }
            }
            window = null;
        }

        private void pcw1_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            txbCalender.Text = pcw1.SelectedDate.ToString();

        }

        private void Pcw1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true;
        }

        private void Fill()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var db = new ColDbEntities1();
            checkPaymentEvents.Clear();
            foreach (var item in db.CheckPaymentEvent.Include(y => y.PaymentMoney_Detail).Include(u => u.PaymentMoney_Detail.PaymentMoneyHeader).AsNoTracking().ToList().GroupBy(u=>u.fk_DetaiId).Select(g => g.OrderByDescending(u => u.Indexer).First()))
            {
                /*foreach (var item2 in item.CheckPaymentEvents)
                {
                    SetAccountName(db, item2);
                }*/
                checkPaymentEvents.Add(item);
            }
            Mouse.OverrideCursor = null;
        }
        private void RefreshDataGridForSetPersianNumber()
        {
            var wy = datagrid.Template;
            var uy = datagrid.ItemsSource;
            datagrid.Template = null;
            datagrid.ItemsSource = null;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                datagrid.Template = wy;
               datagrid.ItemsSource = uy;
            }), DispatcherPriority.Render);
        }

        private void AcDocument_Details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //var detail = checkPaymentEvents.LastOrDefault();
            //if (detail == null)
            //    return;
            //if ((Keyboard.IsKeyDown(Key.Enter) || datagrid.SelectedIndex != -1 || CurrentRowColumnIndex.ColumnIndex != 0) && detail.MoneyType != 3 && detail.ColeMoein == null && detail.PreferentialCode == null)
            //{
            //    datagrid.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        checkPaymentEvents.Remove(detail);
            //    }));
            //}
            //datagrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    CalDebCre();
            //}));
        }

        private void CalDebCre()
        {
            //if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex >= 4)
            //{
            //    var t = datagrid.ItemsSource;
            //    datagrid.ItemsSource = null;
            //    datagrid.ItemsSource = t;
            //}
            datagrid.View?.Refresh();
            return;

            //var c = AcDocument_Details.Sum(y => y.Creditor);
            //var d = AcDocument_Details.Sum(y => y.Debtor);
            //{
            //    datagrid.TableSummaryRows[0].SummaryColumns.Add(new GridSummaryColumn() {Name="hfgh", Format = "{Sum:N0}", MappingName = "Debtor", SummaryType = Syncfusion.Data.SummaryType.DoubleAggregate });

            //}
            //datagrid.TableSummaryRows.Clear();
            //var gridSummaryRow = new Syncfusion.UI.Xaml.Grid.GridSummaryRow();            
            //var Tafazol = AcDocument_Details.Sum(y => y.Debtor) - AcDocument_Details.Sum(y => y.Creditor);
            //if (Tafazol != null)
            //{
            //    var sign = Tafazol.Value >= 0 ? "" : "منفی";
            //    datagrid.TableSummaryRows.Add(new Syncfusion.UI.Xaml.Grid.GridSummaryRow() { Title = $"اختلاف : {string.Format("{0:#,###}", Math.Abs(Tafazol.Value))} {sign}" });
            //}
        }

        private void PART_AdvancedFilterControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
          
        }
        TextBox textBox1, textBox2;
        DatePicker datePicker1, datePicker2;
        private void PART_AdvancedFilterControl_GotFocus(object sender, RoutedEventArgs e)
        {
            var advance = sender as AdvancedFilterControl;
            if (datePicker1 == null)
            {
                var comboBoxes = advance.GetChildsOfType<ComboBox>();                
                var combo = comboBoxes[1];
                var grid = combo.Parent as Grid;
                grid.Children[0].Visibility = Visibility.Collapsed;
                grid.Children[1].Visibility = Visibility.Visible;
                (grid.Children[1] as TextBox).IsReadOnly = true;
                textBox1 = grid.Children[1] as TextBox;
                (comboBoxes[3].Parent as Grid).Children[0].Visibility = Visibility.Collapsed;
                (comboBoxes[3].Parent as Grid).Children[1].Visibility = Visibility.Visible;
                textBox2 = (comboBoxes[3].Parent as Grid).Children[1] as TextBox;
                ((comboBoxes[3].Parent as Grid).Children[1] as TextBox).IsReadOnly = true;
                datePicker1 = grid.Children[2] as DatePicker;
                datePicker2 = ((comboBoxes[3].Parent as Grid).Children[2]) as DatePicker;
                (MyPopupS.Parent as Grid).Children.Remove(MyPopupS);
                MyPopupS.Visibility = Visibility.Visible;
                grid.Children.Add(MyPopupS);
                (MyPopupE.Parent as Grid).Children.Remove(MyPopupE);
                MyPopupE.Visibility = Visibility.Visible;
                (comboBoxes[3].Parent as Grid).Children.Add(MyPopupE);
            }
            if (datePicker1?.IsMouseOver == true)
            {
                //grid.Children.RemoveAt(2);
                FieldInfo fieldInfo = typeof(DatePicker).GetField("_popUp", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                (fieldInfo.GetValue(datePicker1) as Popup).IsOpen = false;
                if (textBox1.Text == null || textBox1.Text == "" || persianCalendar.SelectedDate.ToDateTime() == System.DateTime.Today)
                {
                    persianCalendar.SelectedDate = new Mahdi.PersianDate(System.DateTime.Today.AddDays(-1));
                    persianCalendar.SelectedDate = new Mahdi.PersianDate(System.DateTime.Today);
                }
                MyPopupS.IsOpen = true;
            }
            if (datePicker2?.IsMouseOver == true)
            {
                //grid.Children.RemoveAt(2);
                FieldInfo fieldInfo = typeof(DatePicker).GetField("_popUp", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                (fieldInfo.GetValue(datePicker2) as Popup).IsOpen = false;
                if (textBox2.Text == null || textBox2.Text == "" || persianCalendarE.SelectedDate.ToDateTime() == System.DateTime.Today)
                {
                    persianCalendarE.SelectedDate = new Mahdi.PersianDate(System.DateTime.Today.AddDays(-1));
                    persianCalendarE.SelectedDate = new Mahdi.PersianDate(System.DateTime.Today);
                }
                MyPopupE.IsOpen = true;
            }
        }

        GridFilterControl gridFilterControl;
        private void PART_AdvancedFilterControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(textBox1!=null)
            {
                textBox1.Text = string.Empty;
                textBox1.TextChanged += TextBox1_TextChanged;
                textBox2.Text = string.Empty;
                textBox2.TextChanged += TextBox2_TextChanged; ;
            }
            datePicker1 = datePicker2 = null;
            //textBox1 = null;
            //textBox2 = null;
            var advance = sender as AdvancedFilterControl;            
            advance.Tag = true;
            FieldInfo fieldInfo = typeof(AdvancedFilterControl).GetField("gridFilterCtrl", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            gridFilterControl = (GridFilterControl)fieldInfo.GetValue(advance);
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            var h = textBox2.Text.Split('/');
            textBox2.TextChanged -= TextBox2_TextChanged;
            textBox2.Text = $"{h[2]}/{h[1]}/{h[0]}";
        }

        private void TextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            var h = textBox1.Text.Split('/');
            textBox1.TextChanged -= TextBox1_TextChanged;
            textBox1.Text = $"{h[2]}/{h[1]}/{h[0]}";
        }
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        // تعریف ثابت‌ها
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;  // برای نگه داشتن کلیک راست
        private const int MOUSEEVENTF_RIGHTUP = 0x10;    // برای رها کردن کلیک راست

        // تابع برای ارسال رویداد موس از کتابخانه user32.dll
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        static void LeftDoubleClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

            System.Threading.Thread.Sleep(50);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        static void RightClick()
        {
            // کلیک راست را نگه دارید
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);

            // کلیک راست را رها کنید
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }

        private void persianCalendar_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            MyPopupS.IsOpen = false;
            if (datePicker1 == null && datagrid.Visibility == Visibility.Visible)
            {
                (datagrid.SelectedItem as PaymentMoney_Detail).Date = persianCalendar.SelectedDate.ToDateTime();
                datagrid.IsHitTestVisible = true;
                datagrid.View.Refresh();
                return;
            }
            datePicker1.SelectedDate = persianCalendar.SelectedDate.ToDateTime();
            var persian = new System.Globalization.PersianCalendar();
            var h = $"{persian.GetYear(datePicker1.SelectedDate.Value)}/{persian.GetMonth(datePicker1.SelectedDate.Value)}/{persian.GetDayOfMonth(datePicker1.SelectedDate.Value)}";
            ((MyPopupS.Parent as Grid).Children[1] as TextBox).Text = h;
        }

        private void persianCalendarE_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            if (MyPopupE.IsOpen)
                datagrid.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(80);
                    MyPopupE.IsOpen = false;
                }));
            datePicker2.SelectedDate = persianCalendarE.SelectedDate.ToDateTime();
            var persian = new System.Globalization.PersianCalendar();
            var h = $"{persian.GetYear(datePicker2.SelectedDate.Value)}/{persian.GetMonth(datePicker2.SelectedDate.Value)}/{persian.GetDayOfMonth(datePicker2.SelectedDate.Value)}";
            ((MyPopupE.Parent as Grid).Children[1] as TextBox).Text = h;
        }
        bool rl1, rl2 = false;

        private void persianCalendarE_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rl2 = true;
            RightClick();
        }       

        bool StateLoadView = false;

        private void txtDescription_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void txtMoein_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                txtMoein.Tag = true;
                ShowSearchMoein(txtMoein);
            }
        }

        private winSearch ShowSearchMoein(dynamic y,Window owner= null)
        {            
            var win = new winSearch(mus1);
            win.Closed += (yf, rs) =>
            {
                datagrid.IsHitTestVisible = true;
            };
            win.Width = 640;
            win.datagrid.Columns[0].HeaderText = "نام";
            win.datagrid.Columns[1].HeaderText = "کل";
            win.datagrid.Columns[0].Width = 255;
            win.datagrid.Columns[1].Width = 100;
            win.datagrid.Columns.MoveTo(0, 1);
            win.datagrid.Columns.Add(new GridTextColumn() { TextAlignment = TextAlignment.Center, HeaderText = "معین", MappingName = "AdditionalEntity.Moein", Width = 100, AllowSorting = true });
            win.datagrid.Columns.Add(new GridTextColumn() { TextAlignment = TextAlignment.Center, HeaderText = "نام", MappingName = "AdditionalEntity.MoeinName", AllowSorting = true, ColumnSizer= GridLengthUnitType.AutoWithLastColumnFill });
            win.datagrid.AllowResizingColumns = true;
            if (owner == null)
                win.Tag = this;
            else
                win.Tag = owner;
            if (owner == null)
                owner = MainWindow.Current;
            win.ParentTextBox = y;
            win.SearchTermTextBox.Text = "";
            win.SearchTermTextBox.Select(1, 0);
            win.Owner = owner;
            window = win;
            win.Show();
            win.Focus();
            return win;
        }

        private void txtPreferential_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                txtPreferential.Tag = true;
                ShowSearchPreferential(txtPreferential);
            }
        }

        private winSearch ShowSearchPreferential(dynamic y, Window owner = null)
        {
            var win = new winSearch(mus2);
            win.Closed += (yf, rs) =>
            {
                datagrid.IsHitTestVisible = true;
            };
            win.datagrid.Columns.Add(new GridTextColumn() { TextAlignment = TextAlignment.Center, HeaderText = "گروه", MappingName = "Name2", Width = 150, AllowSorting = true });
            win.Width = 640;
            if (owner == null)
                win.Tag = this;
            else
                win.Tag = owner;
            if (owner == null)
                owner = MainWindow.Current;
            win.ParentTextBox = y;
            win.SearchTermTextBox.Text = "";
            win.SearchTermTextBox.Select(1, 0);
            win.Owner = MainWindow.Current;
            window = win;
            win.Show();
            win.Focus();
            return win;
        }

        private void datagrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentCell = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
            if (currentCell != null)
            {
             
            }           
        }

        private void txtMoein_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtMoein.Text == "")
            {
                txbMoein.Text = string.Empty;
                return;
            }
            var mu = mus1.Find(t => (t.AdditionalEntity as AccountSearchClass).ColMoein == txtMoein.Text);
            if (mu == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("چنین کل و معینی وجود ندارد!");
                txtMoein.Text = txbMoein.Text = string.Empty;
            }
            else
            {
                txtMoein.Tag = mu;
                txbMoein.Text = (mu.AdditionalEntity as AccountSearchClass).MoeinName;
            }
        }

        private void txtPreferential_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtPreferential.Text == "")
            {
                txbPreferential.Text = string.Empty;
                return;
            }
            var mu = mus2.Find(t => t.Value == txtPreferential.Text);
            if (mu == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("چنین تفضیلی وجود ندارد!");
                txtPreferential.Text = txbPreferential.Text = string.Empty;
            }
            else
            {
                txtPreferential.Tag = mu;
                txbPreferential.Text = mu.Name;
            }
        }

        private void ComboBoxAdv_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var comboBoxAdv = sender as ComboBoxAdv; 
            switch(e.Key) 
            {
                case Key.NumPad1:
                case Key.D1:                   
                    comboBoxAdv.SelectedIndex = 0;
                    break;
                case Key.NumPad2:
                case Key.D2:
                    comboBoxAdv.SelectedIndex = 1;
                    break;
                case Key.NumPad3:
                case Key.D3:
                    comboBoxAdv.SelectedIndex = 2;
                    break;
                case Key.NumPad4:
                case Key.D4:
                    comboBoxAdv.SelectedIndex = 3;
                    break;
            }
        }

        private void datagrid_SelectionChanging(object sender, GridSelectionChangingEventArgs e)
        {
            tempSelectedIndex = datagrid.SelectedIndex;
        }

        private void MyPopupS_Closed(object sender, EventArgs e)
        {
            if (datePicker1 == null && datagrid.Visibility == Visibility.Visible)
            {
                datagrid.IsHitTestVisible = true;
            }
        }

        private void dataPager_PageIndexChanged(object sender, Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() != string.Empty)
            {
            }
        }
        
        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            var win = new winSettingCode() { Width = 460 };
            win.grid.Width = 435;
            var db = new ColDbEntities1();
            //var exist = false;
            //if (db.CodeSetting.Any(t => t.Name == "MoeinCodeTransferLCheckPayment"))
            //{
            //    exist = true;
            //}
            var exist = false;
            if (db.CodeSetting.Any(t => t.Name == "MoeinCodeCheckPayment"))
            {
                exist = true;
            }
            GroupBox groupBox = SettingDefinitionGroupBox(win, db, exist, "نوع وجه چک", "ColCodeCheckPayment", "MoeinCodeCheckPayment", null);
            Dispatcher.BeginInvoke(new Action(async () =>
            {
                groupBox.GetChildOfType<TextBox>().Focus();
            }), DispatcherPriority.Render);
            win.stack.Children.Add(groupBox);
            var groupBox2 = SettingDefinitionGroupBox(win, db, exist, "نوع وجه نقد", "ColCodeMoneyPayment", "MoeinCodeMoneyPayment", "PreferentialCodeMoneyPayment");
            win.stack.Children.Add(groupBox2);

            groupBox2 = SettingDefinitionGroupBox(win, db, exist, "نوع وجه تخفیف", "ColCodeDiscountPayment", "MoeinCodeDiscountPayment", "PreferentialCodeDiscountPayment");
            win.stack.Children.Add(groupBox2);            

            win.ShowDialog();
        }

        private GroupBox SettingDefinitionGroupBox(winSettingCode win, ColDbEntities1 db, bool exist, string name, string str1, string str2, string str3)
        {
            var groupBox = new GroupBox() { Header = name };
            var stackPanel = new DockPanel();
            groupBox.Content = stackPanel;

            var keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add(str1, exist ? db.CodeSetting.First(i => i.Name == str1).Value : "");
            keyValuePairs.Add(str2, exist ? db.CodeSetting.First(i => i.Name == str2).Value : "");

            var textInputLayout = new SfTextInputLayout()
            {
                Tag = keyValuePairs,
                Hint = (str1 == "ColCodeCheckPayment" ? "کد کل و معین اسناد پرداختنی "
                : "کد کل و معین "),
                Width = 175
            };
            if (str1 == "ColCodeCheckPayment")
                textInputLayout.Width = 190;
            var textBox = new TextBox() { Text = exist ? keyValuePairs.ElementAt(0).Value + keyValuePairs.ElementAt(1).Value : "", Tag = true };
            textInputLayout.InputView = textBox;
            if (exist)
            {
                var mu = mus1.Find(t => (t.AdditionalEntity as AccountSearchClass).ColMoein == textBox.Text);
                textInputLayout.HelperText = (mu.AdditionalEntity as AccountSearchClass).MoeinName;
            }
            textBox.PreviewKeyDown += (s1, e1) =>
            {
                if (e1.Key == Key.F1)
                {
                    win.childWindow = ShowSearchMoein(s1, win);
                }
                else if (e1.Key == Key.Enter)
                {
                    TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                    request.Wrapped = true;
                    (s1 as TextBox).MoveFocus(request);
                }
            };
            textBox.LostFocus += (s1, e1) =>
            {
                var txt = s1 as TextBox;
                var sfTextInput = txt.GetParentOfType<SfTextInputLayout>();
                if (txt.Text == "")
                {
                    sfTextInput.HelperText = string.Empty;
                    return;
                }
                var mu = mus1.Find(t => (t.AdditionalEntity as AccountSearchClass).ColMoein == txt.Text);
                if (mu == null)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("چنین کل و معینی وجود ندارد!");
                    sfTextInput.HelperText = txt.Text = string.Empty;
                }
                else
                {
                    txt.Tag = mu;
                    sfTextInput.HelperText = (mu.AdditionalEntity as AccountSearchClass).MoeinName;
                    keyValuePairs = sfTextInput.Tag as Dictionary<string, string>;
                    keyValuePairs[keyValuePairs.ElementAt(0).Key] = mu.Value;
                    keyValuePairs[keyValuePairs.ElementAt(1).Key] = (mu.AdditionalEntity as AccountSearchClass).Moein;
                }
            };
            stackPanel.Children.Add(textInputLayout);
            if (str3 != null)
            {
                textInputLayout = new SfTextInputLayout() { Tag = str3, Hint = "کد تفضیل", Margin = new Thickness(10, 0, 10, 0) };
                textBox = new TextBox() { Text = exist ? db.CodeSetting.First(i => i.Name == str3).Value : "", Tag = true };
                textInputLayout.InputView = textBox;
                if (exist)
                {
                    var mu = mus2.Find(t => t.Value == textBox.Text);
                    textInputLayout.HelperText = mu.Name;
                }
                textBox.PreviewKeyDown += (s1, e1) =>
                {
                    if (e1.Key == Key.F1)
                    {
                        win.childWindow = ShowSearchPreferential(s1, win);
                    }
                    else if (e1.Key == Key.Enter)
                    {
                        TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                        request.Wrapped = true;
                        (s1 as TextBox).MoveFocus(request);
                    }
                };
                textBox.LostFocus += (s1, e1) =>
                {
                    var txt = s1 as TextBox;
                    var sfTextInput = txt.GetParentOfType<SfTextInputLayout>();
                    if (txt.Text == "")
                    {
                        sfTextInput.HelperText = string.Empty;
                        return;
                    }
                    var mu = mus2.Find(t => t.Value == txt.Text);
                    if (mu == null)
                    {
                        Xceed.Wpf.Toolkit.MessageBox.Show("چنین تفضیلی وجود ندارد!");
                        sfTextInput.HelperText = txt.Text = string.Empty;
                    }
                    else
                    {
                        txt.Tag = mu;
                        sfTextInput.HelperText = mu.Name;
                    }
                };
                stackPanel.Children.Add(textInputLayout);
            }
            else
            {
                stackPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            return groupBox;
        }

        private void cmbChangeState_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            isCancel = false;
            if (cmbChangeState.SelectedIndex != -1)
            {
                switch (cmbChangeState.SelectedIndex)
                {
                    case 2:
                        var db = new ColDbEntities1();
                        var moein = db.CodeSetting.First(j => j.Name == "MoeinCodeDoneLCheckRecieve").Value;
                        var col = db.CodeSetting.First(j => j.Name == "ColCodeDoneLCheckRecieve").Value;
                        txtMoein.Text = col + moein;
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(30);
                            txtPreferential.Focus();
                        }), DispatcherPriority.Render);
                        txtMoein.Focus();
                        break;                    
                    default:
                        txtMoein.Text = "";
                        txtMoein.Focus();
                        break;
                }
            }
        }

        private void cmbChangeState_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(async ()=>
            {
                await Task.Delay(20);
                txtMoein.Focus();
               
            }), DispatcherPriority.Render);
            }
        }

        private void TabControlExt_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (control.SelectedItem == null || (tempSelectedIndex_TabControl == control.SelectedIndex && sender != null))
                return;
            (datagrid.Parent as TabItemExt).Content = null;
            (control.SelectedItem as TabItemExt).Content = datagrid;
            if (control.SelectedIndex == 0 || control.SelectedIndex == 3)
            {
                borderRirgt.Visibility = Visibility.Visible;
            }
            else
            {
                borderRirgt.Visibility = Visibility.Collapsed;
            }
            if (control.SelectedIndex == 0)
            {
                datagrid.Columns[0].IsHidden = true;
                datagrid.Columns[1].IsHidden = false;
            }
            else
            {
                datagrid.Columns[0].IsHidden = false;
                datagrid.Columns[1].IsHidden = true;
                item1.Visibility= item2.Visibility = item3.Visibility =  Visibility.Visible;
            }
            switch (control.SelectedIndex)
            {
                case 0:
                    dataPager.Source = null;
                    dataPager.Source = checkPaymentEvents;
                    datagrid.SelectedIndex = -1;
                    cmbChangeState.SelectedIndex = -1;
                    break;
                case 1:
                    dataPager.Source = null;
                    mini_checkPaymentEvents.Clear();
                    checkPaymentEvents.Where(u => u.ChEvent.ChEventCode == 6).ForEach(t => mini_checkPaymentEvents.Add(t));
                    dataPager.Source = mini_checkPaymentEvents;
                    datagrid.SelectedIndex = -1;
                    item1.Visibility = Visibility.Collapsed;
                    cmbChangeState.SelectedIndex = -1;
                    break;               
                case 2:
                    dataPager.Source = null;
                    mini_checkPaymentEvents.Clear();
                    checkPaymentEvents.Where(u => u.ChEvent.ChEventCode == 7).ForEach(t => mini_checkPaymentEvents.Add(t));
                    dataPager.Source = mini_checkPaymentEvents;
                    datagrid.SelectedIndex = -1;
                    //item3.Visibility = item4.Visibility = item5.Visibility = item6.Visibility = Visibility.Collapsed;
                    item2.Visibility= Visibility.Collapsed;
                    cmbChangeState.SelectedIndex = -1;
                    break;               
                case 3:
                    dataPager.Source = null;
                    mini_checkPaymentEvents.Clear();
                    checkPaymentEvents.Where(u => u.ChEvent.ChEventCode == 5).ForEach(t=> mini_checkPaymentEvents.Add(t));
                    dataPager.Source = mini_checkPaymentEvents;
                    datagrid.SelectedIndex = -1;
                    datagrid.Columns[0].IsHidden = true;
                    cmbChangeState.SelectedIndex = -1;
                    break;
            }
            tempSelectedIndex_TabControl = control.SelectedIndex;
        }
        int tempSelectedIndex_TabControl = -1;
        private void datagrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double rowHeight = 30; // ارتفاع هر سطر (این مقدار ممکن است بسته به طراحی تغییر کند)

            // ارتفاع موجود در grid را محاسبه کنید
            double availableHeight = datagrid.ActualHeight;

            // محاسبه تعداد سطرهایی که در صفحه جا می‌شوند
            int visibleRows = (int)(availableHeight / rowHeight);

            // تنظیم PageSize بر اساس تعداد سطرهای محاسبه شده
            if (visibleRows > 0)
            {
                var y = dataPager.PageSize;
                dataPager.PageSize = visibleRows - 4;
                if (dataPager.PageSize != y)
                {
                    var g = dataPager.Source;
                    dataPager.Source = null;
                    dataPager.Source = g;
                    dataPager.Visibility = Visibility.Visible;
                    datagrid.SearchHelper.ClearSearch();
                    SearchTermTextBox.Text = "";
                }
            }
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void persianCalendarE_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!rl2)
                e.Handled = true;
            rl2 = false;
        }

        private void persianCalendar_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!rl1)
                e.Handled = true;
            rl1 = false;
        }

        private void persianCalendar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            rl1 = true;
            RightClick();
        }
    }
}
