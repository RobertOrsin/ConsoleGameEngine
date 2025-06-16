namespace ConsoleGameEngine
{
    public abstract partial class GameConsole
    {
        public enum PIXELS
        {
            PIXEL_NONE = '\0',
            PIXEL_SOLID = 0x2588,
            PIXEL_THREEQUARTERS = 0x2593,
            PIXEL_HALF = 0x2592,
            PIXEL_QUARTER = 0x2591,
            LINE_STRAIGHT_HORIZONTAL = '─',
            LINE_STRAIGHT_VERTICAL = '│',
            LINE_CORNER_TOP_LEFT = '┌',
            LINE_CORNER_TOP_RIGHT = '┐',
            LINE_CORNER_BOTTOM_LEFT = '└',
            LINE_CORNER_BOTTOM_RIGHT = '┘',
            LINE_TSECTION_TOP = '┬',
            LINE_TSECTION_BOTTOM = '┴',
            LINE_TSECTION_LEFT = '├',
            LINE_TSECTION_RIGHT = '┤',
            LINE_CROSSSECTION = '┼',
        }
        #endregion
    }
}
