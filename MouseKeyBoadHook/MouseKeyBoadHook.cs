using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace VincaNote.MouseKeyBoadHook
{

    /// <summary>
    /// マウスの操作
    /// </summary>
    public enum MouseMessage
    {
        WM_LBUTTONDOWN = 0x0201,
        WM_LBUTTONUP = 0x0202,
        WM_RBUTTONDOWN = 0x204,
        WM_RBUTTONUP = 0x205,
        WM_MBUTTONDOWN = 0x207,
        WM_MBUTTONUP = 0x208,
        WM_MBUTTONDBLCLK = 0x209,
        WM_MOUSEMOVE = 0x200,
        WM_MOUSEWHEEL = 0x20A,
        WM_XBUTTONDOWN = 0x20B,
        WM_XBUTTONUP = 0x20C
    }

    /// <summary>
    /// キーボードの操作
    /// </summary>
    public enum KeyBoadMessage
    {
        WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x0101,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105
    }

    /// <summary>
    /// フックタイプ
    /// </summary>
    [Flags]
    public enum HookType
    {
        WH_KEYBOARD_LL = 13,
        WH_MOUSE_LL = 14
    }

    /// <summary>
    /// 点のx座標とy軸座標定義
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// 低レベルのマウス入力イベントに関する情報
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MSLLHOOKSTRUCT
    {
        public POINT pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    /// <summary>
    /// 低レベルのキーボード入力イベントに関する情報
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }

    public class MouseKeyBoadHook : IDisposable
    {
        /// <summary>
        /// フック用デリゲート
        private delegate int HookDelegate(int nCode, IntPtr wParam, IntPtr lParam);
        private IntPtr mouseHook   = IntPtr.Zero;
        private IntPtr keyboadHook = IntPtr.Zero;
        private GCHandle mouseHookHandle;
        private GCHandle keyboadHookHandle;

        /// <summary>
        /// キーボードフックイベント
        /// </summary>
        public event EventHandler<KeyBoadHookEventArgs> KeyBoadHooked;
        protected virtual void OnKeyBoadHookEvent(KeyBoadHookEventArgs e)
        {
            EventHandler<KeyBoadHookEventArgs> handler = KeyBoadHooked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// マウスフックイベント
        /// </summary>
        public event EventHandler<MouseHookEventArgs> MouseHooked;
        protected virtual void OnMouseHookEvent(MouseHookEventArgs e)
        {
            EventHandler<MouseHookEventArgs> handler = MouseHooked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// マウスフック状態
        /// </summary>
        public bool IsMouseHook { get; set; }

        /// <summary>
        /// キーボードフック状態
        /// </summary>
        public bool IsKeyboadHook { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MouseKeyBoadHook()
        {
            this.IsMouseHook = false;
            this.IsKeyboadHook = false;
        }

        /// <summary>
        /// フック登録
        /// </summary>
        /// <param name="type"></param>
        public void SetHook(HookType type)
        {

            using (Process process = Process.GetCurrentProcess())
            using (ProcessModule module = process.MainModule)
            {
                // マウスフック登録
                if (type.HasFlag(HookType.WH_MOUSE_LL))
                {
                    HookDelegate mouseHook = new HookDelegate(MouseHookProc);
                    this.mouseHookHandle = GCHandle.Alloc(mouseHook);
                    this.mouseHook = NativeMethods.SetWindowsHookEx((int)HookType.WH_MOUSE_LL,
                                                                    Marshal.GetFunctionPointerForDelegate(mouseHook),
                                                                    NativeMethods.GetModuleHandle(module.ModuleName),
                                                                    0);

                    if (this.mouseHook.Equals(IntPtr.Zero))
                    {
                        int hResult = Marshal.GetHRForLastWin32Error();
                        Marshal.ThrowExceptionForHR(hResult);
                    }

                    this.IsMouseHook = true;
                }

                // キーボードフック登録
                if (type.HasFlag(HookType.WH_KEYBOARD_LL))
                {
                    HookDelegate keyboadHook = new HookDelegate(KeyBoadHookProc);
                    this.keyboadHookHandle = GCHandle.Alloc(keyboadHook);
                    this.keyboadHook = NativeMethods.SetWindowsHookEx((int)HookType.WH_KEYBOARD_LL,
                                                                    Marshal.GetFunctionPointerForDelegate(keyboadHook),
                                                                    NativeMethods.GetModuleHandle(module.ModuleName),
                                                                    0);
                    if (this.keyboadHook.Equals(IntPtr.Zero))
                    {
                        int hResult = Marshal.GetHRForLastWin32Error();
                        Marshal.ThrowExceptionForHR(hResult);
                    }

                    this.IsKeyboadHook = true;
                }
            }
        }

        /// <summary>
        /// フック解除
        /// </summary>
        /// <param name="type"></param>
        public void UnSetHook(HookType type)
        {
            if (type.HasFlag(HookType.WH_MOUSE_LL) && this.mouseHook != IntPtr.Zero)
            {
                this.IsMouseHook = false;
                NativeMethods.UnhookWindowsHookEx(this.mouseHook);
                this.mouseHook = IntPtr.Zero;
                if (this.mouseHookHandle.IsAllocated)
                {
                    this.mouseHookHandle.Free();
                }
            }

            if (type.HasFlag(HookType.WH_KEYBOARD_LL) && this.keyboadHook != IntPtr.Zero)
            {
                this.IsKeyboadHook = false;
                NativeMethods.UnhookWindowsHookEx(this.keyboadHook);
                this.keyboadHook = IntPtr.Zero;
                if (this.keyboadHookHandle.IsAllocated)
                {
                    this.keyboadHookHandle.Free();
                }
            }
        }
        /// <summary>
        /// デストラクタ
        /// </summary>
        ~MouseKeyBoadHook()
        {
            Dispose(false);
        }

        /// <summary>
        /// マウスフック
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                MSLLHOOKSTRUCT msllhook;
                MouseHookEventArgs e;
                msllhook = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                e = new MouseHookEventArgs((MouseMessage)wParam.ToInt32(), msllhook);
                OnMouseHookEvent(e);
                if (e.Cancel)
                {
                    return -1;
                }
            }
            return NativeMethods.CallNextHookEx(this.mouseHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// キーボードフック
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private int KeyBoadHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT kbllhook;
                KeyBoadHookEventArgs e;
                kbllhook = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                e = new KeyBoadHookEventArgs((KeyBoadMessage)wParam.ToInt32(), kbllhook);
                OnKeyBoadHookEvent(e);
                if (e.Cancel)
                {
                    return -1;
                }
            }
            return NativeMethods.CallNextHookEx(this.keyboadHook, nCode, wParam, lParam);
        }

        /// <summary>
        /// Disposeの実装
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                //マネージオブジェクト開放
            }

            //アンマネージオブジェクト開放
            UnSetHook(HookType.WH_KEYBOARD_LL | HookType.WH_MOUSE_LL);
            disposed = true;
        }

        internal static class NativeMethods
        {
            /// <summary>
            /// アプリケーション定義のフックプロシージャをフックチェーン内にインストールします。
            /// フックプロシージャをインストールすると、特定のイベントタイプを監視できます。
            /// 監視の対象になるイベントは、特定のスレッド、または呼び出し側スレッドと同じデスクトップ内のすべてのスレッドに関連付けられているものです。
            /// </summary>
            /// <param name="idHook"></param>
            /// <param name="lpfn"></param>
            /// <param name="hInstance"></param>
            /// <param name="threadId"></param>
            /// <returns></returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern IntPtr SetWindowsHookEx(int idHook, IntPtr lpfn, IntPtr hInstance, int threadId);

            /// <summary>
            /// SetWindowsHookEx 関数を使ってフックチェーン内にインストールされたフックプロシージャを削除します
            /// </summary>
            /// <param name="hhk"></param>
            /// <returns></returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            /// <summary>
            /// 現在のフックチェーン内の次のフックプロシージャに、フック情報を渡します。
            /// フックプロシージャは、フック情報を処理する前でも、フック情報を処理した後でも、この関数を呼び出せます。
            /// </summary>
            /// <param name="idHook"></param>
            /// <param name="nCode"></param>
            /// <param name="wParam"></param>
            /// <param name="lParam"></param>
            /// <returns></returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            /// <summary>
            /// 呼び出し側プロセスのアドレス空間に該当ファイルがマップされている場合、指定されたモジュール名のモジュールハンドルを返します。
            /// </summary>
            /// <param name="lpModuleName"></param>
            /// <returns></returns>
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(String lpModuleName);
        }
    }

    /// <summary>
    /// MouseHookに関するイベント情報
    /// </summary>
    public class MouseHookEventArgs : CancelEventArgs
    {
        private MouseMessage message;
        private int posX;
        private int posY;
        private uint flags;
        private int delta;
        private int xButton;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message"></param>
        /// <param name="keyhook"></param>
        internal MouseHookEventArgs(MouseMessage msg, MSLLHOOKSTRUCT mousuHook)
        {
            this.message = msg;
            this.flags = mousuHook.flags;
            this.posX = mousuHook.pt.x;
            this.posY = mousuHook.pt.y;
            this.delta = 0;

            switch(msg)
            {
                case MouseMessage.WM_MOUSEWHEEL:
                    this.delta = (short)(mousuHook.mouseData >> 16);
                    break;
                case MouseMessage.WM_XBUTTONDOWN:
                case MouseMessage.WM_XBUTTONUP:
                    xButton = (short)(mousuHook.mouseData >> 16);
                    break;
            }
        }

        /// <summary>
        /// マウス操作
        /// </summary>
        public MouseMessage Message { get { return this.message; } }
        /// <summary>
        /// X座標
        /// </summary>
        public int X { get { return this.posX; } }
        /// <summary>
        /// Y座標
        /// </summary>
        public int Y { get { return this.posY; } }
        /// <summary>
        /// Xボタン1
        /// </summary>
        public bool IsXButton1 { get { return xButton.Equals(0x0001); } }
        /// <summary>
        /// Xボタン2
        /// </summary>
        public bool IsXButton2 { get { return xButton.Equals(0x0002); } }
        /// <summary>
        /// ホイール回転量
        /// </summary>
        public int WheelDelta { get { return this.delta; } }

        /// <summary>
        /// イベントがインジェクトされたかどうか
        /// </summary>
        public bool IsInjected { get { return ((this.flags & 0x0001) != 0); } }
        /// <summary>
        /// 低い整合性レベルで実行中のプロセスからイベントがインジェクトされたかどうか
        /// </summary>
        public bool IsLowerInjected { get { return ((this.flags & 0x0002) != 0); } }

    }
    
    /// <summary>
    /// KeyboadHookに関するイベント情報
    /// </summary>
    public class KeyBoadHookEventArgs : CancelEventArgs
    {
        private KeyBoadMessage message;
        private uint flags;
        private uint vkCode;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message"></param>
        /// <param name="keyhook"></param>
        internal KeyBoadHookEventArgs(KeyBoadMessage msg, KBDLLHOOKSTRUCT keyhook)
        {
            this.message = msg;
            this.flags = keyhook.flags;
            this.vkCode = keyhook.vkCode;
        }

        /// <summary>
        /// 仮想キーコード
        /// </summary>
        public uint VirtualKeyCode { get { return this.vkCode; } }
        /// <summary>
        /// キーボード操作
        /// </summary>
        public KeyBoadMessage Message { get { return this.message; } }
        /// <summary>
        /// キーがファンクションキーや数値キーパッド上のキーなどの拡張キーかどうか
        /// </summary>
        public bool IsExtended { get { return ((this.flags & 0x0001) != 0); } }
        /// <summary>
        /// イベントがインジェクトされたかどうか
        /// </summary>
        public bool IsInjected { get { return ((this.flags & 0x0010) != 0); } }
        /// <summary>
        /// 低い整合性レベルで実行中のプロセスからイベントがインジェクトされたかどうか
        /// </summary>
        public bool IsLowerInjected { get { return ((this.flags & 0x0002) != 0); } }
        /// <summary>
        /// ALTキーが押されているかどうか
        /// </summary>
        public bool IsAltKeyPressed { get { return ((this.flags & 0x0020) != 0); } }
        /// <summary>
        /// キーが押されているかどうか
        /// </summary>
        public bool IsKeyPressed { get { return ((this.flags & 0x0080) == 0); } }

    }
}
