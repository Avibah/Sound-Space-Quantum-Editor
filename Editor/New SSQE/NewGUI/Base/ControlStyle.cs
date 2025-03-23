using New_SSQE.Preferences;
using System.Drawing;

namespace New_SSQE.NewGUI.Base
{
    internal enum ControlStyles
    {
        None,
        Button_Uncolored,
        Checkbox_Colored,
        Checkbox_Uncolored,
        Grid_Colored,
        Slider_Uncolored,
        Textbox_Colored,
        Textbox_Uncolored,
        Track_Colored
    }

    internal class ControlStyle
    {
        private readonly Color? primary;
        private readonly Color? secondary;
        private readonly Color? tertiary;
        private readonly Color? quaternary;
        private readonly Color? quinary;

        /// <summary>
        /// Primary (1) color for GUI elements. Used in:
        /// <list type="bullet">
        ///     <item>GuiButton - Fill</item>
        ///     <item>GuiCheckbox - Text color</item>
        ///     <item>GuiGrid - Fill</item>
        ///     <item>GuiSlider - Progress marker</item>
        ///     <item>GuiTextbox - Fill</item>
        ///     <item>GuiTrack - Text A</item>
        /// </list>
        /// </summary>
        public Color Primary => primary ?? Settings.color1.Value;
        
        /// <summary>
        /// Secondary (2) color for GUI elements. Used in:
        /// <list type="bullet">
        ///     <item>GuiButton - Outline</item>
        ///     <item>GuiCheckbox - Toggle marker</item>
        ///     <item>GuiGrid - Outline/Keybinds/Dynamic divisor lines</item>
        ///     <item>GuiSlider - Line</item>
        ///     <item>GuiSliderTimeline - Bookmark text</item>
        ///     <item>GuiTextbox - Text/Cursor</item>
        ///     <item>GuiTrack - Text B</item>
        /// </list>
        /// </summary>
        public Color Secondary => secondary ?? Settings.color2.Value;
        
        /// <summary>
        /// Tertiary (3) color for GUI elements. Used in:
        /// <list type="bullet">
        ///     <item>GuiButton - Hover mask</item>
        ///     <item>GuiCheckbox - Fill</item>
        ///     <item>GuiGrid - Static divisor lines</item>
        ///     <item>GuiSliderTimeline - Visible track space line</item>
        ///     <item>GuiTextbox - Outline</item>
        ///     <item>GuiTrack - Fill</item>
        /// </list>
        /// </summary>
        public Color Tertiary => tertiary ?? Settings.color3.Value;

        /// <summary>
        /// Quaternary (4) color for GUI elements. Used in:
        /// <list type="bullet">
        ///     <item>GuiCheckbox - Outline</item>
        ///     <item>GuiGrid - Grid numbers/Bezier line/Autoplay cursor</item>
        ///     <item>GuiTrack - Outline</item>
        /// </list>
        /// </summary>
        public Color Quaternary => quaternary ?? Settings.color4.Value;
        
        /// <summary>
        /// Quinary (5) color for GUI elements. Used in:
        /// <list type="bullet">
        ///     <item>None</item>
        /// </list>
        /// </summary>
        public Color Quinary => quinary ?? Settings.color5.Value;

