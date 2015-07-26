using System;
using UIKit;

namespace SexyReact.Ios.Samples.Utils
{
    public static class UIFontExtensions
    {
        public static UIFont Derive(this UIFont font, float size) 
        {
            return UIFont.FromName(font.Name, size);
        }

        public static UIFont DeriveBold(this UIFont font, float? size = null) 
        {
            var name = font.Name;
            var dashIndex = name.LastIndexOf('-');
            if (dashIndex != -1)
                name = name.Substring(0, dashIndex);

            return UIFont.FromName(name + "-Bold", size ?? font.PointSize);
        }
    }
}

