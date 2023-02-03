using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ConsoleSpiderSolitare
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CardContainer[] board = new CardContainer[10];
            CardContainer[] stock = new CardContainer[4];
            InitialSetup(board, stock);
            ShuffleDecks(board, stock);
            PlayGame(board, stock);
        }
        static void InitialSetup(CardContainer[] board, CardContainer[] stock)
        {
            for (int i = 0; i < 10; i++) board[i] = new CardContainer();
            for (int i = 0; i < 4; i++) stock[i] = new CardContainer();
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear();
            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            ConsoleHelper.SetCurrentFont("Consolas", 30);
            Console.SetWindowSize(40, 19);
        }
        static void PlayGame(CardContainer[] board, CardContainer[] stock)
        {
            while(true)
            {

                DrawBoard(board, stock);
                Thread.Sleep(150);
            }
        }
        static void ShuffleDecks(CardContainer[] board, CardContainer[] stock)
        {
            Random rand = new Random();
            List<int> randomCards = new List<int>();

            while (randomCards.Count() < 52 * 2)
            {
                int x = rand.Next(1, 13 * 8 + 1);
                if (!randomCards.Contains(x))
                {
                    randomCards.Add(x);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int y = 1; y <= 6; y++)
                {
                    int c = randomCards.Last();
                    randomCards.Remove(randomCards.Last());
                    Card t;
                    if (y == 6) t = new Card((c / 13) % 4, c % 13, false);
                    else t = new Card((c / 13) % 4, c % 13, true);
                    board[i].AddCards(t);
                }
            }
            for (int i = 4; i < 10; i++)
            {
                for (int y = 1; y <= 5; y++)
                {
                    int c = randomCards.Last();
                    randomCards.Remove(randomCards.Last());
                    Card t;
                    if (y == 5) t = new Card((c / 13) % 4, c % 13, false);
                    else t = new Card((c / 13) % 4, c % 13, true);
                    board[i].AddCards(t);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int y = 1; y <= 10; y++)
                {
                    int c = randomCards.Last();
                    randomCards.Remove(randomCards.Last());
                    Card t = new Card((c / 13) % 4, c % 13, false);
                    stock[i].AddCards(t);
                }
            }

        }

        protected static int origCol;
        protected static int origRow;
        static void DrawBoard(CardContainer[] board, CardContainer[] stock)
        {
            Console.Clear();
            int x = origCol;
            int y = origRow;
            for(int i = 0; i < 10; i++)
            {
                for (int j = 0; j < board[i].GetCount(); j++)
                {
                    Console.SetCursorPosition(x, y);
                    int rank = board[i].GetCardAtIndex(j).getRank();
                    int suit = board[i].GetCardAtIndex(j).getSuit();
                    bool hidden = board[i].GetCardAtIndex(j).isHidden();
                    if (hidden)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("▓▓▓");
                    }
                    else
                    {
                        if (suit == 0 || suit == 1) Console.ForegroundColor = ConsoleColor.Red;
                        else Console.ForegroundColor = Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{Carddeck.ranks[rank],2}{Carddeck.suits[suit]}");
                    }
                    y++;
                }
                x += 4;
                y = origRow;
            }
            Console.SetCursorPosition(0, 8);
            Mouse m = new Mouse();
            Point p = m.GetCursorPos();
            Console.WriteLine(p);
            //Console.Clear();
        }
    }
    public class Mouse
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        IntPtr consoleWindow = GetConsoleWindow();
        RECT consoleRect;
        Point mousePos;
        bool titleBar = true; //Compensate for the console title bar
        public Point GetCursorPos()
        {
            GetWindowRect(consoleWindow, out consoleRect);
            mousePos = Control.MousePosition;
            if(titleBar)
            {
                mousePos.X -= 8;
                mousePos.Y -= 30;
            }
            return new Point(mousePos.X - consoleRect.Left, mousePos.Y - consoleRect.Top);
        }
    }
    public class Carddeck
    {
        public static string suits { get; } = "♥♦♣♠";
        public static string[] ranks { get; } = new string[13] { "1", "2", "3", "4", "5", "6", "7", 
                                                                "8", "9", "10", "J", "Q", "K"};
    }
    class Card
    {
        private int suit, rank;
        private bool hidden;
        public Card(int suit, int rank, bool hidden)
        {
            this.suit = suit;
            this.rank = rank;
            this.hidden = hidden;
        }
        public int getSuit() { return suit; }
        public int getRank() { return rank; }
        public bool isHidden() { return hidden; }
    }
    class CardContainer
    {
        private Card[] cards;
        int count;

        public CardContainer()
        {
            count = 0;
            cards = new Card[100];
        }

        public void AddCards(Card a) { cards[count++] = a; }
        public void AddCards(Card[] a)
        {
            for (int i = 0; i < a.Count(); i++)
            {
                cards[count++] = a[i];
            }
        }
        public int GetCount() { return count; }
        public Card[] GetCards() { return cards; }
        public Card GetCardAtIndex(int i) { return cards[i]; }
    }
}