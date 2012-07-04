using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BitmapOptim
{
    public class ImageFile
    {
        public enum ImageType {
            PNG,
            JPEG
        }

        public string Path { get { return m_path; } }
        public ImageType Type { get { return m_type; } }
        public long Size { get { return GetFileSize(); } }
        
        public double Savings { get; set; }

        private ImageType m_type;
        private string m_path;

        public ImageFile(string path)
        {
            this.m_path = path;
            if (m_path.ToLower().EndsWith(".png"))
            {
                this.m_type = ImageType.PNG;
            }
            else if (m_path.ToLower().EndsWith(".jpeg") || m_path.ToLower().EndsWith(".jpg"))
            {
                this.m_type = ImageType.JPEG;
            }
            else
            {
                throw new Exception("Unknown image type");
            }            
        }

        private long GetFileSize()
        {
            return (new FileInfo(this.Path)).Length;
        }
    }
}
