using FluentScheduler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

namespace TaskManager11._06._19
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       Service _appService;

        public MainWindow()
        {
            InitializeComponent();

            _appService = new Service();

            List<Types> periodTypes = Enum.GetValues(typeof(Types)).Cast<Types>().ToList();
            TypeComboBox.ItemsSource = periodTypes;
            TypeComboBox.SelectedValue = Types.Day;

            startDate.SelectedDate = DateTime.Now;
            endDate.SelectedDate = DateTime.Now.AddDays(1);


            startTime.Text = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();
            endTime.Text = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();

        }

        private void OpenExecute(object sender, ExecutedRoutedEventArgs e)
        {
            Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            base.OnClosing(e);

        }

        private async void SendEmailAsync()
        {
            string emailText = new TextRange(richTextBoxMessage.Document.ContentStart, richTextBoxMessage.Document.ContentEnd).Text;
            var result = await _appService.SendEmailAsync("theasanali7@gmail.com", "davisdex", toTextBox.Text, themeText.Text, emailText);

            MessageBox.Show(result);
        }

        private void SendButtonClick(object sender, RoutedEventArgs e)
        {
            if (toTextBox.Text == string.Empty || themeText.Text == string.Empty)
            {
                MessageBox.Show("Заполните все поля для опрации отправки сообщения!");
                return;
            }

            if (TypeComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите период отправки сообщений!");
                return;
            }

            if (!TimeSpan.TryParse(startTime.Text, out TimeSpan timeStart))
            {
                MessageBox.Show("Время старта заполнено не корректно!");
                return;
            }

            if (!TimeSpan.TryParse(startTime.Text, out TimeSpan timeEnd))
            {
                MessageBox.Show("Время конца заполнено не корректно!");
                return;
            }

            if (startDate.SelectedDate.Value.AddHours(timeStart.Hours).AddMinutes(timeStart.Minutes) < DateTime.Now)
            {
                MessageBox.Show("Дата старта операции отправки сообщения не должна быть меньше нынешней даты!");
                return;
            }

            if (endDate.SelectedDate.Value.AddHours(timeEnd.Hours).AddMinutes(timeEnd.Minutes) < startDate.SelectedDate.Value.AddHours(timeStart.Hours).AddMinutes(timeStart.Minutes))
            {
                MessageBox.Show("Дата конца операции отправки сообщения не должна быть меньше даты старта!");
                return;
            }

            const int INTERVAL = 1;
            var periodType = (Types)TypeComboBox.SelectedValue;

            string jobName = "";

            if (periodType == Types.Day)
            {
                jobName = "DayJob";
                JobManager.AddJob(() => Dispatcher.BeginInvoke(new Action(SendEmailAsync), DispatcherPriority.Background), (schedule) => schedule.WithName(jobName).ToRunEvery(INTERVAL).Days().At(timeStart.Hours, timeStart.Minutes));
            }
            else if (periodType == Types.Week)
            {
                jobName = "WeekJob";
                JobManager.AddJob(() => Dispatcher.BeginInvoke(new Action(SendEmailAsync), DispatcherPriority.Background), (schedule) => schedule.WithName(jobName).ToRunEvery(INTERVAL).Weeks().On(DayOfWeek.Monday).At(timeStart.Hours, timeStart.Minutes));
            }
            else if (periodType == Types.Month)
            {
                jobName = "MonthJob";
                JobManager.AddJob(() => Dispatcher.BeginInvoke(new Action(SendEmailAsync), DispatcherPriority.Background), (schedule) => schedule.WithName(jobName).ToRunEvery(INTERVAL).Months().OnTheFirst(DayOfWeek.Monday).At(timeStart.Hours, timeStart.Minutes));
            }
            else if (periodType == Types.Year)
            {
                jobName = "YearJob";
                JobManager.AddJob(() => Dispatcher.BeginInvoke(new Action(SendEmailAsync), DispatcherPriority.Background), (schedule) => schedule.WithName(jobName).ToRunEvery(INTERVAL).Years().On(1).At(timeStart.Hours, timeStart.Minutes));
            }

            if (startDate.DisplayDate == DateTime.Now.Date)
            {
                JobManager.Start();
            }

            if (endDate.SelectedDate.Value.AddHours(timeEnd.Hours).AddMinutes(timeEnd.Minutes) == DateTime.Now.Date)
            {
                JobManager.Stop();
                JobManager.RemoveJob(jobName);
            }
        }

        private void AddOnceInJob(DateTime operationStartDate, TimeSpan timeExecute, string jobName, Action action)
        {
            TimeSpan intervalTime = operationStartDate.AddHours(timeExecute.Hours).AddMinutes(timeExecute.Minutes) - DateTime.Now;
            int intervalCountSeconds = (int)intervalTime.TotalSeconds;

            JobManager.AddJob(() => Dispatcher.BeginInvoke(new Action(action), DispatcherPriority.Background), (schedule) => schedule.WithName(jobName).ToRunOnceIn(intervalCountSeconds).Seconds());

            if (operationStartDate.AddHours(timeExecute.Hours).AddMinutes(timeExecute.Minutes) == DateTime.Now)
            {
                JobManager.Stop();
                JobManager.RemoveJob(jobName);
            }
        }

        private async void MoveDirectoryAsync()
        {
            var result = await _appService.MoveCatalog(fromPathDirectory.Text, toPathDirectory.Text);
            MessageBox.Show(result);
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (fromPathDirectory.Text == string.Empty || toPathDirectory.Text == string.Empty)
            {
                MessageBox.Show("Fill in all fields for the directory movement.!");
                return;
            }

            if (dateToMove.SelectedDate.Value == null || !TimeSpan.TryParse(timeToMove.Text, out TimeSpan timeExecute))
            {
                MessageBox.Show("The execution time is not correct!");
                return;
            }

            if (dateToMove.SelectedDate.Value.AddHours(timeExecute.Hours).AddMinutes(timeExecute.Minutes) < DateTime.Now)
            {
                MessageBox.Show("The start date of the directory transfer operation must not be less than the current date.!");
                return;
            }

            AddOnceInJob(dateToMove.SelectedDate.Value, timeExecute, "moveFile", MoveDirectoryAsync);
        }

        private async void DownloadFileAsync()
        {
            var result = await _appService.DownloadFile(fromPathDownload.Text, toPathDownload.Text);
            MessageBox.Show(result);
        }

        

        private void EnableSendMailMenu(object sender, RoutedEventArgs e)
        {
            mailGrid.IsEnabled = true;
            mailDateGrid.IsEnabled = true;
            moveGrid.IsEnabled = false;
        }

        private void EnableMoveDirectoryMenu(object sender, RoutedEventArgs e)
        {
            moveGrid.IsEnabled = true;
            mailGrid.IsEnabled = false;
            mailDateGrid.IsEnabled = false;
        }

        private void EnableDownloadFileMenu(object sender, RoutedEventArgs e)
        {
            moveGrid.IsEnabled = false;
            mailGrid.IsEnabled = false;
            mailDateGrid.IsEnabled = false;
        }
    }
}