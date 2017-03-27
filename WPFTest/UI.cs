using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Media = System.Windows.Media;

namespace WPFTest
{
    class UI
    {
        private static PrivateFontCollection _fontCollection;

        public static PrivateFontCollection fontCollection
        {
            get
            {
                return _fontCollection;
            }
        }

        public static Color chartColour
        {
            get
            {
                return ColorTranslator.FromHtml("#eaf2ff");
            }
        }

        public static Color dataPointColour
        {
            get
            {
                return ColorTranslator.FromHtml("#009ee8");
            }
        }

        public static Color dataPointHoverColour
        {
            get
            {
                return ColorTranslator.FromHtml("#0077af");
            }
        }

        public static Media.SolidColorBrush dataPointColourXml
        {
            get
            {
                return (Media.SolidColorBrush)(new Media.BrushConverter().ConvertFrom("#009ee8"));
            }
        }

        public static Color dataPointShadowColour
        {
            get
            {
                return Color.LightSkyBlue;
            }
        }

        public static Media.SolidColorBrush leftBorderColor
        {
            get
            {
                return (Media.SolidColorBrush)(new Media.BrushConverter().ConvertFrom("#eaf2ff"));
            }
        }

        public static Media.FontFamily xmlFont
        {
            get
            {
                Font font = new Font(fontCollection.Families[0], 10);
                //option 1
                Media.FontFamily mfont = new Media.FontFamily(font.Name);
                //option 2 does the same thing
                Media.FontFamilyConverter conv = new Media.FontFamilyConverter();
                Media.FontFamily mfont1 = conv.ConvertFromString(font.Name) as Media.FontFamily;
                //option 3
                Media.FontFamily mfont2 = Media.Fonts.SystemFontFamilies.Where(x => x.Source == font.Name).FirstOrDefault();
                return mfont;
            }
        }

        public static void initialiseCustomFonts()
        {
            //Create your private font collection object.
            _fontCollection = new PrivateFontCollection();

            //Select your font from the resources.
            //My font here is "Digireu.ttf"
            int fontLength = Properties.Resources.Roboto_Light.Length;

            // create a buffer to read in to
            byte[] fontdata = Properties.Resources.Roboto_Light;

            // create an unsafe memory block for the font data
            System.IntPtr data = Marshal.AllocCoTaskMem(fontLength);

            // copy the bytes to the unsafe memory block
            Marshal.Copy(fontdata, 0, data, fontLength);

            // pass the font to the font collection
            _fontCollection.AddMemoryFont(data, fontLength);

            // free up the unsafe memory
            Marshal.FreeCoTaskMem(data);
        }
    }
}
