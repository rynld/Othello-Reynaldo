using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Othello_2._0
{
    public class Fichas
    {
        //Estas son las matrices tipicas de las fichas
        public Matrix World
        { get; internal set; }
        public Model modelo
        { get; private set; }
        public int Fila
        {
            get;
            private set;
        }
        public int Columna
        { get; private set; }
        public Tablero.Colores Color
        {
            //El color blanco es para el 1, el negro para el 0;
            get;
            set;
        }
        public Vector3 Posicion
        {
            get;
            set;
        }
        public bool HayQueVirar
        { get; set; }


        public Fichas(Model modelo, Vector3 posicion, Tablero.Colores color, int fila, int columna)
        {
            //El color todavia hay que configurarlo, esto no esta bien.
            //No se como crear la matrix world para el objeto.
            this.HayQueVirar = false;
            this.modelo = modelo;
            this.Color = color;
            this.Posicion = posicion;
            this.Fila = fila;
            this.World = Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(posicion);//Primero se escala y despues se traslada.
            this.Columna = columna;

        }

        public void Vira()
        {
            //Esto es para que cuando la ficha haga la rotacion
            //poder almacener la posicion final, que es una vuelta entera, Pi.
            World = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationX(MathHelper.Pi) * World;
            HayQueVirar = false;
        }



    }
}
