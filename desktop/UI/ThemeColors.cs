using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace WindroseServerManager.Desktop
{
    internal sealed class ThemeColors
    {
        public Color WindowBackground { get; set; }
        public Color HeaderBackground { get; set; }
        public Color HeaderForeground { get; set; }
        public Color HeaderMutedForeground { get; set; }
        public Color HeaderAccent { get; set; }
        public Color GroupBackground { get; set; }
        public Color GroupForeground { get; set; }
        public Color BorderColor { get; set; }
        public Color TabBackground { get; set; }
        public Color TabSelectedBackground { get; set; }
        public Color TabSelectedForeground { get; set; }
        public Color TabInactiveBackground { get; set; }
        public Color TabInactiveForeground { get; set; }
        public Color BodyForeground { get; set; }
        public Color InputBackground { get; set; }
        public Color InputForeground { get; set; }
        public Color StatusBackground { get; set; }
        public Color StatusForeground { get; set; }
        public Color StatusErrorForeground { get; set; }
        public Color MetaForeground { get; set; }
        public Color ButtonNeutral { get; set; }
        public Color ButtonSuccess { get; set; }
        public Color ButtonWarning { get; set; }
        public Color ButtonDanger { get; set; }
        public Color ButtonEasy { get; set; }
        public Color ButtonText { get; set; }
        public Color ServerStateRunning { get; set; }
        public Color ServerStateStarting { get; set; }
        public Color ServerStateStopped { get; set; }

        public static ThemeColors Create(string themeName)
        {
            switch (themeName)
            {
                case "Dark":
                    return CreateDark();
                case "Windrose":
                case "Pirate":
                    return CreatePirate();
                default:
                    return CreateLight();
            }
        }

        public static ThemeColors CreateLight()
        {
            return new ThemeColors
            {
                WindowBackground = Color.FromArgb(246, 241, 233),
                HeaderBackground = Color.FromArgb(241, 233, 220),
                HeaderForeground = Color.FromArgb(74, 59, 43),
                HeaderMutedForeground = Color.FromArgb(109, 91, 72),
                HeaderAccent = Color.FromArgb(145, 94, 52),
                GroupBackground = Color.FromArgb(252, 249, 244),
                GroupForeground = Color.FromArgb(60, 49, 38),
                BorderColor = Color.FromArgb(205, 187, 163),
                TabBackground = Color.FromArgb(246, 241, 233),
                TabSelectedBackground = Color.FromArgb(252, 249, 244),
                TabSelectedForeground = Color.FromArgb(60, 49, 38),
                TabInactiveBackground = Color.FromArgb(233, 223, 209),
                TabInactiveForeground = Color.FromArgb(103, 85, 67),
                BodyForeground = Color.FromArgb(74, 61, 48),
                InputBackground = Color.FromArgb(255, 252, 248),
                InputForeground = Color.FromArgb(52, 44, 37),
                StatusBackground = Color.FromArgb(234, 220, 197),
                StatusForeground = Color.FromArgb(79, 61, 45),
                StatusErrorForeground = Color.FromArgb(150, 63, 44),
                MetaForeground = Color.FromArgb(97, 80, 65),
                ButtonNeutral = Color.FromArgb(119, 103, 89),
                ButtonSuccess = Color.FromArgb(49, 111, 94),
                ButtonWarning = Color.FromArgb(181, 116, 60),
                ButtonDanger = Color.FromArgb(148, 67, 53),
                ButtonEasy = Color.FromArgb(76, 126, 87),
                ButtonText = Color.FromArgb(251, 248, 243),
                ServerStateRunning = Color.FromArgb(54, 127, 92),
                ServerStateStarting = Color.FromArgb(193, 131, 65),
                ServerStateStopped = Color.FromArgb(148, 67, 53)
            };
        }

        public static ThemeColors CreateDark()
        {
            return new ThemeColors
            {
                WindowBackground = Color.FromArgb(21, 24, 28),
                HeaderBackground = Color.FromArgb(18, 21, 25),
                HeaderForeground = Color.FromArgb(219, 223, 227),
                HeaderMutedForeground = Color.FromArgb(161, 170, 179),
                HeaderAccent = Color.FromArgb(130, 170, 165),
                GroupBackground = Color.FromArgb(30, 34, 39),
                GroupForeground = Color.FromArgb(229, 233, 237),
                BorderColor = Color.FromArgb(70, 77, 85),
                TabBackground = Color.FromArgb(21, 24, 28),
                TabSelectedBackground = Color.FromArgb(30, 34, 39),
                TabSelectedForeground = Color.FromArgb(232, 236, 239),
                TabInactiveBackground = Color.FromArgb(24, 28, 32),
                TabInactiveForeground = Color.FromArgb(157, 166, 174),
                BodyForeground = Color.FromArgb(212, 218, 224),
                InputBackground = Color.FromArgb(35, 40, 46),
                InputForeground = Color.FromArgb(238, 242, 245),
                StatusBackground = Color.FromArgb(43, 47, 53),
                StatusForeground = Color.FromArgb(220, 226, 231),
                StatusErrorForeground = Color.FromArgb(219, 123, 108),
                MetaForeground = Color.FromArgb(176, 184, 191),
                ButtonNeutral = Color.FromArgb(74, 87, 99),
                ButtonSuccess = Color.FromArgb(42, 129, 103),
                ButtonWarning = Color.FromArgb(189, 118, 63),
                ButtonDanger = Color.FromArgb(166, 76, 70),
                ButtonEasy = Color.FromArgb(74, 143, 92),
                ButtonText = Color.FromArgb(247, 249, 250),
                ServerStateRunning = Color.FromArgb(52, 157, 112),
                ServerStateStarting = Color.FromArgb(212, 146, 72),
                ServerStateStopped = Color.FromArgb(186, 82, 76)
            };
        }

        public static ThemeColors CreatePirate()
        {
            return new ThemeColors
            {
                WindowBackground = Color.FromArgb(14, 24, 36),
                HeaderBackground = Color.FromArgb(12, 20, 31),
                HeaderForeground = Color.FromArgb(235, 212, 154),
                HeaderMutedForeground = Color.FromArgb(187, 167, 124),
                HeaderAccent = Color.FromArgb(214, 176, 90),
                GroupBackground = Color.FromArgb(20, 33, 49),
                GroupForeground = Color.FromArgb(238, 223, 187),
                BorderColor = Color.FromArgb(171, 132, 64),
                TabBackground = Color.FromArgb(14, 24, 36),
                TabSelectedBackground = Color.FromArgb(20, 33, 49),
                TabSelectedForeground = Color.FromArgb(240, 226, 193),
                TabInactiveBackground = Color.FromArgb(18, 29, 43),
                TabInactiveForeground = Color.FromArgb(185, 162, 111),
                BodyForeground = Color.FromArgb(234, 223, 196),
                InputBackground = Color.FromArgb(226, 212, 183),
                InputForeground = Color.FromArgb(63, 45, 29),
                StatusBackground = Color.FromArgb(61, 48, 31),
                StatusForeground = Color.FromArgb(244, 226, 179),
                StatusErrorForeground = Color.FromArgb(222, 133, 100),
                MetaForeground = Color.FromArgb(217, 202, 167),
                ButtonNeutral = Color.FromArgb(55, 84, 112),
                ButtonSuccess = Color.FromArgb(28, 109, 99),
                ButtonWarning = Color.FromArgb(170, 112, 58),
                ButtonDanger = Color.FromArgb(135, 57, 45),
                ButtonEasy = Color.FromArgb(83, 132, 84),
                ButtonText = Color.FromArgb(249, 240, 217),
                ServerStateRunning = Color.FromArgb(45, 156, 135),
                ServerStateStarting = Color.FromArgb(210, 156, 79),
                ServerStateStopped = Color.FromArgb(167, 79, 63)
            };
        }
    }

}
