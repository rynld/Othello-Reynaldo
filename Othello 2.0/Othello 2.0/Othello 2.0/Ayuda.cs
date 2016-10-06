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


namespace Othello_2._0
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Ayuda : DrawableGameComponent//Esta clase esta bien
    {
        //Estas varibles contienen lo necesario para dibujar los textos.
        SpriteBatch sprite;
        SpriteFont fuente;
        SpriteFont fuente_ayuda;
        Texture2D imagen;
        string texto1;//Los textos que voy a escribir
        string texto2;

        PantallaInicial pantalla_inicial;//Esto es un enlace con la clase principal

        public PantallaInicial.Estados actualestado
        { get; set; }

        public Ayuda(Game game, PantallaInicial pantalla_inicial)
            : base(game)
        {
            this.pantalla_inicial = pantalla_inicial;
            this.texto1 = "El reversi, Othello o Yang es un juego  entre dos personas, que comparten 64 fichas iguales, de caras distintas, que se van colocando por turnos en un tablero dividido en 64 escaques. Las caras de las fichas se distinguen por su color y cada jugador tiene asignado uno de esos colores, ganando quien tenga mas fichas sobre el tablero al finalizar la partida. Se clasifica como juego de tablero, abstracto y territorial; al igual que el go y las amazonas.";
            this.texto2 = "La movilidad media de un jugador a lo largo de la partida es de 8 movimientos. Como en total se pueden hacer 60 movimientos, el numero maximo de posibles partidas es de aproximadamente 10^54. Por otra parte, el numero maximo de posiciones posibles se calcula aproximadamente en 10^30.";
            this.actualestado = PantallaInicial.Estados.PantallaInicial;

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            texto1 = ComoParrafo(texto1);
            texto2 = ComoParrafo(texto2);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            this.sprite = new SpriteBatch(Game.GraphicsDevice);
            this.imagen = Game.Content.Load<Texture2D>("Imagenes/libro");
            this.fuente = Game.Content.Load<SpriteFont>("Fonts/Font");
            this.fuente_ayuda = Game.Content.Load<SpriteFont>("Fonts/FontAyuda");
            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            actualestado = pantalla_inicial.Actual_estado;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (actualestado == PantallaInicial.Estados.Ayuda)
            {
                sprite.Begin();
                sprite.Draw(imagen, Vector2.Zero, Color.White);
                sprite.DrawString(fuente_ayuda, texto1, new Vector2(20, 10), Color.White);
                sprite.DrawString(fuente_ayuda, texto2, new Vector2(20, 200), Color.White);
                sprite.End();
            }
            base.Draw(gameTime);
        }

        //Metodos Auxiliares
        string ComoParrafo(string cadena)
        {

            int longitud = 0;//Esto es para a cada cierta distancia agregar el caracter de salto de linea
            //Estos for son para que el texto se escriba como un parrafo y no como una linea.
            for (int i = 0; i < cadena.Length; i++)
            {
                if (longitud++ > 65 && i >= 65 && cadena[i] == ' ')
                {
                    longitud = 0;
                    cadena = cadena.Insert(i, "\n" + " ");

                }
            }
            return cadena;
        }
    }
}
