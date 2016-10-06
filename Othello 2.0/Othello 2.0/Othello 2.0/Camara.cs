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
    public class Camara : Microsoft.Xna.Framework.GameComponent
    {
        public Camara(Game game, Vector3 posicion)
            : base(game)
        {
            Posicion = posicion;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            CreatePerspectiveFieldOfView();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //Aqui lo que hago es mover la camara por los ejes
            if (Keyboard.GetState().IsKeyDown(Keys.S) && Posicion.Z < 270)
                Posicion += new Vector3(0, 0, 2);
            if (Keyboard.GetState().IsKeyDown(Keys.W) && Posicion.Z > 120)
                Posicion += new Vector3(0, 0, -2);
            if (Keyboard.GetState().IsKeyDown(Keys.A) && Posicion.Y < 170)
                Posicion += new Vector3(0, 2, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.D) && Posicion.Y > -180)
                Posicion += new Vector3(0, -2, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.Q) && Posicion.X > -120)
                Posicion += new Vector3(-2, 0, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.E) && Posicion.X < 120)
                Posicion += new Vector3(2, 0, 0);
            if (Keyboard.GetState().IsKeyDown(Keys.D1))
                Posicion = new Vector3(0, 0, 200);

            CreateLookAt();
            base.Update(gameTime);
        }

        //Metodos auxiliares
        public void CreateLookAt()
        {
            View = Matrix.CreateLookAt(Posicion, Vector3.Zero, Vector3.Up);
        }
        public void CreatePerspectiveFieldOfView()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Game.Window.ClientBounds.Width / (float)Game.Window.ClientBounds.Height, 1, 1000);
        }

        //Propiedades
        public Vector3 Posicion
        { get; set; }
        public Matrix View
        { get; set; }
        public Matrix Projection
        { get; protected set; }
    }
}
