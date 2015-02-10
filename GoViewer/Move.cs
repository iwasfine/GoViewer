using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoViewer
{
    /// <summary>
    /// 每着棋的结构
    /// 棋盘横纵坐标和黑白
    /// </summary>
    public struct Move
    {
        public int row;
        public int col;
        public bool black;
        
        public Move(int i, int j, bool black)
        {
            this.row = i;
            this.col = j;
            this.black = black;
        }
    }
}
