using System;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Media = System.Windows.Media;

namespace WPFTest
{
    class UI
    {
        //Encapsulated variables
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
            //Media.* are WPF API methods and have very insimplstic hex colour conversions
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
            //Media.* are WPF API methods and have very insimplstic hex colour conversions
            get
            {
                return (Media.SolidColorBrush)(new Media.BrushConverter().ConvertFrom("#eaf2ff"));
            }
        }

        public static Media.FontFamily xmlFont
        {
            get
            {
                //Media.* are WPF API methods and have very insimplstic font conversions
                return new Media.FontFamily(new Font(fontCollection.Families[0], 10).Name);
            }
        }

        public static void initialiseCustomFonts()
        {
            /*
             * This approach to adding custom fonts is C++-esque however necessary for efficient usage and control in both WinForms, WinForms hosting
             * and WPF conversions
             */

            //Font collection to store font memory
            _fontCollection = new PrivateFontCollection();
            
            //Google Roboto_Light is stored as ResX, get dimensional length
            int fontLength = Properties.Resources.Roboto_Light.Length;

            //Array of bytes to store font properties
            byte[] fontdata = Properties.Resources.Roboto_Light;

            //Volatile memory block to store dimensional length and reduce memory usage (volatile easier for GC to manage)
            IntPtr data = Marshal.AllocCoTaskMem(fontLength);

            //Byte buffer array copied to the volatile block
            Marshal.Copy(fontdata, 0, data, fontLength);

            //Font is added from memory block to encapsulated font collection, called by collection.Familiies
            _fontCollection.AddMemoryFont(data, fontLength);

            //Force GC remove of byte array
            Marshal.FreeCoTaskMem(data);
        }
    }
}
