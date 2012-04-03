using System;
using System.Collections.ObjectModel;

namespace TimeTable
{
   public class Zeiten
   {
      public DateTime Datum { get; set; }
      public ObservableCollection<Zeit> Items { get; set; }
   }
}
