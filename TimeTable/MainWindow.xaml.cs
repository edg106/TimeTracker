using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;
using MessageBox = System.Windows.MessageBox;

namespace TimeTable
{
   /// <summary>
   /// Interaktionslogik für MainWindow.xaml
   /// </summary>
   public partial class MainWindow
   {
      public MainWindow()
      {
         InitializeComponent();
         int deskWidth = Screen.PrimaryScreen.Bounds.Width;
         Left = deskWidth - 110;
         Top = 25;
      }

      protected override void OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs e)
      {
         if (e.Property == IsMouseOverProperty && ((bool)e.NewValue) == false && !((TimeTableClass)MainOuterGrid.DataContext).PopupIsOpen && !OuterPopUp.IsMouseOver)
            OuterPopUp.IsOpen = false;

         if (e.Property == IsMouseOverProperty && ((bool)e.NewValue))
            OuterPopUp.IsOpen = true;

         base.OnPropertyChanged(e);
      }

      private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
      {
         Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
         e.Handled = true;
      }

      private void TitleBarGridMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         DragMove();
      }

      private void OuterPopUp_MouseLeave_1(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (!((TimeTableClass)MainOuterGrid.DataContext).PopupIsOpen && !IsMouseOver)
            OuterPopUp.IsOpen = false;
      }
   }
}
