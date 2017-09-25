using Autodesk.Revit.UI;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PRSPKT_Apps.Riboon
{
    internal class RibbonUtilities
    {
        internal static object CreatePushButton(string buttonName, string buttonText, string className, string image32Name, string tooltipText = "")
        {
            try
            {
                if (string.IsNullOrEmpty(buttonName)
                    || string.IsNullOrEmpty(buttonText)
                    || string.IsNullOrEmpty(className))
                {
                    throw new Exception();
                }

                var buttonData = new PushButtonData(
                    buttonName,
                    buttonText,
                    Assembly.GetExecutingAssembly().Location,
                    className
                    );

                var img32 = Properties.Resources.ResourceManager.GetObject(image32Name) as Bitmap;
                if (img32 != null)
                {
                    buttonData.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(img32.GetHbitmap(),
                        IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                buttonData.ToolTip = tooltipText;
                return buttonData;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }

        }
    }
}