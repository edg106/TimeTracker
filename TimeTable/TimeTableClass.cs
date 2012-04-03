using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Redmine.Net.Api.Types;

namespace TimeTable
{
   public class TimeTableClass : ViewModelBase
   {
      private DateTime _currentDate;
      private Zeit _newTime;
      private Stopwatch WorkTimer { get; set; }
      private DateTime _startDate;
      ObservableCollection<Zeit> _zeitplan;
      private bool _forceRefresh;
      public TimeTableClass()
      {
         Host = @"http://lily.intern/redmine";
         ApiKey = "3e28d0cc72ecd5a62559f989370f0d92d84ead14";
         StartDate = DateTime.Now;
         WorkTimer = new Stopwatch();
         CurrentDate = DateTime.Today;
         _forceRefresh = true;
         CloseCommand = new ActionCommand(CloseExecuteCommand);
         AbbrechenCommand = new ActionCommand(AbbrechenExecuteCommand);
         OkCommand = new ActionCommand(OkExecuteCommand);
         AddCommand = new ActionCommand(AddExecuteCommand, CanAddExecute);
         StartWorkCommand = new ActionCommand(StartWorkExecuteCommand, CanStartExecute);
         EndWorkCommand = new ActionCommand(EndWorkExecuteCommand, CanAddExecute);
         var t = new Timer(1000);
         t.Elapsed += t_Elapsed;
         t.Start();
         Manager = new OwnRedmineManager(Host, ApiKey);
         CurrentUser = Manager.GetCurrentUser();

      }

      private User CurrentUser { get; set; }

      private OwnRedmineManager Manager { get; set; }

      private bool CanStartExecute(object obj)
      {
         return !WorkTimer.IsRunning;
      }

      private bool CanAddExecute(object obj)
      {
         return WorkTimer.IsRunning;
      }

      private void EndWorkExecuteCommand(object obj)
      {
         WorkTimer.Stop();
      }

      private void StartWorkExecuteCommand(object obj)
      {
         //Manager.GetReport<TimeEntry>();

         WorkTimer.Start();
         StartDate = DateTime.Now;
         OnPropertyChanged(() => InfoLabels);
      }

      private void CloseExecuteCommand(object obj)
      {
         ObjectSerializer.SerializeToXML(KomplettListe);
         Application.Current.Shutdown();
      }

      private void AddExecuteCommand(object obj)
      {
         TimeSpan t = WorkTimer.Elapsed;

         NewTime = new Zeit { Dauer = new SerializableTimeSpan(t) };
         PopupIsOpen = true;
      }

      private void OkExecuteCommand(object obj)
      {
         var temp = new Zeit(NewTime.Dauer, NewTime.Ticket,0, NewTime.Kommentar, NewTime.Gebucht);
         if (temp.Ticket == 741)
            temp.Gebucht = true;
         Zeitplan.Add(temp);

         if (!KomplettListe.Any(x => x.Datum == DateTime.Today))
            KomplettListe.Add(new Zeiten { Datum = DateTime.Today, Items = Zeitplan });
         else
            KomplettListe.First(x => x.Datum == DateTime.Today).Items = Zeitplan;

         ObjectSerializer.SerializeToXML(KomplettListe);

         if (temp.Ticket == 741)
            CommitChange(temp);

         _forceRefresh = true;
         OnPropertyChanged(() => Zeitplan);
         NewTime = null;
         WorkTimer.Restart();
         PopupIsOpen = false;
      }

      private void CommitChange(Zeit temp)
      {
         var issue = Manager.GetObject<Issue>(temp.Ticket.ToString(CultureInfo.InvariantCulture), null);

         if (issue == null)
         {
            MessageBox.Show("Das Ticket ist im Redmine nicht vorhanden");
            return;
         }

         var project = Manager.GetObject<Project>(issue.Project.Id.ToString(CultureInfo.InvariantCulture), null);
         if (project != null && project.Name == "Demo")
         {
            var entry = new TimeEntry
                           {
                              Comments = temp.Kommentar,
                              Hours = SerializableTimeSpan.ToDecimal(temp.Dauer),
                              Issue = new IdentifiableName { Id = temp.Ticket }
                           };
            TaskLabel = GetMessage(Manager.CreateObject(entry));
         }
      }

