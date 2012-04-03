using System;
using System.Globalization;
using System.Xml.Serialization;

namespace TimeTable
{
   [Serializable]
   public class SerializableTimeSpan
   {
      /// <summary>
      /// Initializes a new instance of the <see cref="SerializableTimeSpan"/> class.
      /// </summary>
      public SerializableTimeSpan() { }

      /// <summary>
      /// Initializes a new instance of the <see cref="SerializableTimeSpan"/> class.
      /// </summary>
      /// <param name="dauer">The original timeSpan.</param>
      public SerializableTimeSpan(TimeSpan dauer)
      {
         Duration = dauer;
      }

      private TimeSpan _duration;

      /// <summary>
      /// Gets or sets the duration.
      /// </summary>
      /// <value>The duration.</value>
      /// <remarks>Das echte Property</remarks>
      [XmlIgnore]
      public TimeSpan Duration
      {
         get { return _duration; }
         set { _duration = value; }
      }

      /// <summary>
      /// Gets or sets the duration of the XML.
      /// </summary>
      /// <value>The duration of the XML.</value>
      /// <remarks>Property für XML</remarks>
      [XmlElement("Duration")]
      public string XmlDuration
      {
         get { return Duration.ToString(); }
         set { Duration = TimeSpan.Parse(value); }
      }

      public static decimal ToDecimal(SerializableTimeSpan time)
      {
         var c = new StringToRoundedDateTimeConverter();
         var s = c.Convert(time, null, null, null) as string;
         if (s != null)
         {
            var smin = s.Remove(0, 3);
            double hour = double.Parse(s.Remove(2, s.Length - 2));
            if (smin == "15:00")
            {
               hour += 0.25;
            }
            else if (smin == "30:00")
            {
               hour += 0.50;

            }
            else if (smin == "45:00")
            {
               hour += 0.75;

            }
            return (decimal)hour;
         }
         return 0;
      }

      public static SerializableTimeSpan ToTimeSpan(decimal time)
      {
         var smin = time.ToString(CultureInfo.InvariantCulture).Split(Convert.ToChar("."));
         SerializableTimeSpan hour = null;
         if (smin.Length == 2)
         {
            if (smin[1] == "25")
            {
               hour = new SerializableTimeSpan(new TimeSpan(0, Convert.ToInt32(smin[0]), 15));
            }
            if (smin[1] == "50")
            {
               hour = new SerializableTimeSpan(new TimeSpan(0, Convert.ToInt32(smin[0]), 30));

            }
            if (smin[1] == "75")
            {
               hour = new SerializableTimeSpan(new TimeSpan(0, Convert.ToInt32(smin[0]), 45));

            } 
         }
         return hour;
      }
   }
}
