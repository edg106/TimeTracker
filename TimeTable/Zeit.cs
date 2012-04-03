using System;

namespace TimeTable
{
   public class Zeit : ViewModelBase
   {
      private SerializableTimeSpan _zeit;
      private int _ticket;
      private string _kommentar;
      private bool _gebucht;

      public Zeit(SerializableTimeSpan span, int ticket, int id, string kommentar, bool gebucht)
      {
         Dauer = span;
         Ticket = ticket;
         ID = id;
         Kommentar = kommentar;
         Gebucht = gebucht;
      }

      public Zeit()
      {
         Ticket = 741;
      }


      public SerializableTimeSpan Dauer
      {
         get
         {
            return _zeit;
         }
         set
         {
            _zeit = value;
            OnPropertyChanged(() => Dauer);
         }
      }
      public int Ticket
      {
         get
         {
            return _ticket;
         }
         set
         {
            _ticket = value;
            OnPropertyChanged(() => TicketUri);
            OnPropertyChanged(() => Ticket);
         }
      }

      public int ID { get; set; }

      public string Kommentar
      {
         get { return _kommentar; }
         set { _kommentar = value; 
            OnPropertyChanged(() => Kommentar); }
      }

      public bool Gebucht
      {
         get { return _gebucht; }
         set 
         {
            _gebucht = value;
            OnPropertyChanged(() => Gebucht); 
         }
      }

      [System.Xml.Serialization.XmlIgnoreAttribute]
      public Uri TicketUri { get { return new Uri("http://lily.intern/redmine/issues/" + Ticket); } }

      [System.Xml.Serialization.XmlIgnoreAttribute]
      public DateTime Start { get; set; }
   }
}