      private string GetMessage(string message)
      {
         if (message.Contains("201"))
            return "Gespeichert!";
         if (message.Contains("422"))
            return "Fehler" + Environment.NewLine + message;
         return "ok";
      }

      private string _taskLabel;
      public string TaskLabel
      {
         get { return _taskLabel; }
         set { _taskLabel = value; 
         OnPropertyChanged(()=>TaskLabel);}
      }

      private IEnumerable<TimeEntry> GetValue(int offset)
      {
         var col = new NameValueCollection
                      {
                         //{"from", DateTime.Today.ToString("yy-MM-dd", DateTimeFormatInfo.InvariantInfo)},
                         //{"period_type", "2"},
                         //{"to", DateTime.Today.ToString("yy-MM-dd", DateTimeFormatInfo.InvariantInfo)},
                         {"offset", offset.ToString(CultureInfo.InvariantCulture)},
                         {"limit", "25"}
                      };


         //var deserializer = new XmlSerializer(typeof(List<TimeEntry>));
         //TextReader textReader = new StreamReader(Manager.GetStream<TimeEntry>(col));
         //var zeiten = (ObservableCollection<Zeiten>)deserializer.Deserialize(textReader);
         //textReader.Close();

         //return (IEnumerable<TimeEntry>) zeiten;
         
         
         return Manager.GetObjectList<TimeEntry>(col);
      }

      protected string ApiKey { get; set; }
      protected string Host { get; set; }

      static TimeSpan RoundTimeSpan(TimeSpan value)
      {
         return TimeSpan.FromSeconds(Math.Ceiling(value.TotalSeconds / 1) * 1);
      }

      private void AbbrechenExecuteCommand(object obj)
      {
         NewTime = null;
         PopupIsOpen = false;
      }

      public Zeit NewTime
      {
         get { return _newTime; }
         set
         {
            _newTime = value; OnPropertyChanged(() => NewTime);
         }
      }

      public IList<KeyValuePair<string, TimeSpan>> InfoLabels
      {
         get
         {
            return GetInfoLabels();
         }
      }

      private IList<KeyValuePair<string, TimeSpan>> GetInfoLabels()
      {
         return new List<KeyValuePair<string, TimeSpan>>
                   {
            new KeyValuePair<string,TimeSpan>("VerbeibendeZeitTag", GetRestTimeDay( DateTime.Today.DayOfWeek)),
            new KeyValuePair<string,TimeSpan>("VerbeibendeZeitWoche", GetRestTimeWeek( DateTime.Today.DayOfWeek)),
            new KeyValuePair<string,TimeSpan>("ÜberstundenTag", GetUeberstundenTimeDay( DateTime.Today.DayOfWeek)),
            new KeyValuePair<string,TimeSpan>("ÜberstundenWoche", GetUeberstundenTimeWeek( DateTime.Today.DayOfWeek))
         };
      }

      private TimeSpan GetUeberstundenTimeWeek(DayOfWeek dayOfWeek)
      {
         return new TimeSpan();
      }

      private bool _popupIsOpen;
      public bool PopupIsOpen
      {
         get { return _popupIsOpen; }
         set { _popupIsOpen = value; OnPropertyChanged(() => PopupIsOpen); }
      }

      private TimeSpan GetUeberstundenTimeDay(DayOfWeek dayOfWeek)
      {
         var result = new TimeSpan();

         if (WorkTimer.IsRunning)
         {
            if (DayOfWeek.Friday == dayOfWeek)
               result = ((new TimeSpan(DateTime.Now.TimeOfDay.Ticks) - new TimeSpan(8, 0, 0)) - new TimeSpan(6, 30, 00));

            else if (DayOfWeek.Saturday == dayOfWeek || DayOfWeek.Sunday == dayOfWeek)
               result = (DateTime.Now - StartDate);
            else
               result = ((new TimeSpan(DateTime.Now.TimeOfDay.Ticks) - new TimeSpan(8, 0, 0)) - new TimeSpan(8, 00, 00));

            if (result < new TimeSpan(0, 0, 1))
               return new TimeSpan();

         }
         return RoundTimeSpan(result);

      }

