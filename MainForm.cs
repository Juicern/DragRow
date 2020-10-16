using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DragRow
{
    public partial class MainForm : Form
    {
        // 数据表
        DataTable dataTable = new DataTable();

        // 拖动的源数据行索引
        private int indexOfItemUnderMouseToDrag = -1;
        // 拖动的目标数据行索引
        private int indexOfItemUnderMouseToDrop = -1;
        // 拖动中的鼠标所在位置的当前行索引
        private int indexOfItemUnderMouseOver = -1;
        // 不启用拖放的鼠标范围
        private Rectangle dragBoxFromMouseDown = Rectangle.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("Column1", typeof(string));
            dataTable.Columns.Add("Column2", typeof(string));
            for (int id = 1; id <= 20; id++)
                dataTable.Rows.Add(id, string.Format("A{0}", id), string.Format("B{0}", id));

            dataGridView.DataSource = dataTable;
        }

        private void dataGridView_MouseDown(object sender, MouseEventArgs e)
        {
            // 通过鼠标按下的位置获取所在行的信息
            var hitTest = dataGridView.HitTest(e.X, e.Y);
            if (hitTest.Type != DataGridViewHitTestType.Cell)
                return;

            // 记下拖动源数据行的索引及已鼠标按下坐标为中心的不会开始拖动的范围
            indexOfItemUnderMouseToDrag = hitTest.RowIndex;
            if (indexOfItemUnderMouseToDrag > -1)
            {
                Size dragSize = SystemInformation.DragSize;
                dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);
            }
            else
                dragBoxFromMouseDown = Rectangle.Empty;

        }

        private void dataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            // 释放鼠标按键时清空变量为默认值
            dragBoxFromMouseDown = Rectangle.Empty;
        }

        private void dataGridView_MouseMove(object sender, MouseEventArgs e)
        {
            // 不是鼠标左键按下时移动
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
                return;
            // 如果鼠标在不启用拖动的范围内
            if (dragBoxFromMouseDown == Rectangle.Empty || dragBoxFromMouseDown.Contains(e.X, e.Y))
                return;
            // 如果源数据行索引值不正确
            if (indexOfItemUnderMouseToDrag < 0)
                return;

            // 开始拖动，第一个参数表示要拖动的数据，可以自定义，一般是源数据行
            var row = dataGridView.Rows[indexOfItemUnderMouseToDrag];
            DragDropEffects dropEffect = dataGridView.DoDragDrop(row, DragDropEffects.All);

            //拖动过程结束后清除拖动位置行的红线效果
            OnRowDragOver(-1);
        }

        private void dataGridView_DragOver(object sender, DragEventArgs e)
        {
            // 把屏幕坐标转换成控件坐标
            Point p = dataGridView.PointToClient(new Point(e.X, e.Y));

            // 通过鼠标按下的位置获取所在行的信息
            // 如果不是在数据行或者在源数据行上则不能作为拖放的目标
            var hitTest = dataGridView.HitTest(p.X, p.Y);
            if (hitTest.Type != DataGridViewHitTestType.Cell || hitTest.RowIndex == indexOfItemUnderMouseToDrag)
            {
                e.Effect = DragDropEffects.None;
                OnRowDragOver(-1);
                return;
            }

            // 设置为作为拖放移动的目标
            e.Effect = DragDropEffects.Move;
            // 通知目标行重绘
            OnRowDragOver(hitTest.RowIndex);
        }

        private void dataGridView_DragDrop(object sender, DragEventArgs e)
        {
            // 把屏幕坐标转换成控件坐标
            Point p = dataGridView.PointToClient(new Point(e.X, e.Y));

            // 如果当前位置不是数据行
            // 或者刚好是源数据行的下一行（本示例中假定拖放操作为拖放至目标行的上方）
            // 则不进行任何操作
            var hitTest = dataGridView.HitTest(p.X, p.Y);
            if (hitTest.Type != DataGridViewHitTestType.Cell || hitTest.RowIndex == indexOfItemUnderMouseToDrag + 1)
                return;

            indexOfItemUnderMouseToDrop = hitTest.RowIndex;

            // * 执行拖放操作(执行的逻辑按实际需要)

            var tempRow = dataTable.NewRow();
            tempRow.ItemArray = dataTable.Rows[indexOfItemUnderMouseToDrag].ItemArray;
            dataTable.Rows.RemoveAt(indexOfItemUnderMouseToDrag);

            if (indexOfItemUnderMouseToDrag < indexOfItemUnderMouseToDrop)
                indexOfItemUnderMouseToDrop--;

            dataTable.Rows.InsertAt(tempRow, indexOfItemUnderMouseToDrop);
        }

        private void dataGridView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            // 如果当前行是鼠标拖放过程的所在行
            if (e.RowIndex == indexOfItemUnderMouseOver)
                e.Graphics.FillRectangle(Brushes.Red, e.RowBounds.X, e.RowBounds.Y, e.RowBounds.Width, 2);
        }

        private void OnRowDragOver(int rowIndex)
        {
            // 如果和上次导致重绘的行是同一行则无需重绘
            if (indexOfItemUnderMouseOver == rowIndex)
                return;

            int old = indexOfItemUnderMouseOver;
            indexOfItemUnderMouseOver = rowIndex;

            // 去掉原有行的红线
            if (old > -1)
                dataGridView.InvalidateRow(old);

            // 绘制新行的红线
            if (rowIndex > -1)
                dataGridView.InvalidateRow(rowIndex);
        }
    }
}
