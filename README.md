# Intro
Based on the awesome work by [Javidx9](https://github.com/OneLoneCoder) and contributions in C# by [RobThree](https://gist.github.com/RobThree).

See example-projects for use. Detailed descriptions are coming soon...


# How to use

## Setup

The following code represents the skeleton which is valid for each project.

```
using System;
using ConsoleGameEngine;

namespace YourNamespace
{
  class ClassName : GameConsole
  {
      public CGE_Button()
        : base(200, 120, "Fonts", fontwidth: 4, fontheight: 4)
      { }
      public override bool OnUserCreate()
      {
          //Load sprites, setup variables and whatever
          return true;
      }
      public override bool OnUserUpdate(TimeSpan elapsedTime)
      {
          //game loop, draw and evaluate inputs
          return true;
      }
  }
  
   internal class Program
   {
     static void Main(string[] args)
     {
         Console.OutputEncoding = System.Text.Encoding.GetEncoding(437);
    
         using (var f = new ClassName())
             f.Start();
     }
  }
}
```

## Use sprites

Define a Sprite-Object, initialise it in OnUserCreate and draw in OnUserUpdate.

```
Sprite yourSprite;

public override bool OnUserCreate()
  {
    sprite = new Sprite("Path to file.txt");
    return true;
  }

public override bool OnUserUpdate(TimeSpan fElapsedTime)
{
  DrawSprite(0,0,yourSprite);
  return true;
}
```
You can define Alpha-Values for sprites to not draw their background. By default all '\0'-characters are skiped.

## Use animations

the animation-class contains different approaches.

Initialising an animation-object leaves you with these options:
- init with List<string> --> Paths for each frame stored in .txt
- init with List<Sprite> --> loaded sprites, containing each frame
- init with Sprite --> large sprite containing all animation-frames
- init with string --> Path to large txt-file, containg all animation-frames

If initialised with Sprite or string, it is required to add width and height of a single frame.

Also every call needs a TimeSpan for the frames delay.

Update the sprite and draw its outputed frame.

```
  animation yourAnimation;

  public override bool OnUserCreate()
  {
      yourAnimation = new animation("yourAnimationSpriteSheet.txt", new TimeSpan(0, 0, 0, 0, 100), 64, 64);
  
      return true;
  }

  public override bool OnUserUpdate(TimeSpan elapsedTime)
  {
     yourAnimation.Update();

     DrawSprite(0, 0, yourAnimation.outputSprite);

     return true;
  }
```

## Keyboard-Input
The thread calling OnUserUpdate updates all Keyboard-Keys. Checks each Key-State as follows.

```
public override bool OnUserUpdate(TimeSpan elapsedTime)
{
    if (GetKeyState(ConsoleKey.W).Held)
    {
      //Move player up

    }
    if (GetKeyState(ConsoleKey.S).Held)
    {
        //Move player down

    }
    //do for all keys you like

    return true;
}
```
A key can have this states:
- Pressed
- Held
- Released
- None of the above

## Mouse-Input
To use the mouse create a new Event on ConsoleListener.MouseEvent. Evaluate what you need within the events function.

```
IntPtr inHandle;
delegate void MyDelegate();

int cursorX = 0, cursorY = 0;
bool leftMousebuttonClicked = false, mouseWheelClicked = false, rightMousebuttonClicked = false;

public override bool OnUserCreate()
{
    inHandle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
    uint mode = 0;
    NativeMethods.GetConsoleMode(inHandle, ref mode);
    mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
    mode |= NativeMethods.ENABLE_WINDOW_INPUT; //enable (if you want)
    mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable
    NativeMethods.SetConsoleMode(inHandle, mode);

    ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;

    ConsoleListener.Start();

    return true;
}

private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
{
    cursorX = r.dwMousePosition.X;
    cursorY = r.dwMousePosition.Y;

    leftMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED;
    mouseWheelClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_2ND_BUTTON_PRESSED;
    rightMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.RIGHTMOST_BUTTON_PRESSED;
}

```
Evaluate inside OnUserUpdate.

## Textwriter

Due to the high resolution and small characters. Normal consoletext is unreadable. To create a readable Text-Sprite do the following

```
public override bool OnUserCreate()
{
    TextWriter.LoadFont("fontsheet.txt", 7 , 9);

    return true;
}
public override bool OnUserUpdate(TimeSpan elapsedTime)
{
   
    DrawSprite(0, 0, TextWriter.GenerateTextSprite("Size: 1", TextWriter.Textalignment.Left, 1, backgroundColor: (short)COLOR.FG_YELLOW, foregroundColor: (short)COLOR.FG_RED));
    return true;
}
```

If a different font-sheet is used, adjust character-width and -height in function LoadFont.

If Textsprites are static, define them in OnUserCreate and just call DrawSprite in OnUserUpdate.

Text can be left, right and center-alligned.

Textsize can be changed with sizes 1 to 4.

## Buttons
To use buttons do the following and use the setup-code for Mouseinput.

```
Button button;

 public override bool OnUserCreate()
 {
     //mouse-setup here..

     TextWriter.LoadFont("fontsheet.txt", 7, 9);

     button = new Button(40, 40, TextWriter.GenerateTextSprite("Click me!", TextWriter.Textalignment.Left, 1));
     button.OnButtonClicked(ButtonClicked);

     return true;
 }
public override bool OnUserUpdate(TimeSpan elapsedTime)
{
    Clear();

    DrawSprite(button.x, button.y, button.sprite);

    return true;
}
private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
{
    button.Update(r);
}

public bool ButtonClicked()
{
    //do something
    return true;
}

```
It is possible to create invibisle buttons, by not drawing its sprite.


# PNGToSpriteEditor

As the name says. Drag and drop a PNG-File into the console and it converts it to a 6bit representation. Or create a new file.


- WASD - navigate cursor
- Space - set pixel in active color and brush
- 1/2 - select brush
- 3/4 - select background-color
- 7/8 - select foreground-color
- Arrowkeys - navigate through bigger sprites


Colors are too dark and it has trouble with yellow and brown-tones.

![Example](/ReadMeRessources/Grafikkonverter.PNG)

With 'P' a sprite-file (.txt) is created which can be used with the library.

In the future, this program will be adjusted to the new features of the library.








