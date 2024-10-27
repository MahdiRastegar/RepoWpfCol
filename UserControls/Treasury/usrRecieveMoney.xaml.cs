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
    public partial class usrRecieveMoney : UserControl,ITabForm, ITabEdidGrid,IDisposable
    {
        public bool DataGridIsFocused
        {
            get
            {
                return datagrid.IsFocused;
            }
        }
        RecieveMoneyViewModel acDocumentViewModel;
        ObservableCollection<Bank> Banks = new ObservableCollection<Bank>();
        List<Mu> mus1 = new List<Mu>();
        List<Mu> mus2 = new List<Mu>();
        public usrRecieveMoney()
        {
            temp_recieveMoney_Details = new ObservableCollection<RecieveMoney_Detail>();
            RecieveMoneyHeaders = new ObservableCollection<RecieveMoneyHeader>();
            InitializeComponent();
            acDocumentViewModel = Resources["viewmodel"] as RecieveMoneyViewModel;
            acDocumentViewModel.recieveMoney_Details.CollectionChanged += AcDocument_Details_CollectionChanged;
            txbCalender.Text = pcw1.SelectedDate.ToString();
        }

        public void Dispose()
        {
            if (acDocumentViewModel == null)
                return;
            mus1.Clear();
            mus2.Clear();
            RecieveMoneyHeaders.Clear();
            recieveMoney_Details.Clear();
            datagridSearch.Dispose();
            dataPager.Dispose();
            DataContext = null;
            acDocumentViewModel.recieveMoney_Details.CollectionChanged -= AcDocument_Details_CollectionChanged;
            acDocumentViewModel = null;
            GC.Collect();
        }

        Brush brush = null;
        public ObservableCollection<RecieveMoney_Detail> temp_recieveMoney_Details { get; set; }
        public ObservableCollection<RecieveMoney_Detail> recieveMoney_Details { get; set; }
        public ObservableCollection<RecieveMoneyHeader> RecieveMoneyHeaders { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                if ((sender as TextBox).Name == "txtDescription")
                {
                    datagrid.Focus();
                    Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(10);
                        keybd_event(VK_Down, 0, 0, UIntPtr.Zero); // فشار دادن کلید
                        Thread.Sleep(50); // تاخیر برای شبیه‌سازی فشار دادن
                        keybd_event(VK_Down, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // آزاد کردن کلید 
                        var g = datagrid.GetChildOfType<ComboBoxAdv>();
                        var gridCell = g.GetParentOfType<GridCell>();
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(gridCell.ColumnBase.RowIndex, gridCell.ColumnBase.ColumnIndex));
                        await Task.Delay(10);
                        g.Focus();
                        g.IsDropDownOpen = true;
                    }), DispatcherPriority.Render);
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
        bool AddedMode = true;
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var db = new ColDbEntities1();
            mus1.Clear();
            mus2.Clear();
            Banks = acDocumentViewModel.Banks;
            Banks.Clear();
            db.Bank.ToList().ForEach(t => Banks.Add(t));

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
                    Id= item.Id,
                    Name = $"{item.PreferentialName}",
                    Value = $"{item.PreferentialCode}"
                });
            }
            
            if (AddedMode)
            {               
                recieveMoney_Details = acDocumentViewModel.recieveMoney_Details;
                //AcDocument_Details.Clear();
                var y = db.RecieveMoneyHeader.OrderByDescending(k => k.ReceiptNumber).FirstOrDefault();
                if (y == null)
                {
                    txtSerial.Text = "1";
                }
                else
                {
                    txtSerial.Text = (y.ReceiptNumber + 1).ToString();
                }
            }
            else
            {
                recieveMoney_Details = acDocumentViewModel.recieveMoney_Details;
                recieveMoney_Details.Clear();
                //AcDocument_Details.Clear();
                var h = db.RecieveMoney_Detail.Where(u=>u.fkHeaderId==id).ToList();
                h.ForEach(u => recieveMoney_Details.Add(u));
                RefreshDataGridForSetPersianNumber();
            }            
            dataPager.Source = null;
            dataPager.Source = RecieveMoneyHeaders;
            datagrid.SearchHelper.AllowFiltering = true;
            datagridSearch.SearchHelper.AllowFiltering = true;
            FirstLevelNestedGrid.SearchHelper.AllowFiltering = true;
            isCancel = true;
            txtMoein.Focus();
        }

        private static void SetAccountName(ColDbEntities1 db, RecieveMoney_Detail item2)
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
                                    //var c = int.Parse(cmbType.Text);
                                    //var col = db.tGroup.FirstOrDefault(g => g.GroupCode == c);
                                    //if (col == null)
                                    //{
                                    //    Sf_txtDoumentType.ErrorText = "این کد گروه وجود ندارد";
                                    //    Sf_txtDoumentType.HasError = true;
                                    //    return;
                                    //}
                                    //var RecieveMoney_Detail = db.RecieveMoney_Detail.Find(id);
            
                                    //var nRecieveMoney_Detail = db.RecieveMoney_Detail.FirstOrDefault(g => g.fk_GroupId == col.Id && g.RecieveMoney_DetailName == txtNoDocumen.Text);
                                    //if (RecieveMoney_Detail?.Id != nRecieveMoney_Detail?.Id && nRecieveMoney_Detail != null)
                                    //{
                                    //    Xceed.Wpf.Toolkit.MessageBox.Show("این نام تفضیلی و کد گروه از قبل وجود داشته است!");
                                    //    return;
                                    //}
            
            RecieveMoneyHeader e_addHeader = null;
            RecieveMoneyHeader header = null;
            if (id == Guid.Empty)
            {
                e_addHeader = new RecieveMoneyHeader()
                {
                    Id = Guid.NewGuid(),
                    Date=pcw1.SelectedDate.ToDateTime(),
                    ReceiptNumber = int.Parse(txtSerial.Text),
                    Description = txtDescription.Text,
                    fk_MoeinId=(txtMoein.Tag as Mu).Id,
                    fk_PreferentialId=(txtPreferential.Tag as Mu).Id
                };
                DbSet<RecieveMoney_Detail> details = null;
                foreach (var item in recieveMoney_Details)
                {
                    var en = new RecieveMoney_Detail()
                    {
                        fk_MoeinId = item.Moein.Id,
                        fk_PreferentialId = item.Preferential.Id,
                        RecieveMoneyHeader = e_addHeader,
                        fkBank = item.Bank?.Id,
                        BranchName = item.BranchName,
                        Date = item.Date,
                        Number = item.Number,
                        Price = item.Price,
                        MoneyType = item.MoneyType,
                        Registered = item.Registered,
                        SayadiNumber = item.SayadiNumber,
                        Id = Guid.NewGuid()
                    };
                    db.RecieveMoney_Detail.Add(en);
                    if (item.MoneyType == 1)
                    {
                        db.CheckRecieveEvent.Add(new CheckRecieveEvent()
                        {
                            Id = Guid.NewGuid(),
                            RecieveMoney_Detail = en,
                            ChEvent = db.ChEvent.First(u => u.ChEventCode == 0),
                            fk_MoeinId = item.Moein.Id,
                            fk_PreferentialId = item.Preferential.Id,
                            EventDate = pcw1.SelectedDate.ToDateTime(),
                        });
                    }
                }
                db.RecieveMoneyHeader.Add(e_addHeader);
                if (LoadedFill)
                    RecieveMoneyHeaders.Add(e_addHeader);
                e_addHeader.Moein = db.Moein.Find((txtMoein.Tag as Mu).Id);
                e_addHeader.Preferential = db.Preferential.Find((txtPreferential.Tag as Mu).Id);
            }
            else
            {
                var h = db.RecieveMoney_Detail.Where(v => v.fkHeaderId == id);
                header = RecieveMoneyHeaders.First(u => u.Id == id);
                foreach (var item in h)
                {                    
                    db.RecieveMoney_Detail.Remove(item);
                    header.RecieveMoney_Detail.Remove(header.RecieveMoney_Detail.First(x => x.Id == item.Id));
                }
                var e_Edidet = db.RecieveMoneyHeader.Find(id);
                e_Edidet.ReceiptNumber= header.ReceiptNumber = int.Parse(txtSerial.Text);
                e_Edidet.Description= header.Description = txtDescription.Text;
                e_Edidet.fk_MoeinId = (txtMoein.Tag as Mu).Id;
                header.Moein = db.Moein.Find((txtMoein.Tag as Mu).Id);
                e_Edidet.fk_PreferentialId = (txtPreferential.Tag as Mu).Id;
                header.Preferential = db.Preferential.Find((txtPreferential.Tag as Mu).Id);
                e_Edidet.Date = header.Date = pcw1.SelectedDate.ToDateTime();
                foreach (var item in recieveMoney_Details)
                {
                    var en = new RecieveMoney_Detail()
                    {
                        fkHeaderId = header.Id,
                        fk_MoeinId = item.Moein.Id,
                        fk_PreferentialId = item.Preferential.Id,
                        RecieveMoneyHeader = e_addHeader,
                        fkBank = item.Bank?.Id,
                        BranchName = item.BranchName,
                        Date = item.Date,
                        Number = item.Number,
                        Price = item.Price,
                        MoneyType = item.MoneyType,
                        Registered = item.Registered,
                        SayadiNumber = item.SayadiNumber,
                        Id = Guid.NewGuid()
                    };
                    bool Enter = false;
                    if (item.Id != Guid.Empty && item.MoneyType == 1 && db.RecieveMoney_Detail.Find(item.Id) is RecieveMoney_Detail detail && detail.MoneyType == 1 &&
                    ExtensionMethods.CompareObjects(item, detail) is List<string> fff && !fff.Contains("fk_MoeinId") && !fff.Contains("fk_PreferentialId"))
                    {
                        db.CheckRecieveEvent.Where(y => y.fk_DetaiId == item.Id).ForEach(u =>
                        {
                            u.fk_DetaiId = en.Id;
                        });
                        Enter = true;
                    }
                    db.RecieveMoney_Detail.Add(en);
                    if (!Enter && item.MoneyType == 1)
                    {
                        db.CheckRecieveEvent.Add(new CheckRecieveEvent()
                        {
                            Id = Guid.NewGuid(),
                            RecieveMoney_Detail = en,
                            ChEvent = db.ChEvent.First(u => u.ChEventCode == 0),
                            fk_MoeinId = item.Moein.Id,
                            fk_PreferentialId = item.Preferential.Id,
                            EventDate = pcw1.SelectedDate.ToDateTime(),
                        });
                    }
                    header.RecieveMoney_Detail.Add(en);
                }
            }
            db.SaveChanges();
            if (header != null)
            {
                int i = 0;
                foreach (var item in header.RecieveMoney_Detail)
                {
                    item.Moein = recieveMoney_Details[i].Moein;
                    item.Preferential = recieveMoney_Details[i].Preferential;
                    item.Bank = recieveMoney_Details[i].Bank;
                    i++;
                }
            }
            if(e_addHeader!=null)
            {
                int i = 0;
                foreach (var item in e_addHeader.RecieveMoney_Detail)
                {
                    item.Moein = recieveMoney_Details[i].Moein;
                    item.Preferential = recieveMoney_Details[i].Preferential;
                    item.Bank = recieveMoney_Details[i].Bank;
                    i++;
                }
            }
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            if (recieveMoney_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    recieveMoney_Details.Clear();
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
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات اضافه شد.", "ثبت");
                searchImage.Visibility = Visibility.Visible;
                this.gifImage.Visibility = Visibility.Collapsed;
                txtSerial.Text = (long.Parse(txtSerial.Text) + 1).ToString();
                txtMoein.Text = string.Empty;
                txtPreferential.Text = string.Empty;
                txtDescription.Text = string.Empty;
                txbMoein.Text = string.Empty;
                txbPreferential.Text = string.Empty;

                txtMoein.Focus();
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("اطلاعات ویرایش شد.", "ویرایش");
                btnCancel_Click(null, null);
            }
                            
            isCancel = true;                        
            id = Guid.Empty;
        }
        Guid id = Guid.Empty;
        private bool GetError()
        {
            var haserror = false;
            datagrid.BorderBrush = new  System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));
            if (txtMoein.Text.Trim() == "")
            {
                Sf_txtMoein.HasError = true;
                haserror = true;
            }
            else
            {
                Sf_txtMoein.HasError = false;
                Sf_txtMoein.ErrorText = "";
            }
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
            if (recieveMoney_Details.Count == 0)//AcDocument_Details.Any(g => !viewModel.AllCommodities.Any(y => y.CommodityCode == g.CommodityCode)))
            {
                datagrid.BorderBrush = Brushes.Red;
                haserror = true;
            }
            //else if (recieveMoney_Details.Any(t => t.Price == 0 || t.ColeMoein == "" || t.ColeMoein == null || t.PreferentialCode == "" || t.PreferentialCode == null) ||
            //    recieveMoney_Details.Any(t => t.MoneyType == 1 && (t.Date == null || t.Bank == null || t.Number == null||t.Number=="")))
            else if (recieveMoney_Details.Any(t => t.Error != string.Empty))
            //else if (datagrid.GetChildsByName<Border>("PART_InValidCellBorder").Any(g => g.Visibility == Visibility.Visible))
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
            if (e.Key == Key.F1)
            {
                if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex == 1)
                {
                    dynamic y = null;
                    var element = (datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell)
                            .Content as FrameworkElement;
                    y = element.DataContext;
                    if (element is TextBlock)
                    {
                        if (sender == null)
                            return;
                        if (datagrid.SelectedIndex == -1)
                        {
                            if (y == null)
                            {
                                bool d = datagrid.GetGridModel().AddNewRowController.AddNew();
                            }
                            y = element.DataContext;
                        }
                        var cell = datagrid.SelectionController.CurrentCellManager.CurrentCell.Element;
                        //var screenPosition = cell.PointToScreen(new System.Windows.Point(0, 0));
                        //SetCursorPos((int)screenPosition.X - 30, (int)screenPosition.Y + 15);
                        //LeftDoubleClick();
                        // شبیه‌سازی فشار دادن کلید F2
                        keybd_event(VK_F2, 0, 0, UIntPtr.Zero); // فشار دادن کلید
                        Thread.Sleep(50); // تاخیر برای شبیه‌سازی فشار دادن
                        keybd_event(VK_F2, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // آزاد کردن کلید
                        var th = new Thread(() =>
                        {
                            Thread.Sleep(10);
                            Dispatcher.Invoke(() =>
                            datagrid_PreviewKeyDown(null, e));
                        });
                        th.Start();
                        return;
                    }
                    ShowSearchMoein(y);
                    datagrid.IsHitTestVisible = false;
                }
                else if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex == 2)
                {
                    dynamic y = null;
                    var element = (datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell)
                            .Content as FrameworkElement;
                    y = element.DataContext;
                    if (datagrid.SelectedIndex == -1 || element is TextBlock)
                    {
                        if (y == null || y.PreferentialCode != null)
                        {
                            if (sender == null)
                                return;
                            var cell = datagrid.SelectionController.CurrentCellManager.CurrentCell.Element;
                            keybd_event(VK_F2, 0, 0, UIntPtr.Zero); // فشار دادن کلید
                            Thread.Sleep(50); // تاخیر برای شبیه‌سازی فشار دادن
                            keybd_event(VK_F2, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // آزاد کردن کلید
                            var th = new Thread(() =>
                            {
                                Thread.Sleep(10);
                                Dispatcher.Invoke(() =>
                                datagrid_PreviewKeyDown(null, e));
                            });
                            th.Start();
                            return;
                        }
                    }
                    ShowSearchPreferential(y);
                    datagrid.IsHitTestVisible = false;
                }
                else if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex == 5)
                {
                    dynamic y = null;
                    var element = (datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell)
                            .Content as FrameworkElement;
                    y = element.DataContext;
                    if (datagrid.SelectedIndex == -1 || element is TextBlock)
                    {
                        if (y == null)
                        {
                            if (sender == null)
                                return;
                            var cell = datagrid.SelectionController.CurrentCellManager.CurrentCell.Element;
                            keybd_event(VK_F2, 0, 0, UIntPtr.Zero); // فشار دادن کلید
                            Thread.Sleep(50); // تاخیر برای شبیه‌سازی فشار دادن
                            keybd_event(VK_F2, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // آزاد کردن کلید
                            var th = new Thread(() =>
                            {
                                Thread.Sleep(10);
                                Dispatcher.Invoke(() =>
                                datagrid_PreviewKeyDown(null, e));
                            });
                            th.Start();
                            return;
                        }
                        else if (y.MoneyType != 1)
                            return;
                    }
                    (MyPopupS.Parent as Grid).Children.Remove(MyPopupS);
                    ParentDataGrid.Children.Add(MyPopupS);
                    MyPopupS.Visibility = Visibility.Visible;
                    MyPopupS.IsOpen = true;
                    datagrid.IsHitTestVisible = false;
                }
            }           
        }
        int tempSelectedIndex = -1;
        public void SetEnterToNextCell(RowColumnIndex? rowColumn = null)
        {
            var dataGrid = datagrid;

            // پیدا کردن سطر و ستون فعلی
            var currentCell = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
            if (currentCell != null || rowColumn != null)
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
                if (currentRowIndex >= recieveMoney_Details.Count + 2)
                {
                    currentRowIndex = 0; // به اولین سطر برگردید
                }

                //Updates the PressedRowColumnIndex value in the GridBaseSelectionController.
                try
                {
                    if (currentColumnIndex == 3)
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex + 1));
                    else if (tempSelectedIndex != -1)
                    {
                        var recieveMoney_Detail = recieveMoney_Details[tempSelectedIndex];
                        switch (recieveMoney_Detail.MoneyType)
                        {
                            case 0:
                            case 2:
                                if (currentColumnIndex == 5)
                                    (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex + 1, currentColumnIndex=0));
                                else
                                    (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex));
                                break;
                            case 1:
                                (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex));
                                break;
                            case 3:
                                if (currentColumnIndex == 5)
                                    (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex + 1));
                                else if (currentColumnIndex == 7)
                                    (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex + 1, currentColumnIndex=0));
                                else
                                    (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex));
                                break;
                        }
                    }
                    else
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex));
                }
                catch { }
                if(currentColumnIndex == 0)
                {
                    datagrid.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(50);
                        var comboBoxAdvs = datagrid.GetChildsOfType<ComboBoxAdv>();
                        ComboBoxAdv comboBoxAdv=null;
                        int i = 0;
                        foreach (var item in comboBoxAdvs)
                        {
                            var gridCell = item.GetParentOfType<GridCell>();
                            if (gridCell == datagrid.SelectionController.CurrentCellManager?.CurrentCell.Element)
                            {
                                comboBoxAdv= comboBoxAdvs[i];
                                break;
                            }
                            i++;
                        }
                        if (comboBoxAdv != null)
                        {
                            await Task.Delay(10);
                            comboBoxAdv.Focus();
                            comboBoxAdv.IsDropDownOpen = true;
                        }
                    }), DispatcherPriority.Render);
                }
                dataGrid.Focus();
            }
        }
        private void datagrid_CurrentCellEndEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs e)
        {
            isCancel = false;
            CalDebCre();

            if (window == null && datagrid.GetRecordAtRowIndex(e.RowColumnIndex.RowIndex) is RecieveMoney_Detail acDocument_Detail)
            {
                if ((CurrentCellText ?? "") != "")
                {
                    if (e.RowColumnIndex.ColumnIndex == 1)
                    {
                        var db = new ColDbEntities1();
                        var mu = mus1.Find(t => (t.AdditionalEntity as AccountSearchClass).ColMoein == CurrentCellText);
                        if (mu == null)
                        {

                        }
                        else
                        {
                            var moein = db.Moein.Find((mu.AdditionalEntity as AccountSearchClass).Id);
                            acDocument_Detail.Moein = moein;
                        }
                    }
                    else if(e.RowColumnIndex.ColumnIndex == 2)
                    {
                        var db = new ColDbEntities1();
                        var mu = mus2.Find(t => t.Value == CurrentCellText);
                        if (mu == null)
                        {

                        }
                        else
                        {
                            var preferential = db.Preferential.Find(mu.Id);
                            acDocument_Detail.Preferential = preferential;
                        }
                    }
                }
            }
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                var th = new Thread(() =>
                {
                    StateLoadView=true;
                    Thread.Sleep(30);
                    Dispatcher.Invoke(new Action(() =>
                    SetEnterToNextCell(this.CurrentRowColumnIndex)));
                    Thread.Sleep(30);
                    StateLoadView=false;
                });
                th.Start();
            }
            else
            {
                StateLoadView = true;
                datagrid.Dispatcher.BeginInvoke(new Action(async () =>
                {
                    await Task.Delay(50);
                    StateLoadView = false;
                }), DispatcherPriority.Render);
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
            searchGrid.Visibility = Visibility.Collapsed;
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
            var db = new ColDbEntities1();
            if (!AddedMode)
            {
                if (id != Guid.Empty)
                {
                    var e_Edidet = db.RecieveMoneyHeader.Find(id);
                    var header = RecieveMoneyHeaders.FirstOrDefault(o => o.Id == id);
                    header.RecieveMoney_Detail.Clear();
                    foreach (var item in e_Edidet.RecieveMoney_Detail)
                    {
                        header.RecieveMoney_Detail.Add(item);
                        SetAccountName(db, item);
                    }
                }
                AddedMode = true;                
                column1.Width = new GridLength(170);
                column2.Width = new GridLength(225);
                datagrid.AllowEditing = datagrid.AllowDeleting = true;
                datagrid.AddNewRowPosition = Syncfusion.UI.Xaml.Grid.AddNewRowPosition.Bottom;
            }
            datagrid.Visibility = Visibility.Visible;
            datagridSearch.Visibility = Visibility.Collapsed;
            gridSetting.Visibility = gridConfirm.Visibility = Visibility.Visible;
            Sf_txtMoein.HasError = Sf_txtPreferential.HasError = false;
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
            datagrid.SearchHelper.ClearSearch();
            testsearch.Text = "جستجو...";
            SearchTermTextBox.Text = "";
            dataPager.Visibility = Visibility.Collapsed;
            gridDelete.Visibility = Visibility.Hidden;
            borderEdit.Visibility = Visibility.Hidden;
            txtSerial.Text = "";
            datagrid.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));
            if (recieveMoney_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    recieveMoney_Details.Clear();
                }));
                RefreshDataGridForSetPersianNumber();
            }

            var y = db.RecieveMoneyHeader.OrderByDescending(k => k.ReceiptNumber).FirstOrDefault();
            if (y == null)
            {
                txtSerial.Text = "1";
            }
            else
            {
                var yb = db.RecieveMoneyHeader.OrderByDescending(k => k.ReceiptNumber).FirstOrDefault();
                txtSerial.Text = (y.ReceiptNumber + 1).ToString();
            }
            isCancel = true;
            id = Guid.Empty;
        }

        private void datagrid_SelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs e)
        {
            return;
            if (datagrid.SelectedItem != null && !AddedMode)
            {
                gridDelete.Visibility = Visibility.Visible;
                /*var acDocument_Detail = datagrid.SelectedItem as AcDocument_Detail;
                id = acDocument_Detail.Id;
                cmbType.TextChanged -= txtDoumentType_TextChanged;
                cmbType.Text = acDocument_Detail.tGroup.GroupCode.ToString();
                cmbType.TextChanged += txtDoumentType_TextChanged;
                txtSerial.Text = acDocument_Detail.tGroup.GroupName;
                txtNoDocumen.Text = acDocument_Detail.AcDocument_DetailName;
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
            foreach (var item in db.RecieveMoney_Detail.Where(u => u.fkHeaderId == id))
            {
                db.RecieveMoney_Detail.Remove(item);
            }
            db.RecieveMoneyHeader.Remove(db.RecieveMoneyHeader.Find(id));
            db.SaveChanges();
            try
            {
                RecieveMoneyHeaders.Remove(RecieveMoneyHeaders.First(f => f.Id == id));
            }
            catch
            {

            }
            id = Guid.Empty;
            //btnCancel_Click(null, null);
        }
        
        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTermTextBox.Text.Trim() == string.Empty)
            {
                if (FirstLevelNestedGrid.SearchHelper.SearchText.Trim() != "")
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
                var tt = item.Data as RecieveMoneyHeader;
                if (!tt.RecieveMoney_Detail.Any(i =>
                i.Name.ToLower().Contains(SearchTermTextBox.Text.ToLower()) ||
                i.Price2?.ToString().ToLower().Contains(SearchTermTextBox.Text.ToLower()) == true ||
                i.ColeMoein.ToLower().Contains(SearchTermTextBox.Text.ToLower()) ||
                i.PreferentialCode.ToLower().Contains(SearchTermTextBox.Text.ToLower()) ||
                i.BranchName?.ToString().ToLower().Contains(SearchTermTextBox.Text.ToLower()) == true||
                i.Number?.ToString().ToLower().Contains(SearchTermTextBox.Text.ToLower()) == true||
                i.GetMoneyType?.ToString().ToLower().Contains(SearchTermTextBox.Text.ToLower()) == true||
                i.Bank?.Name?.ToString().ToLower().Contains(SearchTermTextBox.Text.ToLower()) == true))
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
            var count = db.RecieveMoney_Detail.Count();
            var F = db.RecieveMoney_Detail.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                recieveMoney_Details[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "دریافت وجه");
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
            if(window!=null)
            {
                if ((window as winSearch).ParentTextBox is RecieveMoney_Detail)
                {
                    var y = (window as winSearch).ParentTextBox as RecieveMoney_Detail;
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
                else if ((window as winSearch).ParentTextBox is TextBox textBox && textBox.Tag.ToString() != "True")
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

        private void datagrid_AddNewRowInitiating(object sender, Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs e)
        {
            return;
            if(e.NewObject is RecieveMoney_Detail recieveMoney_Detail && recieveMoney_Detail.MoneyType==0 && recieveMoney_Detail.Moein==null)
            {
                
            }
            /*
            var h = acDocumentViewModel.acDocument_Details.FirstOrDefault(q => q.AcCode == ctext);
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
                    searchGrid.Visibility = Visibility.Collapsed;
                    recieveMoney_Details.Clear();
                    temp_recieveMoney_Details.Clear();
                    var header = datagridSearch.SelectedItem as RecieveMoneyHeader;
                    id = header.Id;
                    //header.RecieveMoney_Detail.ForEach(t => temp_recieveMoney_Details.Add(t.DeepClone()));
                    header.RecieveMoney_Detail.ForEach(t => recieveMoney_Details.Add(t));
                    //cmbType.SelectedItem = (cmbType.ItemsSource as List<DocumentType>).First(u => u.Id == header.DocumentType.Id);
                    pcw1.SelectedDate = new PersianCalendarWPF.PersianDate(header.Date);
                    txbCalender.Text = pcw1.SelectedDate.ToString();
                    Sf_txtMoein.HasError = Sf_txtPreferential.HasError = false;
                    datagrid.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)ColorConverter.ConvertFromString("#FF808080"));

                    txtSerial.Text = header.ReceiptNumber.ToString();
                    txtMoein.Text = $"{header.Moein.Col.ColCode}{header.Moein.MoeinCode}";
                    txbMoein.Text = header.Moein.MoeinName;
                    txtMoein.Tag = mus1.Find(t => (t.AdditionalEntity as AccountSearchClass).ColMoein == txtMoein.Text);
                    txtPreferential.Text = header.Preferential.PreferentialCode.ToString();
                    txbPreferential.Text = header.Preferential.PreferentialName;
                    txtPreferential.Tag = mus2.Find(t => t.Value == txtPreferential.Text);
                    txtDescription.Text = header.Description;

                    datagrid.AllowEditing = datagrid.AllowDeleting = true;
                    datagrid.AddNewRowPosition = Syncfusion.UI.Xaml.Grid.AddNewRowPosition.Bottom;
                    datagrid.Visibility = Visibility.Visible;
                    dataPager.Visibility = Visibility.Collapsed;
                    testsearch.Text = "جستجو...";
                    try
                    {
                        datagrid.SearchHelper.ClearSearch();
                    }
                    catch { }
                    SearchTermTextBox.TextChanged-= SearchTermTextBox_TextChanged;
                    SearchTermTextBox.Text = "";
                    SearchTermTextBox.TextChanged+= SearchTermTextBox_TextChanged;
                    datagridSearch.Visibility = Visibility.Collapsed;
                    gridSetting.Visibility = gridConfirm.Visibility = Visibility.Visible;
                    column1.Width = new GridLength(170);
                    column2.Width = new GridLength(225);
                    borderEdit.Visibility = Visibility.Visible;
                    RefreshDataGridForSetPersianNumber();
                    datagrid.SelectedIndex = recieveMoney_Details.Count - 1;
                    isCancel = true;
                    StateLoadView = true;
                    datagrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(async () =>
                {
                    await Task.Delay(200);
                    StateLoadView = false;
                    datagrid.Focus();
                }));
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
                        var e_Edidet = db.RecieveMoneyHeader.Find(id);
                        var header = RecieveMoneyHeaders.FirstOrDefault(o => o.Id == id);
                        header.RecieveMoney_Detail.Clear();
                        foreach (var item in e_Edidet.RecieveMoney_Detail)
                        {
                            header.RecieveMoney_Detail.Add(item);
                            SetAccountName(db, item);
                        }
                    }
                    datagridSearch.ClearFilters();
                    datagridSearch.SortColumnDescriptions.Clear();
                    datagridSearch.SortColumnDescriptions.Add(new SortColumnDescription()
                    {
                        ColumnName = "ReceiptNumber",
                        SortDirection = System.ComponentModel.ListSortDirection.Descending
                    });
                    datagridSearch.SearchHelper.ClearSearch();
                    FirstLevelNestedGrid.SearchHelper.ClearSearch();
                    SearchTermTextBox.Text = "";
                    datagridSearch.SelectedItem = null;
                    var t = dataPager.Source;
                    dataPager.Source = null;
                    borderEdit.Visibility = Visibility.Collapsed;
                    gridDelete.Visibility = Visibility.Visible;
                    searchGrid.Visibility = Visibility.Visible;
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
                    gridSetting.Visibility = gridConfirm.Visibility = Visibility.Collapsed;
                    if ((t as ObservableCollection<RecieveMoneyHeader>).Count == 0)
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
                    column1.Width = new GridLength(0);
                    column2.Width = new GridLength(0);
                    datagrid.AllowEditing = datagrid.AllowDeleting = false;
                    datagrid.AddNewRowPosition = Syncfusion.UI.Xaml.Grid.AddNewRowPosition.None;
                    AddedMode = false;
                    datagrid.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(() =>
                {
                    SetHide_EmptyDetails();
                }));
                    datagrid.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render,
                new Action(async () =>
                {
                    await Task.Delay(50);
                    datagridSearch.SelectedIndex = 0;
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
                foreach (var item in db.RecieveMoneyHeader.Include(y=>y.RecieveMoney_Detail).AsNoTracking().ToList())
                {
                    /*foreach (var item2 in item.RecieveMoney_Detail)
                    {
                        SetAccountName(db, item2);
                    }*/
                    RecieveMoneyHeaders.Add(item);
                }
                LoadedFill = true;
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
                var header = datagridSearch.SelectedItem as RecieveMoneyHeader;
                id = header.Id;
            }
            else if (datagrid.Visibility != Visibility.Visible)
                id = Guid.Empty;
        }

        private void datagrid_RowValidated(object sender, RowValidatedEventArgs e)
        {
            var detail = e.RowData as RecieveMoney_Detail;
            if (datagrid.SelectedIndex!=-1&& detail.MoneyType==0&&detail.Moein==null)
            {
                recieveMoney_Details.Remove(detail);
                return;
            }
            var currentCell = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
            if (window != null)
                (window as winSearch).ParentTextBox = detail;
            /*if (currentCell?.ColumnIndex == 4 && (detail.Debtor ?? 0) != 0)
                detail.Creditor = null;
            if (currentCell?.ColumnIndex == 5 && (detail.Creditor ?? 0) != 0)
                detail.Debtor = null;*/
        }

        private void AcDocument_Details_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var detail = recieveMoney_Details.LastOrDefault();
            if (detail == null)
                return;
            if ((Keyboard.IsKeyDown(Key.Enter) || datagrid.SelectedIndex != -1 || CurrentRowColumnIndex.ColumnIndex != 0) && detail.MoneyType != 3 && detail.ColeMoein == null && detail.PreferentialCode == null)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    recieveMoney_Details.Remove(detail);
                }));
            }
            datagrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                CalDebCre();
            }));
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
            if(datePicker1==null&&datagrid.Visibility== Visibility.Visible)
            {
                (datagrid.SelectedItem as RecieveMoney_Detail).Date = persianCalendar.SelectedDate.ToDateTime();
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
        RowColumnIndex CurrentRowColumnIndex;
    
        private void datagrid_CurrentCellBeginEdit(object sender, CurrentCellBeginEditEventArgs e)
        {
            if(datagrid.SelectedIndex==-1)
            {
                e.Cancel = true;
                Xceed.Wpf.Toolkit.MessageBox.Show("ابتدا باید نوع وجه  را تعیین کنید!");
            }
            else if((datagrid.SelectionController.CurrentCellManager?.CurrentCell.Element as GridCell)?.DataContext is RecieveMoney_Detail recieveMoney_Detail)
            {
                if (recieveMoney_Detail.MoneyType != 3)
                {
                    if (e.RowColumnIndex.ColumnIndex == 1 || e.RowColumnIndex.ColumnIndex == 2)
                        e.Cancel = true;
                    else if (recieveMoney_Detail.MoneyType != 1 && e.RowColumnIndex.ColumnIndex >= 5)
                        e.Cancel = true;
                }
                else if (e.RowColumnIndex.ColumnIndex == 5 || e.RowColumnIndex.ColumnIndex == 7 || e.RowColumnIndex.ColumnIndex == 8)
                    e.Cancel = true;
            }
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
            var ele = (datagrid.SelectionController.CurrentCellManager?.CurrentCell.Element as GridCell).Content;
            if (ele is TextBox textBox)
            {
                if (textBox.Text != "" && e.Record is RecieveMoney_Detail detail && detail.GetType().GetProperty(e.Column.MappingName).GetValue(detail)?.ToString() != textBox.Text && !Keyboard.IsKeyDown(Key.Enter))
                    CurrentCellText = textBox.Text;
            }
            else if(ele is CheckBox check)
            {
                if (e.Record is RecieveMoney_Detail detail && detail.MoneyType != 1 && e.RowColumnIndex.ColumnIndex >= 5)
                {
                    check.IsChecked = null;
                }
                else
                {
                    datagrid.Focus();
                }
            }
        }

        private void datagrid_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (e.RowData is RecieveMoney_Detail detail)
            {
                var dataColumn = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
                var textBox = (dataColumn.Element as GridCell).Content as TextBox;
                if (textBox == null)
                    return;
                var u = textBox.Text == "" ? CurrentCellText : textBox.Text;
                if (dataColumn.ColumnIndex == 1)
                {
                    var mu = mus1.Find(t => (t.AdditionalEntity as AccountSearchClass).ColMoein == u);
                    if (mu == null)
                    {
                        e.IsValid = false;
                        e.ErrorMessages.Add("ColeMoein", "چنین کل و معینی وجود ندارد!");
                    }
                }
                else if (dataColumn.ColumnIndex == 2)
                {
                    var mu = mus2.Find(t => t.Value == u);
                    if (mu == null)
                    {
                        e.IsValid = false;
                        e.ErrorMessages.Add("PreferentialCode", "چنین تفضیلی وجود ندارد!");
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
        bool StateLoadView = false;
        private void ComboBoxAdv_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (StateLoadView || Keyboard.IsKeyDown(Key.Delete))
                return;
            var comboBoxAdv = sender as Syncfusion.Windows.Tools.Controls.ComboBoxAdv;
            var hg = comboBoxAdv.SelectedIndex;
            switch (comboBoxAdv.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
            var v = datagrid.SelectionController.CurrentCellManager.CurrentCell;
            if (v != null)
                CurrentRowColumnIndex = new RowColumnIndex(v.RowIndex-1, v.ColumnIndex);
            if (v != null&&hg!=-1)
            {
                if (datagrid.SelectedIndex == -1)
                {
                    var gridAddNewRowController = datagrid.GetAddNewRowController();
                    try
                    {
                        gridAddNewRowController.AddNew();
                    }
                    catch { }
                    datagrid.GetAddNewRowController().CommitAddNew(true);
                    datagrid.View.Refresh();
                    (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(datagrid.GetLastRowIndex(), v.ColumnIndex + hg == 3 ? 1 : 4));
                    datagrid.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(50);
                        var dataColumn = datagrid.SelectionController.CurrentCellManager.CurrentCell;
                        if (dataColumn == null)
                            return;
                        var recieveMoney_Detail = (dataColumn.Element as GridCell).DataContext as RecieveMoney_Detail;
                        recieveMoney_Detail.MoneyType = (byte)hg;
                        recieveMoney_Detail.ClearErrors();
                        var db = new ColDbEntities1();
                        var tMoein = db.Moein.Include("Col");
                        if (db.CodeSetting.Any(t => t.Name == "MoeinCodeCheckRecieve"))
                        {
                        }
                        else
                        {
                            MessageBox.Show("تنظیمات پیکر بندی را انحام دهید!");
                            return;
                        }
                        switch (recieveMoney_Detail.MoneyType)
                        {
                            case 0:
                                var moein = int.Parse(db.CodeSetting.First(j => j.Name == "MoeinCodeMoneyRecieve").Value);
                                var col = int.Parse(db.CodeSetting.First(j => j.Name == "ColCodeMoneyRecieve").Value);
                                var p = int.Parse(db.CodeSetting.First(j => j.Name == "PreferentialCodeMoneyRecieve").Value);
                                recieveMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == moein && d.Col.ColCode == col);
                                recieveMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == p);
                                break;
                            case 1:
                                moein = int.Parse(db.CodeSetting.First(j => j.Name == "MoeinCodeCheckRecieve").Value);
                                col = int.Parse(db.CodeSetting.First(j => j.Name == "ColCodeCheckRecieve").Value);
                                p = int.Parse(db.CodeSetting.First(j => j.Name == "PreferentialCodeCheckRecieve").Value);
                                recieveMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == moein && d.Col.ColCode == col);
                                recieveMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == p);
                                break;
                            case 2:
                                moein = int.Parse(db.CodeSetting.First(j => j.Name == "MoeinCodeDiscountRecieve").Value);
                                col = int.Parse(db.CodeSetting.First(j => j.Name == "ColCodeDiscountRecieve").Value);
                                p = int.Parse(db.CodeSetting.First(j => j.Name == "PreferentialCodeDiscountRecieve").Value);
                                recieveMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == moein && d.Col.ColCode == col);
                                recieveMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == p);
                                break;
                            case 3:
                                recieveMoney_Detail.Moein = null;
                                recieveMoney_Detail.Preferential = null;
                                break;
                        }
                        datagrid.View.Refresh();
                        datagrid.Focus();
                    }), DispatcherPriority.Render);
                }
                else
                {
                    var recieveMoney_Detail = (v.Element as GridCell).DataContext as RecieveMoney_Detail;
                    if (recieveMoney_Detail != comboBoxAdv.DataContext||v.ColumnIndex!=0)
                        return;
                    recieveMoney_Detail.MoneyType = (byte)hg;
                    recieveMoney_Detail.ClearErrors();
                    var db = new ColDbEntities1();
                    var tMoein = db.Moein.Include("Col");
                    if (db.CodeSetting.Any(t => t.Name == "MoeinCodeCheckRecieve"))
                    {
                    }
                    else
                    {
                        MessageBox.Show("تنظیمات پیکر بندی را انحام دهید!");
                        return;
                    }
                    switch (recieveMoney_Detail.MoneyType)
                    {
                        case 0:
                            var moein = int.Parse(db.CodeSetting.First(j => j.Name == "MoeinCodeMoneyRecieve").Value);
                            var col = int.Parse(db.CodeSetting.First(j => j.Name == "ColCodeMoneyRecieve").Value);
                            var p = int.Parse(db.CodeSetting.First(j => j.Name == "PreferentialCodeMoneyRecieve").Value);
                            recieveMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == moein && d.Col.ColCode == col);
                            recieveMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == p);
                            break;
                        case 1:
                            moein = int.Parse(db.CodeSetting.First(j => j.Name == "MoeinCodeCheckRecieve").Value);
                            col = int.Parse(db.CodeSetting.First(j => j.Name == "ColCodeCheckRecieve").Value);
                            p = int.Parse(db.CodeSetting.First(j => j.Name == "PreferentialCodeCheckRecieve").Value);
                            recieveMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == moein && d.Col.ColCode == col);
                            recieveMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == p);
                            break;
                        case 2:
                            moein = int.Parse(db.CodeSetting.First(j => j.Name == "MoeinCodeDiscountRecieve").Value);
                            col = int.Parse(db.CodeSetting.First(j => j.Name == "ColCodeDiscountRecieve").Value);
                            p = int.Parse(db.CodeSetting.First(j => j.Name == "PreferentialCodeDiscountRecieve").Value);
                            recieveMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == moein && d.Col.ColCode == col);
                            recieveMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == p);
                            break;
                        case 3:
                            recieveMoney_Detail.Moein = null;
                            recieveMoney_Detail.Preferential = null;
                            break;
                    }
                    switch (recieveMoney_Detail.MoneyType)
                    {
                        case 0:
                        case 2:
                            recieveMoney_Detail.Bank = null;
                            recieveMoney_Detail.Date = null;
                            recieveMoney_Detail.Number = null;
                            break;                                                  
                    }
                    StateLoadView = true;
                    (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(datagrid.SelectedIndex+1, v.ColumnIndex + hg == 3 ? 1 : 4));
                    datagrid.View.Refresh();
                    datagrid.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        await Task.Delay(50);
                        StateLoadView = false;
                        datagrid.Focus();
                    }), DispatcherPriority.Render);
                }
            }
        }

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

        private winSearch ShowSearchMoein(dynamic y, Window owner = null)
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
            win.datagrid.Columns.Add(new GridTextColumn() { TextAlignment = TextAlignment.Center, HeaderText = "نام", MappingName = "AdditionalEntity.MoeinName", AllowSorting = true });
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
            win.Owner = MainWindow.Current;
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
            win.Width = 640;
            if(owner == null)
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
                if ((currentCell.Element as GridCell)?.IsMouseOver == true && currentCell.ColumnIndex == 7 && ((currentCell.Element as GridCell).Content as ContentControl)?.Content is TextBlock)
                {
                    var recieveMoney_Detail = (currentCell.Element as GridCell).DataContext as RecieveMoney_Detail;
                    if (recieveMoney_Detail == null)
                        return;
                    if (!(recieveMoney_Detail.MoneyType == 0 || recieveMoney_Detail.MoneyType == 2))
                    {
                        e.Handled = true;
                        keybd_event(VK_F2, 0, 0, UIntPtr.Zero); // فشار دادن کلید
                        Thread.Sleep(50); // تاخیر برای شبیه‌سازی فشار دادن
                        keybd_event(VK_F2, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // آزاد کردن کلید
                    }
                }
                if (currentCell.ColumnIndex == 10)
                {
                    //Dispatcher.BeginInvoke(new Action(async () =>
                    //{
                    //    await Task.Delay(300);
                    //    datagrid.Focus();
                    //}), DispatcherPriority.Render);
                }
            }           
        }

        private void ComboBoxAdv_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBoxAdv = sender as Syncfusion.Windows.Tools.Controls.ComboBoxAdv;
            if(comboBoxAdv.DataContext is RecieveMoney_Detail recieveMoney_Detail)
            {
                comboBoxAdv.SelectedIndex = recieveMoney_Detail.MoneyType;              
            }
        }

        private void ComboBoxAdv_DropDownOpened(object sender, EventArgs e)
        {
            var comboBoxAdv = sender as Syncfusion.Windows.Tools.Controls.ComboBoxAdv;
            if (comboBoxAdv.DataContext ==null&&comboBoxAdv.SelectedIndex!=-1)
            {
                comboBoxAdv.SelectedIndex = -1;
            }
        }
        private void ComboBoxAdv_GotFocus(object sender, RoutedEventArgs e)
        {
            var comboBoxAdv = sender as Syncfusion.Windows.Tools.Controls.ComboBoxAdv;
            if (comboBoxAdv.DataContext==null)
            {
                comboBoxAdv.SelectedIndex = -1;
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
                datagridSearch.ExpandAllDetailsView();
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            var win = new winSettingCode();
            var db = new ColDbEntities1();
            var exist = false;
            if (db.CodeSetting.Any(t => t.Name == "MoeinCodeCheckRecieve"))
            {
                exist = true;
            }
            GroupBox groupBox = SettingDefinitionGroupBox(win, db, exist, "چک", "ColCodeCheckRecieve", "MoeinCodeTransferLCheckRecieve", "PreferentialCodeCheckRecieve");
            win.stack.Children.Add(groupBox);
            groupBox = SettingDefinitionGroupBox(win, db, exist, "نقد", "ColCodeMoneyRecieve", "MoeinCodeMoneyRecieve", "PreferentialCodeMoneyRecieve");
            win.stack.Children.Add(groupBox);

            groupBox = SettingDefinitionGroupBox(win, db, exist, "تخفیف", "ColCodeDiscountRecieve", "MoeinCodeDiscountRecieve", "PreferentialCodeDiscountRecieve");
            win.stack.Children.Add(groupBox);
            win.ShowDialog();
        }

        private GroupBox SettingDefinitionGroupBox(winSettingCode win, ColDbEntities1 db, bool exist,string name,string str1,string str2,string str3)
        {
            var groupBox = new GroupBox() { Header = name};
            var stackPanel = new StackPanel();
            groupBox.Content = stackPanel;

            var keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add(str1, exist ? db.CodeSetting.First(i => i.Name == str1).Value : "");
            keyValuePairs.Add(str2, exist ? db.CodeSetting.First(i => i.Name == str2).Value : "");

            var textInputLayout = new SfTextInputLayout() { Tag = keyValuePairs, Hint = "کد کل و معین " };
            var textBox = new TextBox() { Text = exist ? keyValuePairs.ElementAt(0).Value + keyValuePairs.ElementAt(1).Value : "", Tag = true };
            textInputLayout.InputView = textBox;
            textBox.PreviewKeyDown += (s1, e1) =>
            {
                if (e1.Key == Key.F1)
                {
                    win.childWindow = ShowSearchMoein(s1, win);
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

            textInputLayout = new SfTextInputLayout() { Tag = str3, Hint = "کد تفضیل" };
            textBox = new TextBox() { Text = exist ? db.CodeSetting.First(i => i.Name == str3).Value : "", Tag = true };
            textInputLayout.InputView = textBox;
            textBox.PreviewKeyDown += (s1, e1) =>
            {
                if (e1.Key == Key.F1)
                {
                    win.childWindow = ShowSearchPreferential(s1, win);
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
            return groupBox;
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
