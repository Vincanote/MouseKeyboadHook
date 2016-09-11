using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VincaNote.MouseKeyBoadHook;

namespace TestWPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // MouseKeyBoadHook動作確認用
        MouseKeyBoadHook hook = null;
        MainWindowViewModel viewmodel = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();
            hook = new MouseKeyBoadHook();
            hook.KeyBoadHooked += Hook_KeyBoadHooked;
            hook.MouseHooked += Hook_MouseHooked;
            this.DataContext = viewmodel;
            hook.SetHook(HookType.WH_KEYBOARD_LL | HookType.WH_MOUSE_LL);
        }

        private void Hook_MouseHooked(object sender, MouseHookEventArgs e)
        {
            viewmodel.MouseMessage = e.Message;
            viewmodel.PositionX = e.X;
            viewmodel.PositionY = e.Y;
            viewmodel.IsXButton1 = e.IsXButton1;
            viewmodel.IsXButton2 = e.IsXButton2;
            viewmodel.WheelDelta = e.WheelDelta;
            viewmodel.IsInjectedMouse = e.IsInjected;
            viewmodel.IsLowerInjectedMouse = e.IsLowerInjected;
        }

        private void Hook_KeyBoadHooked(object sender, KeyBoadHookEventArgs e)
        {
            viewmodel.KeyboadMessage = e.Message;
            viewmodel.VirtualKeyCode = KeyInterop.KeyFromVirtualKey((int)e.VirtualKeyCode);
            viewmodel.IsExtended = e.IsExtended;
            viewmodel.IsInjected = e.IsInjected;
            viewmodel.IsLowerInjected = e.IsLowerInjected;
            viewmodel.IsAltKeyPressed = e.IsAltKeyPressed;
            viewmodel.IsKeyPressed = e.IsKeyPressed;

            if(e.IsAltKeyPressed && KeyInterop.KeyFromVirtualKey((int)e.VirtualKeyCode).Equals(Key.F4))
            {
                e.Cancel = true;
            }
        }
    }
}
