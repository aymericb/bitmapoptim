using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Drawing;
using System.Windows.Forms;


namespace BitmapOptim
{
    public class AppConfig
    {

        #region Serialization

        static private string app_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Aymeric Barthe");
        //static private string app_file = Path.Combine(app_dir, "FlashBuilder.xml");

        public static T Load<T>(string app_name) where T : new()
        {
            try
            {
                string app_file = Path.Combine(app_dir, app_name + ".xml");
                if (File.Exists(app_file))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    FileStream fs = new FileStream(app_file, FileMode.Open);
                    T config = (T)serializer.Deserialize(fs);
                    fs.Close();
                    return config;
                }
                else
                {
                    return new T();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Application configuration cannot be read.\nReason: " + e.Message,
                    app_name, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return new T();
            }
        }

        public static void Save<T>(string app_name, T config)
        {
            try
            {
                string app_file = Path.Combine(app_dir, app_name + ".xml");
                if (!Directory.Exists(app_dir))
                    Directory.CreateDirectory(app_dir);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                TextWriter writer = new StreamWriter(app_file);
                serializer.Serialize(writer, config);
            }
            catch (Exception e)
            {
                MessageBox.Show("Application configuration cannot be saved.\nReason: " + e.Message,
                    "BitmapOptim", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        #endregion

        // Helper function
        public static void RestoreWindowBounds(Form form, Rectangle bounds)
        {
            // Check if bounds value is intialized
            if (bounds.Width <= 0 || bounds.Height <= 0 || bounds.Top <= 0 || bounds.Bottom <= 0)
                return;

            // Check if bounds are visible
            if (bounds.Top > SystemInformation.VirtualScreen.Height || bounds.Left > SystemInformation.VirtualScreen.Width)
                return;

            // Restore bounds
            form.DesktopBounds = bounds;
        }

        public AppConfig()
        {
        }

        ~AppConfig()
        {
        }

        // Persistent settings configuration
        public Rectangle MainWindowBounds { get; set; }
    }
}
