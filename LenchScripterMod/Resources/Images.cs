using System.IO;
using System.Reflection;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace Lench.Scripter.Resources
{
    internal static class Images
    {
        public static Texture2D ByteArrayToTexture2D(byte[] image, int width, int height)
        {
            var tex = new Texture2D(width, height);
            tex.LoadImage(image);
            return tex;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            var b = new byte[32768];
            int r;
            while ((r = input.Read(b, 0, b.Length)) > 0)
                output.Write(b, 0, r);
        }

        internal static Texture2D ic_clear_32
        {
            get
            {
                var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lench.Scripter.Resources.ic_clear_32.png");
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(imageStream, memoryStream);
                    return ByteArrayToTexture2D(memoryStream.ToArray(), 32, 32);
                }
            }
        }

        internal static Texture2D ic_code_32
        {
            get
            {
                var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lench.Scripter.Resources.ic_code_32.png");
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(imageStream, memoryStream);
                    return ByteArrayToTexture2D(memoryStream.ToArray(), 32, 32);
                }
            }
        }

        internal static Texture2D ic_edit_32
        {
            get
            {
                var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lench.Scripter.Resources.ic_edit_32.png");
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(imageStream, memoryStream);
                    return ByteArrayToTexture2D(memoryStream.ToArray(), 32, 32);
                }
            }
        }

        internal static Texture2D ic_eye_32
        {
            get
            {
                var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lench.Scripter.Resources.ic_eye_32.png");
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(imageStream, memoryStream);
                    return ByteArrayToTexture2D(memoryStream.ToArray(), 32, 32);
                }
            }
        }

        internal static Texture2D ic_key_32
        {
            get
            {
                var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lench.Scripter.Resources.ic_key_32.png");
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(imageStream, memoryStream);
                    return ByteArrayToTexture2D(memoryStream.ToArray(), 32, 32);
                }
            }
        }

        internal static Texture2D ic_python_32
        {
            get
            {
                var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lench.Scripter.Resources.ic_python_32.png");
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(imageStream, memoryStream);
                    return ByteArrayToTexture2D(memoryStream.ToArray(), 32, 32);
                }
            }
        }

        internal static Texture2D ic_settings_32
        {
            get
            {
                var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lench.Scripter.Resources.ic_settings_32.png");
                using (var memoryStream = new MemoryStream())
                {
                    CopyStream(imageStream, memoryStream);
                    return ByteArrayToTexture2D(memoryStream.ToArray(), 32, 32);
                }
            }
        }
    }
}
