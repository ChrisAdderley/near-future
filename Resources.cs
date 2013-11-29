﻿/// Resources
/// -------------------------
/// Doesn't do much but store GUI textures. More will come!
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NearFuture
{
    public static class Resources
    {
        // foreground for progress bars
        public static Texture2D gui_progressbar = new Texture2D(16, 16);   


        // Styles


        // Load that texture!
        public static void LoadResources()
        {
            gui_progressbar.LoadImage(KSP.IO.File.ReadAllBytes<NearFuture>("ui_progress.png"));
        }
    }
}
