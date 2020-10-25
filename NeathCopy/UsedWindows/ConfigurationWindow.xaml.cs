using NeathCopy.Module2_Configuration;
using NeathCopy.Themes;
using NeathCopyEngine.CopyHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeathCopy.UsedWindows
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        bool loading;

        Dictionary<string, RadioButton> AddDataRbts;
        Dictionary<string, RadioButton> HowStartRbts;
        Dictionary<string, RadioButton> AddNewVisualCopyRbts;
        Dictionary<string, RadioButton> AffterErrorRbts;

        int tmp;
        public ConfigurationWindow()
        {
            InitializeComponent();

            AddDataRbts = new Dictionary<string, RadioButton>();
            AddDataRbts.Add("SameDestiny_Process_ADD_DATA", add_SameDestiny_rb);
            AddDataRbts.Add("SameVolumen_Process_ADD_DATA", add_SameVolumen_rb);
            AddDataRbts.Add("AddToFirsth_Process_ADD_DATA", addToFirsth_rb);
            AddDataRbts.Add("AddToLast_Process_ADD_DATA", addToLast_rb);

            HowStartRbts = new Dictionary<string, RadioButton>();
            HowStartRbts.Add("StartOperation_SetRunningState", sob_AutomatlyStart_rb);
            HowStartRbts.Add("Inqueve_SetRunningState", sob_WaitInQueve_rb);

            AddNewVisualCopyRbts = new Dictionary<string, RadioButton>();
            AddNewVisualCopyRbts.Add("AllInOne_AddNewVC",ancb_All_In_One_rb);
            AddNewVisualCopyRbts.Add("SeparateWindows_AddNewVC",ancb_Use_Separate_Windos_rb);

            AffterErrorRbts = new Dictionary<string, RadioButton>();
            AffterErrorRbts.Add("Keep_AffterErrorAction", keepAffterError_rb);
            AffterErrorRbts.Add("Close_AffterErrorAction", closeAffterError_rb);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loading = true;

            //Themes and languaje
            theme_cb.ItemsSource = Configuration.Thems;
            skins_cb.ItemsSource = Configuration.VisualCopySkins;
            brushes_cb.ItemsSource = Configuration.Brushes;
            font_cb.ItemsSource = Configuration.Fonts;
            language_cb.ItemsSource = Configuration.Languages;

            theme_cb.SelectedItem = Configuration.Main.Theme;
            skins_cb.SelectedItem = Configuration.Main.VisualCopySkin;
            brushes_cb.SelectedItem = Configuration.Main.Brush;
            font_cb.SelectedItem = Configuration.Main.Font;
            language_cb.SelectedItem = Configuration.Main.Language;

            //Add Dara
            AddDataRbts[Configuration.Main.Process_ADD_DATA.Method.Name].IsChecked = true;

            //CopyEngine copyEngine_cb.ItemsSource = Configuration.FileCopiers;
            copyEngine_cb.ItemsSource = Configuration.FileCopiers.Values;
            copyEngine_cb.SelectedItem = Configuration.Main.CurrentFileCopier;

            //How Start
            HowStartRbts[Configuration.Main.SetRunningState.Method.Name].IsChecked = true;

            //Add New VisualCopy
            AddNewVisualCopyRbts[Configuration.Main.AddNewVisualCopy.Method.Name].IsChecked = true;

            //Affter Error Action
            AffterErrorRbts[Configuration.Main.AffterErrorAction.Method.Name].IsChecked = true;

            //BufferSize
            bufferSize_textBox.Text = Configuration.Main.BufferSize.ToString();

            //PlaySounds
            ps_affter_ADD_DATA_cb.IsChecked = Configuration.Main.PlaySound_After_ADD_DATA;
            ps_affter_Finish_cb.IsChecked = Configuration.Main.PlaySound_After_Finish;
            ps_When_Cancel_cb.IsChecked = Configuration.Main.PlaySound_After_Cancel;

            //Update Times
            updateTime_tb.Text = Configuration.Main.UpdateTimeInterval.ToString();
            
            loading = false;
        }

        #region Add Data
        private void addToFirsth_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.Process_ADD_DATA = Configuration.Main.AddToFirsth_Process_ADD_DATA;
        }
        private void add_SameDestiny_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.Process_ADD_DATA = Configuration.Main.SameDestiny_Process_ADD_DATA;
        }
        private void add_SameVolumen_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.Process_ADD_DATA = Configuration.Main.SameVolumen_Process_ADD_DATA;
        }
        private void addToLast_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.Process_ADD_DATA = Configuration.Main.AddToLast_Process_ADD_DATA;
        }

        #endregion

        #region SetRunningState
        private void sob_AutomatlyStart_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.SetRunningState = Configuration.Main.StartOperation_SetRunningState;
        }

        private void sob_WaitInQueve_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.SetRunningState = Configuration.Main.Inqueve_SetRunningState;
        }

        #endregion

        #region Add New VisualCopy
        private void ancb_All_In_One_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.AddNewVisualCopy = Configuration.Main.AllInOne_AddNewVC;
        }

        private void ancb_Use_Separate_Windos_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.AddNewVisualCopy = Configuration.Main.SeparateWindows_AddNewVC;
        }

        #endregion

        #region Affter Error Occur Action
        private void closeAffterError_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.AffterErrorAction = Configuration.Main.Close_AffterErrorAction;
        }

        private void keepAffterError_rb_Checked(object sender, RoutedEventArgs e)
        {
            Configuration.Main.AffterErrorAction = Configuration.Main.Keep_AffterErrorAction;
        }

        #endregion


        #region PlaySounds
        private void ps_affter_ADD_DATA_cb_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Main.PlaySound_After_ADD_DATA = ps_affter_ADD_DATA_cb.IsChecked.Value;
        }
        private void ps_affter_Finish_cb_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Main.PlaySound_After_Finish = ps_affter_Finish_cb.IsChecked.Value;
        }
        private void ps_When_Cancel_cb_Click(object sender, RoutedEventArgs e)
        {
            Configuration.Main.PlaySound_After_Cancel = ps_When_Cancel_cb.IsChecked.Value;
        }

        #endregion

        #region Thems and Languaje
        private void theme_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading) return;

            Configuration.Main.Theme = theme_cb.SelectedItem.ToString();
            ThemesManager.Manager.SetTheme(Configuration.Main.Theme);
        }

        private void language_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading) return;

            Configuration.Main.Language = language_cb.SelectedItem.ToString();
            ThemesManager.Manager.SetLanguages(Configuration.Main.Language);
        }

        private void skins_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading) return;

            Configuration.Main.VisualCopySkin = skins_cb.SelectedItem.ToString();
            ThemesManager.Manager.SetVisualCopySkins(Configuration.Main.VisualCopySkin);
        }

        private void brushes_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading) return;

            Configuration.Main.Brush = brushes_cb.SelectedItem.ToString();
            ThemesManager.Manager.SetBrushes(Configuration.Main.Brush);
        }

        private void font_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loading) return;

            Configuration.Main.Font = font_cb.SelectedItem.ToString();
            ThemesManager.Manager.SetFonts(Configuration.Main.Font);
        }


        #endregion

        private void configWnd_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void updateTime_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                tmp = Configuration.Main.UpdateTimeInterval;
                Configuration.Main.UpdateTimeInterval = int.Parse(updateTime_tb.Text);
            }
            catch (Exception)
            {
                Configuration.Main.UpdateTimeInterval = tmp;
            }
        }


        private void bufferSize_textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                tmp = Configuration.Main.BufferSize;
                Configuration.Main.BufferSize = int.Parse(bufferSize_textBox.Text);
            }
            catch (Exception)
            {
                Configuration.Main.BufferSize = tmp;
            }
        }

        private void copyEngine_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Configuration.Main.CurrentFileCopier = (FileCopier)copyEngine_cb.SelectedItem;
        }

    }
}
