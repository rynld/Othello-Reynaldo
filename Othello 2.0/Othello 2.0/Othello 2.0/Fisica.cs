using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.Threading;


namespace Othello_2._0
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Fisica : DrawableGameComponent
    {
        Camara camara;
        Model esfera;
        Stopwatch crono;
        List<Model> modelos;
        List<BoundingSphere> lista_esferas;
        List<Vector3> posiciones;
        List<MovimientosEjes> movimientos;

        int cantidad_modelos;
        float coeficiente_choque;
        int ultima_pos_1;//Es para evitar que cuando colisionen dos esferas
        int ultima_pos_2;//cada vez que entra al update cambia la direccion de estas.

        bool empezar;
        MouseState mouse_pos;
        MouseState mouse_penultima_pos;
        int cantidad;
        bool arranca;

        #region//Limites de la pantalla.
        int limite_derecha;
        int limite_izquierda;
        int limite_arriba;
        int limite_abajo;
        int tiempo_limite;//Tiempo sin que el mouse se mueva
        #endregion

        #region//Colores de las esferas
        float red;
        float green;
        float blue;
        float suma1;
        float suma2;
        float suma3;
        #endregion

        public bool Pelotas 
        { get { return empezar; } }

        public Fisica(Game game, int cantidad, Camara camara)
            : base(game)
        {
            this.camara = camara;
            this.movimientos = new List<MovimientosEjes>();
            this.lista_esferas = new List<BoundingSphere>();
            this.modelos = new List<Model>();
            this.posiciones = new List<Vector3>();
            this.coeficiente_choque = -1;
            this.cantidad_modelos = cantidad;
            this.ultima_pos_1 = -1;
            this.ultima_pos_2 = -1;
            this.empezar = false;
            this.mouse_pos = new MouseState();
            this.mouse_penultima_pos = new MouseState();
            this.cantidad = 0;
            this.limite_abajo = -75;
            this.limite_arriba = 74;
            this.limite_derecha = 102;
            this.limite_izquierda = -102;
            this.tiempo_limite = 10;
            this.red = 0;
            this.green = 0;
            this.blue = 0;
            this.suma1 = 0.001f;
            this.suma2 = 0.0001f;
            this.suma3 = 0.00003f;
            this.arranca = false;
        }


        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            CreaMovimientos();
            CreaPosiciones();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            esfera = Game.Content.Load<Model>("Modelos/esfera");
            CreaModelos();
            CreaSpheres();

            base.LoadContent();
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            #region//Colision de las pelotas
            if (empezar)
            {
                for (int i = 0; i < lista_esferas.Count; i++)
                {
                    for (int j = 0; j < lista_esferas.Count; j++)
                    {
                        if (i != j)
                        {
                            if (lista_esferas[i].Intersects(lista_esferas[j]))
                            {
                                if ((i != ultima_pos_1 || j != ultima_pos_2) && (i != ultima_pos_2 || j != ultima_pos_1))//Esto es para evitar cosas extranas
                                {
                                    CalculaVelocidad(i, j);
                                    ultima_pos_2 = i;
                                    ultima_pos_1 = j;
                                }
                            }
                        }
                        if (i != j)
                            PegadasEntreSi(i, j);
                    }
                    ColisionParedes(i);
                    Asigna(i);
                    PegadaBorde();
                }
            }
            #endregion

            #region//Movimiento del mouse
            mouse_pos = Mouse.GetState();
            if (mouse_penultima_pos.X == 0 && mouse_penultima_pos.Y == 0)
                mouse_penultima_pos = mouse_pos;
            if (mouse_pos.X == mouse_penultima_pos.X && mouse_pos.Y == mouse_penultima_pos.Y)
                arranca = true;
            else
            {
                arranca = false;
                mouse_penultima_pos = mouse_pos;
            }
            if (arranca && cantidad == 0)
            {
                crono = new Stopwatch();
                cantidad++;
                crono.Start();
            }
            else if (!arranca)
            {
                cantidad = 0;
                crono = new Stopwatch();
                empezar = false;
                Empieza();
            }
            if (crono.ElapsedMilliseconds / 1000 > tiempo_limite)
                empezar = true;
            #endregion

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (empezar)
            {
                for (int i = 0; i < modelos.Count; i++)
                {
                    CambiaColores();
                    foreach (ModelMesh malla in modelos[i].Meshes)
                    {
                        foreach (BasicEffect efectos in malla.Effects)
                        {
                            efectos.View = camara.View;
                            efectos.Projection = camara.Projection;
                            efectos.Alpha = 0.3f;
                            efectos.DiffuseColor = new Vector3(red, green, blue);
                            efectos.World = Matrix.CreateTranslation(lista_esferas[i].Center);
                        }
                        malla.Draw();
                    }
                }
            }
            base.Draw(gameTime);
        }



        //Metodos auxiliares

        #region//Inicializar todas las posiciones, direcciones etc
        void CreaModelos()
        {
            for (int i = 0; i < cantidad_modelos; i++)
                modelos.Add(esfera);
        }
        void CreaSpheres()
        {
            Random r = new Random();
            for (int i = 0; i < cantidad_modelos; i++)
                lista_esferas.Add(new BoundingSphere(posiciones[i], modelos[i].Meshes[0].BoundingSphere.Radius));
        }
        void CreaMovimientos()
        {
            Random r = new Random();
            for (int i = 0; i < cantidad_modelos; i++)
            {
                int valor = r.Next(-1, 2);
                movimientos.Add(new MovimientosEjes(valor == 0 ? 1 : valor, valor == 0 ? -1 : valor));
            }
        }
        void CreaPosiciones()
        {
            Random r = new Random();
            for (int i = 0; i < cantidad_modelos; i++)
                posiciones.Add(new Vector3(r.Next(-95, 95), r.Next(-65, 65), 0));
        }
        void Empieza()
        {
            modelos = new List<Model>();
            lista_esferas = new List<BoundingSphere>();
            movimientos = new List<MovimientosEjes>();
            posiciones = new List<Vector3>();
            CreaPosiciones();
            CreaMovimientos();
            CreaModelos();
            CreaSpheres();

        }
        #endregion

        #region//Manejo de las velocidades y las direcciones de choque
        public void CalculaVelocidad(int i, int j)
        {
            //i y j representan la posicion de las esferas que colisionaron
            if (SonOpuestas(movimientos[i].Velocidad_X, movimientos[j].Velocidad_X) && SonOpuestas(movimientos[i].Velocidad_Y, movimientos[j].Velocidad_Y))//Opuestos por las x y y.
            {
                movimientos[i].Velocidad_X *= coeficiente_choque;
                movimientos[i].Velocidad_Y *= coeficiente_choque;
                movimientos[j].Velocidad_X *= coeficiente_choque;
                movimientos[j].Velocidad_Y *= coeficiente_choque;
            }
            else if (SonOpuestas(movimientos[i].Velocidad_X, movimientos[j].Velocidad_X) && !SonOpuestas(movimientos[i].Velocidad_Y, movimientos[j].Velocidad_Y))//Opuestos por las x
            {
                movimientos[i].Velocidad_X *= coeficiente_choque;
                movimientos[j].Velocidad_X *= coeficiente_choque;
            }
            else if (!SonOpuestas(movimientos[i].Velocidad_X, movimientos[j].Velocidad_X) && SonOpuestas(movimientos[i].Velocidad_Y, movimientos[j].Velocidad_Y))//Opuestos por las y.
            {
                movimientos[i].Velocidad_Y *= coeficiente_choque;
                movimientos[j].Velocidad_Y *= coeficiente_choque;
            }
            else if (!SonOpuestas(movimientos[i].Velocidad_X, movimientos[j].Velocidad_X) && !SonOpuestas(movimientos[i].Velocidad_Y, movimientos[j].Velocidad_Y))//Todos iguales
            {
                if (movimientos[i].Velocidad_X > 0 && movimientos[i].Velocidad_Y > 0)//Tercer cuadrante y hacia arriba
                {
                    if (MasALaIzquierda(posiciones[i], posiciones[j]))
                    {
                        movimientos[i].Velocidad_Y *= coeficiente_choque;
                        movimientos[i].Velocidad_X *= coeficiente_choque;
                    }
                    else
                    {
                        movimientos[j].Velocidad_X *= coeficiente_choque;
                        movimientos[j].Velocidad_Y *= coeficiente_choque;
                    }
                }
                else if (movimientos[i].Velocidad_X > 0 && movimientos[i].Velocidad_Y < 0)//Segundo cuadrante y hacia abajo
                {
                    if (MasALaIzquierda(posiciones[i], posiciones[j]))
                    {
                        movimientos[i].Velocidad_X *= coeficiente_choque;
                        movimientos[i].Velocidad_Y *= coeficiente_choque;
                    }
                    else
                    {
                        movimientos[j].Velocidad_X *= coeficiente_choque;
                        movimientos[j].Velocidad_Y *= coeficiente_choque;
                    }
                }
                else if (movimientos[i].Velocidad_X < 0 && movimientos[i].Velocidad_Y > 0)//Cuarto cuadrante y hacia la izquierda
                {
                    if (MasALaIzquierda(posiciones[i], posiciones[j]))
                    {
                        movimientos[j].Velocidad_X *= coeficiente_choque;
                        movimientos[j].Velocidad_Y *= coeficiente_choque;
                    }
                    else
                    {
                        movimientos[i].Velocidad_X *= coeficiente_choque;
                        movimientos[i].Velocidad_Y *= coeficiente_choque;
                    }
                }
                else if (movimientos[i].Velocidad_X < 0 && movimientos[i].Velocidad_Y < 0)//Primer cuadrante y hacia la izquierda
                {
                    if (MasALaIzquierda(posiciones[i], posiciones[j]))
                    {
                        movimientos[j].Velocidad_Y *= coeficiente_choque;
                        movimientos[j].Velocidad_X *= coeficiente_choque;
                    }
                    else
                    {
                        movimientos[i].Velocidad_Y *= coeficiente_choque;
                        movimientos[i].Velocidad_X *= coeficiente_choque;
                    }
                }
            }


        }
        bool SonOpuestas(float velo_X_1, float velo_X_2)
        {
            if ((velo_X_1 > 0 && velo_X_2 < 0) || (velo_X_1 < 0 && velo_X_2 > 0)) return true;
            return false;
        }
        bool MasALaIzquierda(Vector3 esfera1, Vector3 esfera2)
        {
            //Dice si la esfera 1 esta mas a la izquierda que la esfera 2.
            if (esfera1.X < esfera2.X) return true;
            return false;
        }
        void ColisionParedes(int i)
        {
            //i representa la posicion en la lista de esferas de la esfera que colisiono
            if (lista_esferas[i].Center.X > limite_derecha)
                movimientos[i].Velocidad_X *= coeficiente_choque;
            if (lista_esferas[i].Center.X <= limite_izquierda)
                movimientos[i].Velocidad_X *= coeficiente_choque;
            if (lista_esferas[i].Center.Y > limite_arriba)
                movimientos[i].Velocidad_Y *= coeficiente_choque;
            if (lista_esferas[i].Center.Y <= limite_abajo)
                movimientos[i].Velocidad_Y *= coeficiente_choque;
        }
        void VerificaVelocidad()
        {
            for (int i = 0; i < movimientos.Count; i++)
            {
                if (movimientos[i].Velocidad_X < 0.2 && movimientos[i].Velocidad_Y < 0.2)
                {
                    movimientos.RemoveAt(i);
                    modelos.RemoveAt(i);
                    lista_esferas.RemoveAt(i);
                    posiciones.RemoveAt(i);
                }
            }
        }
        #endregion

        #region//Eliminacion de problemas y el cambio de colores.
        void PegadaBorde()
        {
            //Esta es un comprobacion para que la esfera no se quede pegada a los bordes
            for (int i = 0; i < movimientos.Count; i++)
            {
                if (lista_esferas[i].Center.X > limite_derecha + 2)
                    if (movimientos[i].Velocidad_X > 0)
                        posiciones[i] = new Vector3(posiciones[i].X - 4, posiciones[i].Y, 0);

                if (lista_esferas[i].Center.Y > limite_arriba + 2)
                    if (movimientos[i].Velocidad_Y > 0)
                        posiciones[i] = new Vector3(posiciones[i].X, posiciones[i].Y - 4, 0);

                if (lista_esferas[i].Center.X < limite_izquierda - 2)
                    if (movimientos[i].Velocidad_X < 0)
                        posiciones[i] = new Vector3(posiciones[i].X + 4, posiciones[i].Y, 0);

                if (lista_esferas[i].Center.Y < limite_abajo - 2)
                    if (movimientos[i].Velocidad_Y < 0)
                        posiciones[i] = new Vector3(posiciones[i].X, posiciones[i].Y + 4, 0);
            }

        }
        void PegadasEntreSi(int i, int j)
        {
            //Esto es para que no se queden pegadas entre ellas
            float distancia = Vector3.Distance(lista_esferas[i].Center, lista_esferas[j].Center);
            int cuadrante = Cuandrante(i, j);
            while (distancia <= lista_esferas[i].Radius)
            {
                switch (cuadrante)
                {
                    case 1:
                        posiciones[i] = new Vector3(posiciones[i].X + distancia / 2, posiciones[i].Y + distancia / 2, 0);
                        posiciones[j] = new Vector3(posiciones[j].X - distancia / 2, posiciones[j].Y - distancia / 2, 0);
                        break;

                    case 2:
                        posiciones[i] = new Vector3(posiciones[i].X - distancia / 2, posiciones[i].Y + distancia / 2, 0);
                        posiciones[j] = new Vector3(posiciones[j].X + distancia / 2, posiciones[j].Y - distancia / 2, 0);
                        break;

                    case 3:
                        posiciones[i] = new Vector3(posiciones[i].X - distancia / 2, posiciones[i].Y - distancia / 2, 0);
                        posiciones[j] = new Vector3(posiciones[j].X + distancia / 2, posiciones[j].Y + distancia / 2, 0);
                        break;

                    case 4:
                        posiciones[i] = new Vector3(posiciones[i].X + distancia / 2, posiciones[i].Y - distancia / 2, 0);
                        posiciones[j] = new Vector3(posiciones[j].X - distancia / 2, posiciones[j].Y + distancia / 2, 0);
                        break;
                }
                distancia = Vector3.Distance(posiciones[i], posiciones[j]);
            }
        }
        int Cuandrante(int esfera, int referencia)
        {
            //El cuadrante es de la variable esfera, pues referencia, es el centro del origen relativo a las dos esferas

            if (posiciones[esfera].X > posiciones[referencia].X && posiciones[esfera].Y > posiciones[referencia].Y)
                return 1;
            if (posiciones[esfera].X < posiciones[referencia].X && posiciones[esfera].Y > posiciones[referencia].Y)
                return 2;
            if (posiciones[esfera].X < posiciones[referencia].X && posiciones[esfera].Y < posiciones[referencia].Y)
                return 3;

            return 4;
        }
        void CambiaColores()
        {
            blue += suma1;
            green += suma2;
            red += suma3;

            if (blue > 1) suma1 *= -1;
            if (blue < 0) suma1 *= -1;

            if (red > 1) suma3 *= -1;
            if (red < 0) suma3 *= -1;

            if (green > 1) suma2 *= -1;
            if (green < 0) suma2 *= -1;

        }
        #endregion

        void Asigna(int i)
        {
            //Esto es para actualizar las posiciones
            Vector3 res = posiciones[i];
            res.X += movimientos[i].Velocidad_X;
            res.Y += movimientos[i].Velocidad_Y;
            posiciones[i] = res;

            BoundingSphere respaldo = lista_esferas[i];
            respaldo.Center = posiciones[i];
            lista_esferas[i] = respaldo;
        }
    }
}
