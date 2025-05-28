using FKRemoteDesktop.Structs;
using System.Collections;
using System.Windows.Forms;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Controls
{
    public class ListViewColumnSorter : IComparer
    {
        private int _columnToSort;                                  // 要进行排序的列
        private SortOrder _orderOfSort;                             // 排序方式:正序逆序
        private readonly CaseInsensitiveComparer _objectCompare;    // 比较器（不区分大小）
        private bool _needNumberCompare;                            // 是否要比较数字

        public ListViewColumnSorter()
        {
            _columnToSort = 0;
            _orderOfSort = SortOrder.None;
            _objectCompare = new CaseInsensitiveComparer();
            _needNumberCompare = false;
        }

        public int SortColumn
        {
            set { _columnToSort = value; }
            get { return _columnToSort; }
        }

        public SortOrder Order
        {
            set { _orderOfSort = value; }
            get { return _orderOfSort; }
        }

        public bool NeedNumberCompare
        {
            set { _needNumberCompare = value; }
            get { return _needNumberCompare; }
        }

        // 比较两个对象。若相等，返回0；若X>Y，则返回正值；若X<Y，则返回负值
        public int Compare(object x, object y)
        {
            var listviewX = (ListViewItem)x;
            var listviewY = (ListViewItem)y;
            if (listviewX.SubItems[0].Text == ".." || listviewY.SubItems[0].Text == "..")
                return 0;

            int compareResult;
            if (_needNumberCompare)
            {
                long a, b;
                if (listviewX.Tag is SFileManagerListTag)
                {
                    // 要比较的文件大小
                    a = (listviewX.Tag as SFileManagerListTag).FileSize;
                    b = (listviewY.Tag as SFileManagerListTag).FileSize;
                    compareResult = a >= b ? (a == b ? 0 : 1) : -1;
                }
                else
                {
                    if (long.TryParse(listviewX.SubItems[_columnToSort].Text, out a)
                        && long.TryParse(listviewY.SubItems[_columnToSort].Text, out b))
                    {
                        compareResult = a >= b ? (a == b ? 0 : 1) : -1;
                    }
                    else
                    {
                        compareResult = _objectCompare.Compare(listviewX.SubItems[_columnToSort].Text,
                     listviewY.SubItems[_columnToSort].Text);
                    }
                }
            }
            else
            {
                compareResult = _objectCompare.Compare(listviewX.SubItems[_columnToSort].Text,
                    listviewY.SubItems[_columnToSort].Text);
            }

            if (_orderOfSort == SortOrder.Ascending)
            {
                return compareResult;
            }
            else if (_orderOfSort == SortOrder.Descending)
            {
                return (-compareResult);
            }
            else
            {
                return 0;
            }
        }
    }
}
