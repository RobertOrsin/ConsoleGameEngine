using System;
using System.Collections.Generic;
using System.IO;
using ConsoleGameEngine;
using static ConsoleGameEngine.Other.NativeMethods;
using BigGustave;
using ConsoleGameEngine.Other;

namespace SpriteEditor
{
    class SpriteEditor : GameConsole
    {
        private IntPtr inHandle;
        private MOUSE_EVENT_RECORD oldMouseState;
        delegate void MyDelegate();

        private int _cursorX, _cursorY;
        private bool _leftMousebuttonClicked, _leftMousebuttonHeld, _leftMouseButtonReleased, _mouseWheelClicked, _rightMousebuttonClicked;
        private short _foregroundColor, _backgroundColor;
        private short _foregroundColorReplaceBrush, _backgroundColorReplaceBrush;
        private char _brush = '▓', _replaceBrush = '▓';
        private List<char> _otherBrushes = ['─', '│', '┌', '┐', '└', '┘', '┬', '┴', '├', '┤', '┼'];

        private Sprite _sprite = new Sprite(32, 32, '█', COLOR.BG_BLACK);
        private Button _btnClear, _btnSave, _btnLoad, _btnColorPicker, _btnMark, _btnCopy, _btnAbortMarkAndCopy, _btnConfirmMarkAndCopy, _btnReplaceColor, _btnFillBucket;
        private Button _btnAddGrid;
        private TextBox _tb_Width, _tb_Height, _tb_SaveName;
        private TextBox _tb_GridWidth, _tb_GridHeight;
        private ListBox _lb_SavedFiles;
        private AnimationPreview _animationPreview;

        private bool _colorPickerActive, _markingActive, _fillBucketActive;

        private List<string> _saveFiles = [];

        private readonly int _spriteAreaW = 95, _spriteAreaH = 47;
        private int _spriteCursorX = 0, _spriteCursorY = 0;
        private readonly int _spriteDrawX = 5, _spriteDrawY = 10;

        private bool _marking_visible, _markingDraging;
        private int _markingStartX, _markingStartY, _markingEndX, _markingEndY;
        private int _markingSpriteX, _markingSpriteY;
        private Sprite _markingSprite;

        private TimeSpan _keyInputDelay = new();
        private TimeSpan _keyInputTime = new TimeSpan(0, 0, 0, 0, 120);

