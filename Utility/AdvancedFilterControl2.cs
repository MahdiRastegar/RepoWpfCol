#region Assembly Syncfusion.SfGrid.WPF, Version=26.1462.35.0, Culture=neutral, PublicKeyToken=3d67ed1f87d44c89
// location unknown
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Syncfusion.Data;
using Syncfusion.Windows.Shared;
using Syncfusion.XlsIO.Implementation.PivotAnalysis;
using Syncfusion.UI.Xaml.Grid;

namespace WpfCol
{

    //
    // Summary:
    //     Represents a control that provides advanced filter options to filter the data.
    public class AdvancedFilterControl2 : ContentControl, IDisposable, INotifyPropertyChanged, IDataErrorInfo
    {
        private string filterType1;

        private string filterType2;

        private object filterValue1;

        private object filterValue2;

        private bool isdisposed;

        private bool casingbuttonvisibility = true;

        private object datefilterValue1;

        private object datefilterValue2;

        private object filterSelectedItem1;

        private object filterSelectedItem2;

        private ObservableCollection<FilterElement> comboSource;

        private bool? isORChecked = true;

        internal bool isCaseSensitive1;

        internal bool isCaseSensitive2;

        internal bool propertyChangedfromsettingControlValues;

        internal GridFilterControl gridFilterCtrl;

        //
        // Summary:
        //     Identifies the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2.CanGenerateUniqueItems
        //     dependency property.
        //
        // Remarks:
        //     The identifier for the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2.CanGenerateUniqueItems
        //     dependency property.
        public static readonly DependencyProperty CanGenerateUniqueItemsProperty = DependencyProperty.Register("CanGenerateUniqueItems", typeof(bool), typeof(AdvancedFilterControl2), new PropertyMetadata(true, OnCanGenerateUniqueItemsChanged));

        //
        // Summary:
        //     Identifies the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2.FilterTypeComboItems
        //     dependency property.
        //
        // Remarks:
        //     The identifier for the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2.FilterTypeComboItems
        //     dependency property.
        public static readonly DependencyProperty FilterTypeComboItemsProperty = DependencyProperty.Register("FilterTypeComboItems", typeof(object), typeof(AdvancedFilterControl2), new PropertyMetadata(null));

        private ToggleButton CasingButton1;

        private ToggleButton CasingButton2;

        private DatePicker datePicker1;

        private DatePicker datePicker2;

        private ComboBox MenuComboBox1;

        private ComboBox MenuComboBox2;

        private RadioButton radioButton1;

        private RadioButton radioButton2;

        internal Type ColumnDataType;

        //
        // Summary:
        //     Gets or sets a value that indicates the FilterType1 in AdvancedFilterControl2.
        //
        //
        // Value:
        //     A string that specifies the selected filter type of the Syncfusion.UI.Xaml.Grid.AdvancedFilterType.
        //
        //
        // Remarks:
        //     Which is used to update the selected Syncfusion.Data.FilterType to the first
        //     MenuComboBox in the AdvancedFilterControl2.
        public string FilterType1
        {
            get
            {
                return filterType1;
            }
            set
            {
                filterType1 = value;
                if (FilterType1 != null && (FilterType1.ToString() == GridLocalizationResourceAccessor.Instance.GetString("Null") || FilterType1.ToString() == GridLocalizationResourceAccessor.Instance.GetString("NotNull") || FilterType1.ToString() == GridLocalizationResourceAccessor.Instance.GetString("Empty") || FilterType1.ToString() == GridLocalizationResourceAccessor.Instance.GetString("NotEmpty")))
                {
                    FilterValue1 = null;
                    FilterSelectedItem1 = null;
                }

                OnPropertyChanged("FilterType1");
            }
        }

        //
        // Summary:
        //     Gets or sets a value that indicates the FilterType1 in AdvancedFilterControl2.
        //
        //
        // Value:
        //     A string that specifies the selected filter type of the Syncfusion.UI.Xaml.Grid.AdvancedFilterType.
        //
        //
        // Remarks:
        //     Which is used to update the selected Syncfusion.Data.FilterType to the second
        //     MenuComboBox in the AdvancedFilterControl2.
        public string FilterType2
        {
            get
            {
                return filterType2;
            }
            set
            {
                filterType2 = value;
                if (FilterType2 != null && (FilterType2.ToString() == GridLocalizationResourceAccessor.Instance.GetString("Null") || FilterType2.ToString() == GridLocalizationResourceAccessor.Instance.GetString("NotNull") || FilterType2.ToString() == GridLocalizationResourceAccessor.Instance.GetString("Empty") || FilterType2.ToString() == GridLocalizationResourceAccessor.Instance.GetString("NotEmpty")))
                {
                    FilterValue2 = null;
                    FilterSelectedItem2 = null;
                }

                OnPropertyChanged("FilterType2");
            }
        }

