using System.IO;
using System.Reflection;
using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles
// ReSharper disable InconsistentNaming

namespace Lench.Scripter.Resources
{
    internal static class Images
    {
        private static Texture2D ByteArrayToTexture2D(byte[] image, int width, int height)
        {
            var tex = new Texture2D(width, height);
            tex.LoadImage(image);
            return tex;
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var b = new byte[32768];
            int r;
            while ((r = input.Read(b, 0, b.Length)) > 0)
                output.Write(b, 0, r);
        }

        private static Texture2D GetImage(string name, int width, int height)
        {
            var imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Lench.Scripter.Resources.{name}");
            using (var memoryStream = new MemoryStream())
            {
                CopyStream(imageStream, memoryStream);
                return ByteArrayToTexture2D(memoryStream.ToArray(), width, height);
            }
        }

        public static Texture2D IconPython => GetImage("ic_python.png", 64, 64);
        public static Texture2D IconClear => GetImage("ic_clear.png", 32, 32);

        public static Texture2D ButtonListNormal => GetImage("button_list_normal.png", 64, 64);
        public static Texture2D ButtonListFocus => GetImage("button_list_focus.png", 64, 64);
        public static Texture2D ButtonListHover => GetImage("button_list_hover.png", 64, 64);
        public static Texture2D ButtonListActive => GetImage("button_list_active.png", 64, 64);

        public static Texture2D ButtonKeyNormal => GetImage("button_key_normal.png", 64, 64);
        public static Texture2D ButtonKeyFocus => GetImage("button_key_focus.png", 64, 64);
        public static Texture2D ButtonKeyHover => GetImage("button_key_hover.png", 64, 64);
        public static Texture2D ButtonKeyActive => GetImage("button_key_active.png", 64, 64);

        public static Texture2D ButtonScriptNormal => GetImage("button_script_normal.png", 64, 64);
        public static Texture2D ButtonScriptFocus => GetImage("button_script_focus.png", 64, 64);
        public static Texture2D ButtonScriptHover => GetImage("button_script_hover.png", 64, 64);
        public static Texture2D ButtonScriptActive => GetImage("button_script_active.png", 64, 64);

        public static Texture2D ButtonSettingsNormal => GetImage("button_settings_normal.png", 64, 64);
        public static Texture2D ButtonSettingsFocus => GetImage("button_settings_focus.png", 64, 64);
        public static Texture2D ButtonSettingsHover => GetImage("button_settings_hover.png", 64, 64);
        public static Texture2D ButtonSettingsActive => GetImage("button_settings_active.png", 64, 64);
    }
}
