using LoanManagementApp.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace LoanManagementApp.Helpers
{
    // ═══════════════════════════════════════════════════════════════
    // Converters.cs / کنورٹرز
    // WPF IValueConverter implementations for XAML bindings
    // ═══════════════════════════════════════════════════════════════

    // ─── 1. Boolean to Visibility | بولین سے ویزیبیلٹی ────────────
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bVal = value is bool b && b;
            return bVal ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Visible;
        }
    }

    // ─── 2. Inverse Boolean to Visibility | الٹا بولین سے ویزیبیلٹی
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bVal = value is bool b && b;
            return bVal ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility v && v == Visibility.Collapsed;
        }
    }

    // ─── 3. Inverse Boolean | الٹا بولین ────────────────────────────
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : true;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b ? !b : true;
    }

    // ─── 4. Null to Visibility | null سے ویزیبیلٹی ─────────────────
    [ValueConversion(typeof(object), typeof(Visibility))]
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value == null ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 5. Zero to Visibility | صفر سے ویزیبیلٹی ──────────────────
    [ValueConversion(typeof(int), typeof(Visibility))]
    public class ZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = value is int i ? i : 0;
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 6. Status to Color | حیثیت سے رنگ ──────────────────────────
    [ValueConversion(typeof(LoanStatus), typeof(Brush))]
    public class LoanStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LoanStatus status)
            {
                return status switch
                {
                    LoanStatus.Active => new SolidColorBrush(Color.FromRgb(39, 103, 73)),
                    LoanStatus.Closed => new SolidColorBrush(Color.FromRgb(74, 85, 104)),
                    LoanStatus.Overdue => new SolidColorBrush(Color.FromRgb(197, 48, 48)),
                    LoanStatus.Merged => new SolidColorBrush(Color.FromRgb(43, 108, 176)),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 7. Status to Background | حیثیت سے پس منظر ────────────────
    [ValueConversion(typeof(LoanStatus), typeof(Brush))]
    public class LoanStatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LoanStatus status)
            {
                return status switch
                {
                    LoanStatus.Active => new SolidColorBrush(Color.FromRgb(198, 246, 213)),
                    LoanStatus.Closed => new SolidColorBrush(Color.FromRgb(237, 242, 247)),
                    LoanStatus.Overdue => new SolidColorBrush(Color.FromRgb(254, 215, 215)),
                    LoanStatus.Merged => new SolidColorBrush(Color.FromRgb(190, 227, 248)),
                    _ => new SolidColorBrush(Colors.LightGray)
                };
            }
            return new SolidColorBrush(Colors.LightGray);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 8. Decimal to Currency String | رقم سے کرنسی سٹرنگ ─────────
    [ValueConversion(typeof(decimal), typeof(string))]
    public class DecimalToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal amount = value is decimal d ? d : 0;
            string symbol = parameter as string ?? "PKR";
            return $"{amount:N0} {symbol}";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? str = value?.ToString()?.Replace(",", "").Replace("PKR", "").Trim();
            return decimal.TryParse(str, out decimal result) ? result : 0m;
        }
    }

    // ─── 9. Decimal to Progress | رقم سے پروگریس ────────────────────
    public class AmountToProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return 0.0;
            decimal paid = values[0] is decimal p ? p : 0;
            decimal total = values[1] is decimal t ? t : 0;
            if (total <= 0) return 0.0;
            double pct = (double)(paid / total) * 100;
            return Math.Min(pct, 100.0);
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ─── 10. Boolean to Status Color | بولین سے حیثیت رنگ ──────────
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSuccess = value is bool b && b;
            return isSuccess ? new SolidColorBrush(Color.FromRgb(39, 103, 73)) : new SolidColorBrush(Color.FromRgb(197, 48, 48));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 11. Boolean to Status Background | بولین سے حیثیت پس منظر ──
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToStatusBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSuccess = value is bool b && b;
            return isSuccess ? new SolidColorBrush(Color.FromRgb(198, 246, 213)) : new SolidColorBrush(Color.FromRgb(254, 215, 215));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 12. Overdue to Color | میعاد ختم سے رنگ ────────────────────
    [ValueConversion(typeof(bool), typeof(Brush))]
    public class OverdueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOverdue = value is bool b && b;
            return isOverdue ? new SolidColorBrush(Color.FromRgb(197, 48, 48)) : new SolidColorBrush(Color.FromRgb(39, 103, 73));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 13. DateTime to Short String | DateTime سے مختصر سٹرنگ ─────
    [ValueConversion(typeof(DateTime), typeof(string))]
    public class DateToShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt) return dt.ToString("dd-MMM-yyyy");
            return string.Empty;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DateTime.TryParse(value?.ToString(), out DateTime result)) return result;
            return DateTime.Today;
        }
    }

    // ─── 14. PaymentType to Display | ادائیگی قسم سے ڈسپلے ──────────
    [ValueConversion(typeof(PaymentType), typeof(string))]
    public class PaymentTypeToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PaymentType pt)
            {
                return pt switch
                {
                    PaymentType.Cash => "💵 Cash | نقد",
                    PaymentType.Bank => "🏦 Bank | بینک",
                    PaymentType.Online => "📱 Online | آن لائن",
                    PaymentType.Cheque => "📋 Cheque | چیک",
                    _ => "—"
                };
            }
            return "—";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── 15. String Not Empty to Visibility | سٹرنگ سے ویزیبیلٹی ────
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringNotEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasValue = !string.IsNullOrWhiteSpace(value?.ToString());
            return hasValue ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── Percentage to Width Converter (for MultiBinding) ───────────
    public class PercentageToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 1) return 0.0;
            double percentage = 0;
            if (values[0] is double d) percentage = d;
            else if (values[0] is float f) percentage = f;
            else if (values[0] is int i) percentage = i;
            else if (values[0] is decimal dec) percentage = (double)dec;
            percentage = Math.Max(0, Math.Min(100, percentage));
            return percentage;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ─── Initials Converter - Get initials from full name ──────────
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString())) return "?";
            string name = value.ToString()!.Trim();
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return "?";
            if (parts.Length == 1) return parts[0].Length > 0 ? parts[0][0].ToString().ToUpper() : "?";
            string firstInitial = parts[0].Length > 0 ? parts[0][0].ToString().ToUpper() : "";
            string lastInitial = parts[parts.Length - 1].Length > 0 ? parts[parts.Length - 1][0].ToString().ToUpper() : "";
            return $"{firstInitial}{lastInitial}";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ═══════════════════════════════════════════════════════════════
    // DIALOG CONVERTERS
    // ═══════════════════════════════════════════════════════════════

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToStatusBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSuccess = value is bool b && b;
            return isSuccess ? new SolidColorBrush(Color.FromRgb(39, 103, 73)) : new SolidColorBrush(Color.FromRgb(197, 48, 48));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToStatusForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isSuccess = value is bool b && b;
            return isSuccess ? new SolidColorBrush(Color.FromRgb(154, 230, 180)) : new SolidColorBrush(Color.FromRgb(252, 129, 129));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToWarningBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isWarning = value is bool b && b;
            return isWarning ? new SolidColorBrush(Color.FromArgb(255, 62, 26, 0)) : new SolidColorBrush(Color.FromArgb(255, 10, 37, 64));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    [ValueConversion(typeof(bool), typeof(Brush))]
    public class BoolToWarningBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isWarning = value is bool b && b;
            return isWarning ? new SolidColorBrush(Color.FromRgb(230, 81, 0)) : new SolidColorBrush(Color.FromRgb(2, 119, 189));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    [ValueConversion(typeof(bool), typeof(Style))]
    public class BoolToDangerPrimaryButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isWarning = value is bool b && b;
            string key = isWarning ? "DangerButtonStyle" : "PrimaryButtonStyle";
            if (Application.Current?.Resources[key] is Style style) return style;
            return Application.Current?.Resources["PrimaryButtonStyle"] ?? DependencyProperty.UnsetValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => DependencyProperty.UnsetValue;
    }

    // ─── AED to PKR Currency Converter ─────────────────────────────
    public class AedToPkrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0 PKR";
            decimal amount = 0;
            if (value is decimal dec) amount = dec;
            else if (value is double d) amount = (decimal)d;
            else if (value is float f) amount = (decimal)f;
            else if (value is int i) amount = i;
            else if (value is string s && decimal.TryParse(s, out decimal parsed)) amount = parsed;
            decimal pkrAmount = amount * CurrencyConverter.ExchangeRate;
            return $"{pkrAmount:N0} PKR";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0m;
            string? str = value.ToString();
            if (string.IsNullOrWhiteSpace(str)) return 0m;
            string cleaned = str.Replace(" PKR", "").Replace(",", "").Trim();
            if (decimal.TryParse(cleaned, out decimal pkrAmount))
            {
                if (CurrencyConverter.ExchangeRate > 0)
                    return pkrAmount / CurrencyConverter.ExchangeRate;
            }
            return 0m;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // NEW CONVERTERS (added for missing ones)
    // ═══════════════════════════════════════════════════════════════

    // ─── String to Visibility ──────────────────────────────────────
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString()) ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }

    // ─── Index Converter for ListView row numbers ──────────────────
    /// <summary>
    /// IndexConverter | انڈیکس کنورٹر
    /// Converts a 0-based AlternationIndex to a 1-based row number for display.
    /// 0 سے شروع ہونے والے AlternationIndex کو 1 سے شروع ہونے والے نمبر میں بدلتا ہے۔
    ///
    /// USAGE in XAML:
    ///   ListView AlternationCount="{Binding FilteredCustomers.Count}"
    ///   Binding Path="(ItemsControl.AlternationIndex)"
    ///           RelativeSource="{RelativeSource AncestorType=ListViewItem}"
    ///           Converter="{StaticResource IndexConverter}"
    /// </summary>
    public class IndexConverter : IValueConverter
    {
        // ─── Convert | تبدیل کریں ────────────────────────────────────
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // AlternationIndex is 0-based → return index + 1 for display
            if (value is int index)
                return (index + 1).ToString();

            return "–";
        }

        // ─── ConvertBack | واپس تبدیل کریں ─────────────────────────
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ─── Amount to Height for Chart Bars ───────────────────────────
    public class AmountToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal amount = value is decimal d ? d : 0;
            double maxHeight = 120;
            double maxAmount = 500000;
            double amountDouble = (double)amount;
            double height = (amountDouble / maxAmount) * maxHeight;
            return Math.Max(8, Math.Min(height, maxHeight));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ─── Overdue to Background Converter | میعاد ختم سے پس منظر ──
    public class OverdueToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOverdue = value is bool b && b;
            return isOverdue
                ? Application.Current.Resources["DangerLightBrush"]
                : Application.Current.Resources["SuccessLightBrush"];
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    // ─── Overdue to Text Converter | میعاد ختم سے متن ──
    public class OverdueToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOverdue = value is bool b && b;
            return isOverdue ? "⚠️ Overdue" : "✅ Active";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}