﻿using Mahdi.PersianDateControls;
using PersianCalendarWPF;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
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
    public partial class usrPaymentMoney : UserControl,ITabForm, ITabEdidGrid
    {
        public bool DataGridIsFocused
        {
            get
            {
                return datagrid.IsFocused;
            }
        }
        PaymentMoneyViewModel acDocumentViewModel;
        ObservableCollection<Bank> Banks = new ObservableCollection<Bank>();
        List<Mu> mus1 = new List<Mu>();
        List<Mu> mus2 = new List<Mu>();
        public usrPaymentMoney()
        {
            temp_paymentMoney_Details = new ObservableCollection<PaymentMoney_Detail>();
            PaymentMoneyHeaders = new ObservableCollection<PaymentMoneyHeader>();
            InitializeComponent();
            acDocumentViewModel = Resources["viewmodel"] as PaymentMoneyViewModel;
            acDocumentViewModel.paymentMoney_Details.CollectionChanged += AcDocument_Details_CollectionChanged;
            txbCalender.Text = pcw1.SelectedDate.ToString();
        }

        Brush brush = null;
        public ObservableCollection<PaymentMoney_Detail> temp_paymentMoney_Details { get; set; }
        public ObservableCollection<PaymentMoney_Detail> paymentMoney_Details { get; set; }
        public ObservableCollection<PaymentMoneyHeader> PaymentMoneyHeaders { get; set; }
        private void Txt_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                //if ((sender as TextBox).Name == "txtNoDocumen")
                //{
                //    cmbType.Focus();
                //}
                //else
                //{
                    TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                    request.Wrapped = true;
                    (sender as TextBox).MoveFocus(request);
                //}
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
                paymentMoney_Details = acDocumentViewModel.paymentMoney_Details;
                //AcDocument_Details.Clear();
                var y = db.PaymentMoneyHeader.OrderByDescending(k => k.ReceiptNumber).FirstOrDefault();
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
                paymentMoney_Details = acDocumentViewModel.paymentMoney_Details;
                paymentMoney_Details.Clear();
                //AcDocument_Details.Clear();
                var h = db.PaymentMoney_Detail.Where(u=>u.fkHeaderId==id).ToList();
                h.ForEach(u => paymentMoney_Details.Add(u));
                RefreshDataGridForSetPersianNumber();
            }
            if (PaymentMoneyHeaders.Count == 0)
            {
                foreach (var item in db.PaymentMoneyHeader.ToList())
                {
                    /*foreach (var item2 in item.PaymentMoney_Detail)
                    {
                        SetAccountName(db, item2);
                    }*/
                    PaymentMoneyHeaders.Add(item);
                }
            }
            dataPager.Source = null;
            dataPager.Source = PaymentMoneyHeaders;
            datagrid.SearchHelper.AllowFiltering = true;
            datagridSearch.SearchHelper.AllowFiltering = true;
            FirstLevelNestedGrid.SearchHelper.AllowFiltering = true;
            isCancel = true;
            txtMoein.Focus();
        }

        private static void SetAccountName(ColDbEntities1 db, PaymentMoney_Detail item2)
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
                                    //var PaymentMoney_Detail = db.PaymentMoney_Detail.Find(id);
            
                                    //var nPaymentMoney_Detail = db.PaymentMoney_Detail.FirstOrDefault(g => g.fk_GroupId == col.Id && g.PaymentMoney_DetailName == txtNoDocumen.Text);
                                    //if (PaymentMoney_Detail?.Id != nPaymentMoney_Detail?.Id && nPaymentMoney_Detail != null)
                                    //{
                                    //    Xceed.Wpf.Toolkit.MessageBox.Show("این نام تفضیلی و کد گروه از قبل وجود داشته است!");
                                    //    return;
                                    //}
            
            PaymentMoneyHeader e_addHeader = null;
            PaymentMoneyHeader header = null;
            if (id == Guid.Empty)
            {
                e_addHeader = new PaymentMoneyHeader()
                {
                    Id = Guid.NewGuid(),
                    Date=pcw1.SelectedDate.ToDateTime(),
                    ReceiptNumber = int.Parse(txtSerial.Text),
                    Description = txtDescription.Text,
                    fk_MoeinId=(txtMoein.Tag as Mu).Id,
                    fk_PreferentialId=(txtPreferential.Tag as Mu).Id
                };
                DbSet<PaymentMoney_Detail> details = null;
                foreach (var item in paymentMoney_Details)
                {
                    var en = new PaymentMoney_Detail()
                    {
                        fk_MoeinId = item.Moein.Id,
                        fk_PreferentialId = item.Preferential.Id,
                        PaymentMoneyHeader = e_addHeader,
                        fkBank = item.Bank?.Id,
                        BranchName = item.BranchName,
                        Date = item.Date,
                        Number = item.Number,
                        Price = item.Price,
                        MoneyType = item.MoneyType,
                        Id = Guid.NewGuid()
                    };
                    db.PaymentMoney_Detail.Add(en);
                }
                db.PaymentMoneyHeader.Add(e_addHeader);
                PaymentMoneyHeaders.Add(e_addHeader);
                e_addHeader.Moein = db.Moein.Find((txtMoein.Tag as Mu).Id);
                e_addHeader.Preferential = db.Preferential.Find((txtPreferential.Tag as Mu).Id);
            }
            else
            {
                var h = db.PaymentMoney_Detail.Where(v => v.fkHeaderId == id);
                header = PaymentMoneyHeaders.First(u => u.Id == id);
                foreach (var item in h)
                {
                    db.PaymentMoney_Detail.Remove(item);
                    header.PaymentMoney_Detail.Remove(header.PaymentMoney_Detail.First(x => x.Id == item.Id));
                }
                var e_Edidet = db.PaymentMoneyHeader.Find(id);
                e_Edidet.ReceiptNumber= header.ReceiptNumber = int.Parse(txtSerial.Text);
                e_Edidet.Description= header.Description = txtDescription.Text;
                e_Edidet.fk_MoeinId = (txtMoein.Tag as Mu).Id;
                header.Moein = db.Moein.Find((txtMoein.Tag as Mu).Id);
                e_Edidet.fk_PreferentialId = (txtPreferential.Tag as Mu).Id;
                header.Preferential = db.Preferential.Find((txtPreferential.Tag as Mu).Id);
                e_Edidet.Date = header.Date = pcw1.SelectedDate.ToDateTime();
                foreach (var item in paymentMoney_Details)
                {
                    var en = new PaymentMoney_Detail()
                    {
                        fkHeaderId = header.Id,
                        fk_MoeinId = item.Moein.Id,
                        fk_PreferentialId = item.Preferential.Id,
                        PaymentMoneyHeader = e_addHeader,
                        fkBank = item.Bank?.Id,
                        BranchName = item.BranchName,
                        Date = item.Date,
                        Number = item.Number,
                        Price = item.Price,
                        MoneyType = item.MoneyType,
                        Id = Guid.NewGuid()
                    };
                    db.PaymentMoney_Detail.Add(en);
                    header.PaymentMoney_Detail.Add(en);
                }
            }
            db.SaveChanges();
            if (header != null)
            {
                int i = 0;
                foreach (var item in header.PaymentMoney_Detail)
                {
                    item.Moein = paymentMoney_Details[i].Moein;
                    item.Preferential = paymentMoney_Details[i].Preferential;
                    item.Bank = paymentMoney_Details[i].Bank;
                    i++;
                }
            }
            if(e_addHeader!=null)
            {
                int i = 0;
                foreach (var item in e_addHeader.PaymentMoney_Detail)
                {
                    item.Moein = paymentMoney_Details[i].Moein;
                    item.Preferential = paymentMoney_Details[i].Preferential;
                    item.Bank = paymentMoney_Details[i].Bank;
                    i++;
                }
            }
            datagrid.SelectedIndex = -1;
            datagrid.ClearFilters();
            datagrid.SearchHelper.ClearSearch();
            if (paymentMoney_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    paymentMoney_Details.Clear();
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
            if (paymentMoney_Details.Count == 0)//AcDocument_Details.Any(g => !viewModel.AllCommodities.Any(y => y.CommodityCode == g.CommodityCode)))
            {
                datagrid.BorderBrush = Brushes.Red;
                haserror = true;
            }
            else if (paymentMoney_Details.Any(t =>t.Price==0 || t.ColeMoein == ""|| t.ColeMoein==null || t.PreferentialCode == "" || t.PreferentialCode == null ))
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
                if (datagrid.SelectionController.CurrentCellManager?.CurrentCell?.ColumnIndex == 1)
                {
                    dynamic y = null;
                    var element = (datagrid.SelectionController.CurrentCellManager.CurrentCell.Element as GridCell)
                            .Content as FrameworkElement;
                    y = element.DataContext;
                    if (element is TextBlock)
                    {
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
                            datagrid_PreviewKeyDown(sender, e));
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
                    ShowSearchPreferential(y);
                    datagrid.IsHitTestVisible = false;
                }
            }           
        }
        public void SetEnterToNextCell(RowColumnIndex? rowColumn=null)
        {
            var dataGrid = datagrid;

            // پیدا کردن سطر و ستون فعلی
            var currentCell = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
            if (currentCell != null||rowColumn!=null)
            {
                int currentRowIndex = rowColumn==null? currentCell.RowIndex:rowColumn.Value.RowIndex;
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
                if (currentRowIndex >= paymentMoney_Details.Count+2)
                {
                    currentRowIndex = 0; // به اولین سطر برگردید
                }

                //Updates the PressedRowColumnIndex value in the GridBaseSelectionController.
                try
                {
                    if (currentColumnIndex == 2)
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex+1));
                    else if (currentColumnIndex == 5 && ((datagrid.GetRecordAtRowIndex(currentRowIndex) as AcDocument_Detail)?.Debtor ?? 0) != 0)
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex+1, 0));
                    else
                        (this.datagrid.SelectionController as GridSelectionController).MoveCurrentCell(new RowColumnIndex(currentRowIndex, currentColumnIndex));
                }
                catch { }
            }
        }
        private void datagrid_CurrentCellEndEdit(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs e)
        {
            isCancel = false;
            CalDebCre();

            if (window == null && datagrid.GetRecordAtRowIndex(e.RowColumnIndex.RowIndex) is PaymentMoney_Detail acDocument_Detail)
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
                    Thread.Sleep(30);
                    Dispatcher.Invoke(new Action(() =>
                    SetEnterToNextCell(this.CurrentRowColumnIndex)));
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
                    var e_Edidet = db.PaymentMoneyHeader.Find(id);
                    var header = PaymentMoneyHeaders.FirstOrDefault(o => o.Id == id);
                    header.PaymentMoney_Detail.Clear();
                    foreach (var item in e_Edidet.PaymentMoney_Detail)
                    {
                        header.PaymentMoney_Detail.Add(item);
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
            gridConfirm.Visibility = Visibility.Visible;
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
            if (paymentMoney_Details.Count > 0)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    paymentMoney_Details.Clear();
                }));
                RefreshDataGridForSetPersianNumber();
            }

            var y = db.PaymentMoneyHeader.OrderByDescending(k => k.ReceiptNumber).FirstOrDefault();
            if (y == null)
            {
                txtSerial.Text = "1";
            }
            else
            {
                var yb = db.PaymentMoneyHeader.OrderByDescending(k => k.ReceiptNumber).FirstOrDefault();
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
            if (Xceed.Wpf.Toolkit.MessageBox.Show("آیا می خواهید این اطلاعات پاک شود؟", "حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }
            var db = new ColDbEntities1();
            paymentMoney_Details.ForEach(o =>
            db.PaymentMoney_Detail.Remove(db.PaymentMoney_Detail.Find(o.Id)));
            db.PaymentMoneyHeader.Remove(db.PaymentMoneyHeader.Find(id));
            db.SaveChanges();
            paymentMoney_Details.Remove((datagrid.SelectedItem as PaymentMoney_Detail));
            try
            {
                PaymentMoneyHeaders.Remove(PaymentMoneyHeaders.First(f => f.Id == id));
            }
            catch
            {

            }
            btnCancel_Click(null, null);
        }
        
        private void SearchTermTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
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
                        dataPager.Visibility = Visibility.Collapsed;
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
                var tt = item.Data as PaymentMoneyHeader;
                if (!tt.PaymentMoney_Detail.Any(i =>
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
            var count = db.PaymentMoney_Detail.Count();
            var F = db.PaymentMoney_Detail.OrderBy(d=>d.Id).Skip(10 * e.NewPageIndex).Take(10).ToList();
            int j = 0;
            for (int i = 10 * e.NewPageIndex; i < 10 * (e.NewPageIndex + 1)&&i<count; i++)
            {
                paymentMoney_Details[i] = F[j];
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
            var item = list.FirstOrDefault(u => u.Header == "سند حسابداری");
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
                if ((window as winSearch).ParentTextBox is PaymentMoney_Detail)
                {
                    var y = (window as winSearch).ParentTextBox as PaymentMoney_Detail;
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
            if(e.NewObject is PaymentMoney_Detail paymentMoney_Detail && paymentMoney_Detail.MoneyType==0 && paymentMoney_Detail.Moein==null)
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
                    gridDelete.Visibility = Visibility.Visible;
                    paymentMoney_Details.Clear();
                    temp_paymentMoney_Details.Clear();
                    var header = datagridSearch.SelectedItem as PaymentMoneyHeader;
                    id = header.Id;
                    //header.PaymentMoney_Detail.ForEach(t => temp_paymentMoney_Details.Add(t.DeepClone()));
                    header.PaymentMoney_Detail.ForEach(t => paymentMoney_Details.Add(t));
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
                    gridConfirm.Visibility = Visibility.Visible;
                    column1.Width = new GridLength(170);
                    column2.Width = new GridLength(225);
                    borderEdit.Visibility = Visibility.Visible;
                    RefreshDataGridForSetPersianNumber();
                    datagrid.SelectedIndex = paymentMoney_Details.Count - 1;
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
                    if(!AddedMode)
                    {
                        var db = new ColDbEntities1();
                        var e_Edidet = db.PaymentMoneyHeader.Find(id);
                        var header = PaymentMoneyHeaders.FirstOrDefault(o => o.Id == id);
                        header.PaymentMoney_Detail.Clear();
                        foreach (var item in e_Edidet.PaymentMoney_Detail)
                        {
                            header.PaymentMoney_Detail.Add(item);
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
                    borderEdit.Visibility = gridDelete.Visibility = Visibility.Collapsed;
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
                    if ((t as ObservableCollection<PaymentMoneyHeader>).Count == 0)
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
                searchImage.Opacity = 1;
        }

        private void datagrid_RowValidated(object sender, RowValidatedEventArgs e)
        {
            var detail = e.RowData as PaymentMoney_Detail;
            if (datagrid.SelectedIndex!=-1&& detail.MoneyType==0&&detail.Moein==null)
            {
                paymentMoney_Details.Remove(detail);
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
            var detail = paymentMoney_Details.LastOrDefault();
            if (detail == null)
                return;
            if ((Keyboard.IsKeyDown(Key.Enter) || datagrid.SelectedIndex != -1 || CurrentRowColumnIndex.ColumnIndex != 0) && detail.MoneyType != 3 && detail.ColeMoein == null && detail.PreferentialCode == null)
            {
                datagrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    paymentMoney_Details.Remove(detail);
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
            datePicker1.SelectedDate = persianCalendar.SelectedDate.ToDateTime();
            var persian = new System.Globalization.PersianCalendar();
            var h = $"{persian.GetYear(datePicker1.SelectedDate.Value)}/{persian.GetMonth(datePicker1.SelectedDate.Value)}/{persian.GetDayOfMonth(datePicker1.SelectedDate.Value)}";
            ((MyPopupS.Parent as Grid).Children[1] as TextBox).Text = h;
        }

        private void persianCalendarE_SelectedDateChanged(object sender, RoutedEventArgs e)
        {
            MyPopupE.IsOpen = false;
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
            else if((datagrid.SelectionController.CurrentCellManager?.CurrentCell.Element as GridCell)?.DataContext is PaymentMoney_Detail paymentMoney_Detail)
            {
                if (paymentMoney_Detail.MoneyType != 3)
                {
                    if (e.RowColumnIndex.ColumnIndex == 1 || e.RowColumnIndex.ColumnIndex == 2)
                        e.Cancel = true;                    
                    else if(paymentMoney_Detail.MoneyType!=1&& e.RowColumnIndex.ColumnIndex>=5&& e.RowColumnIndex.ColumnIndex<=7)
                        e.Cancel = true;
                }
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
            var textBox = (datagrid.SelectionController.CurrentCellManager?.CurrentCell.Element as GridCell).Content as TextBox;
            if (textBox.Text != "" && e.Record is PaymentMoney_Detail detail && detail.GetType().GetProperty(e.Column.MappingName).GetValue(detail)?.ToString() != textBox.Text && !Keyboard.IsKeyDown(Key.Enter))
                CurrentCellText = textBox.Text;
        }

        private void datagrid_RowValidating(object sender, RowValidatingEventArgs e)
        {
            if (e.RowData is PaymentMoney_Detail detail)
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
                        var paymentMoney_Detail = (dataColumn.Element as GridCell).DataContext as PaymentMoney_Detail;
                        paymentMoney_Detail.MoneyType = (byte)hg;
                        var db = new ColDbEntities1();
                        var tMoein = db.Moein.Include("Col");
                        switch (paymentMoney_Detail.MoneyType)
                        {
                            case 0:
                                paymentMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == 1 && d.Col.ColCode == 12);
                                paymentMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == 140001);
                                break;
                            case 1:
                                paymentMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == 1 && d.Col.ColCode == 12);
                                paymentMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == 140002);
                                break;
                            case 2:
                                paymentMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == 1 && d.Col.ColCode == 12);
                                paymentMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == 140003);
                                break;
                            case 3:
                                paymentMoney_Detail.Moein = null;
                                paymentMoney_Detail.Preferential = null;
                                break;
                        }
                        datagrid.View.Refresh();
                    }), DispatcherPriority.Render);
                }
                else
                {
                    var paymentMoney_Detail = (v.Element as GridCell).DataContext as PaymentMoney_Detail;
                    if (paymentMoney_Detail != comboBoxAdv.DataContext||v.ColumnIndex!=0)
                        return;
                    paymentMoney_Detail.MoneyType = (byte)hg;                    
                    var db = new ColDbEntities1();
                    var tMoein = db.Moein.Include("Col");
                    switch (paymentMoney_Detail.MoneyType)
                    {
                        case 0:
                            paymentMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == 1 && d.Col.ColCode == 12);
                            paymentMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == 140001);
                            break;
                        case 1:
                            paymentMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == 1 && d.Col.ColCode == 12);
                            paymentMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == 140002);
                            break;
                        case 2:
                            paymentMoney_Detail.Moein = tMoein.First(d => d.MoeinCode == 1 && d.Col.ColCode == 12);
                            paymentMoney_Detail.Preferential = db.Preferential.First(d => d.PreferentialCode == 140003);
                            break;
                        case 3:
                            paymentMoney_Detail.Moein = null;
                            paymentMoney_Detail.Preferential = null;
                            break;
                    }
                    switch (paymentMoney_Detail.MoneyType)
                    {
                        case 0:
                        case 2:
                            paymentMoney_Detail.Bank = null;
                            paymentMoney_Detail.Date = null;
                            paymentMoney_Detail.Number = null;
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

        private void ShowSearchMoein(dynamic y)
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
            win.Tag = this;
            win.ParentTextBox = y;
            win.SearchTermTextBox.Text = "";
            win.SearchTermTextBox.Select(1, 0);
            win.Owner = MainWindow.Current;
            window = win;
            win.Show();
            win.Focus();
        }

        private void txtPreferential_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                txtPreferential.Tag = true;
                ShowSearchPreferential(txtPreferential);
            }
        }

        private void ShowSearchPreferential(dynamic y)
        {
            var win = new winSearch(mus2);
            win.Closed += (yf, rs) =>
            {
                datagrid.IsHitTestVisible = true;
            };
            win.Width = 640;
            win.Tag = this;
            win.ParentTextBox = y;
            win.SearchTermTextBox.Text = "";
            win.SearchTermTextBox.Select(1, 0);
            win.Owner = MainWindow.Current;
            window = win;
            win.Show();
            win.Focus();
        }

        private void datagrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentCell = datagrid.SelectionController.CurrentCellManager?.CurrentCell;
            if (currentCell != null && (currentCell.Element as GridCell)?.IsMouseOver == true && currentCell.ColumnIndex == 7 && ((currentCell.Element as GridCell).Content as ContentControl)?.Content is TextBlock)
            {
                var paymentMoney_Detail = (currentCell.Element as GridCell).DataContext as PaymentMoney_Detail;
                if (paymentMoney_Detail == null)
                    return;
                if (!(paymentMoney_Detail.MoneyType == 0 || paymentMoney_Detail.MoneyType == 2))
                {
                    e.Handled = true;
                    keybd_event(VK_F2, 0, 0, UIntPtr.Zero); // فشار دادن کلید
                    Thread.Sleep(50); // تاخیر برای شبیه‌سازی فشار دادن
                    keybd_event(VK_F2, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // آزاد کردن کلید
                }
            }
        }

        private void ComboBoxAdv_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBoxAdv = sender as Syncfusion.Windows.Tools.Controls.ComboBoxAdv;
            if(comboBoxAdv.DataContext is PaymentMoney_Detail paymentMoney_Detail)
            {
                comboBoxAdv.SelectedIndex = paymentMoney_Detail.MoneyType;              
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