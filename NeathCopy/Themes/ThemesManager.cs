using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NeathCopy.Module2_Configuration;

namespace NeathCopy.Themes
{
    public class ThemesManager
    {
        public static ThemesManager Manager { get; private set; }

        //public delegate void BrushesChangedEventHandle(string brushes);
        ///// <summary>
        ///// Occurs when Brushes are changeds.
        ///// </summary>
        //public static event BrushesChangedEventHandle BrushesChanged;
        //private static void RaiseBrushesChanged(string brushes)
        //{
        //    if (BrushesChanged != null)
        //        BrushesChanged(brushes);
        //}

        public delegate void VisualCopySkingChangedEventHandle(string skin);
        /// <summary>
        /// Occurs when Brushes are changeds.
        /// </summary>
        public  event VisualCopySkingChangedEventHandle VisualCopySkingChanged;
        private  void RaiseVisualCopySkingChanged(string skin)
        {
            if (VisualCopySkingChanged != null)
                VisualCopySkingChanged(skin);
        }

        static ThemesManager()
        {
            Manager = new ThemesManager();
        }
        public  void SetTheme(string themeName)
        {
            var uri = new Uri(string.Format(@"Themes\{0}", themeName), UriKind.Relative);
            var theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries[0] =  theme;
        }
        public  void SetVisualCopySkins(string sking)
        {
            var uri = new Uri(string.Format(@"VisualCopySkins\{0}", sking), UriKind.Relative);
            var theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries[1] = theme;
            RaiseVisualCopySkingChanged(sking);

        }
        public  void SetBrushes(string brushes)
        {
            var uri = new Uri(string.Format(@"Brushes\{0}", brushes), UriKind.Relative);
            var theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries[2] = theme;

            //RaiseBrushesChanged(brushes);
        }
        public  void SetFonts(string font)
        {
            var uri = new Uri(string.Format(@"Fonts\{0}", font), UriKind.Relative);
            var theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries[3] = theme;
        }
        public  void SetLanguages(string languages)
        {
            var uri = new Uri(string.Format(@"Languages\{0}", languages), UriKind.Relative);
            var theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries[4] = theme;
        }

        public  ResourceDictionary GetVisualCopySkins()
        {
            return Application.Current.Resources.MergedDictionaries[1];
        }

        public  void InitResources()
        {
            var uri = new Uri(string.Format(@"Themes\{0}", Configuration.Thems[0]), UriKind.Relative);
            var theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries.Add(theme);

            uri = new Uri(string.Format(@"VisualCopySkins\{0}", Configuration.VisualCopySkins[0]), UriKind.Relative);
            theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries.Add(theme);

            uri = new Uri(string.Format(@"Brushes\{0}",Configuration.Brushes[0]), UriKind.Relative);
            theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries.Add(theme);

            uri = new Uri(string.Format(@"Fonts\{0}", Configuration.Fonts[0]), UriKind.Relative);
            theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries.Add(theme);

            uri = new Uri(string.Format(@"Languages\{0}", Configuration.Languages[0]), UriKind.Relative);
            theme = new ResourceDictionary() { Source = uri };
            Application.Current.Resources.MergedDictionaries.Add(theme);
        }
        public  void SetThemes(Configuration config)
        {
            SetTheme(config.Theme);
            SetVisualCopySkins(config.VisualCopySkin);
            SetBrushes(config.Brush);
            SetFonts(config.Font);
            SetLanguages(config.Language);
        }
    }
}
