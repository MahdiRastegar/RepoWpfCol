using Syncfusion.Linq;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfCol.Interfaces;
using XamlAnimatedGif;

namespace WpfCol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Current;
        public MainWindow()
        {
            InitializeComponent();
            ribbon.RibbonState = Syncfusion.Windows.Tools.RibbonState.Hide;
            Current = this;
            var gifImage = new BitmapImage(new Uri("pack://application:,,,/Images/AddDataLarge.gif"));
            XamlAnimatedGif.AnimationBehavior.SetSourceUri(this.gifImage, gifImage.UriSource);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var M = new winCol();
            //M.ShowDialog();
        }

        private void BtnMoein_Click(object sender, RoutedEventArgs e)
        {
            var M = new winMoein();
            //M.ShowDialog();
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            var u = "3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823066470938446095505822317253594081284811174502841027019385211055596446229489549303819644288109756659334461284756482337867831652712019091456485669234603486104543266482133936072602491412737245870066063155881748815209209628292540917153643678925903600113305305488204665213841469519415116094330572703657595919530921861173819326117931051185480744623799627495673518857527248912279381830119491";
            var y = u.Substring(0, 42);
            var a = "3.1415926535897932384626433832795028841971693993751058209749445923078164062862089469509070248424167755216229746776236362716129014677044253241618530641369520079400145492904783927346278766000080780449455";
            var b = a.Substring(0, 42);

        }
        public IEnumerable<TabItemExt> GetTabControlItems
        {
            get
            {
                return tabcontrol.Items.ToList<TabItemExt>();
            }
        }

        private void rbnCol_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "حساب کل");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "حساب کل" };                
                item.Content = new winCol();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnMoein_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "حساب معین");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "حساب معین" };
                item.Content = new winMoein();
                tabcontrol.Items.Add(item);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!LoadedWin)
                return;
            double newWidth = e.NewSize.Width;
            double newHeight = e.NewSize.Height;
            ribbon.Margin = new Thickness(ribbon.Margin.Left + (newWidth - e.PreviousSize.Width) / 4.33, 0, 0, 0);
            /*var t = ((ribbon.RenderTransform as TransformGroup).Children[3] as TranslateTransform);
            t.X += (newWidth - e.PreviousSize.Width)/3.3333333;*/
        }
        bool LoadedWin = false;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadedWin = true;
            WindowState = WindowState.Maximized;
        }

        private void tabcontrol_TabClosed(object sender, CloseTabEventArgs e)
        {
            (e.TargetTabItem.Content as IDisposable)?.Dispose();
            tabcontrol.Items.Remove(e.TargetTabItem);
        }

        private void tabcontrol_TabClosing(object sender, CancelingRoutedEventArgs e)
        {
            this.Effect = new BlurEffect() { Radius = 4 };
            e.Cancel = !((e.OriginalSource as TabItemExt).Content as ITabForm).CloseForm();
            this.Effect = null;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

        }

        private void ribbon_RibbonStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(ribbon.RibbonState== Syncfusion.Windows.Tools.RibbonState.Hide)
            {
                row.Height = new GridLength();
            }
            else
                row.Height = new GridLength(197);
        }

        private void rbnGroup_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "گروه تفضیلی");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "گروه تفضیلی" };
                item.Content = new usrGroup();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnPreferential_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "حساب تفضیلی");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "حساب تفضیلی" };                
                item.Content = new usrPreferential();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnAGroup_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "گروه حساب");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "گروه حساب" };
                item.Content = new usrAGroup();
                tabcontrol.Items.Add(item);
            }
        }

        private void tabcontrol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ribbon.RibbonState = Syncfusion.Windows.Tools.RibbonState.Hide;
            row.Height = new GridLength();            
        }

        private void rbnAcType_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "نوع سند");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "نوع سند" };
                item.Content = new usrAcType();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnAcDoc_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "سند حسابداری");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "سند حسابداری" };
                item.Content = new usrAccountDocument();
                tabcontrol.Items.Add(item);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((tabcontrol.SelectedItem as TabItemExt)?.Content is ITabEdidGrid usrAccountDocument)
            {
                if (usrAccountDocument.DataGridIsFocused && e.Key == Key.Enter)
                {
                    usrAccountDocument.SetEnterToNextCell();
                    e.Handled = true;
                    return;
                }
            }
            if ((tabcontrol.SelectedItem as TabItemExt)?.Content is UserControl userControl)
            {
                if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) && Keyboard.IsKeyDown(Key.S))
                {
                    Type type = userControl.GetType();

                    MethodInfo method = type.GetMethod("btnConfirm_Click", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (method != null)
                    {
                        object[] parameters = new object[] { null, null };

                        method.Invoke(userControl, parameters);
                    }
                }
            }
        }

        private void rbnBank_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "بانک");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "بانک" };
                item.Content = new usrBank();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnRecieveMoney_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "دریافت وجه");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "دریافت وجه" };
                item.Content = new usrRecieveMoney();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnPaymentMoney_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "پرداخت وجه");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "پرداخت وجه" };
                item.Content = new usrPaymentMoney();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnRecieveCheck_Click(object sender, RoutedEventArgs e)
        {
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "چک های دریافتی");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "چک های دریافتی" };
                item.Content = new usrRecieveCheck();
                tabcontrol.Items.Add(item);
            }
        }

        private void rbnPaymentCheck_Click(object sender, RoutedEventArgs e)
        {
            return;
            var list = GetTabControlItems;
            var item = list.FirstOrDefault(y => y.Header == "چک های پرداختی");
            if (item != null)
            {
                tabcontrol.SelectedItem = item;
            }
            else
            {
                item = new TabItemExt() { Header = "چک های پرداختی" };
                item.Content = new usrPaymentCheck();
                tabcontrol.Items.Add(item);
            }
        }
    }
}