        public SpriteEditor()
          : base(140, 70, "Fonts", fontwidth: 12, fontheight: 12)
        { }
        public override bool OnUserCreate()
        {
            ConsoleGameEngine.Other.TextWriter.InitTextWriter();

            _btnClear = new Button(105, 8, "clear / new", method: BtnClearClicked);
            _tb_Width = new TextBox(119, 7, 6, "Width:");
            _tb_Height = new TextBox(129, 7, 6, "Height:", simple: true);

            _btnAddGrid = new Button(106, 12, "add grid", method: BtnAddGridClicked);
            _tb_GridWidth = new TextBox(119, 11, 6 , "Width:");
            _tb_GridHeight = new TextBox(129, 11, 6, "Height:");

            _btnSave = new Button(106, 19, " save ", method: BtnSaveClicked);
            
            _tb_SaveName = new TextBox(106, 23, 30, "Save Name:");

            _animationPreview = new AnimationPreview(106, 48);

            _btnMark = new Button(3, 61, " Mark ", method:BtnMarkClicked);
            _btnCopy = new Button(12, 61, " Copy ", method: BtnCopyClicked);
            _btnAbortMarkAndCopy = new Button(21, 61, "Abort", method:BtnAbortClicked);
            _btnConfirmMarkAndCopy = new Button(30, 61, " Set ", method: BtnConfirmClicked);
            _btnColorPicker = new Button(39, 61, "pick color", method: BtnColorPickerClicked);
            _btnFillBucket = new Button(57, 61, "bucket", method: BtnFillBucketClicked);

            _btnReplaceColor = new Button(120, 2, "replace", method: BtnReplaceClicked);

            //load savefiles from savefile-folder
            foreach (string file in Directory.EnumerateFiles(@"Savefiles\", "*.txt"))
                _saveFiles.Add(Path.GetFileName(file));
            foreach (string file in Directory.EnumerateFiles(@"Savefiles\", "*.png"))
                _saveFiles.Add(Path.GetFileName(file));

            _lb_SavedFiles = new ListBox(106, 28, 32, 15, _saveFiles, simple: true);

            _btnLoad = new Button(129, 44, " load ", method: BtnLoadClicked);

            inHandle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
            uint mode = 0;
            NativeMethods.GetConsoleMode(inHandle, ref mode);
            mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
            mode |= NativeMethods.ENABLE_WINDOW_INPUT; //enable (if you want)
            mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable
            NativeMethods.SetConsoleMode(inHandle, mode);

            ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;

            ConsoleListener.Start();

            //Load sprites, setup variables and whatever
            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            _keyInputDelay += elapsedTime;

            if (ApplicationIsActivated())
            {
                _tb_Width.UpdateInput(KeyStates, elapsedTime);
                _tb_Height.UpdateInput(KeyStates, elapsedTime);
                _tb_SaveName.UpdateInput(KeyStates, elapsedTime);
                _tb_GridHeight.UpdateInput(KeyStates, elapsedTime);
                _tb_GridWidth.UpdateInput(KeyStates, elapsedTime);
                _animationPreview.UpdateKeyInput(KeyStates, elapsedTime, _sprite);

                //evaluate keyinputs of no textbox is selected
                if (!_tb_Height.selected && !_tb_Width.selected && !_tb_SaveName.selected && !_tb_GridWidth.selected && !_tb_GridHeight.selected)
                {
                    if (GetKeyState(ConsoleKey.W).Held && _keyInputDelay >= _keyInputTime)
                    {
                        _spriteCursorY -= 5;
                        if (_spriteCursorY < 0)
                            _spriteCursorY = 0;

                        _keyInputDelay = new TimeSpan();
                    }
                    if (GetKeyState(ConsoleKey.A).Held && _keyInputDelay >= _keyInputTime)
                    {
                        _spriteCursorX -= 5;
                        if (_spriteCursorX < 0)
                            _spriteCursorX = 0;

                        _keyInputDelay = new TimeSpan();
                    }
                    if (GetKeyState(ConsoleKey.S).Held && _keyInputDelay >= _keyInputTime)
                    {
                        _spriteCursorY += 5;
                        if (_spriteCursorY >= _sprite.Height - _spriteAreaH)
                            _spriteCursorY = _sprite.Height - _spriteAreaH - 1;

                        _keyInputDelay = new TimeSpan();
                    }
                    if (GetKeyState(ConsoleKey.D).Held && _keyInputDelay >= _keyInputTime)
                    {
                        _spriteCursorX += 5;
                        if (_spriteCursorX >= _sprite.Width - _spriteAreaW)
                            _spriteCursorX = _sprite.Width - _spriteAreaW - 1;

                        _keyInputDelay = new TimeSpan();
                    }
                }
            }
            EvaluateGUIClick();

            Clear();

            //GUI
            DrawColorPalette(1, 1, "Foregroundcolor");
            DrawColorPalette(40, 1, "Backgroundcolor");
            DrawBrushes(80, 1, "Brushes");
            DrawActiveBrush(90, 1, "Active Brush", _foregroundColor, _backgroundColor, _brush);
            DrawActiveBrush(105, 1, "Replace with", _foregroundColorReplaceBrush, _backgroundColorReplaceBrush, _replaceBrush);

            DrawSprite(_btnReplaceColor.x, _btnReplaceColor.y, _btnReplaceColor.outputSprite);
            DrawSprite(_btnClear.x, _btnClear.y, _btnClear.outputSprite);
            DrawSprite(_btnSave.x, _btnSave.y, _btnSave.outputSprite);
            DrawSprite(_btnLoad.x, _btnLoad.y, _btnLoad.outputSprite);
            DrawSprite(_btnAddGrid.x, _btnAddGrid.y, _btnAddGrid.outputSprite);

            if (_colorPickerActive)
                DrawASCIIRectangle(_btnColorPicker.x - 1, _btnColorPicker.y - 1, _btnColorPicker.width + 2, _btnColorPicker.height + 2, foreground: (short)COLOR.FG_RED);
            DrawSprite(_btnColorPicker.x, _btnColorPicker.y, _btnColorPicker.outputSprite);

            DrawSprite(_tb_Width.x, _tb_Width.y, _tb_Width.outputSprite);
            DrawSprite(_tb_Height.x, _tb_Height.y, _tb_Height.outputSprite);
            DrawSprite(_tb_SaveName.x, _tb_SaveName.y, _tb_SaveName.outputSprite);
            DrawSprite(_tb_GridWidth.x, _tb_GridWidth.y, _tb_GridWidth.outputSprite);
            DrawSprite(_tb_GridHeight.x, _tb_GridHeight.y, _tb_GridHeight.outputSprite);

            DrawSprite(_lb_SavedFiles.x, _lb_SavedFiles.y, _lb_SavedFiles.outputSprite);

            DrawSprite(_animationPreview.x, _animationPreview.y, _animationPreview.outputSprite);

            DrawSprite(_btnMark.x, _btnMark.y, _btnMark.outputSprite);
            
            DrawSprite(_btnCopy.x, _btnCopy.y, _btnCopy.outputSprite);
            DrawSprite(_btnAbortMarkAndCopy.x, _btnAbortMarkAndCopy.y, _btnAbortMarkAndCopy.outputSprite);
            DrawSprite(_btnConfirmMarkAndCopy.x, _btnConfirmMarkAndCopy.y, _btnConfirmMarkAndCopy.outputSprite);



            if (_fillBucketActive)
                DrawASCIIRectangle(_btnFillBucket.x - 1, _btnFillBucket.y - 1, _btnFillBucket.width + 2, _btnFillBucket.height + 2, foreground: (short)COLOR.FG_RED);
            DrawSprite(_btnFillBucket.x, _btnFillBucket.y, _btnFillBucket.outputSprite);

            //DrawArea
            DrawRectangle(3, 8, 100, 50, (short)COLOR.FG_WHITE);
            DrawRectangle(4, 9, 98, 48, (short)COLOR.FG_DARK_GREY);

            if (_sprite.Width > _spriteAreaW || _sprite.Height > _spriteAreaH)
                DrawPartialSprite(_spriteDrawX, _spriteDrawY, _sprite, _spriteCursorX, _spriteCursorY, _spriteAreaW, _spriteAreaH);
            else
                DrawSprite(_spriteDrawX, _spriteDrawY, _sprite);

            if(_marking_visible)
            {
                _markingSprite = _sprite.ReturnPartialSpriteInverted(_markingStartX, _markingStartY, _markingEndX - _markingStartX + 1, _markingEndY - _markingStartY + 1);
                DrawSprite(_markingSpriteX, _markingSpriteY, _markingSprite);
            }

            if (_markingActive)
                DrawASCIIRectangle(_btnMark.x - 1, _btnMark.y - 1, _btnMark.width + 2, _btnMark.height + 2, foreground: (short)COLOR.FG_RED);

            Print(3, 7, $"{_cursorX - _spriteDrawX};{_cursorY - _spriteDrawY}");
            Print(0, Height - 1, $"marking active:{_markingActive}; draging:{_markingDraging}");

            return true;
        }

        #region INPUTS
        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            _btnClear.Update(r);
            _btnSave.Update(r);
            _btnLoad.Update(r);
            _btnColorPicker.Update(r);
            _btnReplaceColor.Update(r);
            _btnAddGrid.Update(r);
            _btnFillBucket.Update(r);

            _btnMark.Update(r);
            _btnCopy.Update(r);
            _btnAbortMarkAndCopy.Update(r);
            _btnConfirmMarkAndCopy.Update(r);


            _tb_Width.UpdateSelection(r);
            _tb_Height.UpdateSelection(r);
            _tb_SaveName.UpdateSelection(r);
            _tb_GridHeight.UpdateSelection(r);
            _tb_GridWidth.UpdateSelection(r);

            _lb_SavedFiles.Update(r);

            _animationPreview.UpdateMouseInput(r);

            _cursorX = r.dwMousePosition.X;
            _cursorY = r.dwMousePosition.Y;

            _leftMousebuttonClicked = false;
            _leftMouseButtonReleased = false;

            if(r.dwButtonState != oldMouseState.dwButtonState)
            {
                if (r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_1ST_BUTTON_PRESSED)
                {
                    _leftMousebuttonClicked = !_leftMousebuttonHeld;
                    _leftMousebuttonHeld = true;
                }
                else
                {
                    _leftMouseButtonReleased = true;
                    _leftMousebuttonHeld = false;
                }
            }
            oldMouseState = r;

            _mouseWheelClicked = r.dwButtonState == MOUSE_EVENT_RECORD.FROM_LEFT_2ND_BUTTON_PRESSED;
            _rightMousebuttonClicked = r.dwButtonState == MOUSE_EVENT_RECORD.RIGHTMOST_BUTTON_PRESSED;
        }
        private void EvaluateGUIClick()
        {
            if(_markingActive && !_markingDraging)
            {
                if (_cursorX >= 5 && _cursorX <= 102 && _cursorY >= 10 && _cursorY <= 57)
                {
                    if (_leftMousebuttonClicked)
                    {
                        _markingStartX = _cursorX - 5;
                        _markingStartY = _cursorY - 10;

                        _markingSpriteX = _cursorX;
                        _markingSpriteY = _cursorY;

                        _markingEndX = _cursorX - 5;
                        _markingEndY = _cursorY - 10;

                        _marking_visible = true;
                    }

                    if (_leftMousebuttonHeld && !_markingDraging)
                    {
                        _markingEndX = _cursorX - 5;
                        _markingEndY = _cursorY - 10;
                    }
                    else if (_leftMouseButtonReleased)
                    {
                        _markingDraging = true;
                        _markingSpriteX = _markingStartX + 5;
                        _markingSpriteY = _markingStartY + 10;
                    }
                }
            }
            else if(_markingDraging)
            {
                if (_leftMousebuttonClicked || _leftMousebuttonHeld)
                {
                    _markingSpriteX = _cursorX - _markingSprite.Width / 2;
                    _markingSpriteY = _cursorY - _markingSprite.Height / 2;
                }
            }
            else if (_leftMousebuttonClicked || _leftMousebuttonHeld || _rightMousebuttonClicked)
            {
                //color or brush picking
                if (_cursorY == 2 || _cursorY == 3)
                {
                    //foreground color
                    if (_cursorX >= 1 && _cursorX <= 32)
                    {
                        switch (_cursorX)
                        {
                            case 1:
                            case 2: 
                                if(_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_BLACK; 
                                else if(_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_BLACK;
                                break;
                                
                            case 3:
                            case 4:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_DARK_BLUE;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_DARK_BLUE;
                                break;
                            case 5:
                            case 6: 
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_DARK_GREEN;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_DARK_GREEN;
                                break;
                            case 7:
                            case 8:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_DARK_CYAN;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_DARK_CYAN;
                                break;
                            case 9:
                            case 10:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_DARK_RED;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_DARK_RED;
                                break;
                            case 11:
                            case 12:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_DARK_MAGENTA;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_DARK_MAGENTA;
                                break;
                            case 13:
                            case 14:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_DARK_YELLOW;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_DARK_YELLOW;
                                break;
                            case 15:
                            case 16:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_GREY;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_GREY;
                                break;
                            case 17:
                            case 18:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_DARK_GREY;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_DARK_GREY;
                                break;
                            case 19:
                            case 20:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_BLUE;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_BLUE;
                                break;
                            case 21:
                            case 22:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_GREEN;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_GREEN;
                                break;
                            case 23:
                            case 24:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_CYAN;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_CYAN;
                                break;
                            case 25:
                            case 26:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_RED;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_RED;
                                break;
                            case 27:
                            case 28:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_MAGENTA;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_MAGENTA;
                                break;
                            case 29:
                            case 30:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_YELLOW; 
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_YELLOW;
                                break;
                            case 31:
                            case 32:
                                if (_leftMousebuttonClicked)
                                    _foregroundColor = (short)COLOR.FG_WHITE;
                                else if (_rightMousebuttonClicked)
                                    _foregroundColorReplaceBrush = (short)COLOR.FG_WHITE;
                                break;
                        }
                    }
                    //background color
                    else if (_cursorX >= 40 && _cursorX <= 71)
                    {
                        switch (_cursorX)
                        {
                            case 40:
                            case 41:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_BLACK;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_BLACK;
                                break;

                            case 42:
                            case 43:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_DARK_BLUE;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_DARK_BLUE;
                                break;
                            case 44:
                            case 45:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_DARK_GREEN;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_DARK_GREEN;
                                break;
                            case 46:
                            case 47:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_DARK_CYAN;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_DARK_CYAN;
                                break;
                            case 48:
                            case 49:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_DARK_RED;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_DARK_RED;
                                break;
                            case 50:
                            case 51:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_DARK_MAGENTA;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_DARK_MAGENTA;
                                break;
                            case 52:
                            case 53:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_DARK_YELLOW;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_DARK_YELLOW;
                                break;
                            case 54:
                            case 55:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_GREY;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_GREY;
                                break;
                            case 56:
                            case 57:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_DARK_GREY;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_DARK_GREY;
                                break;
                            case 58:
                            case 59:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_BLUE;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_BLUE;
                                break;
                            case 60:
                            case 61:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_GREEN;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_GREEN;
                                break;
                            case 62:
                            case 63:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_CYAN;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_CYAN;
                                break;
                            case 64:
                            case 65:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_RED;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_RED;
                                break;
                            case 66:
                            case 67:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_MAGENTA;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_MAGENTA;
                                break;
                            case 68:
                            case 69:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_YELLOW;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_YELLOW;
                                break;
                            case 70:
                            case 71:
                                if (_leftMousebuttonClicked)
                                    _backgroundColor = (short)COLOR.FG_WHITE;
                                else if (_rightMousebuttonClicked)
                                    _backgroundColorReplaceBrush = (short)COLOR.FG_WHITE;
                                break;
                        }
                    }
                    //brush
                    else if (_cursorX >= 80 && _cursorX <= 87)
                    {
                        switch (_cursorX)
                        {
                            case 80:
                            case 81:
                                if (_leftMousebuttonClicked) 
                                    _brush = '░';
                                else if (_rightMousebuttonClicked)
                                    _replaceBrush = '░';
                                break;
                            case 82:
                            case 83: 
                                if (_leftMousebuttonClicked)
                                    _brush = '▒';
                                else if (_rightMousebuttonClicked)
                                    _replaceBrush = '▒';
                                break;
                            case 84:
                            case 85:
                                if (_leftMousebuttonClicked)
                                    _brush = '▓';
                                else if (_rightMousebuttonClicked)
                                    _replaceBrush = '▓';
                                break;

                            case 86:
                            case 87: 
                                if (_leftMousebuttonClicked)
                                    _brush = '█';
                                else if (_rightMousebuttonClicked)
                                    _replaceBrush = '█';
                                break;
                        }
                    }
                }
                else if(_cursorY == 4) //other brushes
                {
                    if (_cursorX >= 80 && _cursorX <= 90) 
                    {
                        if (_leftMousebuttonClicked)
                            _brush = _otherBrushes[_cursorX - 80];
                        else if(_rightMousebuttonClicked)
                            _replaceBrush = _otherBrushes[_cursorX - 80];

                    }
                }
                //draw on sprite
                else if (_cursorX >= 5 && _cursorX <= 102 && _cursorY >= 10 && _cursorY <= 57)
                {
                    if (_cursorX - 5 < _sprite.Width && _cursorY - 10 < _sprite.Height)
                    {
                        if (!_colorPickerActive && !_markingActive && !_fillBucketActive)
                        {
                            if (_leftMousebuttonClicked || _leftMousebuttonHeld)
                            {
                                short color = (short)(_backgroundColor << 4);
                                color += _foregroundColor;
                                _sprite.SetPixel(_cursorX - 5 + _spriteCursorX, _cursorY - 10 + _spriteCursorY, _brush, color);
                            }
                            else if (_rightMousebuttonClicked)
                            {
                                short color = (short)(_backgroundColorReplaceBrush << 4);
                                color += _foregroundColorReplaceBrush;
                                _sprite.SetPixel(_cursorX - 5 + _spriteCursorX, _cursorY - 10 + _spriteCursorY, _replaceBrush, color);
                            }
                        }
                        else if (_markingActive)
                        {

                        }
                        else if (_colorPickerActive)
                        {
                            short colorToPick = _sprite.GetColor(_cursorX - 5 + _spriteCursorX, _cursorY - 10 + _spriteCursorY);
                            _foregroundColor = (short)(colorToPick & 0x0F);
                            _backgroundColor = (short)(colorToPick >> 4);
                            _brush = _sprite.GetChar(_cursorX - 5 + _spriteCursorX, _cursorY - 10 + _spriteCursorY);
                        }
                        else if (_fillBucketActive)
                        {
                            short color = (short)(_backgroundColor << 4);
                            color += _foregroundColor;

                            int x = _cursorX - 5 + _spriteCursorX;
                            int y = _cursorY - 10 + _spriteCursorY;


                            _sprite.FillBucket(x, y, _brush, color, _sprite.GetChar(x, y), _sprite.GetColor(x, y));
                        }
                    }
                }
                //colorPickerActive = false;
            }
        }
        private bool BtnClearClicked()
        {
            if (_tb_Width.content != "" && _tb_Height.content != "")
                _sprite = new Sprite(Convert.ToInt32(_tb_Width.content), Convert.ToInt32(_tb_Height.content), '█', COLOR.BG_BLACK);

            _spriteCursorX = 0;
            _spriteCursorY = 0;

            _tb_Width.content = "";
            _tb_Height.content = "";

            return true;
        }
        private bool BtnSaveClicked()
        {
            string exportPath = _tb_SaveName.content != "" ? @"Savefiles\" + _tb_SaveName.content + ".txt" : @"Savefiles\" + "NewFile" + ".txt";

            if(!File.Exists(exportPath))
                _saveFiles.Add(Path.GetFileName(exportPath));

            _tb_SaveName.content = Path.GetFileNameWithoutExtension(exportPath);

            using (StreamWriter outputfile = new StreamWriter(exportPath))
            {
                outputfile.Write($"{_sprite.Width};{_sprite.Height};");

                for (int j = 0; j < _sprite.Height; j++)
                {
                    for (int i = 0; i < _sprite.Width; i++) //sprite.Width
                    {
                        outputfile.Write($"{_sprite.GetChar(i, j)},");
                    }
                }
                outputfile.Write(";");

                for (int j = 0; j < _sprite.Height; j++)
                {
                    for (int i = 0; i < _sprite.Width; i++) //sprite.Width
                    {
                        outputfile.Write($"{_sprite.GetColor(i, j)},");
                    }
                }
            }

            
            return true;
        }
        private bool BtnLoadClicked()
        {

            string ext = Path.GetExtension(_saveFiles[_lb_SavedFiles.selectedEntry]);
            //check extension of file
            switch (Path.GetExtension(_saveFiles[_lb_SavedFiles.selectedEntry]))
            {
                case ".txt":
                    _sprite = new Sprite("Savefiles\\" + _saveFiles[_lb_SavedFiles.selectedEntry]);
                    break;
                case ".png":
                    Png png = Png.Open("Savefiles\\" + _saveFiles[_lb_SavedFiles.selectedEntry]);

                    _sprite = new Sprite(png.Width, png.Height);

                    for (int x = 0; x < png.Width; x++)
                    {
                        for (int y = 0; y < png.Height; y++)
                        {
                            byte red = png.GetPixel(x, y).R;
                            byte green = png.GetPixel(x, y).G;
                            byte blue = png.GetPixel(x, y).B;

                            short col = ClosedConsoleColor3Bit(red, green, blue, out char pixel);

                            _sprite.SetPixel(x, y, pixel, col);
                        }
                    }
                    break;
                default:
                    return false;
            }

            _tb_SaveName.content = Path.GetFileNameWithoutExtension(_saveFiles[_lb_SavedFiles.selectedEntry]);

            return true;
        }
        private bool BtnColorPickerClicked()
        {
            _markingActive = false;
            _fillBucketActive = false;
            _colorPickerActive = !_colorPickerActive;
            return true;
        }
        private bool BtnMarkClicked()
        {
            _fillBucketActive = false;   
            _colorPickerActive = false;
            _markingActive = !_markingActive;
            return true;
        }
        private bool BtnCopyClicked()
        {
            return true;
        }
        private bool BtnAbortClicked()
        {
            _marking_visible = false;
            _markingSprite = null;
            _markingDraging = false;
            return true;
        }
        private bool BtnConfirmClicked()
        {
            if(_markingSprite != null)
                _sprite.AddSpriteToSprite(_markingSpriteX - 5, _markingSpriteY - 10, _markingSprite.ReturnPartialSpriteInverted(0, 0, _markingSprite.Width, _markingSprite.Height));
            _marking_visible = false;
            _markingSprite = null;
            _markingDraging = false;
            _markingActive = false;
            
            return true;
        }
        private bool BtnReplaceClicked()
        {
            short color = (short)((_backgroundColor << 4) + _foregroundColor);
            short replaceColor = (short)((_backgroundColorReplaceBrush << 4) + _foregroundColorReplaceBrush);

            for (int x = 0; x < _sprite.Width; x++)
            {
                for(int y = 0; y < _sprite.Height; y++)
                {
                    if(_sprite.GetColor(x,y) == color && _sprite.GetChar(x,y) == _brush)
                    {
                        _sprite.SetPixel(x, y, _replaceBrush, replaceColor);
                    }
                }
            }

            return true;
        }
        private bool BtnAddGridClicked()
        {
            if(_tb_GridHeight.content != "" && _tb_GridWidth.content != "")
            {
                int gridheight = Convert.ToInt32(_tb_GridHeight.content);
                int gridwidth = Convert.ToInt32(_tb_GridWidth.content);

                for(int i = 0; i < Width; i+=gridwidth)
                {
                    for (int j = 0; j < Height; j++)
                        _sprite.SetPixel(i, j, (char)PIXELS.PIXEL_SOLID, 0x44);

                }

                for(int i = 0; i <Height; i+=gridheight)
                {
                    for(int j = 0; j < Width; j++)
                    {
                        _sprite.SetPixel(j, i, (char)PIXELS.PIXEL_SOLID, 0x44);
                    }
                }
            }
            return true;
        }
        private bool BtnFillBucketClicked()
        {
            _colorPickerActive = false;
            _markingActive = false;
            _fillBucketActive = !_fillBucketActive;
            return true;
        }
        #endregion

        #region DRAWING UI
        private void DrawColorPalette(int x, int y, string headline)
        {
            Print(x,y,headline);
            short color = 0x00;
            for(int i = x; i < x + 32; i+=2)
            {
                SetChar(i, y + 1, (char)PIXELS.PIXEL_SOLID, color);
                SetChar(i, y + 2, (char)PIXELS.PIXEL_SOLID, color);
                SetChar(i + 1, y + 1, (char)PIXELS.PIXEL_SOLID, color);
                SetChar(i + 1, y + 2, (char)PIXELS.PIXEL_SOLID, color);

                color++;
            }
        }
        private void DrawBrushes(int x, int y, string headline)
        {
            Print(x,y,headline);

            //Mainbrushes
            char[] brushes = new char[4] { '░', '▒', '▓', '█' };
            for(int i = 0; i < 8; i+=2)
            {
                SetChar(x + i, y + 1, brushes[i / 2]);
                SetChar(x + i, y + 2, brushes[i / 2]);
                SetChar(x + i + 1, y + 1, brushes[i / 2]);
                SetChar(x + i + 1, y + 2, brushes[i / 2]);
            }
            //Other
            for(int i = 0; i < _otherBrushes.Count; i++)
                SetChar(x+i, y+3, _otherBrushes[i]);
        }
        private void DrawActiveBrush(int x, int y, string headline, short foregroundColor, short backgroundColor, char brush)
        {
            Print(x,y,headline);

            short color = (short)(backgroundColor << 4);
            color += foregroundColor;

            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    SetChar(x + i + headline.Length / 2 , y + j + 1, brush, color);
                }
            }

        }
        #endregion

        private class AnimationPreview
        {
            public int x, y;
            private int spriteW, spriteH;

            private short foregroundColor, backgroundColor;

            private Button btn_Start, btn_Stop, btn_Forward, btn_Backwards;
            private TextBox tb_SpriteW, tb_SpriteH, tb_FrameDelay, tb_FrameCount;

            public Sprite outputSprite;
            private int frameCounter = 0;
            private bool loop = false;

            private TimeSpan frameDelay = new TimeSpan(0, 0, 0, 0, 0);
            private DateTime lastUpdate = DateTime.Now;

            public AnimationPreview(int x, int y, int spriteW = 16, int spriteH = 16, short backgroundColor = (short)COLOR.FG_BLACK, short foregroundColor = (short)COLOR.FG_WHITE)
            {
                this.x = x;
                this.y = y;
                this.spriteW = spriteW;
                this.spriteH = spriteH;
                this.backgroundColor = backgroundColor;
                this.foregroundColor = foregroundColor;

                btn_Backwards = new Button(x + 1, y + spriteH + 2, "<", method: BtnBackwardsClick);
                btn_Start = new Button(x + 5, y + spriteH + 2, "»", method: BtnStartClick);
                btn_Stop = new Button(x + 9, y + spriteH + 2, "■", method: BtnStopClick);
                btn_Forward = new Button(x + 13, y + spriteH + 2, ">", method: BtnForwardClick);

                tb_SpriteW = new TextBox(x + spriteW + 3, y, 3, "Width");
                tb_SpriteH = new TextBox(x + spriteW + 3, y + 5, 3, "Height");
                tb_FrameDelay = new TextBox(x + spriteW + 3, y + 10, 3, "Delay", content:"100");
                tb_FrameCount = new TextBox(x + spriteW + 3, y + 15, 5 , "#Frms", content:"1");
            }

            public void UpdateMouseInput(MOUSE_EVENT_RECORD r)
            {
                btn_Backwards.Update(r);
                btn_Start.Update(r);
                btn_Stop.Update(r);
                btn_Forward.Update(r);

                tb_SpriteW.UpdateSelection(r);
                tb_SpriteH.UpdateSelection(r);
                tb_FrameDelay.UpdateSelection(r);
                tb_FrameCount.UpdateSelection(r);
            }
            public void UpdateKeyInput(KeyState[] KeyStates, TimeSpan elapsedTime, Sprite sprite)
            {
                if(tb_FrameDelay.content != "")
                    frameDelay = new TimeSpan(0, 0, 0, 0, Convert.ToInt32(tb_FrameDelay.content));

                if (frameDelay < DateTime.Now - lastUpdate && loop)
                {
                    lastUpdate = DateTime.Now;
                    frameCounter++;

                    if(tb_FrameCount.content != "")
                        if (frameCounter > Convert.ToInt32(tb_FrameCount.content))
                            frameCounter = 0;
                }
                tb_SpriteW.UpdateInput(KeyStates, elapsedTime);
                tb_SpriteH.UpdateInput(KeyStates, elapsedTime);
                tb_FrameDelay.UpdateInput(KeyStates, elapsedTime);
                tb_FrameCount.UpdateInput(KeyStates, elapsedTime);

                BuildSprite(sprite);
            }

            private void BuildSprite(Sprite sprite)
            {
                short color = (short)((foregroundColor << 4) + backgroundColor);
                outputSprite = new Sprite(8 + spriteW, 8 + spriteH);

                #region frame around spriteFrame
                //corners
                outputSprite.SetPixel(0, 0, '┌', color);
                outputSprite.SetPixel(spriteW + 1, 0, '┐', color);
                outputSprite.SetPixel(0, spriteH + 1, '└', color);
                outputSprite.SetPixel(spriteW + 1, spriteH + 1, '┘', color);

                for(int i = 0; i < spriteW; i++)
                {
                    //horizontal lines
                    outputSprite.SetPixel(1 + i, 0, '─', color);
                    outputSprite.SetPixel(1 + i, spriteH + 1, '─', color);
                    for (int j = 0; j < spriteH; j++)
                    {
                        //vertical lines
                        outputSprite.SetPixel(0, 1 + j, '│', color);
                        outputSprite.SetPixel(spriteW + 1, 1 + j, '│', color);
                    }
                }
                #endregion

                #region UI-Elements
                outputSprite.AddSpriteToSprite(1, spriteH + 2, btn_Backwards.outputSprite);
                outputSprite.AddSpriteToSprite(5, spriteH + 2, btn_Start.outputSprite);
                outputSprite.AddSpriteToSprite(9, spriteH + 2, btn_Stop.outputSprite);
                outputSprite.AddSpriteToSprite(13, spriteH + 2, btn_Forward.outputSprite);

                outputSprite.AddSpriteToSprite(spriteW + 3, 0, tb_SpriteW.outputSprite);
                outputSprite.AddSpriteToSprite(spriteW + 3, 5, tb_SpriteH.outputSprite);
                outputSprite.AddSpriteToSprite(spriteW + 3, 10, tb_FrameDelay.outputSprite);
                outputSprite.AddSpriteToSprite(spriteW + 3, 15, tb_FrameCount.outputSprite);
                #endregion

                #region animationFrame
                outputSprite.AddSpriteToSprite(1, 1, sprite.ReturnPartialSprite(frameCounter * (tb_SpriteW.content == "" ? spriteW : Convert.ToInt32(tb_SpriteW.content)), 0, (tb_SpriteW.content == "" ? spriteW : Convert.ToInt32(tb_SpriteW.content)), (tb_SpriteH.content == "" ? spriteH : Convert.ToInt32(tb_SpriteH.content))));

                #endregion
            }
            #region BtnClicks
            private bool BtnBackwardsClick()
            {
                frameCounter--;

                if (frameCounter < 0)
                    frameCounter = Convert.ToInt32(tb_FrameCount.content) - 1;
                return true;
            }
            private bool BtnStartClick()
            {
                loop = true;
                return true;
            }
            private bool BtnForwardClick()
            {
                frameCounter++;

                if (frameCounter > Convert.ToInt32(tb_FrameCount.content))
                    frameCounter = 0;

                return true;
            }
            private bool BtnStopClick()
            {
                loop = false;
                return true;
            }
            #endregion
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            using (var f = new SpriteEditor())
                f.Start();
        }
    }
}
