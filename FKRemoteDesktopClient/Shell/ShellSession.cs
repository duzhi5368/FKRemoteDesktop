using FKRemoteDesktop.Framework;
using FKRemoteDesktop.Message.SubMessages;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Shell
{
    public class ShellSession : IDisposable
    {
        private Process _prc;                                   // 控制台进程
        private bool _read;                                     // 是否应当从输出中继续读取
        private readonly object _readLock = new object();       // 读取变量的锁
        private readonly object _readStreamLock = new object(); // StreamReader的锁
        private Encoding _encoding;                             // 当前控制台的编码
        private StreamWriter _inputWriter;                      // 输入控制台的输入流
        private readonly FKClient _client;                      // 发送消息的网络客户端

        public ShellSession(FKClient client)
        {
            _client = client;
        }

        private void CreateSession()
        {
            lock (_readLock)
            {
                _read = true;
            }

            var cultureInfo = CultureInfo.InstalledUICulture;
            _encoding = Encoding.GetEncoding(cultureInfo.TextInfo.OEMCodePage);
            _prc = new Process
            {
                StartInfo = new ProcessStartInfo("cmd")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = _encoding,
                    StandardErrorEncoding = _encoding,
                    WorkingDirectory = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)),
                    Arguments = $"/K CHCP {_encoding.CodePage}"
                }
            };
            _prc.Start();

            RedirectIO();

            _client.Send(new DoShellExecuteResponse
            {
                Output = "\n>> 创建 Shell 会话完成\n"
            });
        }

        private void RedirectIO()
        {
            _inputWriter = new StreamWriter(_prc.StandardInput.BaseStream, _encoding);
            new Thread(RedirectStandardOutput).Start();
            new Thread(RedirectStandardError).Start();
        }

        private void ReadStream(int firstCharRead, StreamReader streamReader, bool isError)
        {
            lock (_readStreamLock)
            {
                var streamBuffer = new StringBuilder();
                streamBuffer.Append((char)firstCharRead);
                while (streamReader.Peek() > -1)
                {
                    var ch = streamReader.Read();
                    streamBuffer.Append((char)ch);
                    if (ch == '\n')
                        SendAndFlushBuffer(ref streamBuffer, isError);
                }
                SendAndFlushBuffer(ref streamBuffer, isError);
            }
        }

        private void SendAndFlushBuffer(ref StringBuilder textBuffer, bool isError)
        {
            if (textBuffer.Length == 0)
                return;
            var toSend = ConvertEncoding(_encoding, textBuffer.ToString());
            if (string.IsNullOrEmpty(toSend))
                return;
            _client.Send(new DoShellExecuteResponse { Output = toSend, IsError = isError });
            textBuffer.Clear();
        }

        private void RedirectStandardOutput()
        {
            try
            {
                int ch;
                while (_prc != null && !_prc.HasExited && (ch = _prc.StandardOutput.Read()) > -1)
                {
                    ReadStream(ch, _prc.StandardOutput, false);
                }

                lock (_readLock)
                {
                    if (_read)
                    {
                        _read = false;
                        throw new ApplicationException("Shell 会话异常关闭");
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException || ex is InvalidOperationException)
                {
                    _client.Send(new DoShellExecuteResponse
                    {
                        Output = "\n>> Shell 会话异常关闭\n",
                        IsError = true
                    });
                    CreateSession();
                }
            }
        }

        private void RedirectStandardError()
        {
            try
            {
                int ch;
                while (_prc != null && !_prc.HasExited && (ch = _prc.StandardError.Read()) > -1)
                {
                    ReadStream(ch, _prc.StandardError, true);
                }

                lock (_readLock)
                {
                    if (_read)
                    {
                        _read = false;
                        throw new ApplicationException("Shell 会话异常关闭");
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                if (ex is ApplicationException || ex is InvalidOperationException)
                {
                    _client.Send(new DoShellExecuteResponse
                    {
                        Output = "\n>> Shell 会话异常关闭\n",
                        IsError = true
                    });
                    CreateSession();
                }
            }
        }

        public bool ExecuteCommand(string command)
        {
            if (_prc == null || _prc.HasExited)
            {
                try
                {
                    CreateSession();
                }
                catch (Exception ex)
                {
                    _client.Send(new DoShellExecuteResponse
                    {
                        Output = $"\n>> 创建 Shell 会话失败: {ex.Message}\n",
                        IsError = true
                    });
                    return false;
                }
            }
            _inputWriter.WriteLine(ConvertEncoding(_encoding, command));
            _inputWriter.Flush();
            return true;
        }

        private string ConvertEncoding(Encoding sourceEncoding, string input)
        {
            var utf8Text = Encoding.Convert(sourceEncoding, Encoding.UTF8, sourceEncoding.GetBytes(input));
            return Encoding.UTF8.GetString(utf8Text);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_readLock)
                {
                    _read = false;
                }
                if (_prc == null)
                    return;
                if (!_prc.HasExited)
                {
                    try
                    {
                        _prc.Kill();
                    }
                    catch { }
                }
                if (_inputWriter != null)
                {
                    _inputWriter.Close();
                    _inputWriter = null;
                }
                _prc.Dispose();
                _prc = null;
            }
        }
    }
}