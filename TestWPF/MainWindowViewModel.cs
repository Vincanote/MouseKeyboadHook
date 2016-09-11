using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VincaNote.MouseKeyBoadHook;
using System.Windows.Input;

namespace TestWPF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        //マウス関係
        private MouseMessage _mouseMessage = MouseMessage.WM_LBUTTONDOWN;
        public MouseMessage MouseMessage
        {
            get { return _mouseMessage; }
            set { _mouseMessage = value; NotifyPropertyChanged(); }
        }

        private int _PositionX = 0;
        public int PositionX
        {
            get { return _PositionX; }
            set { _PositionX = value; NotifyPropertyChanged(); }
        }

        private int _PositionY = 0;
        public int PositionY
        {
            get { return _PositionY; }
            set { _PositionY = value; NotifyPropertyChanged(); }
        }

    
        /// <summary>
        /// Xボタン1
        /// </summary>
        private bool _IsXButton1 = false;
        public bool IsXButton1 {
            get { return _IsXButton1; }
            set { _IsXButton1 = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// Xボタン2
        /// </summary>
        private bool _IsXButton2 = false;
        public bool IsXButton2
        {
            get { return _IsXButton2; }
            set { _IsXButton2 = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// ホイール回転量
        /// </summary>
        private int delta = 0;
        public int WheelDelta {
            get { return this.delta; }
            set { delta = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// イベントがインジェクトされたかどうか
        /// </summary>
        private bool _IsInjectedMouse = false;
        public bool IsInjectedMouse
        {
            get { return _IsInjectedMouse; }
            set { _IsInjectedMouse = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// 低い整合性レベルで実行中のプロセスからイベントがインジェクトされたかどうか
        /// </summary>
        private bool _IsLowerInjectedMouse = false;
        public bool IsLowerInjectedMouse
        {
            get { return _IsLowerInjectedMouse; }
            set { _IsLowerInjectedMouse = value; NotifyPropertyChanged(); }
        }

        // キーボード関係
        private KeyBoadMessage _keyboadMessage = KeyBoadMessage.WM_KEYDOWN;
        public KeyBoadMessage KeyboadMessage
        {
            get { return _keyboadMessage; }
            set { _keyboadMessage = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// 仮想キーコード
        /// </summary>
        private Key vkcode = Key.System;
        public Key VirtualKeyCode {
            get { return this.vkcode; }
            set { vkcode = value; NotifyPropertyChanged(); }
        }

        /// <summary>
        /// キーがファンクションキーや数値キーパッド上のキーなどの拡張キーかどうか
        /// </summary>
        private bool _IsExtended = false;
        public bool IsExtended {
            get { return _IsExtended; }
            set { _IsExtended = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// イベントがインジェクトされたかどうか
        /// </summary>
        private bool _IsInjected = false;
        public bool IsInjected {
            get { return _IsInjected; }
            set { _IsInjected = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// 低い整合性レベルで実行中のプロセスからイベントがインジェクトされたかどうか
        /// </summary>
        private bool _IsLowerInjected = false;
        public bool IsLowerInjected {
            get { return _IsLowerInjected; }
            set { _IsLowerInjected = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// ALTキーが押されているかどうか
        /// </summary>
        private bool _IsAltKeyPressed = false;
        public bool IsAltKeyPressed {
            get { return _IsAltKeyPressed; }
            set { _IsAltKeyPressed = value; NotifyPropertyChanged(); }
        }
        /// <summary>
        /// キーが押されているかどうか
        /// </summary>
        private bool _IsKeyPressed = false;
        public bool IsKeyPressed {
            get { return _IsKeyPressed; }
            set { _IsKeyPressed = value; NotifyPropertyChanged(); }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
