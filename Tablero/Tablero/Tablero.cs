using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tablero
{
    public class Tablero
    {
        public Colores[,] Board
        { get; set; }
        //Estas propiedades van a guardar la cantidad de fichas de cada color.
        public int FichasBlancas
        { get; private set; }
        public int FichasNegras
        { get; private set; }

        int[] pos_fila;
        int[] pos_col;

        public Tablero()
        {
            this.Board = new Colores[8, 8];
            Board[3, 3] = Colores.Blanco;
            Board[4, 4] = Colores.Blanco;
            Board[3, 4] = Colores.Oscuro;
            Board[4, 3] = Colores.Oscuro;
            pos_fila = new int[] { -1, -1, 0, 1, 1, 1, 0, -1 };
            pos_col = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
        }

        public void Cuenta()
        {
            //Este metodo cuenta la cantidad  de fichas negras y blancas.
            int blancas = 0;
            int negras = 0;
            for (int i = 0; i < Board.GetLength(0); i++)
            {
                for (int j = 0; j < Board.GetLength(1); j++)
                {
                    if (Board[i, j] == Colores.Blanco) blancas++;
                    else if (Board[i, j] == Colores.Oscuro) negras++;
                }
            }
            FichasBlancas = blancas;
            FichasNegras = negras;
        }


        public List<List<int>> CalculaJugadas(Colores color, List<Colores> colores, List<int> fichasposiciones)
        {
            //La lista de fichas va a contener las posiciones.Primero van las filas y despues van las columnas.
            //La lista de colores va a contener los colores de cada ficha.
            //Este metodo va a calcular las posibles posiciones donde un jugador puede jugar.En la lista primero va la fila y despues la columna.
            //Un numero par y su sucesor es una posicion.
            #region
            /*Esta es la variable principal, pues va a almacenar cada posicion en la que se puede jugar,
             * y por cada posicion posible va a almacenar las posiciones de las fichas que se pueden virar.
             * Los primeros dos elementos de cada lista van a indicar la fila y la columna respectivamente.
             * Seguido van las posiciones de las fichas a virar, primero va la fila y despues la columna.*/
            List<List<int>> posiciones = new List<List<int>>();
            #endregion

            List<int> jugadas = new List<int>();
            List<int> provisional = new List<int>();//Esta variable a a almacenar las casillas a virar en cada iteracion del ciclo mas interno.



            Colores colorcontrario;

            if (color == Colores.Blanco) colorcontrario = Colores.Oscuro;
            else colorcontrario = Colores.Blanco;

            for (int i = 0; i < colores.Count; i++)
            {
                //El for externo recorre la lista de fichas, y por cada ficha del color buscado
                //da las posiciones adyacentes que estan vacias.
                if (colores[i] == color)
                {
                    jugadas = CualesSonCasillaVacia(fichasposiciones[i * 2], fichasposiciones[2 * i + 1]);
                    bool entro = false;
                    for (int j = 0; j < jugadas.Count; j += 2)
                    {
                        //Este for obtiene por cada posicion adyacente vacia de una determinada ficha, las posiciones que son validas para jugar.
                        if (!DeterminaCualesSonValidas(jugadas[j], jugadas[j + 1], colorcontrario, color, out provisional))
                        {
                            jugadas.RemoveAt(j);
                            jugadas.RemoveAt(j);
                            j -= 2;
                            entro = true;
                        }
                        if (!entro && provisional.Count != 0 && !YaExiste(posiciones, jugadas[j], jugadas[j + 1]))
                        {
                            provisional.Insert(0, jugadas[j + 1]);
                            provisional.Insert(0, jugadas[j]);
                            posiciones.Add(provisional);
                        }
                        entro = false;
                    }

                }
            }


            //En este punto la lista jugadas tiene las posiciones donde es posible que se haga una jugada.
            //Y la lista casillas_a_virar contiene todas las posiciones de las casillas que se van a virar.

            return posiciones;
        }
        List<int> CualesSonCasillaVacia(int fila, int columna)
        {
            //Este metodo verifica si se puede poner una ficha alrededor de las posiciones que se pasa como parametro.
            List<int> posiciones = new List<int>();

            for (int i = 0; i < pos_col.Length; i++)
            {
                if (fila + pos_fila[i] >= 0 && fila + pos_fila[i] < Board.GetLength(0) && columna + pos_col[i] >= 0 && columna + pos_col[i] < Board.GetLength(1) && Board[fila + pos_fila[i], columna + pos_col[i]] == Colores.Ninguno)
                {
                    posiciones.Add(fila + pos_fila[i]);
                    posiciones.Add(columna + pos_col[i]);
                }
            }
            return posiciones;
        }
        bool DeterminaCualesSonValidas(int fila, int columna, Colores color_actual, Colores color_contrario, out List<int> casillas_a_virar)
        {
            //Este metodo verifica que si la posicion que se le pasa como parametro es valida, entonces lo agrega a la lista de posiciones.
            //El parametro de salida es para hacer todo de una vez
            List<int> pos_validas = new List<int>();
            List<int> posiciones = new List<int>();//Aqui van a estar las posiciones de las fichas que hay que virar por cada casilla
            bool agrega = false;

            for (int i = 0; i < pos_col.Length; i++)
            {
                for (int j = 1; j < 1000; j++)
                {
                    if (fila + pos_fila[i] * j < 0 || fila + pos_fila[i] * j >= Board.GetLength(0) || columna + pos_col[i] * j < 0 || columna + pos_col[i] * j >= Board.GetLength(1) || Board[fila + pos_fila[i] * j, columna + pos_col[i] * j] == Colores.Ninguno)
                    {
                        //Si se encontro una casilla vacia entonces por ese camino no se puede poner ninguna ficha
                        pos_validas = new List<int>();
                        break;
                    }
                    if (Board[fila + pos_fila[i] * j, columna + pos_col[i] * j] == color_contrario)
                    {
                        //Si hay una ficha del color contrario agrega esa posicion
                        pos_validas.Add(fila + pos_fila[i] * j);
                        pos_validas.Add(columna + pos_col[i] * j);
                        continue;
                    }
                    else if (Board[fila + pos_fila[i] * j, columna + pos_col[i] * j] == color_actual)
                    {
                        if (fila + pos_fila[i] * j != fila + pos_fila[i] || columna + pos_col[i] * j != columna + pos_col[i])
                        {
                            //Si la casilla es del mismo color y no esta adyacente a la inicial, agrego a posiciones y salgo
                            agrega = true;
                            UneListas(posiciones, pos_validas);
                            pos_validas = new List<int>();
                        }
                        break;
                    }
                }
            }

            pos_validas = new List<int>();
            casillas_a_virar = posiciones;
            return agrega;
        }
        void UneListas(List<int> a_copiar, List<int> elementos_a_copiar)
        {
            //Este metodo concatena la segunda lista en la primera lista.
            //Y no repite elementos.
            for (int i = 0; i < a_copiar.Count; i += 2)
            {
                for (int j = 0; j < elementos_a_copiar.Count; j += 2)
                {
                    if (a_copiar[i] == elementos_a_copiar[j] && a_copiar[i + 1] == elementos_a_copiar[j + 1])
                    {
                        elementos_a_copiar.RemoveAt(j);
                        elementos_a_copiar.RemoveAt(j);
                    }
                }
            }
            for (int i = 0; i < elementos_a_copiar.Count; i++)
                a_copiar.Add(elementos_a_copiar[i]);
        }
        bool YaExiste(List<List<int>> contenedor, int fila, int columna)
        {
            //Revisa si la fila y la columna ya existen
            for (int i = 0; i < contenedor.Count; i++)
                if (contenedor[i][0] == fila && contenedor[i][1] == columna) return true;
            return false;
        }
    }
    public enum Colores
    { Ninguno, Blanco, Oscuro }
}
