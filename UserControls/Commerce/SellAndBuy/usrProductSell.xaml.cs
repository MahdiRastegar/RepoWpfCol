using Mahdi.PersianDateControls;
using PersianCalendarWPF;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TextInputLayout;
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
    public partial class usrProductSell : UserControl,ITabForm,ITabEdidGrid,IDisposable
    {
        public bool DataGridIsFocused
        {
            get
            {
                return datagrid.IsFocused;
            }
        }
        ProductSellViewModel ProductSellViewModel;
        List<Mu> mus1 = new List<Mu>();
        List<Mu> mus2 = new List<Mu>();
        public usrProductSell()
        {
            ProductSell_Details = new ObservableCollection<ProductSell_Detail>();
            ProductSellHeaders = new ObservableCollection<ProductSellHeader>();
            InitializeComponent();
            ProductSellViewModel = Resources["viewmodel"] as ProductSellViewModel;
            ProductSellViewModel.ProductSell_Details.CollectionChanged += ProductSell_Details_CollectionChanged;
            txbCalender.Text = pcw1.SelectedDate.ToString();
        }

        public void Dispose()
        {
            if (ProductSellViewModel == null)
                return;
            ProductSellHeaders.Clear();
            ProductSell_Details.Clear();
            datagridSearch.Dispose();
            dataPager.Dispose();
            DataContext = null;
            ProductSellViewModel.ProductSell_Details.CollectionChanged -= ProductSell_Details_CollectionChanged;
            ProductSellViewModel = null;
            GC.Collect();
        }

        Brush brush = null;
        public ObservableCollection<ProductSell_Detail> ProductSell_Details { get; set; }
        public ObservableCollection<ProductSellHeader> ProductSellHeaders { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                //if ((sender as TextBox).Name == "txtInvoiceNumber")
                //{
                    
                //}
                //else
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
            if ((sender as TextBox).Name != "txtDescription"&& (sender as TextBox).Name != "txtCarPlate"&& (sender as TextBox).Name != "txtCarType" && (sender as TextBox).Name != "txtWayBillNumber")
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

            mus1.Clear();
            mus2.Clear();
            //var storages = db.Preferential.ToList();
            var commodities = db.Commodity.ToList();
            //foreach (var item in storages)
            //{                
            //    mus1.Add(new Mu()
            //    {
            //        Id = item.Id,
            //        Name = $"{item.PreferentialName}",
            //        Value = $"{item.PreferentialCode}",
            //    });
            //}
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

            if (AddedMode)
            {
                if (db.ProductSellHeader.Count() == 0)
                {
                    txtInvoiceNumber.IsReadOnly = false;
                }
                else
                {
                    txtInvoiceNumber.IsReadOnly = true;
                    txtInvoiceNumber.Text = (db.ProductSellHeader.Max(t => t.InvoiceNumber) + 1).ToString();
                }

                ProductSell_Details = ProductSellViewModel.ProductSell_Details;
                //ProductSell_Details.Clear();                
                dataPager.Source = null;
                dataPager.Source = ProductSell_Details;
                txtPreferential.Focus();
            }
            else
            {
                ProductSell_Details = ProductSellViewModel.ProductSell_Details;
                ProductSell_Details.Clear();
                //ProductSell_Details.Clear();
                var h = db.ProductSell_Detail.Where(u=>u.fk_HeaderId==id).ToList();
                h.ForEach(u => ProductSell_Details.Add(u));
                RefreshDataGridForSetPersianNumber();
            }
            dataPager.Source = null;
            dataPager.Source = ProductSellHeaders;
            datagrid.SearchHelper.AllowFiltering = true;
            datagridSearch.SearchHelper.AllowFiltering = true;
            FirstLevelNestedGrid.SearchHelper.AllowFiltering = true;
            isCancel = true;
        }

        private static void SetAccountName(ColDbEntities1 db, ProductSell_Detail item2)
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

            ProductSellHeader e_Edidet = null;
            var code = int.Parse(txtPreferential.Text);
            var preferential = db.Preferential.First(t => t.PreferentialCode == code);
            if (id == Guid.Empty)
            {
                var h = long.Parse(txtInvoiceNumber.Text);
                if (db.ProductSellHeader.Any(u => u.InvoiceNumber == h&&u.fk_PreferentialId==preferential.Id))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("شماره فاکتور برای این تفضیلی تکراریست!");
                    return;
                }
            }
            else
            {
                e_Edidet = db.ProductSellHeader.Find(id);
                var h = long.Parse(txtInvoiceNumber.Text);
                if (h != e_Edidet.InvoiceNumber && db.ProductSellHeader.Any(u => u.InvoiceNumber == h && u.fk_PreferentialId == preferential.Id))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("شماره فاکتور برای این تفضیلی تکراریست!");
                    return;
                }
            }            
            ProductSellHeader e_addHeader = null;
            ProductSellHeader header = null;
            var yx = db.ProductSellHeader.OrderByDescending(k => k.Serial).FirstOrDefault();
            string serial = "1";
            if (yx != null)
            {
                serial = (yx.Serial + 1).ToString();
            }

            var header1 = db.ProductSellHeader.OrderByDescending(k => k.InvoiceNumber).FirstOrDefault();
            string InvoiceNumber = txtInvoiceNumber.Text;
            if (header1 != null)
            {
                InvoiceNumber = (header1.InvoiceNumber + 1).ToString();
            }
            var list = db.Preferential.ToList();
            if (id == Guid.Empty)
            {
                e_addHeader = new ProductSellHeader()
                {
                    Id = Guid.NewGuid(),
                    Date = pcw1.SelectedDate.ToDateTime(),
                    InvoiceNumber = long.Parse(InvoiceNumber),
                    Serial = long.Parse(serial),
                    Description = txtDescription.Text,
                    Preferential = preferential,
                    WayBillNumber = txtWayBillNumber.Text,
                    CarPlate = txtCarPlate.Text,
                    CarType = txtCarType.Text,
                    SumDiscount = decimal.Parse(txtSumDiscount.Text.Replace(",", "")),
                    InvoiceDiscount = decimal.Parse(txtInvoiceDiscount.Text),
                    Preferential4 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtReciever.Text, out code) ? code : -1)),
                    Preferential2 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtFreight.Text, out code) ? code : -1)),
                    Preferential1 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtDriver.Text, out code) ? code : -1)),
                    Preferential3 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtPersonnel.Text, out code) ? code : -1))
                };
                if (txtOrderNumber.Text != "")
                    e_addHeader.OrderNumber = long.Parse(txtOrderNumber.Text);
                if (txtShippingCost.Text != "")
                    e_addHeader.ShippingCost = decimal.Parse(txtShippingCost.Text);
                DbSet<ProductSell_Detail> details = null;
                int index = 0;
                foreach (var item in ProductSell_Details)
                {
                    index++;
                    var en = new ProductSell_Detail()
                    {
                        ProductSellHeader = e_addHeader,
                        fk_CommodityId = item.Commodity.Id,
                        Value = item.Value,
                        Indexer = index,
                        Discount = item.Discount,
                        Fee = item.Fee,
                        TaxPercent = item.Commodity.Taxable == true ? item.TaxPercent : 0,
                        Id = Guid.NewGuid()
                    };
                    db.ProductSell_Detail.Add(en);
                }
                db.ProductSellHeader.Add(e_addHeader);
                if (LoadedFill)
                    ProductSellHeaders.Add(e_addHeader);
            }
            else
            {
                var h = db.ProductSell_Detail.Where(v => v.fk_HeaderId == id);
                header = ProductSellHeaders.First(u => u.Id == id);
                foreach (var item in h)
                {
                    db.ProductSell_Detail.Remove(item);
                    header.ProductSell_Detail.Remove(header.ProductSell_Detail.First(x => x.Id == item.Id));
                }                
                e_Edidet.Date = header.Date = pcw1.SelectedDate.ToDateTime();
                e_Edidet.Description= header.Description=txtDescription.Text;
                e_Edidet.Preferential = header.Preferential = preferential;

                e_Edidet.Preferential4 = header.Preferential4 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtReciever.Text, out code) ? code : -1));
                e_Edidet.Preferential2 = header.Preferential2 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtFreight.Text, out code) ? code : -1));
                e_Edidet.Preferential1 = header.Preferential1 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtDriver.Text, out code) ? code : -1));
                e_Edidet.Preferential3 = header.Preferential3 = list.FirstOrDefault(t => t.PreferentialCode == (int.TryParse(txtPersonnel.Text, out code) ? code : -1));

                if (txtOrderNumber.Text != "")
                    e_Edidet.OrderNumber = header.OrderNumber = long.Parse(txtOrderNumber.Text);
                if (txtShippingCost.Text != "")
                    e_Edidet.ShippingCost = header.ShippingCost = decimal.Parse(txtShippingCost.Text);
                e_Edidet.WayBillNumber = header.WayBillNumber = txtWayBillNumber.Text;
                e_Edidet.CarPlate = header.CarPlate = txtCarPlate.Text;
                e_Edidet.CarType = header.CarType = txtCarType.Text;
                e_Edidet.SumDiscount = header.SumDiscount = decimal.Parse(txtSumDiscount.Text);
                e_Edidet.InvoiceDiscount = header.InvoiceDiscount = decimal.Parse(txtInvoiceDiscount.Text);

                int index = 0;
                foreach (var item in ProductSell_Details)
                {
                    index++;
                    var en = new ProductSell_Detail()
                    {
                        ProductSellHeader = e_Edidet,
                        fk_CommodityId = item.Commodity.Id,
                        Value = item.Value,
                        Indexer = index,
                        Discount = item.Discount,
                        Fee = item.Fee,
                        TaxPercent = item.Commodity.Taxable == true ? item.TaxPercent : 0,
                        Id = Guid.NewGuid()
                    };
                    db.ProductSell_Detail.Add(en);
                    header.ProductSell_Detail.Add(en);
                }
                //e_Edidet.fk_GroupId = ProductSell_Detail.fk_GroupId = col.Id;
                //e_Edidet.ProductSell_DetailName = ProductSell_Detail.ProductSell_DetailName = txtInvoiceNumber.Text;
            }
            if (!db.SafeSaveChanges())  return;
            if (header != null)
            {
                int i = 0;
                foreach (var item in header.ProductSell_Detail)
                {
                    item.Commodity = ProductSell_Details[i].Commodity;
                    i++;
                }
            }
            if(e_addHeader!=null)
            {
                int i = 0;
                foreach (var item in e_addHeader.ProductSell_Detail)
                {
                    item.Commodity = ProductSell_Details[i].Commodity;
                    i++;
                }
            }
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            if (ProductSell_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ProductSell_Details.Clear();
                }));
                RefreshDataGridForSetPersianNumber();
            }
            //datagrid.TableSummaryRows.Clear();
            SearchTermTextBox.Text = "";
            if (id == Guid.Empty)
            {
                this.gifImage.Visibility = Visibility.Visible;
                var gifImage = new BitmapImage(new Uri("pack://application:,,,/Images/AddDataLarge.gif"));
                XamlAnimatedGif.AnimationBehavior.SetSourceUri(this.gifImage, gifImage.UriSource);
                var th = new Thread(() =>
                {
                    Thread.Sleep(2570);
                    Dispatcher.Invoke(() =>
                    {
                        searchImage.Visibility = Visibility.Visible;
                        this.gifImage.Visibility = Visibility.Collapsed;
                    });
                });
                th.Start();
                searchImage.Visibility = Visibility.Collapsed;
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت فاکتور فروش");
                searchImage.Visibility = Visibility.Visible;
                this.gifImage.Visibility = Visibility.Collapsed;
                txtInvoiceNumber.Text = (long.Parse(txtInvoiceNumber.Text) + 1).ToString();
                txtSerial.Text = (long.Parse(serial) + 1).ToString();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش فاکتور فروش");
            }
            btnCancel_Click(null, null);
            txtInvoiceNumber.IsReadOnly = true;
            txtInvoiceNumber.Text = (db.ProductSellHeader.Max(t => t.InvoiceNumber) + 1).ToString();
            txtPreferential.Focus();
                            
            isCancel = true;                        
            id = Guid.Empty;
        }
        Guid id = Guid.Empty;
        private bool GetError()
        {
            var haserror = false;
            datagrid.BorderBrush = new  System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));

            if (txtInvoiceNumber.Text.Trim() == "")
            {
                Sf_txtInvoiceNumber.HasError = true;
                haserror = true;
            }
            else
                Sf_txtInvoiceNumber.HasError = false;
            
            if (txtPreferential.Text.Trim() == "")
            {
                Sf_txtPreferential.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtPreferential.HasError = false;
                Sf_txtPreferential.ErrorText = "";
            }
            if (ProductSell_Details.Count == 0)//ProductSell_Details.Any(g => !viewModel.AllCommodities.Any(y => y.CommodityCode == g.CommodityCode)))
            {
                datagrid.BorderBrush = Brushes.Red;
                haserror = true;
            }
            else if (ProductSell_Details.Any(t => t.Commodity == null || t.Value == 0 )|| (ProductSell_Details.Any(t => t.Error != string.Empty)))
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

            if (window == null && datagrid.GetRecordAtRowIndex(e.RowColumnIndex.RowIndex) is ProductSell_Detail ProductSell_Detail)
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
                            ProductSell_Detail.Commodity = commodity;
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

        bool isCancel = true;
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (AddedMode&&isCancel)
            {
                return;
            }
            if (searchImage.ToolTip.ToString() == "جستجو" && sender != null && Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            searchImage.Visibility = Visibility.Visible;
            searchImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Data.png"));
            var db = new ColDbEntities1();
            if (db.ProductSellHeader.Count() == 0)
            {
                txtInvoiceNumber.IsReadOnly = false;
                txtInvoiceNumber.Text = string.Empty;
            }
            else
            {
                txtInvoiceNumber.IsReadOnly = true;
                txtInvoiceNumber.Text = (db.ProductSellHeader.Max(t => t.InvoiceNumber) + 1).ToString();
            }
            searchImage.ToolTip = "جستجو";
            GStop1.Color = new Color()
            {
                R = 244,
                G = 248,
                B = 255,
                A = 255
            };            
            searchImage.Opacity = 1;
            if (!AddedMode)
            {
                if (id != Guid.Empty)
                {
                    var e_Edidet = db.ProductSellHeader.Find(id);
                    var header = ProductSellHeaders.FirstOrDefault(o => o.Id == id);
                    header.ProductSell_Detail.Clear();
                    foreach (var item in e_Edidet.ProductSell_Detail)
                    {
                        header.ProductSell_Detail.Add(item);
                        SetAccountName(db, item);
                    }
                }
                AddedMode = true;
                column3.Width = column2.Width = column1.Width = new GridLength(225);
                datagrid.AllowEditing = datagrid.AllowDeleting = true;
                datagrid.AddNewRowPosition = Syncfusion.UI.Xaml.Grid.AddNewRowPosition.Bottom;
            }
            datagrid.Visibility = Visibility.Visible;
            datagridSearch.Visibility = Visibility.Collapsed;
            gridConfirm.Visibility = Visibility.Visible;
            Sf_txtInvoiceNumber.HasError = false;
            txtDescription.Text = string.Empty;
            txtPreferential.Text = string.Empty;
            Sf_txtPreferential.HasError = false;
            Sf_txtPreferential.HelperText = "";

            txtReciever.Text = string.Empty;
            txtFreight.Text = string.Empty;
            txtDriver.Text = string.Empty;
            txtPersonnel.Text = string.Empty;

            Sf_txtReciever.HelperText = string.Empty;
            Sf_txtFreight.HelperText = string.Empty;
            Sf_txtDriver.HelperText = string.Empty;
            Sf_txtPersonnel.HelperText = string.Empty;

            txtOrderNumber.Text = string.Empty;
            Sf_txtOrderNumber.HasError = false;

            txtWayBillNumber.Text = string.Empty;
            Sf_txtWayBillNumber.HasError = false;

            txtCarPlate.Text = string.Empty;
            Sf_txtCarPlate.HasError = false;

            txtCarType.Text = string.Empty;
            Sf_txtCarType.HasError = false;

            txtShippingCost.Text = string.Empty;
            Sf_txtShippingCost.HasError = false;

            txtInvoiceDiscount.Text = "0";
            Sf_txtInvoiceDiscount.HasError = false;

            txtSum.Text = string.Empty;
            Sf_txtSum.HasError = false;

            txtSumDiscount.Text = string.Empty;
            Sf_txtSumDiscount.HasError = false;
            //txtCodeProductSell_Detail.Text = (en.ProductSell_DetailCode + 1).ToString();

            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            //datagrid.TableSummaryRows.Clear();
            datagrid.SearchHelper.ClearSearch();
            SearchTermTextBox.Text = "";
            dataPager.Visibility = Visibility.Collapsed;
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            txtSerial.Text = "";
            datagrid.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));
            if (ProductSell_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ProductSell_Details.Clear();
                }));
                RefreshDataGridForSetPersianNumber();
            }
            
            if(sender!=null)
                txtPreferential.Focus();
            isCancel = true;
            id = Guid.Empty;
        }

        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            return;
            if (datagrid.SelectedItem != null && !AddedMode)
            {
                gridDelete.Visibility = Visibility.Visible;
                /*var ProductSell_Detail = datagrid.SelectedItem as ProductSell_Detail;
                id = ProductSell_Detail.Id;
                cmbType.TextChanged -= txtDoumentType_TextChanged;
                cmbType.Text = ProductSell_Detail.tGroup.GroupCode.ToString();
                cmbType.TextChanged += txtDoumentType_TextChanged;
                txtSerial.Text = ProductSell_Detail.tGroup.GroupName;
                txtInvoiceNumber.Text = ProductSell_Detail.ProductSell_DetailName;
                gridDelete.Visibility = Visibility.Visible;
                borderEdit.Visibility = Visibility.Visible;
                cmbType.IsReadOnly = true;
                isCancel = true;
                GetError();*/
            }
            else
                gridDelete.Visibility = Visibility.Collapsed;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (id == Guid.Empty)
                return;
            if (Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید این اطلاعات پاک شود؟", "حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            var db = new ColDbEntities1();
            foreach (var item in db.ProductSell_Detail.Where(u => u.fk_HeaderId == id))
            {
                db.ProductSell_Detail.Remove(item);
            }
            db.ProductSellHeader.Remove(db.ProductSellHeader.Find(id));
            if (!db.SafeSaveChanges())  return;
            try
            {
                ProductSellHeaders.Remove(ProductSellHeaders.First(f => f.Id == id));
            }
            catch
            {

            }
            //btnCancel_Click(null, null);
        }
        
        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() == string.Empty)
            {
                if (FirstLevelNestedGrid.SearchHelper.SearchText.Trim() != "")
                {

                }
                else if (datagrid.Visibility != Visibility.Visible)
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
                    datagridSearch.SelectedIndex = -1;
                    if (SearchTermTextBox.Text.Trim() == "")
                    {
                        dataPager.Visibility = Visibility.Visible;
                        datagridSearch.SearchHelper.ClearSearch();
                        FirstLevelNestedGrid.SearchHelper.ClearSearch();
                        var g = dataPager.Source;
                        dataPager.Source = null;
                        dataPager.Source = g;
                    }
                    else
                    {
                        //dataPager.Visibility = Visibility.Collapsed;
                        datagridSearch.SearchHelper.Search("");
                        FirstLevelNestedGrid.SearchHelper.Search(SearchTermTextBox.Text);
                        SetHide_EmptyDetails();
                        //datagridSearch.View.Refresh();

                        //var h2 = FirstLevelNestedGrid.SearchHelper.GetSearchRecords();
                        //var h1 = datagridSearch.SearchHelper.GetSearchRecords();

                        /*foreach (ProductSellHeader item in datagridSearch.DetailsViewDefinition)
                        {
                            if(item.ProductSell_Detail.Count!=0)
                            {

                            }
                            else
                            {

                            }
                        }*/
                        //datagridSearch.SearchHelper.Search(SearchTermTextBox.Text);
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

        private void SetHide_EmptyDetails()
        {
            if (SearchTermTextBox.Text == "")
                return;
            int ir = 0;
            var list = new List<int>();
            foreach (var item in datagridSearch.View?.Records)
            {
                var tt = item.Data as ProductSellHeader;
                if (!tt.ProductSell_Detail.Any(i => i.Value.ToString().Contains(SearchTermTextBox.Text.ToLower())==true ||
                i.Commodity.Unit.Name.ToString().Contains(SearchTermTextBox.Text.ToLower()) ||
                i.Commodity.Code.ToString().Contains(SearchTermTextBox.Text.ToLower()) ||
                i.Commodity.Name.ToLower().Contains(SearchTermTextBox.Text.ToLower()) == true))
                {
                    //datagridSearch.View.Records.Remove(item);
                    list.Add(ir);
                }
                else
                {
                    item.IsExpanded = true;
                    //this.datagrid.ExpandDetailsViewAt(this.datagrid.ResolveToRecordIndex(ir));
                }
                ir++;
            }
            var l = 0;
            for (var i = 0; i < list.Count; i++)
            {
                datagridSearch.View.Records.RemoveAt(list[i] - l);
                l++;
            }

            datagridSearch.ExpandAllDetailsView();
                
        }

        private void txtInvoiceNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void TxtCodeProductSell_Detail_TextChanged(object sender, TextChangedEventArgs e)
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
            //db.ProductSell_Detail.Where(ex)
            var count = db.ProductSell_Detail.Count();
            var F = db.ProductSell_Detail.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                ProductSell_Details[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "فاکتور فروش");
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
            datagridSearch.AllowFiltering = !datagridSearch.AllowFiltering;
            FirstLevelNestedGrid.AllowFiltering = !FirstLevelNestedGrid.AllowFiltering;
            if (!datagridSearch.AllowFiltering)
                datagridSearch.ClearFilters();
            if (!FirstLevelNestedGrid.AllowFiltering)
                FirstLevelNestedGrid.ClearFilters();
        }

        public void SetNull()
        {
            if(window!=null&&(window as winSearch).ParentTextBox is ProductSell_Detail storage)
            {
                var y = (window as winSearch).ParentTextBox as ProductSell_Detail;
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
            var h = ProductSellViewModel.ProductSell_Details.FirstOrDefault(q => q.AcCode == ctext);
            if (h != null)
            {
                (e.NewObject as UtililtyCommodity).CommodityId = h.ID;
                (e.NewObject as UtililtyCommodity).CommodityCode = ctext;
                (e.NewObject as UtililtyCommodity).Discount = 0;
                (e.NewObject as UtililtyCommodity).Tax = h.Tax;
            }*/
        }

        private void searchImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (searchImage.Opacity != .6)
            {
                if (!AddedMode && (searchImage.Source as BitmapImage).UriSource.AbsoluteUri.Contains("dataedit.png"))
                {
                    if (datagridSearch.SelectedItem == null)
                        return;
                    searchImage.Visibility = Visibility.Visible;
                    searchImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Data.png"));
                    searchImage.ToolTip = "جستجو";
                    GStop1.Color = new Color()
                    {
                        R = 244,
                        G = 248,
                        B = 255,
                        A = 255
                    };
                    searchImage.Opacity = 1;
                    gridDelete.Visibility = Visibility.Collapsed;
                    ProductSell_Details.Clear();
                    var header = datagridSearch.SelectedItem as ProductSellHeader;
                    id = header.Id;
                    header.ProductSell_Detail.ForEach(t => ProductSell_Details.Add(t));
                    pcw1.SelectedDate = new PersianCalendarWPF.PersianDate(header.Date);
                    txbCalender.Text = pcw1.SelectedDate.ToString();
                    txtInvoiceNumber.Text = header.InvoiceNumber.ToString();
                    txtPreferential.Text = header.Preferential.PreferentialCode.ToString();
                    Sf_txtPreferential.HelperText = header.Preferential.PreferentialName.ToString();
                    txtDescription.Text = header.Description.ToString();
                    txtSerial.Text = header.Serial.ToString();

                    txtOrderNumber.Text = header.OrderNumber?.ToString();

                    txtWayBillNumber.Text = header.WayBillNumber;

                    txtCarPlate.Text = header.CarPlate;

                    txtCarType.Text = header.CarType;

                    txtShippingCost.Text = header.ShippingCost.ToString();

                    txtInvoiceDiscount.Text = header.InvoiceDiscount.ToString();

                    txtSumDiscount.Text = header.SumDiscount.ToString();

                    txtReciever.Text = header.Preferential4.PreferentialCode.ToString();
                    txtFreight.Text = header.Preferential2.PreferentialCode.ToString();
                    txtDriver.Text = header.Preferential1.PreferentialCode.ToString();
                    txtPersonnel.Text = header.Preferential3.PreferentialCode.ToString();

                    Sf_txtReciever.HelperText = header.Preferential4.PreferentialName.ToString();
                    Sf_txtFreight.HelperText = header.Preferential2.PreferentialName.ToString();
                    Sf_txtDriver.HelperText = header.Preferential1.PreferentialName.ToString();
                    Sf_txtPersonnel.HelperText = header.Preferential3.PreferentialName.ToString();

                    var Y = header.ProductSell_Detail.Sum(y => y.Sum);
                    txtSum.Text = Y.ToString();

                    datagrid.AllowEditing = datagrid.AllowDeleting = true;
                    datagrid.AddNewRowPosition = Syncfusion.UI.Xaml.Grid.AddNewRowPosition.Bottom;
                    datagrid.Visibility = Visibility.Visible;
                    dataPager.Visibility = Visibility.Collapsed;
                    testsearch.Text = "جستجو...";
                    datagrid.SearchHelper.ClearSearch();
                    SearchTermTextBox.TextChanged-= SearchTermTextBox_TextChanged;
                    SearchTermTextBox.Text = "";
                    SearchTermTextBox.TextChanged+= SearchTermTextBox_TextChanged;
                    datagridSearch.Visibility = Visibility.Collapsed;
                    gridConfirm.Visibility = Visibility.Visible;
                    Sf_txtInvoiceNumber.HasError = false;
                    column3.Width = column2.Width = column1.Width = new GridLength(225);
                    borderEdit.Visibility = Visibility.Visible;
                    RefreshDataGridForSetPersianNumber();
                    datagrid.SelectedIndex = ProductSell_Details.Count - 1;
                    isCancel = true;
                }
                else
                {
                    if (!isCancel)
                    {
                        if (Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید از این عملیات انصراف دهید؟", "انصراف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                        {
                            return;
                        }
                    }
                    FillHeaders();
                    if (!AddedMode)
                    {
                        var db = new ColDbEntities1();
                        var e_Edidet = db.ProductSellHeader.Find(id);
                        var header = ProductSellHeaders.FirstOrDefault(o => o.Id == id);
                        header.ProductSell_Detail.Clear();
                        e_Edidet.ProductSell_Detail = e_Edidet.ProductSell_Detail
                       .OrderBy(d => d.Indexer)
                       .ToList();
                        foreach (var item in e_Edidet.ProductSell_Detail)
                        {
                            header.ProductSell_Detail.Add(item);
                            SetAccountName(db, item);
                        }
                    }
                    datagridSearch.ClearFilters();
                    datagridSearch.SortColumnDescriptions.Clear();
                    try
                    {
                        datagridSearch.SortColumnDescriptions.Add(new SortColumnDescription()
                        {
                            ColumnName = "Serial",
                            SortDirection = System.ComponentModel.ListSortDirection.Descending
                        });
                    }
                    catch { }
                    btnCancel_Click(null, null);
                    datagridSearch.SearchHelper.ClearSearch();
                    FirstLevelNestedGrid.SearchHelper.ClearSearch();
                    SearchTermTextBox.Text = "";
                    datagridSearch.SelectedItem = null;
                    var t = dataPager.Source;
                    //foreach (var item in t as ObservableCollection<ProductSellHeader>)
                    //{
                    //    item.RefreshSumColumns();
                    //}
                    dataPager.Source = null;
                    datagridSearch.SelectedIndex = 0;
                    borderEdit.Visibility = Visibility.Collapsed;
                    gridDelete.Visibility= Visibility.Visible;
                    datagrid.Visibility = Visibility.Collapsed;
                    datagridSearch.Visibility = Visibility.Visible;
                    dataPager.Visibility = Visibility.Visible;
                    testsearch.Text = "جستجو در جزئیات...";
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        datagridSearch.Visibility = Visibility.Collapsed;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            datagridSearch.Visibility = Visibility.Visible;
                            dataPager.Source = t;
                        }), DispatcherPriority.Render);
                    }), DispatcherPriority.Render);
                    gridConfirm.Visibility = Visibility.Collapsed;
                    if ((t as ObservableCollection<ProductSellHeader>).Count == 0)
                        searchImage.Opacity = .6;
                    searchImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/dataedit.png"));
                    searchImage.ToolTip = "ویرایش";
                    GStop1.Color = new Color()
                    {
                        R = 209,
                        G = 226,
                        B = 255,
                        A = 240
                    };
                    column3.Width = column2.Width = column1.Width = new GridLength(0);
                    datagrid.AllowEditing = datagrid.AllowDeleting = false;
                    datagrid.AddNewRowPosition = Syncfusion.UI.Xaml.Grid.AddNewRowPosition.None;
                    AddedMode = false;
                    datagrid.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(() =>
                {
                    SetHide_EmptyDetails();
                }));
                }
            }
        }

        bool LoadedFill = false;
        private void FillHeaders()
        {
            if (!LoadedFill)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var db = new ColDbEntities1();
                var documents = db.ProductSellHeader
                    .Include(h => h.ProductSell_Detail)
                    .AsNoTracking()
                    .ToList();
                foreach (var doc in documents)
                {
                    doc.ProductSell_Detail = doc.ProductSell_Detail
                        .OrderBy(d => d.Indexer)
                        .ToList();

                    foreach (var item2 in doc.ProductSell_Detail)
                    {
                        SetAccountName(db, item2);
                    }
                    ProductSellHeaders.Add(doc);
                }
                LoadedFill = true;
                Mouse.OverrideCursor = null;
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ProductSellHeaders.ForEach(y => y.ProductSell_Detail = y.ProductSell_Detail
                   .OrderBy(d => d.Indexer)
                   .ToList());
                Mouse.OverrideCursor = null;
            }
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

        private void datagridSearch_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            if (datagridSearch.SelectedItem != null)
            {
                searchImage.Opacity = 1;
                var header = datagridSearch.SelectedItem as ProductSellHeader;
                id = header.Id;
            }
            else if (datagrid.Visibility != Visibility.Visible)
                id = Guid.Empty;
        }

        private void datagrid_RowValidated(object sender, RowValidatedEventArgs e)
        {
            //var detail = e.RowData as ProductSell_Detail;
            //if (datagrid.SelectedIndex!=-1&& detail.ColeMoein == null && detail.PreferentialCode == null && detail.Debtor == null && detail.Creditor == null && detail.Description == null)
            //{
            //    ProductSell_Details.Remove(detail);
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

        private void ProductSell_Details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //var detail = ProductSell_Details.LastOrDefault();
            //if (detail == null)
            //    return;
            //if (detail.ColeMoein == null && detail.PreferentialCode == null && detail.Debtor == null && detail.Creditor == null && detail.Description == null)
            //{
            //    datagrid.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        ProductSell_Details.Remove(detail);
            //    }));
            //}
            //datagrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    CalDebCre();
            //}));
        }

        private void CalDebCre()
        {
            if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex >= 1)
            {
                var Y = ProductSell_Details.Sum(y => y.Sum);
                txtSum.Text = Y.ToString();
                txtSumDiscount.Text = (Y - decimal.Parse(txtInvoiceDiscount.Text.Replace(",", ""))).ToString();
                //return;

                var t = datagrid.ItemsSource;
                datagrid.ItemsSource = null;
                datagrid.ItemsSource = t;
                //}
                datagrid.View?.Refresh();
            }
        }

        private void datagridSearch_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter) 
            {
                searchImage_PreviewMouseDown(null, null);
            }
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
            if (textBox.Text != "" && e.Record is ProductSell_Detail detail && detail.Commodity?.Code.ToString() != textBox.Text && !Keyboard.IsKeyDown(Key.Enter))
                CurrentCellText = textBox.Text;
        }

        private void datagrid_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (e.RowData is ProductSell_Detail detail)
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
                        //e.IsValid = false;
                        //e.ErrorMessages.Add("Code", "چنین کالایی وجود ندارد!");
                        //Xceed.Wpf.Toolkit.MessageBox.Show("چنین کالایی وجود ندارد!");
                        detail.Commodity = null;
                        textBox.Text = "";
                        CurrentCellText = "";
                    }
                }
            }
        }

        private void datagridSearch_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var t = datagridSearch.GetChildByName<Grid>("PART_GroupDropAreaGrid");
            if (t == null) return;
            var textBlock = (t.Children[0] as Grid).Children[1] as TextBlock;
            textBlock.Foreground = Brushes.DarkBlue;
            textBlock.FontWeight= FontWeights.Bold;
        }

        private void datagridSearch_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // ارتفاع سطرهای grid را محاسبه کنید (می‌توانید ارتفاع سطر ثابت فرض کنید)
            double rowHeight = 30; // ارتفاع هر سطر (این مقدار ممکن است بسته به طراحی تغییر کند)

            // ارتفاع موجود در grid را محاسبه کنید
            double availableHeight = datagridSearch.ActualHeight;

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
                    datagridSearch.SearchHelper.ClearSearch();
                    SearchTermTextBox.Text = "";
                }
            }
        }

        private void persianCalendarE_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!rl2)
                e.Handled = true;
            rl2 = false;
        }

        private void dataPager_PageIndexChanged(object sender, Syncfusion.UI.Xaml.Controls.DataPager.PageIndexChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() != string.Empty)
                datagridSearch.ExpandAllDetailsView();
        }

        private void txtPreferential_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
        }

        private void txtPreferential_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                var textbox = sender as TextBox;
                switch (textbox.Name)
                {
                    case "txtPreferential":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtInvoiceNumber.Focus();
                        }));
                        break;
                    case "txtReciever":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtOrderNumber.Focus();
                        }));
                        break;
                    case "txtFreight":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtWayBillNumber.Focus();
                        }));
                        break;
                        break;
                    case "txtDriver":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtCarType.Focus();
                        }));
                        break;
                    case "txtPersonnel":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtDescription.Focus();
                        }));
                        break;
                }
                return;
            }
            e.Handled = !IsTextAllowed(e.Text);
        }

        private void txtPreferential_LostFocus(object sender, RoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            var Sf_textbox = textbox.GetParentOfType<SfTextInputLayout>();
            if (textbox.Text == "")
            {
                textbox.Text = string.Empty;
                Sf_textbox.HelperText = string.Empty;
                return;
            }
            var db = new ColDbEntities1();
            var code = int.Parse(textbox.Text);
            var mu = db.Preferential.FirstOrDefault(t => t.PreferentialCode == code);
            if (mu == null)
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("چنین کد تفضیلی وجود ندارد!");
                textbox.Text = Sf_textbox.HelperText = string.Empty;
            }
            else
            {
                Sf_textbox.HelperText = mu.PreferentialName;
                switch(textbox.Name)
                {
                    case "txtPreferential":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtInvoiceNumber.Focus();
                        }));
                        break;
                    case "txtReciever":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtOrderNumber.Focus();
                        }));
                        break;
                    case "txtFreight":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtWayBillNumber.Focus();
                        }));
                        break;
                        break;
                    case "txtDriver":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtCarType.Focus();
                        }));
                        break;
                    case "txtPersonnel":
                        Dispatcher.BeginInvoke(new Action(async () =>
                        {
                            await Task.Delay(50);
                            txtDescription.Focus();
                        }));
                        break;
                }
            }
        }

        private void txtPreferential_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textbox = sender as TextBox;
            if (e.Key == Key.F1)
            {
                var db = new ColDbEntities1();
                var list = db.Preferential.ToList().Select(r => new Mu() { Name = r.PreferentialName, Value = r.PreferentialCode.ToString(),Name2=r.tGroup.GroupName }).ToList();
                var win = new winSearch(list);
                win.Closed += (yf, rs) =>
                {
                    datagrid.IsHitTestVisible = true;
                };
                win.datagrid.Columns.Add(new GridTextColumn() { TextAlignment = TextAlignment.Center, HeaderText = "گروه تفضیلی", MappingName = "Name2", Width = 150, AllowSorting = true });
                win.Width = 640;
                win.Tag = this;
                win.ParentTextBox = textbox;
                win.SearchTermTextBox.Text = "";
                win.SearchTermTextBox.Select(1, 0);
                win.Owner = MainWindow.Current;
                window = win;
                win.Show();
                win.Focus();
            }
        }

        private void txtInvoiceDiscount_PreviewTextInput(object sender, TextCompositionEventArgs e)
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
            if (txtInvoiceDiscount.Text == "")
                txtInvoiceDiscount.Text = "0";
        }

        private void txtInvoiceDiscount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }
        }

        private void txtInvoiceDiscount_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtInvoiceDiscount.Text == "")
                txtInvoiceDiscount.Text = "0";
            try
            {
                txtSumDiscount.Text = (decimal.Parse(txtSum.Text.Replace(",", "")) - decimal.Parse(txtInvoiceDiscount.Text.Replace(",", ""))).ToString();
            }
            catch { }
        }

        private void txtSum_TextChanged(object sender, TextChangedEventArgs e)
        {
            isCancel = false;
            var textbox = sender as TextBox;
            decimal ds = 0;
            if (decimal.TryParse(textbox.Text.Trim().Replace(",", ""), out ds) && ds >= 0)
            {

                int temp = textbox.SelectionStart;
                textbox.TextChanged -= txtSum_TextChanged;
                textbox.Text = string.Format("{0:#,###}", ds);
                if (textbox.SelectionStart != temp)
                    textbox.SelectionStart = temp + 1;
                if (textbox.Text == "")
                    textbox.Text = "0";
                textbox.TextChanged += txtSum_TextChanged;
            }            
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
                if (currentRowIndex >= ProductSell_Details.Count + 2)
                {
                    currentRowIndex = 0; // به اولین سطر برگردید
                }

                //Updates the PressedRowColumnIndex value in the GridBaseSelectionController.
                try
                {
                    if (currentColumnIndex == 1)
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex + 1));
                    else if (currentColumnIndex == 5 && ((datagrid.GetRecordAtRowIndex(currentRowIndex) as ProductSell_Detail)?.Value ?? 0) != 0)
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex + 1, 0));
                    else
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex));
                }
                catch { }
            }
        }
    }
}
