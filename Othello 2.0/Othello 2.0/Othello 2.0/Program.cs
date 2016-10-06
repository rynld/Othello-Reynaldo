using System;

namespace Othello_2._0
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PantallaInicial game = new PantallaInicial())
            {
                game.Run();
            }
        }
    }
#endif
}

