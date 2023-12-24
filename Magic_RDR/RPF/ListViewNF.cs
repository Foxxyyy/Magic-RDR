using System.Collections;
using System.Windows.Forms;

namespace Magic_RDR.RPF
{
    class ListViewNF : ListView
    {
        public ListViewNF()
        {
            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }
    }

    public class ListViewColumnSorter : IComparer
    {
        private int sortColumn = 0; //Initialize with -1 to indicate no column is sorted.
        private SortOrder sortOrder = SortOrder.Ascending; //Default sorting order is ascending.

        public int SortColumn
        {
            get { return sortColumn; }
            set { sortColumn = value; }
        }

        public SortOrder SortOrder
        {
            get { return sortOrder; }
            set { sortOrder = value; }
        }

        public int Compare(object x, object y)
        {
            if (!(x is ListViewItem))
                return 0;
            if (!(y is ListViewItem))
                return 0;
            if (SortColumn < 0)
                return 0;

            ListViewItem itemX = (ListViewItem)x;
            ListViewItem itemY = (ListViewItem)y;

            if (itemX.ListView.Columns[SortColumn].Tag == null)
            {
                itemX.ListView.Columns[SortColumn].Tag = "Text";
            }

            if (itemX.ListView.Columns[SortColumn].Tag.ToString() == "Numeric")
            {
                int fl1 = int.Parse(itemX.SubItems[SortColumn].Text);
                int fl2 = int.Parse(itemY.SubItems[SortColumn].Text);

                if (SortOrder == SortOrder.Ascending)
                    return fl1.CompareTo(fl2);
                else
                    return fl2.CompareTo(fl1);
            }
            else
            {
                //If not numeric, perform a regular string comparison.
                string textX = itemX.SubItems[SortColumn].Text;
                string textY = itemY.SubItems[SortColumn].Text;

                return string.Compare(textX, textY) * (SortOrder == SortOrder.Ascending ? 1 : -1);
            }
        }
    }
}