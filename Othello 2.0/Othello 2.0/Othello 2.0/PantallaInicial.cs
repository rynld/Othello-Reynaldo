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
using System.Threading;


namespace Othello_2._0
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PantallaInicial : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        //Aqui se almacenan los estados de dibujo del juego.
        public enum Estados
        { PantallaInicial, Configuracion, Jugando, Ayuda };

        //Estas variables son para las imagenes, sonidos, etc.
        #region
        SpriteBatch sprite;
        SpriteFont fuente;
        Texture2D fondo;
        SoundEffect sound;
        SoundEffectInstance instancia;
        Song cancion;
        #endregion

        //Estas variables son construir los botones.
        #region
        List<string> textos;
        List<Vector2> vectores;
        List<Rectangle> rectangulos;
        #endregion

        int pos_boton; //Esta variable es para obtener la posicion del boton sobre el que esta el mouse
        int anterior;//Esta variable es para saber cuando debo empezar el efecto de sonido.
        bool esta_jugando;//Esta variable es para saber si ya empezo a jugar.

        Contrincante oponente_anterior;//Estas dos variables son para que si el jugador cambia de oponente
        Contrincante oponente_actual;//se reinicie el juego

        //Estas son todas las clases que necesito para establecer una conexion entre todos los gamecomponent.
        Configuracion configuracion;
        Ayuda ayuda;
        Juego juego;

        public Estados Actual_estado
        { get; set; }//El estado de esta propiedad es la que permite que se dibuje lo que le toca.

        public PantallaInicial()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            IsMouseVisible = true;

            this.textos = new List<string>();
            this.vectores = new List<Vector2>();
            this.rectangulos = new List<Rectangle>();
            this.Actual_estado = Estados.PantallaInicial;
            this.pos_boton = -1;
            this.anterior = -2;
            this.esta_jugando = false;
            this.configuracion = new Configuracion(this, this);
            this.ayuda = new Ayuda(this, this);
            this.juego = new Juego(this, configuracion.ConJugador, configuracion.NivelDificultad, this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            if (configuracion.ConJugador)
            {
                oponente_anterior = Contrincante.Jugador;
                oponente_actual = Contrincante.Jugador;
            }
            else
            {
                oponente_anterior = Contrincante.Jugador_Virtual;
                oponente_actual = Contrincante.Jugador_Virtual;
            }

            Components.Add(configuracion);
            Components.Add(ayuda);
            Components.Add(juego);
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            sprite = new SpriteBatch(GraphicsDevice);

            fondo = Content.Load<Texture2D>("Imagenes/bombilla");
            fuente = Content.Load<SpriteFont>("Fonts/Font");
            sound = Content.Load<SoundEffect>("Musica/CLICK1");
            cancion = Content.Load<Song>("Musica/malaguena");

            Crea(new Metodos(CreaString),new Metodos(CreaVectores),new Metodos(CreaRectangulos));

            this.instancia = sound.CreateInstance();
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(cancion);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.Escape)) Actual_estado = Estados.PantallaInicial;//Esto es para salir a la pantalla inicial
            if (Actual_estado == Estados.Jugando) esta_jugando = true;

            //Aqui hay todo lo relacionado con el manejo de la pantalla principal
            #region
            if (Actual_estado == Estados.PantallaInicial)
            {
                if (oponente_actual != oponente_anterior)
                    CreaNuevoJuego(configuracion.ConJugador, configuracion.NivelDificultad);

                //Si el mouse esta sobre algun letrero, marcarlo.
                MouseState mouse = Mouse.GetState();
                for (int i = 0; i < rectangulos.Count; i++)
                {
                    //Las sig dos lineas son para diferenciar cual boton debo marcar(En el caso de los dos primeros elementos de la variable textos)
                    if (!esta_jugando && i == 0) continue;
                    else if (!esta_jugando && i == 0) continue;
                    if (mouse.X >= rectangulos[i].X && mouse.X <= rectangulos[i].X + rectangulos[i].Width &&
                        mouse.Y >= rectangulos[i].Y && mouse.Y <= rectangulos[i].Y + rectangulos[i].Height)
                    {
                        pos_boton = i;

                        if (anterior != pos_boton && configuracion.Sonidomouse)
                        {
                            instancia.Play();
                            anterior = i;
                        }
                        //Esto es por si se da click sobre el boton salir.
                        if (pos_boton == rectangulos.Count - 1 && mouse.LeftButton == ButtonState.Pressed) Exit();
                        //Esto es para cuando se de click sobre el boton Ayuda
                        if (pos_boton == 2 && mouse.LeftButton == ButtonState.Pressed)
                            Actual_estado = Estados.Ayuda;
                        //Esto es cuando se de click sobre Nuevo o Continuar.
                        if (pos_boton == 0 && mouse.LeftButton == ButtonState.Pressed)
                            Actual_estado = Estados.Jugando;
                        if (pos_boton == 1 && mouse.LeftButton == ButtonState.Pressed)
                        {
                            if (esta_jugando)//Es para cuando se de empezar y ya se este jugando
                                CreaNuevoJuego(configuracion.ConJugador, configuracion.NivelDificultad);
                            Actual_estado = Estados.Jugando;
                        }
                        //Esto es cuando se de click sobre el boton Configuracion
                        if (pos_boton == 3 && mouse.LeftButton == ButtonState.Pressed)
                            Actual_estado = Estados.Configuracion;

                        break;
                    }
                    else pos_boton = -1;

                }
            }
            #endregion

            //Aqui es para saber si debo reiniciar el juego.
            #region
            if (Actual_estado == Estados.Configuracion)
            {
                if (configuracion.ConJugador && oponente_actual == Contrincante.Jugador_Virtual)
                {
                    oponente_actual = Contrincante.Jugador;
                    oponente_anterior = Contrincante.Jugador_Virtual;
                }
                else if (!configuracion.ConJugador && oponente_actual == Contrincante.Jugador)
                {
                    oponente_actual = Contrincante.Jugador_Virtual;
                    oponente_anterior = Contrincante.Jugador;
                }
            }
            else oponente_anterior = oponente_actual;
            #endregion

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.AliceBlue);
            if (Actual_estado == Estados.PantallaInicial)
            {
                sprite.Begin();
                sprite.Draw(fondo, Vector2.Zero, Color.White);
                sprite.DrawString(fuente, "Reynaldo Cruz. C-123", new Vector2(550, 570), Color.Black, 0, Vector2.Zero, 0.8f, SpriteEffects.None, 1);
                for (int i = 0; i < textos.Count; i++)
                {
                    if (!esta_jugando && i == 0) continue;
                    if (i == pos_boton)
                        sprite.DrawString(fuente, textos[i], vectores[i], Color.Yellow, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0.5f);
                    else
                        sprite.DrawString(fuente, textos[i], vectores[i], Color.White);
                }
                sprite.End();
            }
            base.Draw(gameTime);
        }

        //Metodos auxiliares
        #region//Estos metodos son los encargados de la creacion de los botones.
        void CreaString()
        {
            //Aqui creo los botones de la pantalla inicial
            textos.Add("Continuar");
            textos.Add("Empezar");
            textos.Add("Ayuda");
            textos.Add("Configuracion");
            textos.Add("Salir");
        }
        void CreaVectores()
        {
            //Aqui le asigno la posicion a los botones en la pantalla
            vectores.Add(new Vector2(100, 50));
            vectores.Add(new Vector2(100, 100));
            vectores.Add(new Vector2(100, 150));
            vectores.Add(new Vector2(100, 200));
            vectores.Add(new Vector2(100, 250));
        }
        void CreaRectangulos()
        {
            //Los rectangulos son para que los botones tengan un area determinada
            for (int i = 0; i < textos.Count; i++)
            {
                Vector2 dimension = fuente.MeasureString(textos[i]);
                rectangulos.Add(new Rectangle((int)vectores[i].X, (int)vectores[i].Y, (int)dimension.X, (int)dimension.Y));
            }

        }
        void Crea(params Metodos[] metodos)
        {
            //Aqui llamo a todos los metodos.

            for (int i = 0; i < metodos.Length; i++)
            {
                Thread hilo = new Thread(new ThreadStart(metodos[i]));
                hilo.Start();
                hilo.Join();
            }

        }
        delegate void Metodos();
        #endregion

        public void CreaNuevoJuego(bool juga_es_opo, int nivel_dif)
        {
            //Este metodo reinicia todos los valores necesarios para crear una nueva instancia de la clase juego.
            Components.RemoveAt(Components.Count - 3);//Elimino el gamecomponent fisica
            Components.RemoveAt(Components.Count - 2);//Elimino la camara que se crea con la clase juego
            Components.RemoveAt(Components.Count - 1);//Elimino al gamecomponent juego
            esta_jugando = false;
            juego = new Juego(this, configuracion.ConJugador, configuracion.NivelDificultad, this);
            Components.Add(juego);
        }
    }

    enum Contrincante
    { Jugador, Jugador_Virtual }
}
