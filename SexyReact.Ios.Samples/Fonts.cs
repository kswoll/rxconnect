using System;
using UIKit;
using SexyReact.Ios.Samples.Utils;

namespace SexyReact.Ios.Samples
{
    public static class Fonts
    {
        private static Lazy<UIFont> defaultFont = new Lazy<UIFont>(() => UIFont.SystemFontOfSize(24));
        private static Lazy<UIFont> defaultFontBold = new Lazy<UIFont>(() => defaultFont.Value.DeriveBold());

        public static UIFont DefaultFont 
        {
            get { return defaultFont.Value; }    
        }

        public static UIFont DefaultFontBold
        {
            get { return defaultFontBold.Value; }
        }
    }
}