        //
        // Summary:
        //     Gets or sets a value that indicates the DateFilterValue1 in AdvancedFilterControl2.
        //
        //
        // Value:
        //     An object that specifies the selected DateFilterValue of the corresponding Syncfusion.UI.Xaml.Grid.AdvancedFilterType.DateFilter.
        //
        //
        // Remarks:
        //     Which is used to update the selected DateFilterValue to the first DatePicker
        //     in the AdvancedFilterControl2.
        public object DateFilterValue1
        {
            get
            {
                return datefilterValue1;
            }
            set
            {
                datefilterValue1 = value;
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    if (gridFilterCtrl != null)
                    {
                        FilterValue1 = gridFilterCtrl.GetFormattedString(value);
                    }
                }
                else
                {
                    FilterValue1 = value;
                }

                OnPropertyChanged("DateFilterValue1");
            }
        }

        //
        // Summary:
        //     Gets or sets a value that indicates the DateFilterValue1 in AdvancedFilterControl2.
        //
        //
        // Value:
        //     An object that specifies the selected DateFilterValue of the corresponding Syncfusion.UI.Xaml.Grid.AdvancedFilterType.DateFilter.
        //
        //
        // Remarks:
        //     Which is used to update the selected DateFilterValue to the second DatePicker
        //     in the AdvancedFilterControl2.
        public object DateFilterValue2
        {
            get
            {
                return datefilterValue2;
            }
            set
            {
                datefilterValue2 = value;
                if (value != null && !string.IsNullOrEmpty(value.ToString()))
                {
                    if (gridFilterCtrl != null)
                    {
                        FilterValue2 = gridFilterCtrl.GetFormattedString(value);
                    }
                }
                else
                {
                    FilterValue2 = value;
                }

                OnPropertyChanged("DateFilterValue2");
            }
        }

        //
        // Summary:
        //     Gets or sets the text for the first editble filter UIElement.This UIElement will
        //     be a TextBox or ComboBox.
        //
        // Remarks:
        //     The value of this property is validated by using the type of the column property.
        //     And you can set the column type while applying the filtering by using this Syncfusion.UI.Xaml.Grid.GridFilterControl.SetColumnDataType(System.Type)
        //     method.
        public object FilterValue1
        {
            get
            {
                return filterValue1;
            }
            set
            {
                if (FilterValue1 != value)
                {
                    if ((FilterValue1 != null && !FilterValue1.Equals(value)) || FilterValue1 == null)
                    {
                        filterValue1 = value;
                        OnPropertyChanged("FilterValue1");
                    }

                    SetOkButtonState(filterValue1, FilterValue2);
                }
            }
        }

        //
        // Summary:
        //     Gets or sets the text of second editable filter UIElement.This UIElement will
        //     be a TextBox or ComboBox.
        //
        // Remarks:
        //     The value of this property is validated by using the type of the column property.
        //     And you can set the column type while applying the filtering by using this Syncfusion.UI.Xaml.DataGrid.GridFilterControl.SetColumnDataType(Type)
        //     method.
        public object FilterValue2
        {
            get
            {
                return filterValue2;
            }
            set
            {
                if (FilterValue2 != value)
                {
                    if ((FilterValue2 != null && !FilterValue2.Equals(value)) || FilterValue2 == null)
                    {
                        filterValue2 = value;
                        OnPropertyChanged("FilterValue2");
                    }

                    SetOkButtonState(FilterValue1, filterValue2);
                }
            }
        }

        //
        // Summary:
        //     Gets or sets the selectedItem to the first filter comboBox in AdvancedFilterControl2.
        //
        //
        // Remarks:
        //     Which is used to select the existing filter data in the first Combobox while
        //     opening the AdvancedFilterControl2.
        public object FilterSelectedItem1
        {
            get
            {
                return filterSelectedItem1;
            }
            set
            {
                if (filterSelectedItem1 != value)
                {
                    filterSelectedItem1 = value;
                    OnPropertyChanged("FilterSelectedItem1");
                    SetOkButtonState(filterSelectedItem1, FilterSelectedItem2);
                }
            }
        }

        //
        // Summary:
        //     Gets or sets the selectedItem to the second filter comboBox in AdvancedFilterControl2.
        //
        //
        // Remarks:
        //     Which is used to select the existing filter data in the second Combobox while
        //     opening the AdvancedFilterControl2.
        public object FilterSelectedItem2
        {
            get
            {
                return filterSelectedItem2;
            }
            set
            {
                if (filterSelectedItem2 != value)
                {
                    filterSelectedItem2 = value;
                    OnPropertyChanged("FilterSelectedItem2");
                    SetOkButtonState(filterSelectedItem2, FilterSelectedItem1);
                }
            }
        }

