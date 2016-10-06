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
    public class Configuracion : DrawableGameComponent
    {
        #region//Texturas, sonidos etc.
        SpriteBatch sprite;
        SpriteFont fuente;
        SpriteFont pequena;
        Texture2D fondo;
        Texture2D marcado;
        Texture2D recta;
        SoundEffect sonido;
        SoundEffectInstance instancia;
        #endregion

        PantallaInicial pantalla_inicial;//Enalzar esta clase con la clase principal

        #region//Estas variables son para separar los diferentes textos.
        List<string> textos;
        List<string> titulos;
        List<Vector2> vectores;
        List<Rectangle> rectangulos;
        List<Vector2> vec_titulos;
        #endregion

        PantallaInicial.Estados actualestado;

        bool musicafondo;//Es marcar una sola vez las diferentes opciones.
        int dificultad;//Es para los niveles de dificultad
        bool puedes_entrar;//Es para que entre una sola vez a donde los if que la utilizo


        #region//Propiedades
        public int NivelDificultad
        { get; private set; }
        public bool ConJugador
        { get; private set; }
        public bool Sonidomouse
        { get; set; }//Es para saber si el sonido del mouse se activa
        #endregion

        public Configuracion(Game game, PantallaInicial pantalla_inicial)
            : base(game)
        {
            this.pantalla_inicial = pantalla_inicial;
            this.actualestado = PantallaInicial.Estados.PantallaInicial;
            this.textos = new List<string>();
            this.vectores = new List<Vector2>();
            this.rectangulos = new List<Rectangle>();
            this.titulos = new List<string>();
            this.vec_titulos = new List<Vector2>();
            this.musicafondo = true;
            this.ConJugador = true;
            this.NivelDificultad = 1;
            this.Sonidomouse = true;
            this.dificultad = 0;
            this.puedes_entrar = true;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            Crea(new Metodos(CreaTextos), new Metodos(CreaVectores), new Metodos(CreaTitulos), new Metodos(CreaVecTitulos));
            base.Initialize();
        }

        protected override void LoadContent()
        {
            sprite = new SpriteBatch(Game.GraphicsDevice);
            fondo = Game.Content.Load<Texture2D>("Imagenes/mar");
            fuente = Game.Content.Load<SpriteFont>("Fonts/Font");
            pequena = Game.Content.Load<SpriteFont>("Fonts/FontAyuda");
            sonido = Game.Content.Load<SoundEffect>("Musica/tada");
            marcado = Game.Content.Load<Texture2D>("Imagenes/poster");
            recta = Game.Content.Load<Texture2D>("Imagenes/recta");
            instancia = sonido.CreateInstance();

            //Llamo a este metodo aqui porque tengo que esperar que se crea el SpriteFont
            Crea(new Metodos(CreaRectangulos));
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            actualestado = pantalla_inicial.Actual_estado;
            MouseState mouse = Mouse.GetState();
            if (actualestado == PantallaInicial.Estados.Configuracion)
            {
                for (int i = 0; i < rectangulos.Count; i++)
                {
                    if (mouse.X >= rectangulos[i].X && mouse.X <= rectangulos[i].X + rectangulos[i].Width &&
                        mouse.Y >= rectangulos[i].Y && mouse.Y <= rectangulos[i].Y + rectangulos[i].Height &&
                        mouse.LeftButton == ButtonState.Pressed)//Si el click izquierdo es presionado entro.
                    {
                        instancia.Play();

                        //Estos if son para que las opciones se marquen una sola vez.
                        //El uno indica que esta activado, el cero desactivado.
                        //Los indices son en el orden en que yo los declare.
                        if (musicafondo && i == 1) musicafondo = false;
                        else if (!musicafondo && i == 0) musicafondo = true;
                        if (Sonidomouse && i == 3) Sonidomouse = false;
                        else if (!Sonidomouse && i == 2) Sonidomouse = true;
                        if (dificultad == 0 && i == 5) dificultad = 1;
                        else if (dificultad == 1 && i == 4) dificultad = 0;

                        ConJugador = (dificultad == 0);
                        if (ConJugador) NivelDificultad = 0;
                        if ((i == 6 || i == 5) && !ConJugador)
                            NivelDificultad = 1;
                        else if (i == 7 && !ConJugador) NivelDificultad = 2;
                    }
                }
                //Las lineas siguientes controlan la reproduccion de la cancion de fondo
                //mediante la activacion o desactivacion del respectivo boton.
                if (!musicafondo && puedes_entrar)
                {
                    puedes_entrar = false;
                    MediaPlayer.Stop();
                }
                else if (musicafondo && !puedes_entrar)
                {
                    MediaPlayer.Play(Game.Content.Load<Song>("Musica/malaguena"));
                    puedes_entrar = true;
                }
            }
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            if (actualestado == PantallaInicial.Estados.Configuracion)
            {
                sprite.Begin();
                sprite.Draw(fondo, Vector2.Zero, Color.White);
                sprite.Draw(recta, new Vector2(50, 305), Color.White);
                sprite.Draw(recta, new Vector2(50, 60), Color.White);
                sprite.DrawString(fuente, "Camara", new Vector2(40, 400), Color.White);
                sprite.DrawString(fuente, "Q, W, E, A, S, D, 1, ESC", new Vector2(150, 405), Color.White, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0.7f);
                //Este for dibuja los titulos
                for (int i = 0; i < titulos.Count; i++)
                    sprite.DrawString(fuente, titulos[i], vec_titulos[i], Color.White);

                //Este for dibuja las opciones
                int limite = ConJugador ? 2 : 0;//Esto es para que cuando se juegue contra la computadora, se muestren los niveles de dificultad
                for (int i = 0; i < textos.Count - limite; i++)
                    sprite.DrawString(fuente, textos[i], vectores[i], Color.AliceBlue, 0, Vector2.Zero, 0.7f, SpriteEffects.None, 0);

                //Aqui es para dibujar la marca de las opciones una sola vez.
                if (musicafondo) sprite.Draw(marcado, new Vector2(380, 105), Color.White);
                else if (!musicafondo) sprite.Draw(marcado, new Vector2(580, 105), Color.White);
                if (Sonidomouse) sprite.Draw(marcado, new Vector2(380, 166), Color.White);
                else if (!Sonidomouse) sprite.Draw(marcado, new Vector2(580, 166), Color.White);
                if (dificultad == 0) sprite.Draw(marcado, new Vector2(40, 340), Color.White);
                else if (dificultad == 1) sprite.Draw(marcado, new Vector2(280, 340), Color.White);
                if (!ConJugador && NivelDificultad != 2) sprite.Draw(marcado, new Vector2(470, 340), Color.White);
                else if (!ConJugador && NivelDificultad == 2) sprite.Draw(marcado, new Vector2(470, 390), Color.White);
                sprite.End();
            }
            base.Draw(gameTime);
        }

        //Metodos auxiliares

        #region//Creacion de botones, etc

        //Los dos metodos de abajo se encargan de crear los textos y colocar las opciones que se pueden marcar.
        public void CreaTextos()
        {
            textos.Add("Activado");
            textos.Add("Desactivado");
            textos.Add("Activado");
            textos.Add("Desactivado");
            textos.Add("Dos Jugadores");
            textos.Add("Computadora");
            textos.Add("Facil");
            textos.Add("Dificil");

        }
        public void CreaVectores()
        {
            vectores.Add(new Vector2(400, 96));
            vectores.Add(new Vector2(600, 96));
            vectores.Add(new Vector2(400, 156));
            vectores.Add(new Vector2(600, 156));
            vectores.Add(new Vector2(50, 330));
            vectores.Add(new Vector2(300, 330));
            vectores.Add(new Vector2(480, 330));
            vectores.Add(new Vector2(480, 380));
        }
        public void CreaRectangulos()
        {
            for (int i = 0; i < vectores.Count; i++)
                rectangulos.Add(new Rectangle((int)vectores[i].X, (int)vectores[i].Y, (int)fuente.MeasureString(textos[i]).X, (int)fuente.MeasureString(textos[i]).Y));
        }

        //Los dos metodos de abajo se encargar de crear las posiciones y textos de los botones que son para indicar.
        public void CreaTitulos()
        {
            titulos.Add("Audio");
            titulos.Add("Musica de Fondo");
            titulos.Add("Sonido del Mouse");
            titulos.Add("Modos de Juego");
        }
        public void CreaVecTitulos()
        {
            vec_titulos.Add(new Vector2(50, 30));
            vec_titulos.Add(new Vector2(50, 90));
            vec_titulos.Add(new Vector2(50, 150));
            vec_titulos.Add(new Vector2(50, 270));
        }

        void Crea(params Metodos[] metodos)
        {
            //Aqui llamo a todos los metodos.

            for (int i = 0; i < metodos.Length; i++)
            {
                new Thread(new ThreadStart(metodos[i])).Start();
            }

        }
        delegate void Metodos();//Es para el metodo crea
        #endregion

    }
}
