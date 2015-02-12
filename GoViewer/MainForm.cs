using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GoViewer
{
    /// <summary>
    /// 处理鼠标，显示等等
    /// </summary>
    public partial class MainForm : Form
    {
        private BoardPanel boardPanel;
        /// <summary>
        /// 初始化新棋盘
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            board = new Board();
            this.boardPanel = new BoardPanel();
            this.boardPanel.Size = new System.Drawing.Size(this.Width - 145, this.Height - 150);
            this.boardPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.boardPanel_Paint);
            this.boardPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.boardPanel_MouseClick);
            this.boardPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.boardPanel_MouseMove);
            this.boardPanel.Resize += new System.EventHandler(this.boardPanel_Resize);
            this.Controls.Add(this.boardPanel);

        }

        /// <summary>
        /// 退出菜单项
        /// </summary>
        private void itemQuit_Click(object sender, EventArgs e)
        {
            Dispose();
        }

        private Board board;
        private const bool BLACK = true;
        private const bool WHITE = false;

        private int width;
        private int height;
        private int size;
        private int mouseI;
        private int mouseJ;
        private bool mouseIn = false;
        private int timerCount;

        private bool? turn;
        private Image img;

        //棋盘面板重绘，画棋盘棋子
        private void boardPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(img, 0, 0);
        }

        /// <summary>
        /// 画棋子
        /// </summary>
        /// <param name="g"></param>
        /// <param name="i">第i行</param>
        /// <param name="j">第j列</param>
        /// <param name="isBlack">true为黑， false为白, null为空</param>
        private void drawStone(Graphics g, int i, int j, bool? isBlack)
        {
            if (isBlack == null) return;
            int x = width / 2 - 9 * size + j * size;
            int y = height / 2 - 9 * size + i * size;

            //设置颜色渐变画棋子
            Brush brush;
            if (isBlack == BLACK)
                brush = new LinearGradientBrush(new Rectangle(x, y, size - 2, size - 2), Color.Gray, Color.Black, 60f);
            else
                brush = new LinearGradientBrush(new Rectangle(x, y, size - 2, size - 2), Color.White, Color.Gray, 60f);

            g.FillEllipse(brush, x + 1, y + 1, size - 2, size - 2);
        }

        /// <summary>
        /// 窗口大小改变重绘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boardPanel_Resize(object sender, EventArgs e)
        {
            width = boardPanel.Width;
            height = boardPanel.Height;
            size = Math.Min(width, height) / 21;
            drawImage();
            Refresh();
        }

        /// <summary>
        /// 鼠标点击棋盘响应
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boardPanel_MouseClick(object sender, MouseEventArgs e)
        {
            setBoard(e.X, e.Y, turn);
            if (mouseIn)                    //鼠标是否点在棋盘范围
            {
                //更新需更新的部分棋盘
                drawImage();
                foreach (Point p in board.needToInvalidate)
                {
                    int x = width / 2 - 9 * size + p.Y * size;
                    int y = height / 2 - 9 * size + p.X * size;
                    Rectangle rec = new Rectangle(x, y, size, size);
                    boardPanel.Invalidate(rec);

                }
                //黑白互换
                if (board.needToInvalidate.Count != 0)
                    if (turn == true) turn = false;
                    else if (turn == false) turn = true;
            }
            mouseIn = false;
        }

        /// <summary>
        /// 测试坐标是否在棋盘范围内
        /// 如在转换成棋盘矩阵坐标并落子
        /// </summary>
        /// <param name="p1">棋盘面板横坐标</param>
        /// <param name="p2">棋盘面板纵坐标</param>
        /// <param name="turn">黑或白或空</param>
        private void setBoard(int p1, int p2, bool? turn)
        {
            mouseI = (p2 - (height / 2 - 9 * size)) / size;
            mouseJ = (p1 - (width / 2 - 9 * size)) / size;
            if (mouseI < 0 || mouseI > 18 || mouseJ < 0 || mouseJ > 18)
            {
                mouseIn = false;
                return;
            }
            mouseIn = true;
            board.setStone(mouseI, mouseJ, turn);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="isBlack"></param>
        private void drawSquare(Graphics g, int i, int j, bool? isBlack)
        {
            if (isBlack == null) return;
            int x = width / 2 - 9 * size + j * size + size / 2 - 3;
            int y = height / 2 - 9 * size + i * size + size / 2 - 3;
            if (isBlack == true)
                g.FillRectangle(Brushes.Black, x, y, 6, 6);
            else
                g.FillRectangle(Brushes.White, x, y, 6, 6);

        }

        /// <summary>
        /// 鼠标在棋盘上移动处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void boardPanel_MouseMove(object sender, MouseEventArgs e)
        {
            //setIndex(e.X, e.Y);
        }

        /// <summary>
        /// 窗口加载后初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            width = boardPanel.Width;
            height = boardPanel.Height;
            size = Math.Min(width, height) / 21;
            turn = BLACK;
            drawImage();
        }

        /// <summary>
        /// 将棋盘棋子画入位图
        /// </summary>
        private void drawImage()
        {
            img = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(img);

            //使绘图质量最高，即消除锯齿
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.FillRectangle(Brushes.Gray, width / 2 - 9 * size - size / 4 + 3, height / 2 - 9 * size - size / 4 + 3, size * 19 + size / 2, size * 19 + size / 2);
            g.FillRectangle(Brushes.Peru, width / 2 - 9 * size - size / 4, height / 2 - 9 * size - size / 4, size * 19 + size / 2, size * 19 + size / 2);
            Pen pen = new Pen(Brushes.Black, 1);
            for (int i = 0; i < 19; i++)
            {
                g.DrawLine(pen, width / 2 - 9 * size + i * size + size / 2, height / 2 - 9 * size + size / 2,
                    width / 2 - 9 * size + i * size + size / 2, height / 2 + 9 * size + size / 2);
                g.DrawLine(pen, width / 2 - 9 * size + size / 2, height / 2 - 9 * size + i * size + size / 2,
                    width / 2 + 9 * size + size / 2, height / 2 - 9 * size + i * size + size / 2);
            }
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    g.FillEllipse(Brushes.Black, width / 2 - 9 * size + size / 2 + ((i + 1) * 6 - 3) * size - 4,
                        height / 2 - 9 * size + size / 2 + ((j + 1) * 6 - 3) * size - 4, 8, 8);
                }
            for (int i = 0; i < 19; i++)
                for (int j = 0; j < 19; j++)
                    drawStone(g, i, j, board.getStone(i, j));
        }

        private List<Move> moves;

        /// <summary>
        /// 清除棋盘，设置定时器
        /// 将刚下完的或文件调入的棋谱定时显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMode_Click(object sender, EventArgs e)
        {
            moves = new List<Move>(board.Moves);
            board.clear();
            timerCount = 0;
            timerView.Enabled = true;
            drawImage();
            boardPanel.Refresh();

        }

        /// <summary>
        /// 定时器事件操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerView_Tick(object sender, EventArgs e)
        {
            if (timerCount >= moves.Count)
            {
                timerView.Enabled = false;
                board.Moves = new List<Move>(moves);
                return;
            }
            board.setStone(moves[timerCount].row, moves[timerCount].col, moves[timerCount].black == BLACK);
            timerCount++;
            drawImage();
            foreach (Point p in board.needToInvalidate)
            {
                int x = width / 2 - 9 * size + p.Y * size;
                int y = height / 2 - 9 * size + p.X * size;
                Rectangle rec = new Rectangle(x, y, size, size);
                boardPanel.Invalidate(rec);

            }
        }

        /// <summary>
        /// 打开文件按钮和菜单项操作
        /// 解析部分gib文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "弈城棋谱|*.gib";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FileStream aFile = new FileStream(dialog.FileName, FileMode.Open);
                StreamReader sr = new StreamReader(aFile);
                board.Moves = new List<Move>();


                string strLine = sr.ReadLine();
                while (strLine != null)
                {
                    string[] Strs = strLine.Split(' ');
                    if (Strs[0] != "STO")
                    {
                        strLine = sr.ReadLine();
                        continue;
                    }
                    board.Moves.Add(new Move(int.Parse(Strs[5]), int.Parse(Strs[4]), Strs[3] == "1"));
                    strLine = sr.ReadLine();
                }
                sr.Close();
            }

            //显示终局棋盘
            moves = new List<Move>(board.Moves);
            board.clear();
            drawImage();
            boardPanel.Refresh();
            foreach (var item in moves)
            {
                board.setStone(item.row, item.col, item.black == BLACK);
            }
            drawImage();
            boardPanel.Refresh();
            board.Moves = new List<Move>(moves);
        }
    }
}