      private TimeSpan GetRestTimeWeek(DayOfWeek dayOfWeek)
      {
         return new TimeSpan();
      }

      public ICommand OkCommand { get; set; }
      public ICommand StartWorkCommand { get; set; }
      public ICommand EndWorkCommand { get; set; }
      public ICommand CloseCommand { get; set; }
      public ICommand AddCommand { get; set; }
      public ICommand AbbrechenCommand { get; set; }

      private TimeSpan GetRestTimeDay(DayOfWeek dayOfWeek)
      {
         TimeSpan result;

         if (DayOfWeek.Friday == dayOfWeek)
            result = RoundTimeSpan(new TimeSpan(6, 30, 00) - (new TimeSpan(DateTime.Now.TimeOfDay.Ticks) - new TimeSpan(8, 0, 0)));

         else if (DayOfWeek.Saturday == dayOfWeek || DayOfWeek.Sunday == dayOfWeek)
            result = new TimeSpan();
         else
            result = RoundTimeSpan(new TimeSpan(8, 00, 00) - (new TimeSpan(DateTime.Now.TimeOfDay.Ticks) - new TimeSpan(8, 0, 0)));
         return result;
      }

      private ICollection<TimeEntry> _timeEntries;
      private ICollection<TimeEntry> TimeEntries
      {
         get { return _timeEntries ?? (_timeEntries = new Collection<TimeEntry>()); }
      }

      public ObservableCollection<Zeit> Zeitplan
      {
         get
         {
            if (_zeitplan == null)
            {

               _zeitplan = new ObservableCollection<Zeit>();
            }

            if (_forceRefresh)
            {
               _timeEntriesReady = false;
               int offset = 1;
               while (!_timeEntriesReady)
               {
                  var liste = GetValue(offset);

                  foreach (TimeEntry entry in liste)
                  {
                     if (!entry.SpentOn.HasValue || entry.SpentOn.Value != CurrentDate)
                     {
                        _timeEntriesReady = true;
                        break;
                     }

                     if (TimeEntries != null && entry.User.Id == CurrentUser.Id)
                     {
                        Debug.Print("AddEntry" + entry.Id);
                        TimeEntries.Add(entry);
                     }
                  }

                  offset++;
               }

               _zeitplan = new ObservableCollection<Zeit>();

               StringToRoundedDateTimeConverter c = new StringToRoundedDateTimeConverter();

               foreach (TimeEntry entry in TimeEntries)
                  _zeitplan.Add(new Zeit(SerializableTimeSpan.ToTimeSpan(entry.Hours),entry.Id,entry.Issue.Id,entry.Comments,true ));


               _forceRefresh = false;
            }

            return _zeitplan;
         }
         set { _zeitplan = value; }
      }

      private ObservableCollection<Zeit> GetZeitplan(DateTime currentDate)
      {
         if (KomplettListe != null && KomplettListe.Count != 0)
         {
            var item = KomplettListe.Where(x => x.Datum == currentDate);
            {
               var firstOrDefault = item.FirstOrDefault();
               if (firstOrDefault != null)
                  return firstOrDefault.Items;
            }
         }
         return new ObservableCollection<Zeit>();
      }

      private ObservableCollection<Zeiten> _komplettListe;
      private bool _timeEntriesReady;

      private ObservableCollection<Zeiten> KomplettListe
      {
         get
         {
            if (_komplettListe == null || _forceRefresh)
               _komplettListe = ObjectSerializer.DeserializeFromXML();
            return _komplettListe;
         }
      }

      public DateTime StartDate { get { return _startDate; } set { _startDate = value; OnPropertyChanged(() => StartDate); } }
      public DateTime CurrentDate
      {
         get { return _currentDate; }
         set
         {
            _currentDate = value;
            _forceRefresh = true;
            OnPropertyChanged(() => Zeitplan);
         }
      }

      public string Uhrzeit { get { return DateTime.Now.ToLongTimeString(); } }

      public bool MainPopUpIsOpen { get; set; }

      void t_Elapsed(object sender, ElapsedEventArgs e)
      {
         OnPropertyChanged(() => Uhrzeit);
         OnPropertyChanged(() => InfoLabels);
      }
   }
}