        /// <summary>
        /// Color usage in GUI elements:
        /// <list type="bullet">
        ///     <item>GuiButton:
        ///         <para><paramref name="primary"/> - Fill</para>
        ///         <para><paramref name="secondary"/> - Outline</para>
        ///         <para><paramref name="tertiary"/> - Hover mask</para>
        ///     </item>
        ///     <item>GuiCheckbox:
        ///         <para><paramref name="primary"/> - Text color</para>
        ///         <para><paramref name="secondary"/> - Toggle marker</para>
        ///         <para><paramref name="tertiary"/> - Fill</para>
        ///         <para><paramref name="quaternary"/> - Outline</para>
        ///     </item>
        ///     <item>GuiGrid:
        ///         <para><paramref name="primary"/> - Fill</para>
        ///         <para><paramref name="secondary"/> - Outline/Keybinds</para>
        ///         <para><paramref name="tertiary"/> - Static divisor lines</para>
        ///         <para><paramref name="quaternary"/> - Dynamic divisor lines</para>
        ///         <para><paramref name="quinary"/> - Grid numbers/Bezier line/Autoplay cursor</para>
        ///     </item>
        ///     <item>GuiSlider:
        ///         <para><paramref name="primary"/> - Progress marker</para>
        ///         <para><paramref name="secondary"/> - Line</para>
        ///         <para>GuiSliderTimeline: <paramref name="secondary"/> - Bookmark text</para>
        ///         <para>GuiSliderTimeline: <paramref name="tertiary"/> - Visible track space line</para>
        ///     </item>
        ///     <item>GuiTextbox:
        ///         <para><paramref name="primary"/> - Fill</para>
        ///         <para><paramref name="secondary"/> - Text/Cursor</para>
        ///         <para><paramref name="tertiary"/> - Outline</para>
        ///     </item>
        ///     <item>GuiTrack:
        ///         <para><paramref name="primary"/> - Text A</para>
        ///         <para><paramref name="secondary"/> - Text B</para>
        ///         <para><paramref name="tertiary"/> - Fill</para>
        ///         <para><paramref name="quaternary"/> - Outline</para>
        ///     </item>
        /// </list>
        /// </summary>
        /// <param name="primary">Primary color, or Settings.color1 if null</param>
        /// <param name="secondary">Secondary color, or Settings.color2 if null</param>
        /// <param name="tertiary">Tertiary color, or Settings.color3 if null</param>
        /// <param name="quaternary">Quaternary color, or Settings.color4 if null</param>
        /// <param name="quinary">Quinary color, or Settings.color5 if null</param>
        public ControlStyle(Color? primary = null, Color? secondary = null, Color? tertiary = null, Color? quaternary = null, Color? quinary = null)
        {
            this.primary = primary;
            this.secondary = secondary;
            this.tertiary = tertiary;
            this.quaternary = quaternary;
            this.quinary = quinary;
        }

        /// <summary>
        /// Allows usage of a predefined style to make coloring elements easier
        /// </summary>
        /// <param name="style">The predefined style to pull colors from</param>
        public ControlStyle(ControlStyles style)
        {
            switch (style)
            {
                case ControlStyles.Button_Uncolored:
                    primary = Color.FromArgb(26, 26, 26);
                    secondary = Color.FromArgb(52, 52, 52);
                    tertiary = Color.FromArgb(255, 255, 255);
                    break;
                case ControlStyles.Checkbox_Colored:
                    tertiary = Color.FromArgb(13, 13, 13);
                    quaternary = Color.FromArgb(52, 52, 52);
                    break;
                case ControlStyles.Checkbox_Uncolored:
                    primary = Color.FromArgb(255, 255, 255);
                    secondary = Color.FromArgb(75, 75, 75);
                    tertiary = Color.FromArgb(13, 13, 13);
                    quaternary = Color.FromArgb(52, 52, 52);
                    break;
                case ControlStyles.Grid_Colored:
                    primary = Color.FromArgb(39, 39, 39);
                    secondary = Color.FromArgb(52, 52, 52);
                    tertiary = Color.FromArgb(13, 13, 13);
                    quaternary = Color.FromArgb(255, 255, 255);
                    break;
                case ControlStyles.Slider_Uncolored:
                    primary = Color.FromArgb(255, 255, 255);
                    secondary = Color.FromArgb(75, 75, 75);
                    break;
                case ControlStyles.Textbox_Colored:
                    primary = Color.FromArgb(26, 26, 26);
                    tertiary = Color.FromArgb(128, 128, 128);
                    break;
                case ControlStyles.Textbox_Uncolored:
                    primary = Color.FromArgb(26, 26, 26);
                    secondary = Color.FromArgb(255, 255, 255);
                    tertiary = Color.FromArgb(128, 128, 128);
                    break;
                case ControlStyles.Track_Colored:
                    tertiary = Color.FromArgb(39, 39, 39);
                    quaternary = Color.FromArgb(52, 52, 52);
                    break;
            }
        }
    }
}
