using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PathfindingVisualizer
{
    using Point = System.Drawing.Point;
    internal class Cell
    {
        public Point position;
        public int size;
        public Color color;

        public Cell(Point position, int size, Color color)
        {
            this.position = position;
            this.size = size;
            this.color = color;
        }

        //public override string ToString()
        //{
        //    return position != new Point(250, 300)? "N" : "C";
        //}
    }
}
