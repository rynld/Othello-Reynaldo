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
using Tablero;
using System.Threading;


namespace Othello_2._0
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Juego : DrawableGameComponent
    {
        //Variables de la clase
        #region
        //Esta clase controla la esencia del juego
        PantallaInicial.Estados actualestado;//Todos los diferentes estados son para saber que es lo que tengo que dibujar
        PantallaInicial entrada_juego;//Esto es para enlazar la clase principal con esta

        Camara camara;
        //Estas clases son para almacenar las fichas y el tablero, etc.
        #region
        SpriteBatch spriteBatch;
        SpriteFont font;
        List<Fichas> lista_fichas;//Aqui se almacenan todas las fichas que estan en el tablero
        Tablero.Tablero tablero;
        Model superficie;
        Model ficha;
        Model mantel;
        Model piso;
        BoundingBox box;//Esto es para saber cuales son los puntos de la superficie donde se juega.
        Rectangle rectangulo;//Es para poder obtner toda las coordenadas de los puntos.
        Fisica fisica;//Controla el movimiento de las pelotas
        Thread primer_hilo;
        Thread seg_hilo;
        #endregion

        bool jueganblancas;//Es para saber quien le toca jugar.
        int cuantas_entro;//Es para que solo entre una vez al update.
        bool obten_posiciones_blancas;//Esta variable es para no calcular las posiciones cada vez que se entre al Update

        List<Vector3> posiciones;//Aqui se almacenan las coordenadas de las fichas
        List<List<int>> posibles_posiciones;//Aqui se almacenan las posibles posiciones donde cada jugados puede "jugar".

        bool contra_jugador;
        int nivel_dificultad;//1 indica facil, 2 indica dificil.

        bool gira_fichas;//Es para saber si las fichas tienen que virarse
        float angulo_giro_completo;

        float angulo_semigiro;
        bool sumar;//Esta variable es la que decide cuando sumar un valor al angulo de semigiro.

        Vector3 origen;//Posicion de donde las fichas salen
        Vector3 mov_actual;//Poscion actual de las fichas que estan volando;
        Vector3 pos;//Coordenadas del raton en el mundo
        float an_rotacion;//Es el angulo de rotacion cuando las fichas vuelan.
        bool vuela;//Es para saber cuando la ultima ficha que agregue debe volar.
        Traslacion traslado;//Es para saber cuando debo dibujar las fichas que se van a virar.

        bool ya_termine;//Es para saber cuando se acabo el juego.
        bool ya_solto;//Para que no juegue cuando deje el clcik apretado.

        #endregion

        #region//Esto es nuevo
        Texture2D textura;
        Effect efectos;//Es para simular la luz difusa
        float coordenadas;//Indica la posicion del vector director de la luz difusa
        bool suma;//Es para mantener el vector director de la luz difusa en un rango
        #endregion

        public Juego(Game game, bool contra_jugador, int nivel_dificultad, PantallaInicial entrada_juego)
            : base(game)
        {
            this.entrada_juego = entrada_juego;
            this.camara = new Camara(game, new Vector3(0, 0, 200));
            this.lista_fichas = new List<Fichas>();
            this.posiciones = new List<Vector3>();
            this.posibles_posiciones = new List<List<int>>();
            this.cuantas_entro = 1;
            this.jueganblancas = true;
            this.obten_posiciones_blancas = true;
            this.contra_jugador = contra_jugador;
            this.nivel_dificultad = nivel_dificultad;
            this.tablero = new Tablero.Tablero();
            this.gira_fichas = false;
            this.angulo_giro_completo = 0;
            this.angulo_semigiro = 0;
            this.sumar = true;
            this.origen = new Vector3(250, 0, 6);//6 es la distancia de las fichas en el eje z
            this.mov_actual = origen;
            this.an_rotacion = 0;
            this.vuela = false;
            this.ya_solto = true;
            this.ya_termine = false;
            this.traslado = Traslacion.Fijo;
            this.fisica = new Fisica(game, 10, camara);

            this.coordenadas = 0.3f;
            this.suma = true;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            Game.Components.Add(camara);
            Game.Components.Add(fisica);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            ficha = Game.Content.Load<Model>("Modelos/ficha");
            #region//Creacion de las cuatro primeras fichas
            lista_fichas.Add(new Fichas(ficha, new Vector3(-5, 5, 6), Tablero.Colores.Blanco, 3, 3));
            lista_fichas.Add(new Fichas(ficha, new Vector3(5, -5, 6), Tablero.Colores.Blanco, 4, 4));
            lista_fichas.Add(new Fichas(ficha, new Vector3(5, 5, 6), Tablero.Colores.Oscuro, 3, 4));
            lista_fichas.Add(new Fichas(ficha, new Vector3(-5, -5, 6), Tablero.Colores.Oscuro, 4, 3));
            lista_fichas[0].Vira();
            lista_fichas[1].Vira();
            #endregion

            spriteBatch = new SpriteBatch(GraphicsDevice);
            superficie = Game.Content.Load<Model>("Modelos/tablero");
            rectangulo = ObtenSuperficie();
            mantel = Game.Content.Load<Model>("Modelos/mantel");
            piso = Game.Content.Load<Model>("Modelos/a");
            font = Game.Content.Load<SpriteFont>("Fonts/Font");
            efectos = Game.Content.Load<Effect>("DifusseLight");
            textura = Game.Content.Load<Texture2D>("Imagenes/madera");

            base.LoadContent();
        }
        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        public override void Update(GameTime gameTime)
        {
            if (fisica.Pelotas)
            {
                actualestado = PantallaInicial.Estados.PantallaInicial;
                entrada_juego.Actual_estado = actualestado;
            }
            else
                actualestado = entrada_juego.Actual_estado;

            /*gira_ficha esta en el if para que no se puede jugar cuandos se esta girando
            *esto es para evitar movimientos incorrectos*/
            if (actualestado == PantallaInicial.Estados.Jugando && !gira_fichas)
            {
                #region//Esto es para salir a la pantalla principal
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    actualestado = PantallaInicial.Estados.PantallaInicial;
                    entrada_juego.Actual_estado = actualestado;
                }
                #endregion

                #region //Aqui se calculan las posiciones, con sus respectivas fichas a virar.

                if (tablero.FichasBlancas == 0 || tablero.FichasNegras == 0)
                    posibles_posiciones = new List<List<int>>();
                else
                {
                    if (jueganblancas && obten_posiciones_blancas)
                    {
                        primer_hilo = new Thread(new ThreadStart(Jugadas));
                        primer_hilo.Start();
                        primer_hilo.Join();

                        ya_termine = true;//Esto es para que solo se dibuje el cartel final cuando no sea posible jugar en niguna posicion.
                        obten_posiciones_blancas = false;

                        if (posibles_posiciones.Count == 0)
                            jueganblancas = false;
                    }
                    if (!jueganblancas && !obten_posiciones_blancas)
                    {
                        seg_hilo = new Thread(new ThreadStart(Jugadas));
                        seg_hilo.Start();
                        seg_hilo.Join();

                        ya_termine = true;//Esto es para que solo se dibuje el cartel final cuando no sea posible jugar en niguna posicion.
                        if (posibles_posiciones.Count == 0)
                        {
                            jueganblancas = true;
                            obten_posiciones_blancas = true;
                        }

                    }
                }

                #endregion

                #region //Aqui se se hace todo lo relacionado con el cambio de las fichas.
                #region //Aqui es jugador contra jugador
                MouseState estado_raton = Mouse.GetState();
                if (estado_raton.LeftButton == ButtonState.Pressed && cuantas_entro++ == 1 && ya_solto)
                {
                    ya_solto = false;
                    int fila;
                    int columna;
                    pos = VectorChoque(estado_raton);
                    CalculaPosiciones(pos, out fila, out columna);
                    pos = CalculaCoordenadas(fila, columna);
                    if (fila >= 0 && fila < tablero.Board.GetLength(0) && columna >= 0 && columna < tablero.Board.GetLength(1))
                    {
                        List<int> fichas_a_virar = BuscaPosiciones(posibles_posiciones, fila, columna);

                        if (jueganblancas)
                        {
                            if (posibles_posiciones.Count != 0 && fichas_a_virar.Count != 0)
                                jueganblancas = false;//Si se puede jugar hago el cambio

                            if (Juega(fichas_a_virar, fila, columna, pos, Tablero.Colores.Blanco))
                                gira_fichas = true;//Si es posible jugar entonces hago que las fichas giren
                        }
                        else
                        {
                            if (posibles_posiciones.Count != 0 && fichas_a_virar.Count != 0)
                            {
                                obten_posiciones_blancas = true;
                                jueganblancas = true;
                            }
                            if (Juega(fichas_a_virar, fila, columna, pos, Tablero.Colores.Oscuro))
                                gira_fichas = true;
                        }

                    }
                }
                #endregion

                #region//La computadora siempre va a jugar con las fichas negras.
                if (!contra_jugador && !jueganblancas && !obten_posiciones_blancas && !gira_fichas)
                {

                    obten_posiciones_blancas = true;
                    jueganblancas = true;
                    #region
                    if (nivel_dificultad == 1)
                    {
                        Random r = new Random();
                        int pos_fic = r.Next(posibles_posiciones.Count);
                        List<int> a_virar = posibles_posiciones[pos_fic];

                        if (Juega(a_virar)) gira_fichas = true;
                    }
                    #endregion
                    #region
                    if (nivel_dificultad == 2)
                    {
                        int index = 0;
                        for (int i = 0; i < posibles_posiciones.Count; i++)//Aqui obtengo la mayor cantidad de jugadas.
                            if (posibles_posiciones[i].Count >= posibles_posiciones[index].Count) index = i;

                        List<int> a_virar = posibles_posiciones[index];
                        if (Juega(a_virar)) gira_fichas = true;
                    }
                    #endregion

                }
                #endregion
                if (estado_raton.LeftButton == ButtonState.Released)
                {
                    cuantas_entro = 1;//Esto es para que solo entre una sola vez.
                    ya_solto = true;
                }
                #endregion

                new Thread(new ThreadStart(tablero.Cuenta)).Start();
            }
            base.Update(gameTime);
        }


        public override void Draw(GameTime gameTime)
        {
            if (actualestado == PantallaInicial.Estados.Jugando)
            {
                #region//Dibujo de las fichas
                Rotacion();//Estos metodos estan aqui para que solo cambie el angulo de giro cuando se va a pintar
                AnguloRotacion();//No los pongo en el update porque solo necesito que ellos cambien cuando voy a pintar
                if (vuela) Traslada(pos, origen, jueganblancas);
                for (int i = 0; i < lista_fichas.Count; i++)
                {
                    foreach (ModelMesh mallas in lista_fichas[i].modelo.Meshes)
                    {
                        foreach (BasicEffect efectos in mallas.Effects)
                        {
                            efectos.View = camara.View;
                            efectos.Projection = camara.Projection;
                            if (i == lista_fichas.Count - 1 && vuela)//Para que solo vuele la ultima
                                efectos.World = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(0.3f) * Matrix.CreateRotationX(an_rotacion += 0.05f) * Matrix.CreateTranslation(mov_actual);
                            else
                            {
                                if (lista_fichas[i].HayQueVirar)//Simula el giro completo
                                    efectos.World = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationX(angulo_giro_completo) * lista_fichas[i].World;
                                else if (jueganblancas && lista_fichas[i].Color == Tablero.Colores.Blanco && !lista_fichas[i].HayQueVirar)//Simula el semigiro
                                    efectos.World = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationX(angulo_semigiro) * lista_fichas[i].World;
                                else if (!jueganblancas && lista_fichas[i].Color == Tablero.Colores.Oscuro && !lista_fichas[i].HayQueVirar)//Simula el semigiro
                                    efectos.World = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateRotationX(angulo_semigiro) * lista_fichas[i].World;
                                else
                                    efectos.World = lista_fichas[i].World;
                            }
                        }
                        mallas.Draw();
                    }
                }
                #endregion

                #region//Aqui se dibuja todo menos las fichas
                foreach (ModelMesh mallas in superficie.Meshes)
                {
                    foreach (BasicEffect efectos in mallas.Effects)
                    {
                        efectos.View = camara.View;
                        efectos.Projection = camara.Projection;
                        efectos.World = Matrix.Identity;
                    }
                    mallas.Draw();
                }

                foreach (ModelMesh mallas in mantel.Meshes)
                {
                    //El mantel se dibuja en la posicion correcta
                    //La escala del mantel debe ser de 0.01;
                    foreach (BasicEffect efectos in mallas.Effects)
                    {
                        efectos.View = camara.View;

                        efectos.Projection = camara.Projection;
                        efectos.World = Matrix.Identity * Matrix.CreateScale(0.01f) * Matrix.CreateTranslation(new Vector3(0, 0, 5));
                    }
                    mallas.Draw();
                }


                if (suma) coordenadas += 0.0003f;
                else coordenadas -= 0.0003f;
                if (coordenadas > 1.5f) suma = false;
                if (coordenadas < 0.3f) suma = true;
                foreach (ModelMesh mallas in piso.Meshes)
                {
                    //Esto dibuja el piso, hay que escalarlo en 3
                    //Solo hice lo de los shader con este modelo, porque en los demas la textura esta dentro del mismo
                    //modelo, y no se como acceder a ella
                    foreach (ModelMeshPart partes in mallas.MeshParts)
                    {
                        partes.Effect = efectos;
                        efectos.Parameters["View"].SetValue(camara.View);
                        efectos.Parameters["Projection"].SetValue(camara.Projection);
                        efectos.Parameters["World"].SetValue(Matrix.Identity * Matrix.CreateScale(3f) * Matrix.CreateTranslation(new Vector3(0, 0, -20)));
                        efectos.Parameters["ModelTexture"].SetValue(textura);
                        efectos.Parameters["DiffuseLightDirection"].SetValue(new Vector3(coordenadas, coordenadas, 0));
                        efectos.Parameters["AmbientColor"].SetValue(Color.Black.ToVector4());
                        efectos.Parameters["AmbientIntensity"].SetValue(1);
                        efectos.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(mallas.ParentBone.Transform)));
                        efectos.Parameters["DiffuseColor"].SetValue(Color.White.ToVector4());
                        efectos.Parameters["DiffuseIntensity"].SetValue(1);
                    }
                    mallas.Draw();
                }
               
                #endregion

                #region//Dibujo de los textos
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                spriteBatch.DrawString(font, "Blancas" + tablero.FichasBlancas.ToString(), new Vector2(15, 13), Color.White, 0, Vector2.Zero, jueganblancas ? 1.2f : 1, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, "Oscuras " + tablero.FichasNegras.ToString(), new Vector2(15, 40), Color.White, 0, Vector2.Zero, !jueganblancas ? 1.2f : 1, SpriteEffects.None, 0);

                if (posibles_posiciones.Count == 0 && ya_termine)
                {
                    if (tablero.FichasNegras == tablero.FichasBlancas)
                        spriteBatch.DrawString(font, "Empate", new Vector2((GraphicsDevice.Viewport.Width / 2) - 80, GraphicsDevice.Viewport.Height / 2), Color.Snow, angulo_semigiro, new Vector2(70, 5), 1, SpriteEffects.None, 0.5f);
                    else
                        spriteBatch.DrawString(font, tablero.FichasBlancas > tablero.FichasNegras ? "Blancas Ganaron" : "Oscuras Ganaron", new Vector2((GraphicsDevice.Viewport.Width / 2) - 80, GraphicsDevice.Viewport.Height / 2), Color.Snow, angulo_semigiro, new Vector2(70, 5), 1, SpriteEffects.None, 0.5f);
                }
                spriteBatch.End();
                #endregion
            }

            base.Draw(gameTime);
        }

        //Estos son metodos auxiliares

        public void Jugadas()
        {
            posibles_posiciones = tablero.CalculaJugadas(jueganblancas ? Colores.Oscuro : Colores.Blanco, ColoresFichas(), PosicionesFichas());
        }

        Rectangle ObtenSuperficie()
        {
            //El rectangulo que se devuelve es el rectangulo donde se ponen las fichas.
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[superficie.Meshes[0].MeshParts[0].VertexBuffer.VertexCount];
            superficie.Meshes[0].MeshParts[0].VertexBuffer.GetData<VertexPositionNormalTexture>(vertices);
            Vector3[] vertexs = new Vector3[vertices.Length];
            for (int index = 0; index < vertexs.Length; index++)
                vertexs[index] = vertices[index].Position;//Aqui solo guardo la posicion de cada vertices
            box = BoundingBox.CreateFromPoints(vertexs);
            return new Rectangle((int)box.Min.X, (int)box.Max.Y, (int)Math.Abs(box.Max.X - box.Min.X), (int)Math.Abs(box.Min.Y - box.Max.Y));
        }

        bool Juega(List<int> rota_pos, int fila, int columna, Vector3 coordenadas, Tablero.Colores color)
        {
            //En los for voy cambiando los colores de las fichas que vire.
            if (rota_pos != null && rota_pos.Count > 0)
            {
                for (int k = 2; k < rota_pos.Count; k += 2)
                {
                    for (int j = 0; j < lista_fichas.Count; j++)
                    {
                        if (lista_fichas[j].Fila == rota_pos[k] && lista_fichas[j].Columna == rota_pos[k + 1])
                        {
                            lista_fichas[j].Color = color;
                            tablero.Board[lista_fichas[j].Fila, lista_fichas[j].Columna] = lista_fichas[j].Color;
                            lista_fichas[j].HayQueVirar = true;
                        }
                    }
                }


                tablero.Board[fila, columna] = color;
                vuela = true;
                lista_fichas.Add(new Fichas(ficha, origen, color, fila, columna));

                return true;
            }
            return false;

        }
        bool Juega(List<int> rota_pos)
        {
            //Lo mismo, pero este es para la computadora
            if (rota_pos != null && rota_pos.Count > 0)
            {
                for (int k = 2; k < rota_pos.Count; k += 2)
                {
                    for (int j = 0; j < lista_fichas.Count; j++)
                    {
                        if (lista_fichas[j].Fila == rota_pos[k] && lista_fichas[j].Columna == rota_pos[k + 1])
                        {
                            lista_fichas[j].HayQueVirar = true;
                            lista_fichas[j].Color = Tablero.Colores.Oscuro;
                            tablero.Board[lista_fichas[j].Fila, lista_fichas[j].Columna] = lista_fichas[j].Color;
                        }
                    }
                }
                tablero.Board[rota_pos[0], rota_pos[1]] = Tablero.Colores.Oscuro;
                lista_fichas.Add(new Fichas(ficha, CalculaCoordenadas(rota_pos[0], rota_pos[1]), Tablero.Colores.Oscuro, rota_pos[0], rota_pos[1]));
                return true;
            }
            return false;
        }

        #region//Traslacion y rotacion
        void AnguloRotacion()
        {
            //Aqui simulo el semigiro de las fichas que le tocan jugar.
            if (sumar)
                angulo_semigiro += 0.05f;
            else
                angulo_semigiro -= 0.05f;
            if (angulo_semigiro > 0.6) sumar = false;
            if (angulo_semigiro < -0.6) sumar = true;
        }
        void Rotacion()
        {
            //Aqui calculo el giro completo de las fichas que hay que virar.
            if (angulo_giro_completo <= 3.14 && gira_fichas)
                angulo_giro_completo += 0.02f;
            if (angulo_giro_completo > 3.14)
            {
                gira_fichas = false;
                for (int i = 0; i < lista_fichas.Count; i++)
                    if (lista_fichas[i].HayQueVirar) lista_fichas[i].Vira();//Esta linea es para que la ficha que se viro tenga su nueva matriz de posicion

                angulo_giro_completo = 0;
            }

        }
        void Traslada(Vector3 objetivo, Vector3 salida, bool juegan_blancas)
        {
            //Aqui simulo que la ficha se mueve por una circusnferencia

            float velo_y = (salida.Y - objetivo.Y) / 120;//Velocidad en que se mueve por cada eje.
            float velo_x = (salida.X - objetivo.X) / 120;
            float radio = (salida.X - objetivo.X) / 2;//radio de la circunsferencia
            mov_actual.Z = (float)Math.Sqrt(Math.Pow(radio, 2) - Math.Pow(mov_actual.X - (salida.X + objetivo.X) / 2, 2));//Ecuacion de la circunsferencia
            mov_actual.Y -= velo_y;
            if (mov_actual.X > objetivo.X) mov_actual.X -= velo_x;
            else
            {
                mov_actual = origen;//Reinicio los valores.
                vuela = false;
            }

            if (mov_actual.X <= objetivo.X) traslado = Traslacion.Tras_finalizada;
            else traslado = Traslacion.Trasladando;

            if (traslado == Traslacion.Tras_finalizada)
            {
                lista_fichas[lista_fichas.Count - 1].World = Matrix.CreateScale(0.15f) * Matrix.CreateTranslation(objetivo);
                if (!jueganblancas)
                    lista_fichas[lista_fichas.Count - 1].Vira();
            }

        }
        #endregion

        #region//Vectores y coordenadas
        void CalculaPosiciones(Vector3 choque, out int fila, out int columna)
        {
            //Aqui es para determinar la fila y la columna en que se encuentra la ficha.
            float ancho = rectangulo.Width / 8;
            float largo = rectangulo.Height / 8; ;
            float suma = rectangulo.X;
            int contador = 0;
            while (suma <= choque.X)
            {
                suma += ancho;
                contador++;
            }
            columna = contador - 1;
            suma = rectangulo.Y;
            contador = 0;
            while (suma >= choque.Y)
            {
                suma -= largo;
                contador++;
            }
            fila = contador - 1;
        }
        Vector3 CalculaCoordenadas(int fila, int columna)
        {
            //Aqui es para dada una fila y una columna obtener las coordenadas de la ficha
            int origen_x = rectangulo.X;
            int origen_y = rectangulo.Y;
            int ancho = (int)(rectangulo.Width / 8);
            int largo = (int)(rectangulo.Height / 8);
            int contador = 0;
            Vector3 pos = new Vector3();
            while (contador++ <= fila)
                origen_y -= largo;

            contador = 0;

            while (contador++ <= columna)
                origen_x += ancho;

            pos.Y = origen_y + 5;
            pos.X = origen_x - 5;
            pos.Z = 6;
            return pos;
        }
        Vector3 VectorChoque(MouseState estado)
        {
            //Aqui se calculan las coordenadas del choque con el tablero
            Vector3 ini = GraphicsDevice.Viewport.Unproject(new Vector3(estado.X, estado.Y, 0), camara.Projection, camara.View, Matrix.Identity);
            Vector3 fin = GraphicsDevice.Viewport.Unproject(new Vector3(estado.X, estado.Y, 1), camara.Projection, camara.View, Matrix.Identity);
            Vector3 direccion = fin - ini;
            direccion.Normalize();


            Ray rayo = new Ray(ini, direccion);
            float? distancia = rayo.Intersects(box);
            return distancia == null ? new Vector3(0, 0, 0) : ((distancia.Value * direccion) + ini);
        }
        #endregion

        #region//Metodos para la clase Tablero
        List<int> BuscaPosiciones(List<List<int>> posiciones, int fila, int columna)
        {
            /*Aqui se itera por posiciones y se buscan la lista donde el primer y segundo elemento de la lista
            *representa la fila y la columna. Y en los demas elementos de la lista un numeri par y su sucesor representa
             una fila y una columna*/

            List<int> totales_pos = new List<int>();

            int pos_lista = -1;
            for (int i = 0; i < posiciones.Count; i++)
            {
                if (posiciones[i][0] == fila && posiciones[i][1] == columna)
                {
                    pos_lista = i;
                    break;
                }
            }
            if (pos_lista != -1)
            {
                for (int i = 0; i < posiciones[pos_lista].Count; i++)
                    totales_pos.Add(posiciones[pos_lista][i]);
            }
            return totales_pos;


        }
        List<int> PosicionesFichas()
        {
            //Estas posiciones son para la clase tablero.
            //Los elementos de la forma 2*n son las filas, los elemtentos de la forma 2*n +1 son columnas
            //Un numero par y su sucesor es una posicion valida.
            List<int> listaposiciones = new List<int>();
            for (int i = 0; i < lista_fichas.Count; i++)
            {
                listaposiciones.Add(lista_fichas[i].Fila);
                listaposiciones.Add(lista_fichas[i].Columna);

            }
            return listaposiciones;
        }
        List<Tablero.Colores> ColoresFichas()
        {
            //Aqui se obtienen los colores de cada ficha, que son los necesarios para 
            //que la clase tablero haga todos sus calculos.
            List<Tablero.Colores> colores = new List<Tablero.Colores>();
            for (int i = 0; i < lista_fichas.Count; i++)
                colores.Add(lista_fichas[i].Color);
            return colores;
        }
        #endregion

        enum Traslacion
        { Fijo, Trasladando, Tras_finalizada }


    }
}
