/*
 * BitmapOptim - The Image Optimizer for Windows
 * 
 * Copyright (C) 2012 Aymeric Barthe
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

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
        
        public double? Savings { get; set; }

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

        static public string GetReadableSize(long file_size)
        {
            if (file_size < 1024)   // bytes
            {
                return file_size + " bytes";
            }
            else if (file_size >= 1024 && file_size < 1024 * 1024)  // KB
            {
                int t = (int) (((double)file_size) / 1024.0 * 10.0);
                return t / 10.0 + " KB";
            }
            else if (file_size >= 1024 * 1024 && file_size < 1024 * 1024 * 1024) // MB
            {
                int t = (int)(((double)file_size) / (1024.0 * 1024.0) * 10.0);
                return t / 10.0 + " MB";
            }
            else // GB
            {
                int t = (int)(((double)file_size) / (1024.0 * 1024.0 * 1024.0) * 10.0);
                return t / 10.0 + " GB";
            }
        }
    }
}
