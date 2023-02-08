using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ConsoleSpiderSolitare
{
    internal class Program
    {
        static void Main(string[] args)
        {
            GameEngine g = new GameEngine();

            InitialSetup();
            PlayGame(g);
        }
        static void InitialSetup()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            ConsoleHelper.SetCurrentFont("Consolas", 30);
            Console.SetWindowSize(40, 19);
            Console.Clear();
        }
        static void PlayGame(GameEngine g)
        {
            Mouse m = new Mouse();
            
            Coor emptyCoor = new Coor(-1, -1);
            Coor curSelect = emptyCoor;
            Coor firstSelect = emptyCoor;
            Coor secondSelect = emptyCoor;
            g.DrawBoard(curSelect);
            while (true)
            {
                if (Control.MouseButtons.ToString() == "Left")
                {
                    curSelect = m.GetCursorCoor();
                    if (g.ButtonPressed(curSelect)) ;
                    else if (g.IsValidCardSelected(curSelect))
                    {
                        if (firstSelect == emptyCoor && secondSelect == emptyCoor)
                        {
                            firstSelect = curSelect;
                        }
                        else if (firstSelect != emptyCoor && firstSelect != curSelect && secondSelect == emptyCoor)
                        {
                            secondSelect = curSelect;
                        }
                        if (firstSelect != emptyCoor && secondSelect != emptyCoor)
                        {
                            Console.WriteLine(g.MoveCards(firstSelect, secondSelect));
                            firstSelect = emptyCoor;
                            secondSelect = emptyCoor;
                        }
                    }
                    else firstSelect = emptyCoor;
                    g.CheckForWinningDeck();
                    g.DrawBoard(firstSelect);
                    Thread.Sleep(120);
                }
                Thread.Sleep(10);
            }
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
            if (titleBar)
            {
                mousePos.X -= 8;
                mousePos.Y -= 30;
            }
            return new Point(mousePos.X - consoleRect.Left, mousePos.Y - consoleRect.Top);
        }
        public Coor GetCursorCoor()
        {
            mousePos = GetCursorPos();
            return new Coor(mousePos.X / 56, mousePos.Y / 30);
        }
    }
    public class Coor
    {
        public int Row, Col;
        public Coor(int Col, int Row)
        {
            this.Row = Row;
            this.Col = Col;
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
        private int id;
        private int suit, rank;
        private bool hidden;
        public Card(int suit, int rank, bool hidden, int id)
        {
            this.suit = suit;
            this.rank = rank;
            this.hidden = hidden;
            this.id = id;
        }
        public int GetSuit() { return suit; }
        public int GetRank() { return rank; }
        public bool IsHidden() { return hidden; }
        public int GetId() { return id; }
        public void Unhide() { hidden = false; }
        public Card DeepCopy()
        {
            Card temp = (Card)this.MemberwiseClone() ;
            temp.suit = this.suit;
            temp.rank = this.rank;
            temp.hidden = this.hidden;
            temp.id = this.id;
            return temp;
        }
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
        public Card GetCardAtIndex(int j) { return cards[j]; }
        public Card[] GetCardsFromIndex(int j)
        {
            Card[] output = new Card[GetCountFromIndex(j)];
            int c = 0;
            for (int i = j; i < count; i++)
            {
                output[c++] = cards[i];
            }
            return output;
        }
        public int GetCountFromIndex(int j)
        {
            return count - j;
        }
        public void RemoveCardsFromIndex(int j)
        {
            for (int i = j; i < count; i++)
            {
                cards[i] = null;
            }
            count -= count - j;
            if (count > 0 && cards[count - 1].IsHidden()) cards[count - 1].Unhide();
        }
        public Card GetLastCard() { return cards[count - 1]; }
        public CardContainer DeepCopy()
        {
            CardContainer temp = (CardContainer)this.MemberwiseClone();
            Card[] tempCards = new Card[100];
            for(int i = 0; i < count; i++)
            {
                tempCards[i] = cards[i].DeepCopy();
            }
            temp.cards = tempCards;
            temp.count = this.count;
            return temp;
        }
    }
    class GameEngine
    {
        List<CardContainer[]> boardHistory = new List<CardContainer[]>();
        List<int> stockHistory = new List<int>();
        CardContainer[] board = new CardContainer[10];
        CardContainer[] stock = new CardContainer[4];
        int stockCount;
        public GameEngine()
        {
            board = new CardContainer[10];
            stock = new CardContainer[4];
            for (int i = 0; i < 10; i++) board[i] = new CardContainer();
            for (int i = 0; i < 4; i++) stock[i] = new CardContainer();
            ShuffleDecks();
        }
        public CardContainer[] GetBoard() { return board; }
        public CardContainer[] GetStock() { return stock; }
        public void ShuffleDecks(int cardVar = 1)
        {
            board = new CardContainer[10];
            stock = new CardContainer[5];
            for (int i = 0; i < 10; i++) board[i] = new CardContainer();
            for (int i = 0; i < 5; i++) stock[i] = new CardContainer();
            stockCount = 5;
            Random rand = new Random();
            List<int> randomCards = new List<int>();

            while (randomCards.Count() < 104)
            {
                int x = rand.Next(0, 104);
                if (!randomCards.Contains(x))
                {
                    randomCards.Add(x);
                }
            }
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < 4; i++)
            {
                for (int y = 1; y <= 6; y++)
                {
                    int c = randomCards.Last();
                    randomCards.Remove(randomCards.Last());
                    Card t;
                    if (y == 6) t = new Card((c / 13) % cardVar, c % 13, false, c);
                    else t = new Card((c / 13) % cardVar, c % 13, true, c);
                    board[i].AddCards(t);
                }
            }
            for (int i = 4; i < 10; i++)
            {
                for (int y = 1; y <= 5; y++)
                {
                    if(randomCards.Count() == 0)
                    {
                        Card t = new Card(0, 0, true, -1);
                        board[i].AddCards(t);
                    }
                    else
                    {
                        int c = randomCards.Last();
                        randomCards.Remove(randomCards.Last());
                        Card t;
                        if (y == 5) t = new Card((c / 13) % cardVar, c % 13, false, c);
                        else t = new Card((c / 13) % cardVar, c % 13, true, c);
                        board[i].AddCards(t);
                    }

                }
            }
            
            for (int i = 0; i < 5; i++)
            {
                for (int y = 1; y <= 10; y++)
                {
                    int c = randomCards.Last();
                    randomCards.Remove(randomCards.Last());
                    Card t = new Card((c / 13) % cardVar, c % 13, false, c);
                    stock[i].AddCards(t);
                }
            }
            Console.SetCursorPosition(0, 0);
        }
        public bool IsValidCardSelected(Coor select)
        {
            if (0 <= select.Col && select.Col < 10)
            {
                if (board[select.Col].GetCount() == 0 && select.Row == 0) return true;
                if (0 <= select.Row && select.Row < board[select.Col].GetCount())
                {
                    return !board[select.Col].GetCardAtIndex(select.Row).IsHidden();
                }
            }
            return false;
        }
        public bool MoveCards(Coor first, Coor second)
        {
            if (first.Col == second.Col) return false;
            if (board[first.Col].GetCount() == 0) return false;

            Card[] firstCards = board[first.Col].GetCardsFromIndex(first.Row);
            int firstCount = board[first.Col].GetCountFromIndex(first.Row);

            if (board[second.Col].GetCount() > 0)
            {
                if (firstCards[0].GetRank() + 1 != board[second.Col].GetLastCard().GetRank()) return false;
            }
            for (int i = 0; i < firstCount - 1; i++)
            {
                if (firstCards[i].GetRank() != firstCards[i+1].GetRank() + 1) return false;
                if (firstCards[i].GetSuit() != firstCards[i + 1].GetSuit()) return false;
            }

            SaveBoardToHistory();
            board[second.Col].AddCards(firstCards);
            board[first.Col].RemoveCardsFromIndex(first.Row);
            return true;
        }
        public bool ButtonPressed(Coor select)
        {
            if(select.Row == 18)
            {
                switch (select.Col)
                {
                    case 0:
                        ShuffleDecks();
                        return true;
                    case 2:
                        UndoMove(ref board);
                        return true;
                    case 4:
                    case 6:
                        DealFromStock();
                        return true;
                }
            }
            return false;
        }
        public void UndoMove(ref CardContainer[] board)
        {
            if (stockHistory.Count >= 1)
            {
                board = boardHistory.Last();
                boardHistory.Remove(boardHistory.Last());
            }
            if (stockHistory.Count >= 1)
            {
                stockCount = stockHistory.Last();
                stockHistory.Remove(stockHistory.Last());
            }

        }
        public void DrawBoard(Coor curSelect)
        {
            Console.Clear();
            Coor brush = new Coor(0, 0);
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < board[i].GetCount(); j++)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.SetCursorPosition(brush.Col, brush.Row);
                    int rank = board[i].GetCardAtIndex(j).GetRank();
                    int suit = board[i].GetCardAtIndex(j).GetSuit();
                    bool hidden = board[i].GetCardAtIndex(j).IsHidden();
                    if (hidden)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("▓▓▓");
                    }
                    else
                    {
                        if (j == curSelect.Row && i == curSelect.Col) Console.BackgroundColor = ConsoleColor.DarkCyan;
                        if (suit == 0 || suit == 1) Console.ForegroundColor = ConsoleColor.Red;
                        else Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"{Carddeck.ranks[rank],2}{Carddeck.suits[suit]}");
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    }
                    brush.Row++;
                }
                brush.Col += 4;
                brush.Row = 0;
            }
            Console.SetCursorPosition(0, 18);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("NEW");
            Console.Write("     ");
            Console.Write("UNDO");
            Console.Write("     ");
            Console.Write("STOCK X{0}", stockCount);
        }
        public void SaveBoardToHistory()
        {
            CardContainer[] tempBoard = new CardContainer[10];
            for (int i = 0; i < 10; i++)
            {
                tempBoard[i] = board[i].DeepCopy();
            }
            boardHistory.Add(tempBoard);
            stockHistory.Add(stockCount);
        }
        public bool DealFromStock()
        {
            if (stockCount > 0)
            {
                SaveBoardToHistory();
                stockCount--;
                for (int i = 0; i < 10; i++)
                {
                    board[i].AddCards(stock[stockCount].GetCardAtIndex(i));
                }
                return true;
            }
            else return false;
        }
        public void CheckForWinningDeck()
        {
            for (int i = 0; i < 10; i++)
            {
                CardContainer cards = board[i];
                if (cards.GetCount() < 13) continue;
                int suit = cards.GetLastCard().GetSuit();
                int foundIndx = -1;
                for (int j = cards.GetCount() - 2; j >= 0; j--)
                {
                    Console.SetCursorPosition(0, 0);
                    if (cards.GetCardAtIndex(j).GetRank() != cards.GetCardAtIndex(j + 1).GetRank() + 1 ||
                        cards.GetCardAtIndex(j).GetSuit() != suit) break;
                    else if (cards.GetCardAtIndex(j).GetRank() == 12)
                    {
                        foundIndx = j;
                        break;
                    }
                }
                if (foundIndx != -1)
                {
                    SaveBoardToHistory();
                    board[i].RemoveCardsFromIndex(foundIndx);
                }
            }
        }
    }
}