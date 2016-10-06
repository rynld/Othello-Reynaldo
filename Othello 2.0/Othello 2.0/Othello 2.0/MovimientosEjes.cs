using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Othello_2._0
{
     class MovimientosEjes
    {
        public float Velocidad_X
        { get; set; }
        public float Velocidad_Y
        { get; set; }

        public MovimientosEjes(float velo_x, float velo_y)
        {
            this.Velocidad_X = velo_x;
            this.Velocidad_Y = velo_y;
        }
    }
}

