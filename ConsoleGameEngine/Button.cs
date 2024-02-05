using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleGameEngine.NativeMethods;

namespace ConsoleGameEngine
{
    public class Button
    {
        public int x, y, width, height;
        public Sprite sprite;
        Func<bool> method;

        public Button(int x, int y, int width, int height, Sprite sprite)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.sprite = sprite;
        }
        public Button(int x, int y, Sprite sprite)
        {
            this.x = x;
            this.y = y;
            this.sprite = sprite;
            width = sprite.Width;
            height = sprite.Height;
        }

        public void OnButtonClicked(Func<bool> method)
        {
            this.method = method;
        }

        public void Update(MOUSE_EVENT_RECORD r)
        {
            int mouseX = r.dwMousePosition.X, mouseY = r.dwMousePosition.Y;
            uint mouseState = r.dwButtonState;

            if(mouseState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED)
            {
                if(mouseX <= x + width && mouseX >= x && mouseY <= y + height && mouseY > y) 
                {
                    method();
                }
            }   
        }
    }
}
