using CGEOnlineTools;
using ConsoleGameEngine;
using ConsoleGameEngine.Other;
using System;
using static ConsoleGameEngine._3DEngine;
using static ConsoleGameEngine.Other.NativeMethods;

namespace UdtLikeClientApp
{
    class OnlineGame : GameConsole
    {
        IntPtr inHandle;
        delegate void MyDelegate();

        private Sprite _backgroundSprite;
        private ConsoleGameEngine.Player _player;
        private List<ConsoleGameEngine.Player> _otherPlayers = [];

        private int updateID;

        #region login process
        TextBox _tbPlayerName;
        TextBox _tbIpAddress;
        TextBox _tbPort;
        Button _btnLogIn;
        #endregion

        bool _logedIn;

        CGEClient client;

        public OnlineGame()
          : base(200, 110, "Fonts", fontwidth: 4, fontheight: 4)
        { }
        public override bool OnUserCreate()
        {
            #region init of handlers
            inHandle = NativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE);
            uint mode = 0;
            NativeMethods.GetConsoleMode(inHandle, ref mode);
            mode &= ~NativeMethods.ENABLE_QUICK_EDIT_MODE; //disable
            mode |= NativeMethods.ENABLE_WINDOW_INPUT; //enable (if you want)
            mode |= NativeMethods.ENABLE_MOUSE_INPUT; //enable
            NativeMethods.SetConsoleMode(inHandle, mode);

            ConsoleListener.MouseEvent += ConsoleListener_MouseEvent;
            
            #endregion


            ConsoleGameEngine.Other.TextWriter.InitTextWriter();

            _tbPlayerName = new TextBox(50, 20, 100, "name            ", false);
            _tbIpAddress = new TextBox(50, 40, 100,  "server-ip       ", false);
            _tbPort = new TextBox(50, 60, 100,       "server-port     ", false);

            //preentered for debug
            _tbPlayerName.content = "rolph";
            _tbIpAddress.content = "127.0.0.1";
            _tbPort.content = "12345";


            _btnLogIn = new Button(50, 80, ConsoleGameEngine.Other.TextWriter.GenerateTextSprite("join"));
            _btnLogIn.OnButtonClicked(JoinButtonClicked);




            _backgroundSprite = new Sprite("Assets\\background.txt");


            ConsoleListener.Start();
            return true;
        }
        public override bool OnUserUpdate(TimeSpan elapsedTime)
        {
            Clear();

            if (!_logedIn)
            {
                _tbPlayerName.UpdateInput(KeyStates, elapsedTime);
                _tbIpAddress.UpdateInput(KeyStates, elapsedTime);
                _tbPort.UpdateInput(KeyStates, elapsedTime);


                DrawSprite(_tbPlayerName.x, _tbPlayerName.y, _tbPlayerName.outputSprite);
                DrawSprite(_tbIpAddress.x, _tbIpAddress.y, _tbIpAddress.outputSprite);
                DrawSprite(_tbPort.x, _tbPort.y, _tbPort.outputSprite);
                DrawSprite(_btnLogIn.x, _btnLogIn.y, _btnLogIn.outputSprite);

            }
            else
            {
                DrawSprite(0, 0, _backgroundSprite);


                if(GameConsole.ApplicationIsActivated())
                    _player.Update(KeyStates, elapsedTime, this);
                DrawSprite((int)_player.xPosition, (int)_player.yPosition, _player.outputSprite);

                foreach (ConsoleGameEngine.Player otherPlayer in _otherPlayers)
                {
                    otherPlayer.Update(null, elapsedTime, this);
                    DrawSprite((int)otherPlayer.xPosition, (int)otherPlayer.yPosition, otherPlayer.outputSprite);
                }

                //Update Message to server
                client.clientInfo.X = (int)_player.xPosition;
                client.clientInfo.Y = (int)_player.yPosition;

            }
            return true;
        }

        private void ConsoleListener_MouseEvent(MOUSE_EVENT_RECORD r)
        {
            if (!_logedIn)
            {
                _tbPlayerName.UpdateSelection(r);
                _tbIpAddress.UpdateSelection(r);
                _tbPort.UpdateSelection(r);
                _btnLogIn.Update(r);
            }
        }

        private bool JoinButtonClicked()
        {
            _player = new ConsoleGameEngine.Player(_tbPlayerName.content);
            _player.LoadAnimation("Assets\\running ninja.txt");

            client = new CGEClient(_player.displayName, _tbIpAddress.content, Convert.ToInt32(_tbPort.content));

            client.OnServerMessage += (msg) =>
            {
                HandleServerMessage(msg);
            };

            client.Start();

            _logedIn = true;

            return true;

        }

        private void HandleServerMessage(string msg)
        {
            var messageParts = msg.Split('\n');
            updateID++;

            foreach (var part in messageParts)
            {
                if (part != null && part != "")
                {
                    var parameters = part.Split(';');
                    var _name = parameters[0];
                    var _xPos = parameters[1];
                    var _yPos = parameters[2];

                    if (_name != client.clientInfo.Username)
                    {
                        if (_otherPlayers.Find(e => e.displayName == _name) == null)
                        {
                            _otherPlayers.Add(new ConsoleGameEngine.Player(_name));
                            _otherPlayers.Last().xPosition = Convert.ToDouble(_xPos);
                            _otherPlayers.Last().yPosition = Convert.ToDouble(_yPos);
                            _otherPlayers.Last().LoadAnimation("Assets\\running ninja.txt");
                            _otherPlayers.Last().updatedID = updateID;
                        }
                        else
                        {
                            _otherPlayers.Find(e => e.displayName == _name).xPosition = Convert.ToDouble(_xPos);
                            _otherPlayers.Find(e => e.displayName == _name).yPosition = Convert.ToDouble(_yPos);
                            _otherPlayers.Find(e => e.displayName == _name).updatedID = updateID;
                        }
                    }
                }
            }

            _otherPlayers.RemoveAll(e => e.updatedID != updateID);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            using (var f = new OnlineGame())
                f.Start();
        }
    }
}