using System;
using System.Globalization;
using System.Windows.Data;

namespace TimeTable
{
   public class StringToRoundedDateTimeConverter : IValueConverter
   {
      static TimeSpan RoundTimeSpan(TimeSpan value)
      {
         return TimeSpan.FromMinutes(System.Math.Ceiling(value.TotalMinutes / 15) * 15);
      }

      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var serializableTimeSpan = value as SerializableTimeSpan;
         if (serializableTimeSpan != null)
            return RoundTimeSpan((serializableTimeSpan).Duration).ToString();

         return "";
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {

         if (value != null)
            return new SerializableTimeSpan(TimeSpan.Parse(value.ToString()));
         return new TimeSpan();
      }
   }

   public class StringToDateTimeConverter : IValueConverter
   {
     
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var serializableTimeSpan = value as SerializableTimeSpan;
         if (serializableTimeSpan != null)
            return ((serializableTimeSpan).Duration).ToString();

         return "";
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {

         if (value != null)
            return new SerializableTimeSpan(TimeSpan.Parse(value.ToString()));
         return new TimeSpan();
      }
   }
}