        //
        // Summary:
        //     Gets or sets the collection of Syncfusion.UI.Xaml.Grid.FilterElement.
        //
        // Value:
        //     The collection of FilterElement.
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldbeReadOnly")]
        public ObservableCollection<FilterElement> ComboItemsSource
        {
            get
            {
                return comboSource;
            }
            set
            {
                comboSource = value;
                OnPropertyChanged("ComboItemsSource");
            }
        }

        //
        // Summary:
        //     Gets or sets a value indicating whether CasingButton should be visible or not.
        //
        //
        // Value:
        //     true if CasingButton is visible; otherwise,false. The default value is true.
        public bool CasingButtonVisibility
        {
            get
            {
                return casingbuttonvisibility;
            }
            set
            {
                casingbuttonvisibility = value;
                OnPropertyChanged("CasingButtonVisibility");
            }
        }

        //
        // Summary:
        //     Gets or sets a value indicating whether OR in radio button is checked or not.
        //
        //
        // Value:
        //     true if OR in radio button is checked; otherwise,false. The default value is
        //     true.
        public bool? IsORChecked
        {
            get
            {
                return isORChecked;
            }
            set
            {
                isORChecked = value;
                OnPropertyChanged("IsORChecked");
            }
        }

        //
        // Summary:
        //     Gets or sets a value that indicates whether the case sensitive filter is enabled
        //     for the first CasingButton.
        //
        // Value:
        //     true if CaseSensitive button is clicked; otherwise,false. The default value is
        //     false.
        public bool IsCaseSensitive1
        {
            get
            {
                return isCaseSensitive1;
            }
            set
            {
                isCaseSensitive1 = value;
                RefreshCasingButton1State();
                OnPropertyChanged("IsCaseSensitive1");
            }
        }

        //
        // Summary:
        //     Gets or sets a value that indicates whether the case sensitive filter is enabled
        //     for the second CasingButton.
        //
        // Value:
        //     true if CaseSensitive button is clicked; otherwise,false. The default value is
        //     false.
        public bool IsCaseSensitive2
        {
            get
            {
                return isCaseSensitive2;
            }
            set
            {
                isCaseSensitive2 = value;
                RefreshCasingButton2State();
                OnPropertyChanged("IsCaseSensitive2");
            }
        }

        //
        // Summary:
        //     Gets or sets a value indicating whether all the unique items in the column are
        //     loaded or not.
        //
        // Value:
        //     true if ComboBox is loaded in advanced filter; false if TextBox is loaded that
        //     allows you to manually enter text for filtering. The default value is true.
        public bool CanGenerateUniqueItems
        {
            get
            {
                return (bool)GetValue(CanGenerateUniqueItemsProperty);
            }
            set
            {
                SetValue(CanGenerateUniqueItemsProperty, value);
            }
        }

        //
        // Summary:
        //     Gets or sets the collections of Syncfusion.Data.FilterType to the first and second
        //     MenuComboBox in AdvancedFilterControl2.
        //
        // Remarks:
        //     And the Syncfusion.Data.FilterType collection is varied depending upon the Syncfusion.UI.Xaml.Grid.AdvancedFilterType..
        public object FilterTypeComboItems
        {
            get
            {
                return GetValue(FilterTypeComboItemsProperty);
            }
            set
            {
                SetValue(FilterTypeComboItemsProperty, value);
            }
        }

        //
        // Summary:
        //     Gets the Error.
        [SuppressMessage("Performance", "CA1822:Mark members as static")]
        public string Error => null;

        //
        // Summary:
        //     Gets the Error message for the specified column.
        //
        // Parameters:
        //   columnName:
        //     The specified column.
        //
        // Returns:
        //     The Error message for the specified column.
        public string this[string columnName]
        {
            get
            {
                string empty = string.Empty;
                object obj = null;
                if (columnName == "FilterValue1")
                {
                    obj = GetFirstFilterValue();
                    if (!IsValidFilterValue(obj))
                    {
                        return GridLocalizationResourceAccessor.Instance.GetString("EnterValidFilterValue");
                    }
                }
                else if (columnName == "FilterValue2")
                {
                    obj = GetSecondFilterValue();
                    if (!IsValidFilterValue(obj))
                    {
                        return GridLocalizationResourceAccessor.Instance.GetString("EnterValidFilterValue");
                    }
                }

                return empty;
            }
        }

        //
        // Summary:
        //     Occurs when a property value changes.
        public event PropertyChangedEventHandler PropertyChanged;

        private static void OnCanGenerateUniqueItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        //
        // Summary:
        //     Initializes a new instance of Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2 class.
        public AdvancedFilterControl2()
        {
            base.DefaultStyleKey = typeof(AdvancedFilterControl2);
        }

