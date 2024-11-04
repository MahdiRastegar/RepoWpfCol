using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.Xml.Serialization;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace WpfCol
{
    public static class ExtensionMethods
    {
        public static List<string> CompareObjects<T>(T obj1, T obj2)
        {
            List<string> result = new List<string>();
            if (obj1 == null || obj2 == null)
            {
                return null;
            }

            // گرفتن تمام پروپرتی‌های کلاس
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    // گرفتن مقادیر پروپرتی برای هر شیء
                    object value1 = property.GetValue(obj1);
                    object value2 = property.GetValue(obj2);

                    // مقایسه مقادیر
                    if (!Equals(value1, value2))
                    {
                        result.Add(property.Name);
                    }
                }
                catch { continue; }
            }
            return result;
        }
        public static bool SafeSaveChanges(this DbContext  dbContext)
        {
            try
            {
                dbContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException?.InnerException != null && dbEx.InnerException.InnerException.Message.Contains("FK_"))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("این حساب دارای گردش است ابتدا باید گردش و حسابهای وابسته آن پاک شود!", "خطای پایگاه داده", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                throw new Exception(dbEx.InnerException?.Message ?? dbEx.Message);
            }
            catch (Exception ex)
            {
                if(ex.Message.Contains("foreign-key"))
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("این حساب دارای گردش است ابتدا باید گردش و حسابهای وابسته آن پاک شود!", "خطای پایگاه داده", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                throw new Exception(ex.Message);
            }
        }
        public static void AddUniqueItem(this List<string> strings,string a)
        {
            if(!strings.Contains(a))
                strings.Add(a);
        }
        public static object GetValTypeOfCell(XLCellValue? xLCell, Type type,bool isRequied=true)
        {
            if (!xLCell.HasValue)
                return null;
            if (xLCell.Value.IsBlank)
            {
                if (isRequied)
                    throw new Exception("مقدار فیلد اجباری پر نشده است");
                return null;
            }
            if (type.FullName.Contains("Int32"))
                return int.Parse(xLCell.ToString());
            else if (type.FullName.Contains("Int64"))
                return long.Parse(xLCell.ToString());
            else if (type.FullName.Contains("Byte"))
                return byte.Parse(xLCell.ToString());
            else if (type.FullName.Contains("String"))
                return xLCell.ToString();
            else if (type.FullName.Contains("Decimal"))
                return decimal.Parse(xLCell.ToString());
            else if (type.FullName.Contains("Double"))
                return double.Parse(xLCell.ToString());
            else if (type.FullName.Contains("Boolean"))
                return bool.Parse(xLCell.ToString());
            /*else if (xLCell.Value.IsBoolean)
                return bool.Parse(xLCell.ToString());*/
            return null;
        }
        public static string ToStringNumEn(this string str)
        {
            if (str == null)
                return null;
            var result = "";
            foreach (char c in str) 
            {
                result += GetChar(c);
            }
            return result;
        }
        public static DateTime ToPersianDate(this DateTime dateTime)
        {
            if (dateTime.Year < 2000)
                return dateTime;
            var pc = new PersianCalendar();
            return new DateTime(pc.GetYear(dateTime), pc.GetMonth(dateTime), pc.GetDayOfMonth(dateTime));
        }
        public static string ToPersianDateString(this DateTime dateTime)
        {                        
            var pc = new PersianCalendar();
            return $"{pc.GetYear(dateTime)}/{pc.GetMonth(dateTime)}/{pc.GetDayOfMonth(dateTime)}";
        }
        public static DateTime ToPersianDateTime(this DateTime dateTime)
        {
            if (dateTime.Year < 2000)
                return dateTime;
            var pc = new PersianCalendar();
            return new DateTime(pc.GetYear(dateTime), pc.GetMonth(dateTime), pc.GetDayOfMonth(dateTime),pc.GetHour(dateTime), pc.GetMinute(dateTime), pc.GetSecond(dateTime),pc);
        }
        public static string ToPersianDateTimeString(this DateTime dateTime)
        {
            if (dateTime.Year < 2000)
                return dateTime.ToShortDateString();
            var pc = new PersianCalendar();
            return $"{pc.GetYear(dateTime)}/{pc.GetMonth(dateTime)}/{pc.GetDayOfMonth(dateTime)}";
        }
        public static DateTime ToPersianDateTime(this string dateTimestring)
        {
            var strs = dateTimestring.Split('/');
            var dateTime = new DateTime(int.Parse(strs[0]), int.Parse(strs[1]), int.Parse(strs[2]));     
            var pc = new PersianCalendar();
            return new DateTime(pc.GetYear(dateTime), pc.GetMonth(dateTime), pc.GetDayOfMonth(dateTime), pc.GetHour(dateTime), pc.GetMinute(dateTime), pc.GetSecond(dateTime));
        }
        public static DateTime ToDateTimeOfString(this string dateTimestring)
        {
            var strs = dateTimestring.Split('/');
            var pc = new PersianCalendar();
            return new DateTime(int.Parse(strs[0]), int.Parse(strs[1]), int.Parse(strs[2]), pc);
        }
        public static DateTime ToEnglishDateTime(this string dateTimestring)
        {
            var strs = dateTimestring.Split('/');
 
            var pc = new PersianCalendar();
            return new DateTime(int.Parse(strs[0]), int.Parse(strs[1]), int.Parse(strs[2]),pc);
        }
        public static DateTime ToCurrentDateTime(this DateTime dateTime)
        {
            if ((CultureInfo.CurrentCulture.Calendar.GetYear(dateTime) < 2000 && CultureInfo.CurrentCulture.Calendar.GetYear(DateTime.Now) < 2000)
                || (CultureInfo.CurrentCulture.Calendar.GetYear(dateTime) > 2000 && CultureInfo.CurrentCulture.Calendar.GetYear(DateTime.Now) > 2000))
                return dateTime;
            
            var pc = new PersianCalendar();
            /*if (DateTime.Now.Year < 2000)
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);*/
            DateTime dt = new DateTime(dateTime.Year,dateTime.Month,dateTime.Day, pc);
            return dt;
        }
        public static string ToShortDateString3(this DateTime dateTime)
        {
            var m1 = dateTime.Month.ToString().Length == 1 ? "0" + dateTime.Month : dateTime.Month.ToString();
            var m2 = dateTime.Day.ToString().Length == 1 ? "0" + dateTime.Day : dateTime.Day.ToString();
            return $"1402/{m1}/{m2}";
            //return $"{dateTime.Year}/{dateTime.Month}/{dateTime.Day}";
        }
        public static string ToShortDateString2(this DateTime dateTime)
        {
            return $"{dateTime.Year}/{dateTime.Month}/{dateTime.Day}";
        }
        public static string ToStringD(this long val)
        {
            var l = 10 - val.ToString().Length;
            if (l > 0)
            {
                string g = "";
                for (int i = 0; i < l; i++)
                {
                    g += "0";
                }
                return val.ToString().Insert(0, g);
            }
            return val.ToString();
        }
        public static char GetChar(char ch)
        {
            switch (ch) 
            {
                case '۰':
                    return '0';
                case '۱':
                    return '1';
                case '۲':
                    return '2';
                case '۳':
                    return '3';
                case '۴':
                    return '4';
                case '۵':
                    return '5';
                case '۶':
                    return '6';
                case '۷':
                    return '7';
                case '۸':
                    return '8';
                case '۹':
                    return '9';
            }
            return ch;
        }
        public static List<T> ToListof<T>(this DataTable dt)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            var objectProperties = typeof(T).GetProperties(flags);
            var targetList = dt.AsEnumerable().Select(dataRow =>
            {
                var instanceOfT = Activator.CreateInstance<T>();

                foreach (var properties in objectProperties.Where(properties => columnNames.Contains(properties.Name) && dataRow[properties.Name] != DBNull.Value))
                {
                    properties.SetValue(instanceOfT, dataRow[properties.Name], null);
                }
                return instanceOfT;
            }).ToList();

            return targetList;
        }
        public static List<T> ToListofOnColumn<T>(this DataTable dt)
        {
            var list = new List<T>();
            foreach (var item in dt.Rows)
            {
                list.Add((T)Convert.ChangeType((item as DataRow)[0], typeof(T)));
            }
            return list;
        }
        public static T GetParentOfType<T>(this DependencyObject element) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(element);

            // جستجو در والدین تا زمانی که نوع مورد نظر را پیدا کنیم
            while (parent != null && !(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as T;
        }
        public static T GetChildOfType<T>(this DependencyObject depObj)
    where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
        public static List<T> GetChildsOfType<T>(this DependencyObject parent) where T : DependencyObject
        {
            List<T> result = new List<T>();

            if (parent == null) return result;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                // اگر فرزند از نوع T باشد، به لیست اضافه می‌شود
                if (child is T childOfType)
                {
                    result.Add(childOfType);
                }

                // جستجوی بازگشتی برای یافتن فرزندان بیشتر
                result.AddRange(GetChildsOfType<T>(child));
            }

            return result;
        }
        public static T GetChildByName<T>(this DependencyObject parent, string childName) where T : FrameworkElement
        {
            // تعداد فرزندان را بررسی کنید
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // بررسی کنید که آیا فرزند از نوع T است
                if (child is T frameworkElement)
                {
                    if (frameworkElement.Name == childName)
                        return frameworkElement;
                }

                // جستجو در فرزندان دیگر
                T result = GetChildByName<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null;
        }
        public static List<T> GetChildsByName<T>(this DependencyObject parent, string name) where T : FrameworkElement
        {
            List<T> children = new List<T>();

            // تعداد کل فرزندان
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // اگر فرزند از نوع کنترل خواسته شده باشد و نام آن مطابق باشد
                if (child is T frameworkElement && frameworkElement.Name == name)
                {
                    children.Add(frameworkElement);
                }

                // جستجوی بازگشتی در فرزندان و افزودن نتایج به لیست
                children.AddRange(GetChildsByName<T>(child, name));
            }
            return children;
        }
        public static string ToComma(this decimal? number)
        {
            string numberStr = number.ToString(); // تبدیل عدد به رشته
            string result = "";
            int length = numberStr.Length;

            for (int i = 0; i < length; i++)
            {
                result += numberStr[length - 1 - i]; // اضافه کردن رقم‌ها به رشته نتیجه

                // اضافه کردن کاما بعد از هر سه رقم (غیر از آخرین قسمت)
                if ((i + 1) % 3 == 0 && (i + 1) < length)
                {
                    result += ",";
                }
            }

            // معکوس کردن رشته برای نمایش صحیح
            char[] charArray = result.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        public static T DeepClone<T>(this T obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            using (StringWriter stringWriter = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));

                // سریال‌سازی به XML
                serializer.Serialize(stringWriter, obj);
                string xmlData = stringWriter.ToString();

                using (StringReader stringReader = new StringReader(xmlData))
                {
                    // دی‌سریال‌سازی و ایجاد کپی عمیق
                    return (T)serializer.Deserialize(stringReader);
                }
            }
        }
    }
}
