using Mahdi.PersianDateControls;
using PersianCalendarWPF;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Windows.Controls.Input;
using Syncfusion.Windows.Shared;
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
    public partial class usrStorageBetweenTransfer : UserControl,ITabForm,ITabEdidGrid,IDisposable
    {
        public bool DataGridIsFocused
        {
            get
            {
                return datagrid.IsFocused;
            }
        }
        StorageReceiptViewModel StorageReceiptViewModel;
        List<Mu> mus2 = new List<Mu>();
        public usrStorageBetweenTransfer()
        {
            StorageReceipt_Details = new ObservableCollection<StorageReceipt_Detail>();
            StorageReceiptHeaders = new ObservableCollection<StorageReceiptHeader>();
            InitializeComponent();
            StorageReceiptViewModel = Resources["viewmodel"] as StorageReceiptViewModel;
            StorageReceiptViewModel.StorageReceipt_Details.CollectionChanged += StorageReceipt_Details_CollectionChanged;
            txbCalender.Text = pcw1.SelectedDate.ToString();
        }

        public void Dispose()
        {
            if (StorageReceiptViewModel == null)
                return;
            StorageReceiptHeaders.Clear();
            StorageReceipt_Details.Clear();
            DataContext = null;
            StorageReceiptViewModel.StorageReceipt_Details.CollectionChanged -= StorageReceipt_Details_CollectionChanged;
            StorageReceiptViewModel = null;
            GC.Collect();
        }

        Brush brush = null;
        public ObservableCollection<StorageReceipt_Detail> StorageReceipt_Details { get; set; }
        public ObservableCollection<StorageReceiptHeader> StorageReceiptHeaders { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                if ((sender as TextBox).Name == "txtNoDocumen")
                {
                    cmbType.Focus();
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
            if ((sender as TextBox).Name != "cmbType")
                e.Handled = !IsTextAllowed(e.Text);            
        }
        private static readonly Regex _regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }
        bool AddedMode = true;
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var db = new ColDbEntities1();

            var temp1 = cmbType.SelectedIndex;
            var temp2 = cmbTypeT.SelectedIndex;
            cmbType.ItemsSource = db.CodingReceiptTypes.ToList();
            if (temp1 == -1)
                cmbType.SelectedItem = (cmbType.ItemsSource as List<CodingReceiptTypes>).FirstOrDefault(t => t.IsDefault == true);
            else
                cmbType.SelectedIndex = temp1;
            cmbTypeT.ItemsSource = db.CodingTypesTransfer.ToList();
            if (temp2 == -1)
                cmbTypeT.SelectedItem = (cmbTypeT.ItemsSource as List<CodingTypesTransfer>).FirstOrDefault(t => t.IsDefault == true);
            else
                cmbTypeT.SelectedIndex = temp2;
            mus2.Clear();
            var commodities = db.Commodity.ToList();            
            foreach (var item in commodities)
            {                
                mus2.Add(new Mu()
                {
                    Id= item.Id,
                    Name = $"{item.Name}",
                    Value = $"{item.Code}",
                    Name2 = item.Unit.Name
                });
            }
            cmbType.SelectedIndex = 0;
            if (temp1 > 0)
                cmbType.SelectedIndex = temp1;
            if (temp2 > 0)
                cmbTypeT.SelectedIndex = temp2;
            if (AddedMode)
            {               
                StorageReceipt_Details = StorageReceiptViewModel.StorageReceipt_Details;                
            }            
            datagrid.SearchHelper.AllowFiltering = true;
            cmbType.Focus();
            isCancel = true;
        }

        private static void SetAccountName(ColDbEntities1 db, StorageReceipt_Detail item2)
        {/*
            var strings = item2.AcCode.Split('-');
            var moein = int.Parse(strings[0]);
            var tafzil = int.Parse(strings[2]);
            item2.AccountName = $"{db.Preferential.First(i => i.PreferentialCode == tafzil).PreferentialName}-{db.Moein.First(p => p.MoeinCode == moein).MoeinName}";*/
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            bool haserror = false;
            haserror = GetError();

            if (haserror)
                return;
            var db = new ColDbEntities1();

            var codingReceipt = db.CodingReceiptTypes.FirstOrDefault(y => y.Name == cmbType.Text);
            if (codingReceipt == null)
            {
                Sf_txtDoumentType.HasError = true;
                Sf_txtDoumentType.ErrorText = "این نوع رسید وجود ندارد!";
                return;
            }
            var codingTransfer = db.CodingTypesTransfer.FirstOrDefault(y => y.Name == cmbTypeT.Text);
            if (codingTransfer == null)
            {
                Sf_txtDoumentTypeT.HasError = true;
                Sf_txtDoumentTypeT.ErrorText = "این نوع حواله وجود ندارد!";
                return;
            }
            StorageReceiptHeader e_Edidet = null;
            var code = int.Parse(txtStorage.Text);
            var storage = db.Storage.First(t => t.StorageCode == code);
            var code2 = int.Parse(txtStorageDestination.Text);
            var storage2 = db.Storage.First(t => t.StorageCode == code2);

            var storageReceiptHeader = new StorageReceiptHeader()
            {
                Id = Guid.NewGuid(),
                Date = pcw1.SelectedDate.ToDateTime(),
                Serial = db.StorageReceiptHeader
    .Select(u => (long?)u.Serial)
    .DefaultIfEmpty(1)
    .Max().Value+1,
                NoDoument = db.StorageReceiptHeader
    .Select(u => (long?)u.NoDoument)
    .DefaultIfEmpty(1)
    .Max().Value+1,
                Description = txtDescription.Text,
                CodingReceiptTypes = codingReceipt,
                Storage = storage2,
            };
            int index = 0;
            foreach (var item in StorageReceipt_Details)
            {
                index++;
                var en = new StorageReceipt_Detail()
                {
                    StorageReceiptHeader = storageReceiptHeader,
                    fk_CommodityId = item.Commodity.Id,
                    Value = item.Value,
                    Indexer = index,
                    Id = Guid.NewGuid()
                };
                db.StorageReceipt_Detail.Add(en);
            }
            db.StorageReceiptHeader.Add(storageReceiptHeader);


            var storageTransferHeader = new StorageTransferHeader()
            {
                Id = Guid.NewGuid(),
                Date = pcw1.SelectedDate.ToDateTime(),
                Serial = db.StorageTransferHeader
    .Select(u => (long?)u.Serial)
    .DefaultIfEmpty(1)
    .Max().Value+1,
                NoDoument = db.StorageTransferHeader
    .Select(u => (long?)u.NoDoument)
    .DefaultIfEmpty(1)
    .Max().Value+1,
                Description = txtDescription.Text,
                CodingTypesTransfer = codingTransfer,
                Storage = storage,
            };
            index = 0;
            foreach (var item in StorageReceipt_Details)
            {
                index++;
                var en = new StorageTransfer_Detail()
                {
                    StorageTransferHeader = storageTransferHeader,
                    fk_CommodityId = item.Commodity.Id,
                    Value = item.Value,
                    Indexer = index,
                    Id = Guid.NewGuid()
                };
                db.StorageTransfer_Detail.Add(en);
            }
            db.StorageTransferHeader.Add(storageTransferHeader);


            if (!db.SafeSaveChanges()) return;            
            
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            if (StorageReceipt_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    StorageReceipt_Details.Clear();
                }));
                RefreshDataGridForSetPersianNumber();
            }
            //datagrid.TableSummaryRows.Clear();
            SearchTermTextBox.Text = "";

            this.gifImage.Visibility = Visibility.Visible;
            var gifImage = new BitmapImage(new Uri("pack://application:,,,/Images/AddDataLarge.gif"));
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(this.gifImage, gifImage.UriSource);
            var th = new Thread(() =>
            {
                Thread.Sleep(2570);
                Dispatcher.Invoke(() =>
                {
                    this.gifImage.Visibility = Visibility.Collapsed;
                });
            });
            th.Start();
            Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت حواله بین انبار");
            btnCancel_Click(null, null);
            this.gifImage.Visibility = Visibility.Collapsed;

            cmbType.SelectedIndex = 0;
            cmbType.Focus();

            isCancel = true;
        }
        private bool GetError()
        {
            var haserror = false;
            datagrid.BorderBrush = new  System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));

            if (cmbType.Text.Trim() == "")
            {
                Sf_txtDoumentType.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtDoumentType.HasError = false;
                Sf_txtDoumentType.ErrorText = "";
            }
            if (cmbTypeT.Text.Trim() == "")
            {
                Sf_txtDoumentTypeT.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtDoumentTypeT.HasError = false;
                Sf_txtDoumentTypeT.ErrorText = "";
            }
            if (txtStorage.Text.Trim() == "")
            {
                Sf_txtStorage.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtStorage.HasError = false;
                Sf_txtStorage.ErrorText = "";
            }
            if (txtStorageDestination.Text.Trim() == "")
            {
                Sf_txtStorageDestination.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtStorageDestination.HasError = false;
                Sf_txtStorageDestination.ErrorText = "";
            }
            if (StorageReceipt_Details.Count == 0)//StorageReceipt_Details.Any(g => !viewModel.AllCommodities.Any(y => y.CommodityCode == g.CommodityCode)))
            {
                datagrid.BorderBrush = Brushes.Red;
                haserror = true;
            }
            else if (StorageReceipt_Details.Any(t => t.Commodity == null || t.Value == 0 ))
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
        const uint KEYEVENTF_KEYUP = 0x0002; // نشان دهنده آزاد کردن کلید
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int X, int Y);
        private void datagrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {                
                if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex == 0)
                {
                    dynamic y = null;
                    var element = (datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell)
                            .Content as FrameworkElement;
                    y = element.DataContext;
                    if (datagrid.SelectedIndex == -1 || element is TextBlock)
                    {
                        if (y == null || y.Commodity != null)
                        {
                            var cell = datagrid.SelectionController.CurrentCellManager.CurrentCell.Element;
                            keybd_event(VK_F2, 0, 0, UIntPtr.Zero); // فشار دادن کلید
                            Thread.Sleep(50); // تاخیر برای شبیه‌سازی فشار دادن
                            keybd_event(VK_F2, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // آزاد کردن کلید
                            var th = new Thread(() =>
                            {
                                Thread.Sleep(10);
                                Dispatcher.Invoke(() =>
                                datagrid_PreviewKeyDown(sender, e));
                            });
                            th.Start();
                            return;
                        }
                    }
                    var win = new winSearch(mus2);
                    win.Closed += (yf, rs) =>
                    {
                        datagrid.IsHitTestVisible = true;
                    };
                    win.datagrid.Columns.Add(new GridTextColumn() {TextAlignment= TextAlignment.Center, HeaderText = "واحد اندازه گیری", MappingName = "Name2", Width = 150, AllowSorting = true });
                    win.Width = 640;                    
                    win.Tag = this;
                    win.ParentTextBox = y;
                    win.SearchTermTextBox.Text = "";
                    win.SearchTermTextBox.Select(1, 0);
                    win.Owner = MainWindow.Current;
                    window = win;
                    win.Show();
                    win.Focus();
                    datagrid.IsHitTestVisible = false;
                }
            }           
        }     
        private void datagrid_CurrentCellEndEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs e)
        {
            isCancel = false;
            CalDebCre();

            if (window == null && datagrid.GetRecordAtRowIndex(e.RowColumnIndex.RowIndex) is StorageReceipt_Detail StorageReceipt_Detail)
            {
                if ((CurrentCellText ?? "") != "")
                {                   
                    if(e.RowColumnIndex.ColumnIndex == 0)
                    {
                        var db = new ColDbEntities1();
                        var mu = mus2.Find(t => t.Value == CurrentCellText);
                        if (mu == null)
                        {

                        }
                        else
                        {
                            var commodity = db.Commodity.Find(mu.Id);
                            StorageReceipt_Detail.Commodity = commodity;
                        }
                    }
                }
            }
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                var th = new Thread(() =>
                {
                    Thread.Sleep(30);
                    Dispatcher.Invoke(new Action(() =>
                    SetEnterToNextCell(this.CurrentRowColumnIndex)));
                });
                th.Start();
            }
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
        }

        private void cmbType_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(50);
                    txtStorage.Focus();
                }));
                return;
            }
            cmbType.SelectedIndex = -1;
        }
        bool isCancel = true;
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (AddedMode&&isCancel)
            {
                return;
            }
            if (sender != null && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            GStop1.Color = new Color()
            {
                R = 244,
                G = 248,
                B = 255,
                A = 255
            };            
            
            datagrid.Visibility = Visibility.Visible;
            gridConfirm.Visibility = Visibility.Visible;
            cmbType.IsReadOnly = false;
            Sf_txtDoumentType.HasError = false;
            Sf_txtDoumentType.ErrorText = "";
            Sf_txtDoumentTypeT.HasError = false;
            Sf_txtDoumentTypeT.ErrorText = "";
            txtStorage.Text = string.Empty;
            txtStorageDestination.Text = string.Empty;
            txtDescription.Text = string.Empty;
            Sf_txtStorage.HasError = false;
            Sf_txtStorage.HelperText = "";
            Sf_txtStorageDestination.HasError = false;
            Sf_txtStorageDestination.HelperText = "";

            //txtCodeStorageReceipt_Detail.Text = (en.StorageReceipt_DetailCode + 1).ToString();

            cmbType.Focus();
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            //datagrid.TableSummaryRows.Clear();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";
            borderEdit.Visibility = Visibility.Hidden;
            cmbType.SelectedIndex = 0;
            datagrid.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));
            if (StorageReceipt_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    StorageReceipt_Details.Clear();
                }));
                RefreshDataGridForSetPersianNumber();
            }            
            isCancel = true;
        }

        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() == string.Empty)
            {
                if (datagrid.Visibility != Visibility.Visible)
                    return;
            }
            try
            {
                if (datagrid.Visibility == Visibility.Visible)
                {
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
                }
                if (SearchTermTextBox.Text == "")
                    RefreshDataGridForSetPersianNumber();                
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

        private void TxtCodeStorageReceipt_Detail_TextChanged(object sender, TextChangedEventArgs e)
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
            //db.StorageReceipt_Detail.Where(ex)
            var count = db.StorageReceipt_Detail.Count();
            var F = db.StorageReceipt_Detail.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                StorageReceipt_Details[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "حواله بین انبار");
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
            
        }

        public void SetNull()
        {
            if(window!=null&&(window as winSearch).ParentTextBox is StorageReceipt_Detail storage)
            {
                var y = (window as winSearch).ParentTextBox as StorageReceipt_Detail;
                //((datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell).Content as FrameworkElement).DataContext = null;
                //((datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell).Content as FrameworkElement).DataContext = y;
                var detail = y;                
                var v = datagrid.SelectionController.CurrentCellManager.CurrentCell;
                if ((window as winSearch)?.MuText != null)
                {
                    var db = new ColDbEntities1();
                    storage.Commodity = db.Commodity.Find((window as winSearch)?.MuText.Id);
                    datagrid.Dispatcher.BeginInvoke(new Action(() =>
                    {                        
                        //MMM
                        var th = new Thread(() =>
                        {
                            Thread.Sleep(100);
                            Dispatcher.Invoke(() =>
                            {
                                var i = 1;
                                if (v.ColumnIndex == 0)
                                    i++;
                                if (datagrid.SelectedIndex == -1)
                                {
                                    datagrid.GetAddNewRowController().CommitAddNew();
                                    datagrid.View.Refresh();
                                    datagrid.SelectedIndex = datagrid.GetLastRowIndex() - 1;
                                    if (datagrid.SelectedIndex != -1)
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

        private void datagrid_AddNewRowInitiating(object sender, Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs e)
        {
            /*
            var h = StorageReceiptViewModel.StorageReceipt_Details.FirstOrDefault(q => q.AcCode == ctext);
            if (h != null)
            {
                (e.NewObject as UtililtyCommodity).CommodityId = h.ID;
                (e.NewObject as UtililtyCommodity).CommodityCode = ctext;
                (e.NewObject as UtililtyCommodity).Discount = 0;
                (e.NewObject as UtililtyCommodity).Tax = h.Tax;
            }*/
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

        private void datagrid_RowValidated(object sender, RowValidatedEventArgs e)
        {
            //var detail = e.RowData as StorageReceipt_Detail;
            //if (datagrid.SelectedIndex!=-1&& detail.ColeMoein == null && detail.PreferentialCode == null && detail.Debtor == null && detail.Creditor == null && detail.Description == null)
            //{
            //    StorageReceipt_Details.Remove(detail);
            //    return;
            //}
            //var currentCell = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
            //if (window != null)
            //    (window as winSearch).ParentTextBox = detail;
            //if (currentCell?.ColumnIndex == 4 && (detail.Debtor ?? 0) != 0)
            //    detail.Creditor = null;
            //if (currentCell?.ColumnIndex == 5 && (detail.Creditor ?? 0) != 0)
            //    detail.Debtor = null;
        }

        private void StorageReceipt_Details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //var detail = StorageReceipt_Details.LastOrDefault();
            //if (detail == null)
            //    return;
            //if (detail.ColeMoein == null && detail.PreferentialCode == null && detail.Debtor == null && detail.Creditor == null && detail.Description == null)
            //{
            //    datagrid.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        StorageReceipt_Details.Remove(detail);
            //    }));
            //}
            //datagrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    CalDebCre();
            //}));
        }

        private void CalDebCre()
        {
            if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex >= 4)
            {
                var t = datagrid.ItemsSource;
                datagrid.ItemsSource = null;
                datagrid.ItemsSource = t;
            }
            datagrid.View?.Refresh();
        }

        private void cmbType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            isCancel = false;
            if (cmbType.SelectedIndex != -1)
                txtStorage.Focus();
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
        RowColumnIndex CurrentRowColumnIndex;
    
        private void datagrid_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
            if (SearchTermTextBox.Text != "")
            {
                datagrid.SearchHelper.ClearSearch();
                SearchTermTextBox.Text = "";
            }
            CurrentRowColumnIndex = e.RowColumnIndex;
            CurrentCellText = "";
        }
        string CurrentCellText;
        private void datagrid_CurrentCellValueChanged(object sender, CurrentCellValueChangedEventArgs e)
        {
            var textBox = (datagrid.SelectionController.CurrentCellManager?.CurrentCell.Element as GridCell).Content as TextBox;
            if (textBox.Text != "" && e.Record is StorageReceipt_Detail detail && detail.Commodity?.Code.ToString() != textBox.Text && !Keyboard.IsKeyDown(Key.Enter))
                CurrentCellText = textBox.Text;
        }

        private void datagrid_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (e.RowData is StorageReceipt_Detail detail)
            {
                var dataColumn = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
                var textBox = (dataColumn.Element as GridCell).Content as TextBox;
                if (textBox == null)
                    return;
                var u = textBox.Text == "" ? CurrentCellText : textBox.Text;                
                if (dataColumn.ColumnIndex == 0 && u != "")
                {
                    var mu = mus2.Find(t => t.Value == u);
                    if (mu == null)
                    {
                        //Xceed.Wpf.Toolkit.MessageBox.Show("چنین کالایی وجود ندارد!");
                        detail.Commodity = null;
                        textBox.Text = "";
                        CurrentCellText = "";
                        //e.IsValid = false;
                        //e.ErrorMessages.Add("Code", "چنین کالایی وجود ندارد!");
                    }
                }
            }
        }


        private void persianCalendarE_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!rl2)
                e.Handled = true;
            rl2 = false;
        }

        private void txtStorage_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void txtStorage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                cmbTypeT.Focus();                
                return;
            }
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void txtStorage_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtStorage.Text == "")
            {
                txtStorage.Text = string.Empty;
                Sf_txtStorage.HelperText = string.Empty;
                return;
            }
            var db = new ColDbEntities1();
            var code = int.Parse(txtStorage.Text);
            var mu = db.Storage.FirstOrDefault(t => t.StorageCode == code);
            if (mu == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("چنین کد انبار وجود ندارد!");
                txtStorage.Text = Sf_txtStorage.HelperText = string.Empty;
            }
            else
            {
                Sf_txtStorage.HelperText = mu.StorageName;
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(50);
                    cmbTypeT.Focus();
                }));
                
            }
        }

        private void txtDescription_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
            }
        }

        private void cmbType_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cmbType.SelectedIndex == -1)
                cmbType.Text = "";
        }

        private void txtStorage_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                var db = new ColDbEntities1();
                var list = db.Storage.ToList().Select(r => new Mu() { Name = r.StorageName, Value = r.StorageCode.ToString(),Name2=r.GroupStorage.GroupName }).ToList();
                var win = new winSearch(list);
                win.Closed += (yf, rs) =>
                {
                    datagrid.IsHitTestVisible = true;
                };
                win.datagrid.Columns.Add(new GridTextColumn() { TextAlignment = TextAlignment.Center, HeaderText = "گروه انبار", MappingName = "Name2", Width = 150, AllowSorting = true });
                win.Width = 640;
                win.Tag = this;
                win.ParentTextBox = sender;
                win.SearchTermTextBox.Text = "";
                win.SearchTermTextBox.Select(1, 0);
                win.Owner = MainWindow.Current;
                window = win;
                win.Show();
                win.Focus();
            }
        }
        private void txtStorageDestination_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                txtDescription.Focus();
                return;
            }
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void txtStorageDestination_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtStorageDestination.Text == "")
            {
                txtStorageDestination.Text = string.Empty;
                Sf_txtStorageDestination.HelperText = string.Empty;
                return;
            }
            var db = new ColDbEntities1();
            var code = int.Parse(txtStorageDestination.Text);
            var mu = db.Storage.FirstOrDefault(t => t.StorageCode == code);
            if (mu == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("چنین کد انبار وجود ندارد!");
                txtStorageDestination.Text = Sf_txtStorageDestination.HelperText = string.Empty;
            }
            else
            {
                Sf_txtStorageDestination.HelperText = mu.StorageName;
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(50);
                    txtDescription.Focus();
                }));

            }
        }

        private void cmbTypeT_LostFocus(object sender, RoutedEventArgs e)
        {
            if (cmbType.SelectedIndex == -1)
                cmbType.Text = "";
        }

        private void cmbTypeT_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(50);
                    txtStorageDestination.Focus();
                }));
                return;
            }
            cmbTypeT.SelectedIndex = -1;
        }

        private void cmbTypeT_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            isCancel = false;
            if (cmbTypeT.SelectedIndex != -1)
                txtStorageDestination.Focus();
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

        public void SetEnterToNextCell(RowColumnIndex? rowColumn = null)
        {
            var dataGrid = datagrid;

            // پیدا کردن سطر و ستون فعلی
            var currentCell = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
            if (currentCell != null)
            {
                int currentRowIndex = rowColumn == null ? currentCell.RowIndex : rowColumn.Value.RowIndex;
                int currentColumnIndex = rowColumn == null ? currentCell.ColumnIndex : rowColumn.Value.ColumnIndex;

                // افزایش اندیس ستون
                currentColumnIndex++;

                // اگر به انتهای ستون‌ها رسیدیم، به سطر بعد بروید
                if (currentColumnIndex >= dataGrid.Columns.Count)
                {
                    currentColumnIndex = 0; // به اولین ستون برگردید
                    currentRowIndex++; // به سطر بعد بروید
                }

                // اگر به انتهای سطرها رسیدیم، به اولین سطر برگردید
                if (currentRowIndex >= StorageReceipt_Details.Count + 2)
                {
                    currentRowIndex = 0; // به اولین سطر برگردید
                }

                //Updates the PressedRowColumnIndex value in the GridBaseSelectionController.
                try
                {
                    if (currentColumnIndex == 1)
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex + 1));
                    else if (currentColumnIndex == 3 && ((datagrid.GetRecordAtRowIndex(currentRowIndex) as StorageReceipt_Detail)?.Value ?? 0) != 0)
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex + 1, 0));
                    else
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex));
                }
                catch { }
            }
        }
    }
}
