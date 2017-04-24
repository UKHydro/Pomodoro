using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;
using MessageBox = System.Windows.MessageBox;

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly NotifyIcon _ni;
        System.ComponentModel.BackgroundWorker _workTime;
        private int _clicks;

        public MainWindow()
        {
            CreateWorkTime();

            InitializeComponent();

            var bitmap = Image.FromFile(ConfigurationManager.AppSettings.Get("picture")) as Bitmap; 

            var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();

            this.bg_image.ImageSource = bitmapImage;

            _ni = new NotifyIcon();
            _ni.Icon = Pomodoro.Properties.Resources.pomodoro;
            ToolStripMenuItem tmiClose = new ToolStripMenuItem();
            tmiClose.Text = Properties.Resources.MainWindow_MainWindow_Close;
            tmiClose.Click += tmiClose_Click;

            var tmiStart = new ToolStripMenuItem()
            {
                Text = Properties.Resources.MainWindow_MainWindow_Start,

            };
            tmiStart.Click += tmiStart_Click;

            var tmiStop = new ToolStripMenuItem()
            {
                Text = Properties.Resources.MainWindow_MainWindow_Stop,

            };
            tmiStop.Click += tmiStop_Click;


            ContextMenuStrip cmsClose = new ContextMenuStrip();
            cmsClose.Items.Add(tmiStart);
            cmsClose.Items.Add(tmiStop);
            cmsClose.Items.Add(tmiClose);

            _ni.ContextMenuStrip = cmsClose;
            _ni.BalloonTipText = "Pomodoro is in the tray, if you need to close it before the end of the Pomodoro Period.Right click for more options";
            _ni.Visible = true;
        }

        private void CreateWorkTime()
        {
            _workTime = new BackgroundWorker();
            _workTime.DoWork += WorkTime_DoWork;
            _workTime.RunWorkerCompleted += WorkTime_Done;
        }

        void tmiClose_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void cmClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnWork_Click(object sender, RoutedEventArgs e)
        {
            StartWork();
        }

        private void StartWork()
        {
            this.ShowInTaskbar = false;
            this.Hide();
            _workTime.RunWorkerAsync("PomodoroPeriod");
        }

        private void StartBreak()
        {
            this.ShowInTaskbar = false;
            this.Hide();
            _workTime.RunWorkerAsync("BreakPeriod");
        }


        private void WorkTime_Done(object sender, RunWorkerCompletedEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Show();
            var p = new SoundPlayer(Pomodoro.Properties.Resources.beeps);
            p.Play();
        }

        private void WorkTime_DoWork(object sender, DoWorkEventArgs e)
        {
            var value = (string)e.Argument;
            AppSettingsReader asr = new AppSettingsReader();
            int workTime = 60 * 1000 * (int)asr.GetValue(value, typeof(int));
            System.Threading.Thread.Sleep(workTime);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ni.ShowBalloonTip(2000);
        }


        void tmiStart_Click(object sender, EventArgs e)
        {
            StartWork();
        }

        private void tmiStop_Click(object sender, EventArgs e)
        {
            _workTime.Dispose();
            CreateWorkTime();
            this.ShowInTaskbar = true;
            this.Show();
            
        }

        private void btnBreakFromWork_Click(object sender, RoutedEventArgs e)
        {
            _clicks++;

            if (_clicks % 4 == 0)
            {
                MessageBox.Show("This is your 4th Break, you get a longer break.");

                StartWork();               
            }
            else
            {
                StartBreak();
            }
        }
    }
}