        //
        // Summary:
        //     Builds the visual tree for the AdvancedFilterControl2 when a new template is applied.
        public override void OnApplyTemplate()
        {
            UnWireEvents();
            base.OnApplyTemplate();
            CasingButton1 = GetTemplateChild("PART_CasingButton1") as ToggleButton;
            CasingButton2 = GetTemplateChild("PART_CasingButton2") as ToggleButton;
            MenuComboBox1 = GetTemplateChild("PART_MenuComboBox1") as ComboBox;
            MenuComboBox2 = GetTemplateChild("PART_MenuComboBox2") as ComboBox;
            datePicker1 = GetTemplateChild("PART_DatePicker1") as DatePicker;
            datePicker2 = GetTemplateChild("PART_DatePicker2") as DatePicker;
            radioButton1 = GetTemplateChild("PART_RadioButton1") as RadioButton;
            radioButton2 = GetTemplateChild("PART_RadioButton2") as RadioButton;
            WireEvents();
            GenerateFilterTypeComboItems();
            RefreshCasingButton1State();
            RefreshCasingButton2State();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (datePicker1 != null && datePicker1.IsDropDownOpen)
            {
                datePicker1.IsDropDownOpen = false;
            }

            if (datePicker2 != null && datePicker2.IsDropDownOpen)
            {
                datePicker2.IsDropDownOpen = false;
            }
        }

        //
        // Summary:
        //     Disposes all the resources used by the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2
        //     class.
        public void Dispose()
        {
            Dispose(isDisposing: true);
            GC.SuppressFinalize(this);
        }

