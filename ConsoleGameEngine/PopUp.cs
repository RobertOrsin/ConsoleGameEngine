using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ConsoleGameEngine.NativeMethods;

namespace ConsoleGameEngine
{
    
    public class PopUp
    {
        int x, y;
        public bool visible = false;
        Button OKButton, CancleButton;
        String text;
        GameConsole.COLOR backgroundColor, foregroundColor;
        PopUpState state;
        
        public PopUp(int x, int y, string text, out Sprite outputSprite, GameConsole.COLOR backgroundColor = GameConsole.COLOR.FG_BLUE, GameConsole.COLOR foregroundColor = GameConsole.COLOR.FG_WHITE)
        {
            this.x = x;
            this.y = y;
            this.text = text;
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
            state = PopUpState.none;

            Sprite textSprite = TextWriter.GenerateTextSprite(this.text, TextWriter.Textalignment.Center, 1, (short)this.backgroundColor, (short)this.foregroundColor);
            OKButton = new Button(0,0,TextWriter.GenerateTextSprite("  OK  ", TextWriter.Textalignment.Left, 1), feedbackSprite: TextWriter.GenerateTextSprite("  OK  ", TextWriter.Textalignment.Left, 1, backgroundColor: 0, foregroundColor: 15));
            CancleButton = new Button(0, 0, TextWriter.GenerateTextSprite("CANCLE", TextWriter.Textalignment.Left, 1), feedbackSprite: TextWriter.GenerateTextSprite("CANCLE", TextWriter.Textalignment.Left, 1, backgroundColor: 0, foregroundColor: 15));

            OKButton.OnButtonClicked(OKButtonClicked);
            CancleButton.OnButtonClicked(CancleButtonClicked);

            if(textSprite.Width > OKButton.width + CancleButton.width)
                outputSprite = new Sprite(textSprite.Width + 4, textSprite.Height + 6 + OKButton.height, GameConsole.COLOR.BG_BLUE);
            else
                outputSprite = new Sprite(OKButton.width + CancleButton.width + 4, OKButton.width + CancleButton.width + 4, GameConsole.COLOR.BG_BLUE);

            outputSprite.AddSpriteToSprite(2, 2, textSprite);
            outputSprite.AddSpriteToSprite(2, 2 +textSprite.Height + 2, OKButton.outputSprite);
            outputSprite.AddSpriteToSprite(outputSprite.Width - 2 - CancleButton.width, 2 + textSprite.Height + 2, CancleButton.outputSprite);

            OKButton.x = x + 2;
            OKButton.y = y + 4 + textSprite.Height;

            CancleButton.x = x + outputSprite.Width - CancleButton.width;
            CancleButton.y = y + 4 + textSprite.Height;
        }

        public PopUpState Update(MOUSE_EVENT_RECORD r)
        {
            if (visible)
            {
                state = PopUpState.none;
                OKButton.Update(r);
                CancleButton.Update(r);
            }
            else
            { state = PopUpState.none; }


            return state;
        }

        private bool OKButtonClicked()
        {
            state = PopUpState.okClicked;
            return true;
        }
        private bool CancleButtonClicked()
        {
            state = PopUpState.cancleClicked;
            return true;
        }
    }

    public enum PopUpState
    {
        none,
        okClicked,
        cancleClicked,
    }
}
