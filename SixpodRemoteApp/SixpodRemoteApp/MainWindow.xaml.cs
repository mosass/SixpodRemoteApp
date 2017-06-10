using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SixpodRemoteApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SixpodSocket remoteRobot;
        private bool walking;
        private REMOTECMD gaitType;

        public MainWindow()
        {
            InitializeComponent();
            walking = false;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            remoteRobot = new SixpodSocket(txt_ipaddress.Text, this);

            sld_stepTime.Minimum = 1;
            sld_stepTime.Maximum = 10;
            sld_stepTime.SmallChange = 1;
            sld_stepTime.Value = 5;

            sld_gaitZPercent.Minimum = 0;
            sld_gaitZPercent.Maximum = 100;
            sld_gaitZPercent.SmallChange = 1;
            sld_gaitZPercent.LargeChange = 5;
            sld_gaitZPercent.Value = 50;

            gaitType = REMOTECMD.WALKING_WAV;
            rdo_wave.IsChecked = true;
        }

        private void txt_log_TextChanged(object sender, TextChangedEventArgs e)
        {
            txt_log.ScrollToEnd();
        }

        private void sld_stepTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txt_set_stepTime.Text = (sld_stepTime.Value / 10f).ToString("F2");  
        }

        private void sld_gaitZPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            txt_set_gaitZPercent.Text = ((int) sld_gaitZPercent.Value).ToString("G") + " %";
        }

        private void btn_apply_Click(object sender, RoutedEventArgs e)
        {
            // if connected
            if (remoteRobot.remote_flag)
            {
                float step_time_val = (float) (sld_stepTime.Value / 10f);
                float stepZUp_val = (float) (sld_gaitZPercent.Value / 100f);
                remoteRobot.sendCmd(REMOTECMD.SET_STEP_TIME, step_time_val);
                remoteRobot.sendCmd(REMOTECMD.SET_STEP_UPZ, stepZUp_val);
            }
        }

        private void btn_walking_Click(object sender, RoutedEventArgs e)
        {
            if(remoteRobot.remote_flag)
            {
                if (walking)
                {
                    remoteRobot.sendCmd(REMOTECMD.WALKING_STOP);
                    walking = false;
                    btn_walking.Content = "Start";
                }
                else
                {
                    remoteRobot.sendCmd(gaitType);
                    walking = true;
                    btn_walking.Content = "Stop";
                }
            }
        }

        private void rdo_wave_Checked(object sender, RoutedEventArgs e)
        {
            gaitType = REMOTECMD.WALKING_WAV;
        }

        private void rdo_ripple_Checked(object sender, RoutedEventArgs e)
        {
            gaitType = REMOTECMD.WALKING_RIPP;
        }

        private void rdo_tripod_Checked(object sender, RoutedEventArgs e)
        {
            gaitType = REMOTECMD.WALKING_TRI;
        }

        private void btn_updatefilelist_Click(object sender, RoutedEventArgs e)
        {
            string[] filename = remoteRobot.sendCmd(REMOTECMD.GET_FILE_LIST).Split(',');
            lsb_filename.Items.Clear();
            foreach (string s in filename)
            {
                lsb_filename.Items.Add(s);
            }
        }

        private void lsb_filename_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lsb_filename.SelectedIndex >= 0)
            {
                remoteRobot.sendCmd(REMOTECMD.SET_SELECT_POS, lsb_filename.SelectedIndex + 1);
            }
        }

        private void btn_connectLogs_Click(object sender, RoutedEventArgs e)
        {
            remoteRobot.setIpAddress(txt_ipaddress.Text);

            if (remoteRobot.logs_recv_flag)
            {
                remoteRobot.logs_recv_flag = false;
            }
            else
            {
                new Task(() =>
                {
                    remoteRobot.StartDebugClient();
                }).Start();
            }
        }

        private void btn_connectRemote_Click(object sender, RoutedEventArgs e)
        {
            remoteRobot.setIpAddress(txt_ipaddress.Text);

            if (remoteRobot.remote_flag)
            {
                remoteRobot.StopClient();
            }
            else
            {
                new Task(() => {
                    remoteRobot.StartClient();
                }).Start();
            }
        }
    }
}