        //
        // Summary:
        //     Disposes all the resources used by the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2
        //     class.
        //
        // Parameters:
        //   isDisposing:
        //     Indicates whether the call is from Dispose method or from a finalizer.
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed)
            {
                return;
            }

            UnWireEvents();
            if (isDisposing)
            {
                gridFilterCtrl = null;
                if (ComboItemsSource != null)
                {
                    ComboItemsSource.Clear();
                    ComboItemsSource = null;
                }

                FilterTypeComboItems = null;
            }

            isdisposed = true;
        }

        private void ApplyImmediateFilters()
        {
            if (gridFilterCtrl != null && gridFilterCtrl.ImmediateUpdateColumnFilter)
            {
                string value = this["FilterValue1"];
                string value2 = this["FilterValue2"];
                if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(value2))
                {
                    gridFilterCtrl.GetType().GetMethod("InvokeFilter").Invoke(gridFilterCtrl,null);
                }
            }
        }

        //
        // Summary:
        //     Get the first filter value based on the first edit element ie.(TextBox or ComboBox)
        //     value and first date picker value and first MenuComboBox selectedItem value in
        //     AdvancedFilterControl2.
        //
        // Returns:
        //     Return the Filter value.
        //
        // Remarks:
        //     Which mainly is used to update the error message to the editable filter element
        //     of AdvancedFilterControl2 if the returned filter value is wrong or invalid.
        public virtual object GetFirstFilterValue()
        {
            return GetFilterValue(FilterValue1, DateFilterValue1, FilterSelectedItem1);
        }

        //
        // Summary:
        //     Get the second filter value based on the second edit element ie.(TextBox or ComboBox)
        //     value and second date picker value and second MenuComboBox selectedItem value
        //     in AdvancedFilterControl2.
        //
        // Returns:
        //     Return the Filter value.
        //
        // Remarks:
        //     Which mainly is used to update the error message to the editable filter element
        //     of AdvancedFilterControl2 if the returned filter value is wrong or invalid.
        public virtual object GetSecondFilterValue()
        {
            return GetFilterValue(FilterValue2, DateFilterValue2, FilterSelectedItem2);
        }

        //
        // Summary:
        //     Invokes to get Filter value
        //
        // Parameters:
        //   filterValue:
        //     FilterVValue
        //
        //   dateFilterValue:
        //     DateFilterValue
        //
        //   filterSelectedItem:
        //     FilterSelectedItem
        [SuppressMessage("Globalization", "CA1305:Specify IFormatProvider")]
        private object GetFilterValue(object filterValue, object dateFilterValue, object filterSelectedItem)
        {
            if (gridFilterCtrl == null)
            {
                return filterValue;
            }

            if (gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.TextFilter && ColumnDataType != typeof(object))
            {
                return filterValue;
            }

            if (filterSelectedItem is FilterElement && (filterSelectedItem as FilterElement).DisplayText.Equals(filterValue))
            {
                return GetFilterElementValue(filterSelectedItem);
            }

            if (gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter)
            {
                if (CanGenerateUniqueItems && dateFilterValue != null)
                {
                    if (gridFilterCtrl.GetFormattedString(dateFilterValue).Equals(filterValue))
                    {
                        return dateFilterValue;
                    }

                    return filterValue;
                }

                if (filterValue != null && !filterValue.Equals(string.Empty))
                {
                    if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn).DisplayBinding is Binding binding && binding.Converter != null && binding.Converter is GridValueConverter)
                    {
                        return binding.Converter.ConvertBack(filterValue, null, binding.ConverterParameter, binding.ConverterCulture);
                    }

                    if (TypeConverterHelper.CanConvert(typeof(DateTime?), filterValue.ToString()))
                    {
                        return DateTime.Parse(filterValue.ToString());
                    }
                }

                return filterValue;
            }

            return filterValue;
        }

        [SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity")]
        internal void GenerateFilterTypeComboItems()
        {
            ObservableCollection<string> observableCollection = new ObservableCollection<string>();
            if (gridFilterCtrl != null)
            {
                if (gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.TextFilter)
                {
                    VisualStateManager.GoToState(this, "TextFilter", useTransitions: true);
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Equalss"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotEquals"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("BeginsWith"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotBeginsWith"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("EndsWith"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotEndsWith"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Contains"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotContains"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Empty"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotEmpty"));
                    if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn) != null && (gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn).AllowBlankFilters)
                    {
                        observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Null"));
                        observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotNull"));
                    }
                }
                else if (gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.NumberFilter)
                {
                    VisualStateManager.GoToState(this, "NumberFilter", useTransitions: true);
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Equalss"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotEquals"));
                    if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn) != null && (gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn).AllowBlankFilters)
                    {
                        observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Null"));
                        observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotNull"));
                    }

                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("LessThan"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("LessThanorEqual"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("GreaterThan"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("GreaterThanorEqual"));
                }
                else if (gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter)
                {
                    VisualStateManager.GoToState(this, "DateFilter", useTransitions: true);
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Equalss"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotEquals"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Before"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("BeforeOrEqual"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("After"));
                    observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("AfterOrEqual"));
                    if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn) != null && (gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn).AllowBlankFilters)
                    {
                        observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Null"));
                        observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotNull"));
                    }
                }
            }
            else
            {
                observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("Null"));
                observableCollection.Add(GridLocalizationResourceAccessor.Instance.GetString("NotNull"));
            }

            if (FilterTypeComboItems == null)
            {
                FilterTypeComboItems = observableCollection;
                return;
            }

            bool flag = (FilterTypeComboItems as ObservableCollection<string>).Any((string s) => s.Equals(GridLocalizationResourceAccessor.Instance.GetString("Null")));
            if (gridFilterCtrl != null && (gridFilterCtrl.GetType().GetField("OkButton").GetValue(gridFilterCtrl) as Button) != null)
            {
                if (!(gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn).AllowBlankFilters && flag)
                {
                    (FilterTypeComboItems as ObservableCollection<string>).Remove(GridLocalizationResourceAccessor.Instance.GetString("Null"));
                    (FilterTypeComboItems as ObservableCollection<string>).Remove(GridLocalizationResourceAccessor.Instance.GetString("NotNull"));
                }
                else if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn).AllowBlankFilters && !flag)
                {
                    (FilterTypeComboItems as ObservableCollection<string>).Add(GridLocalizationResourceAccessor.Instance.GetString("Null"));
                    (FilterTypeComboItems as ObservableCollection<string>).Add(GridLocalizationResourceAccessor.Instance.GetString("NotNull"));
                }
            }
        }

        internal void MaintainAPIChanges()
        {
            if (gridFilterCtrl.AdvancedFilterStyle != null)
            {
                base.Style = gridFilterCtrl.AdvancedFilterStyle;
            }
        }

        private static object GetFilterElementValue(object filtervalue)
        {
            if (filtervalue != null)
            {
                if (filtervalue is FilterElement)
                {
                    return (filtervalue as FilterElement).ActualValue;
                }

                return filtervalue;
            }

            return null;
        }

        private object GetFilterElementDisplayValue(object filtervalue)
        {
            if (filtervalue == null || comboSource == null)
            {
                return null;
            }

            FilterElement filterElement = comboSource.FirstOrDefault((FilterElement element) => element != null && element.ActualValue != null && element.ActualValue.Equals(filtervalue));
            if (filterElement != null)
            {
                return filterElement.DisplayText;
            }

            return filtervalue;
        }

        internal void SetAdvancedFilterControlValues(ObservableCollection<FilterPredicate> fp)
        {
            propertyChangedfromsettingControlValues = true;
            if (comboSource == null)
            {
                if (fp.Count == 1)
                {
                    FilterType2 = GridLocalizationResourceAccessor.Instance.GetString("Equalss");
                    FilterValue2 = null;
                    DateFilterValue2 = null;
                }

                propertyChangedfromsettingControlValues = false;
                return;
            }

            string obj = string.Empty;
            if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn) is GridMaskColumn)
            {
                GridMaskColumn gridMaskColumn = (gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn) as GridMaskColumn;
                obj = MaskedEditorModel.GetMaskedText(gridMaskColumn.Mask, string.Empty, gridMaskColumn.DateSeparator, gridMaskColumn.TimeSeparator, gridMaskColumn.DecimalSeparator, NumberFormatInfo.CurrentInfo.NumberGroupSeparator, gridMaskColumn.PromptChar, NumberFormatInfo.CurrentInfo.CurrencySymbol);
            }

            var c = (gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn);
            if (fp.Count > 0)
            {
                if (gridFilterCtrl != null && gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter && !bool.Parse(c.GetType().GetField("isDisplayMultiBinding").GetValue(c).ToString()))
                {
                    if (gridFilterCtrl != null && fp[0].FilterValue != null)
                    {
                        FilterValue1 = gridFilterCtrl.GetFormattedString(fp[0].FilterValue);
                    }
                }
                else
                {
                    FilterValue1 = GetFilterElementDisplayValue(fp[0].FilterValue);
                }

                //FilterType1 = FilterHelpers.GetResourceWrapper(fp[0].FilterType, FilterValue1);
                if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn) is GridMaskColumn && FilterValue1 != null && FilterValue1.Equals(obj))
                {
                    FilterType1 = GridLocalizationResourceAccessor.Instance.GetString("Empty");
                }

                IsCaseSensitive1 = fp[0].IsCaseSensitive;
                if (fp.Count == 1)
                {
                    FilterType2 = GridLocalizationResourceAccessor.Instance.GetString("Equalss");
                    FilterValue2 = null;
                    DateFilterValue2 = null;
                }
            }

            if (fp.Count == 2)
            {
                if (gridFilterCtrl != null && gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.DateFilter && !bool.Parse(c.GetType().GetField("isDisplayMultiBinding").GetValue(c).ToString()))
                {
                    if (gridFilterCtrl != null && fp[1].FilterValue != null)
                    {
                        FilterValue2 = gridFilterCtrl.GetFormattedString(fp[1].FilterValue);
                    }
                }
                else
                {
                    FilterValue2 = GetFilterElementDisplayValue(fp[1].FilterValue);
                }
                //typeof(FilterHelpers).GetProperty(
                //FilterType2 = FilterHelpers.GetResourceWrapper(fp[1].FilterType, FilterValue2);
                if ((gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn) is GridMaskColumn && FilterValue2 != null && FilterValue2.Equals(obj))
                {
                    FilterType2 = GridLocalizationResourceAccessor.Instance.GetString("Empty");
                }

                IsCaseSensitive2 = fp[1].IsCaseSensitive;
                IsORChecked = fp[1].PredicateType == PredicateType.Or;
            }

            propertyChangedfromsettingControlValues = false;
        }

        internal void ResetAdvancedFilterControlValues()
        {
            propertyChangedfromsettingControlValues = true;
            FilterValue1 = null;
            FilterValue2 = null;
            DateFilterValue1 = null;
            DateFilterValue2 = null;
            FilterSelectedItem1 = null;
            FilterSelectedItem2 = null;
            FilterType1 = GridLocalizationResourceAccessor.Instance.GetString("Equalss");
            FilterType2 = GridLocalizationResourceAccessor.Instance.GetString("Equalss");
            IsORChecked = true;
            if (CasingButton1 != null)
            {
                IsCaseSensitive1 = false;
            }

            if (CasingButton2 != null)
            {
                IsCaseSensitive2 = false;
            }

            propertyChangedfromsettingControlValues = false;
        }

        private void RefreshCasingButton1State()
        {
            if (CasingButton1 != null)
            {
                if (IsCaseSensitive1)
                {
                    VisualStateManager.GoToState(CasingButton1, "CaseSensitive", useTransitions: true);
                }
                else
                {
                    VisualStateManager.GoToState(CasingButton1, "NotCaseSensitive", useTransitions: true);
                }
            }
        }

        private void RefreshCasingButton2State()
        {
            if (CasingButton2 != null)
            {
                if (IsCaseSensitive2)
                {
                    VisualStateManager.GoToState(CasingButton2, "CaseSensitive", useTransitions: true);
                }
                else
                {
                    VisualStateManager.GoToState(CasingButton2, "NotCaseSensitive", useTransitions: true);
                }
            }
        }

        private void OnRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (gridFilterCtrl != null && gridFilterCtrl.ImmediateUpdateColumnFilter)
            {
                if (gridFilterCtrl.AllowBlankFilters && FilterValue1 == null && FilterValue2 == null && !FilterType1.Equals(GridLocalizationResourceAccessor.Instance.GetString("Null")) && !FilterType2.Equals(GridLocalizationResourceAccessor.Instance.GetString("Null")) && !FilterType1.Equals(GridLocalizationResourceAccessor.Instance.GetString("NotNull")) && !FilterType2.Equals(GridLocalizationResourceAccessor.Instance.GetString("NotNull")) && !FilterType1.Equals(GridLocalizationResourceAccessor.Instance.GetString("Empty")) && !FilterType2.Equals(GridLocalizationResourceAccessor.Instance.GetString("Empty")) && !FilterType1.Equals(GridLocalizationResourceAccessor.Instance.GetString("NotEmpty")) && !FilterType2.Equals(GridLocalizationResourceAccessor.Instance.GetString("NotEmpty")))
                {
                    propertyChangedfromsettingControlValues = true;
                    FilterType1 = GridLocalizationResourceAccessor.Instance.GetString("Null");
                    FilterType2 = GridLocalizationResourceAccessor.Instance.GetString("Null");
                    propertyChangedfromsettingControlValues = false;
                }

                ApplyImmediateFilters();
            }
        }

        private void OnDatePickerLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is DatePicker datePicker && !datePicker.IsKeyboardFocusWithin)
            {
                datePicker.IsDropDownOpen = false;
            }
        }

        private void OnDatePickerMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void OnMenuComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (ComboItemsSource != null && comboBox.SelectedValue != null && (gridFilterCtrl.GetType().GetField("OkButton").GetValue(gridFilterCtrl) as Button) != null)
            {
                (gridFilterCtrl.GetType().GetField("OkButton").GetValue(gridFilterCtrl) as Button).IsEnabled = ((IsFilterHasvalues() || IsNullOrEmptyFilterType(FilterType1) || IsNullOrEmptyFilterType(FilterType2)) ? true : false);
            }
            var c = gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn;
            bool flag = (c.GetType().GetProperty("DataGrid").GetValue(c) as SfDataGrid).FilterRowPosition != 0 && (gridFilterCtrl.GetType().GetProperty("Column").GetValue(gridFilterCtrl) as GridColumn).FilteredFrom == FilteredFrom.FilterRow && ((comboBox == MenuComboBox1 && FilterValue1 == null) || (comboBox == MenuComboBox2 && FilterValue2 == null));
            if (comboBox.SelectedValue != null && !propertyChangedfromsettingControlValues && !flag && e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                ApplyImmediateFilters();
            }
        }

        //
        // Summary:
        //     Invoked when the Syncfusion.UI.Xaml.Grid.AdvancedFilterControl2.PropertyChanged
        //     event occurs.
        //
        // Parameters:
        //   propertyName:
        //     The corresponding property.
        public void OnPropertyChanged(string propertyName)
        {
            if (gridFilterCtrl != null && bool.Parse(gridFilterCtrl.GetType().GetProperty("IsInSuspend").GetValue(gridFilterCtrl).ToString()))
            {
                return;
            }

            if ((propertyName == "FilterValue1" || propertyName == "FilterValue2") && !propertyChangedfromsettingControlValues)
            {
                ApplyImmediateFilters();
            }

            if (this.PropertyChanged != null)
            {
                if (propertyName == "ComboItemsSource")
                {
                    propertyChangedfromsettingControlValues = true;
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    propertyChangedfromsettingControlValues = false;
                }
                else
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        private void WireEvents()
        {
            base.Loaded += AdvancedFilterControl_Loaded;
            if (MenuComboBox1 != null)
            {
                MenuComboBox1.SelectionChanged += OnMenuComboBoxSelectionChanged;
            }

            if (MenuComboBox2 != null)
            {
                MenuComboBox2.SelectionChanged += OnMenuComboBoxSelectionChanged;
            }

            if (datePicker1 != null)
            {
                datePicker1.LostFocus += OnDatePickerLostFocus;
                datePicker1.MouseDown += OnDatePickerMouseDown;
            }

            if (datePicker2 != null)
            {
                datePicker2.LostFocus += OnDatePickerLostFocus;
                datePicker2.MouseDown += OnDatePickerMouseDown;
            }

            if (radioButton1 != null)
            {
                radioButton1.Click += OnRadioButtonClick;
            }

            if (radioButton2 != null)
            {
                radioButton2.Click += OnRadioButtonClick;
            }
        }

        private void AdvancedFilterControl_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshCasingButton1State();
            RefreshCasingButton2State();
        }

        private void UnWireEvents()
        {
            base.Loaded -= AdvancedFilterControl_Loaded;
            if (MenuComboBox1 != null)
            {
                MenuComboBox1.SelectionChanged -= OnMenuComboBoxSelectionChanged;
            }

            if (MenuComboBox2 != null)
            {
                MenuComboBox2.SelectionChanged -= OnMenuComboBoxSelectionChanged;
            }

            if (datePicker1 != null)
            {
                datePicker1.LostFocus -= OnDatePickerLostFocus;
                datePicker1.MouseDown -= OnDatePickerMouseDown;
            }

            if (datePicker2 != null)
            {
                datePicker2.LostFocus -= OnDatePickerLostFocus;
                datePicker2.MouseDown -= OnDatePickerMouseDown;
            }

            if (radioButton1 != null)
            {
                radioButton1.Click -= OnRadioButtonClick;
            }

            if (radioButton2 != null)
            {
                radioButton2.Click -= OnRadioButtonClick;
            }
        }

        private bool IsValidFilterValue(object value)
        {
            if (ColumnDataType == typeof(object) || (gridFilterCtrl != null && gridFilterCtrl.AdvancedFilterType == AdvancedFilterType.TextFilter))
            {
                return true;
            }

            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                return TypeConverterHelper.CanConvert(ColumnDataType, value.ToString());
            }

            return true;
        }

        //
        // Summary:
        //     Returns true if filter type is null,notnull,empty or not empty
        //
        // Parameters:
        //   type:
        //
        // Returns:
        //     bool
        internal static bool IsNullOrEmptyFilterType(object type)
        {
            string empty = string.Empty;
            if (type == null)
            {
                return false;
            }

            empty = type.ToString();
            if (empty != null && (empty == GridLocalizationResourceAccessor.Instance.GetString("Null") || empty == GridLocalizationResourceAccessor.Instance.GetString("NotNull") || empty == GridLocalizationResourceAccessor.Instance.GetString("Empty") || empty == GridLocalizationResourceAccessor.Instance.GetString("NotEmpty")))
            {
                return true;
            }

            return false;
        }

        //
        // Summary:
        //     Returns true if any one filter has values
        //
        // Returns:
        //     bool
        internal bool IsFilterHasvalues()
        {
            object firstFilterValue = GetFirstFilterValue();
            object secondFilterValue = GetSecondFilterValue();
            if ((firstFilterValue == null || string.IsNullOrEmpty(firstFilterValue.ToString())) && (secondFilterValue == null || string.IsNullOrEmpty(secondFilterValue.ToString())))
            {
                return false;
            }

            return true;
        }

        //
        // Summary:
        //     Enable or Disable Advance filter OK Button
        //
        // Parameters:
        //   filtervalue1:
        //
        //   filtervalue2:
        //
        // Returns:
        //     void
        internal void SetOkButtonState(object filtervalue1, object filtervalue2)
        {
            if (gridFilterCtrl != null)
            {
                if ((filtervalue1 != null && !string.IsNullOrEmpty(filtervalue1.ToString())) || (filtervalue2 != null && !string.IsNullOrEmpty(filtervalue2.ToString())) || ((IsNullOrEmptyFilterType(FilterType1) || IsNullOrEmptyFilterType(FilterType2)) && (gridFilterCtrl.GetType().GetField("OkButton").GetValue(gridFilterCtrl) as Button) != null))
                {
                    (gridFilterCtrl.GetType().GetField("OkButton").GetValue(gridFilterCtrl) as Button).IsEnabled = true;
                }
                else if (((filtervalue1 == null || string.IsNullOrEmpty(filtervalue1.ToString())) && (filtervalue2 == null || string.IsNullOrEmpty(filtervalue2.ToString()))) || ((IsNullOrEmptyFilterType(FilterType1) || IsNullOrEmptyFilterType(FilterType2)) && (gridFilterCtrl.GetType().GetField("OkButton").GetValue(gridFilterCtrl) as Button) != null))
                {
                    (gridFilterCtrl.GetType().GetField("OkButton").GetValue(gridFilterCtrl) as Button).IsEnabled = false;
                }
            }
        }
    }
}
#if false // Decompilation log
'156' items in cache
------------------
Resolve: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\PresentationFramework.dll'
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll'
------------------
Resolve: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\WindowsBase.dll'
------------------
Resolve: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\PresentationCore.dll'
------------------
Resolve: 'Syncfusion.Shared.Wpf, Version=26.1462.35.0, Culture=neutral, PublicKeyToken=3d67ed1f87d44c89'
Found single assembly: 'Syncfusion.Shared.Wpf, Version=26.1462.35.0, Culture=neutral, PublicKeyToken=3d67ed1f87d44c89'
Load from: 'C:\Program Files (x86)\Syncfusion\Essential Studio\WPF\26.1.35\Assemblies\4.6.2\Syncfusion.Shared.WPF.dll'
------------------
Resolve: 'System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Xaml.dll'
------------------
Resolve: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.dll'
------------------
Resolve: 'Syncfusion.Data.WPF, Version=26.1462.35.0, Culture=neutral, PublicKeyToken=3d67ed1f87d44c89'
Found single assembly: 'Syncfusion.Data.WPF, Version=26.1462.35.0, Culture=neutral, PublicKeyToken=3d67ed1f87d44c89'
Load from: 'C:\Program Files (x86)\Syncfusion\Essential Studio\WPF\26.1.35\Assemblies\4.6.2\Syncfusion.Data.WPF.dll'
------------------
Resolve: 'System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'System.ComponentModel.DataAnnotations, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.ComponentModel.DataAnnotations.dll'
------------------
Resolve: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Core.dll'
------------------
Resolve: 'System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Data.dll'
------------------
Resolve: 'System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'System.Runtime.Serialization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Runtime.Serialization.dll'
------------------
Resolve: 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'Microsoft.CSharp, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\Microsoft.CSharp.dll'
------------------
Resolve: 'ReachFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Found single assembly: 'ReachFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35'
Load from: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\ReachFramework.dll'
#endif
