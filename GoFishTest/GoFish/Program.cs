using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFishGame
{
    public enum Face { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King };
    public enum Suit { Diamonds, Clubs, Hearts, Spades };

    class Program
    {
        static void Main(string[] args)
        {
            var numTricks = 0;
            Player[] players = new Player[]
            {
                new RandomPlayer("Doug"),
                new LastCardPersonOnLeftPlayer("Peter"),
                new Cheater("Scott"),
                new PlayerWithMemory("John", new string[]{ "Doug" , "Peter", "Scott"})
            };

            PlayerWithMemory playerWithMemory = (PlayerWithMemory)players[3];


            Console.WriteLine("Go Fish Game \t\t by Devin Treichel \t\t C#");
            Console.WriteLine("___________________________________________________________________\n\n\n");

            Deck deck = new Deck();

            Console.WriteLine("Fresh Deck of Cards:\n");
            Console.WriteLine(deck);

            deck.Shuffle();
            deck.CutDeck();

            foreach (var player in players)
            {
                player.DealHand(deck);

            }
            while(numTricks < 13)
            {
                for(var i = 0; i < 4; i++)
                {                   
                    var currentPlayer = players[i];
                    Console.WriteLine("It is now " + currentPlayer.Name + "'s turn");
                    var go = deck.DeckSize() > 0 || currentPlayer.Hand.HandSize() > 0;
                    while (go)
                    {
                        var faceToAskFor = currentPlayer.ChooseRankToAskFor();
                        var playerToAsk = currentPlayer.ChoosePlayerToAsk(players);
                        Console.WriteLine(playerToAsk.Name + ", do you have any " + faceToAskFor.ToString() + "?");
                        if (!currentPlayer.Equals(playerWithMemory))
                        { playerWithMemory.RememberChoice(currentPlayer.Name, faceToAskFor); }
                        var count = playerToAsk.Hand.CardCountForFace(faceToAskFor);
                        if (count > 0)
                        {
                            Console.WriteLine("Dang, I have " + count + " of those");
                            var cards = playerToAsk.Hand.GiveCards(count, faceToAskFor);
                            foreach (var card in cards) { currentPlayer.Hand.AddCard(card); }
                            go = true;
                        }
                        else
                        {
                            Console.WriteLine("GO FISH");
                            if (deck.DeckSize() > 0)
                            { currentPlayer.Hand.AddCard(deck.DealCard()); }
                        }
                        if (currentPlayer.AttemptToPlayTrick(deck, faceToAskFor)) { numTricks++; }
                        go = count > 0 && deck.DeckSize() > 0 && currentPlayer.Hand.HandSize() > 0;
                    }
                    if (numTricks == 13) { break; }
                }
            }
            Player winner = players[0];
            for(var i = 1;  i < players.Length; i++)
            {
                if(players[i].Points > winner.Points) { winner = players[i]; }
            }
            Console.WriteLine("The winner is " + winner.Name + " with " + winner.Points + " points");
            Console.ReadLine();
        }
    }

    // ====================================================================================

    public class Card
    {
        public Face Face { get; private set; }
        public Suit Suit { get; private set; }

        public Card(Suit suit, Face face)
        {
            this.Face = face;
            this.Suit = suit;
        }

        public override string ToString()
        {
            return Face + " of " + Suit;
        }

        public override bool Equals(object obj)
        {
            if (!obj.GetType().Equals(typeof(Card))) { return false; }
            Card c = (Card)obj;
            return c.Face == Face && c.Suit == Suit;
        }
    }

    // ====================================================================================

    public class Deck
    {

        private Card[] deck;
        private Random random = new Random();


        public Deck()
        {

            var suits = Enum.GetNames(typeof(Suit)).Length;
            var faces = Enum.GetNames(typeof(Face)).Length;

            int deckSize = suits * faces;
            deck = new Card[deckSize];

            int i = 0;
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Face face in Enum.GetValues(typeof(Face)))
                {
                    Card card = new Card(suit, face);
                    deck[i++] = card;
                }
            }
        }


        public void Shuffle()
        {
            if (deck.Length == 0) return;

            for (int i = 0; i < deck.Length; i++)
            {
                int r = random.Next(i, deck.Length);
                Card card = deck[i];
                deck[i] = deck[r];
                deck[r] = card;
            }
        }


        public void CutDeck()
        {
            if (deck.Length == 0)
                return;

            int cutPoint = random.Next(1, deck.Length);

            Card[] newDeck = new Card[deck.Length];

            int i, j = 0;

            for (i = cutPoint; i < deck.Length; i++)
            {
                newDeck[j] = deck[i];
                j++;
            }

            for (i = 0; i < cutPoint; i++)
            {
                newDeck[j] = deck[i];
                j++;
            }
            deck = newDeck;
        }


        public Card DealCard()
        {
            if (deck.Length == 0)
                return null;

            Card card = deck[deck.Length - 1];
            Array.Resize(ref deck, deck.Length - 1);

            return card;
        }

        public int DeckSize()
        {
            return deck.Length;
        }


        public override string ToString()
        {
            string a = "";

            for (int i = 0; i < deck.Length; i++)
            {
                if ((i + 1) % 4 == 0)
                {
                    a += "[" + deck[i] + "]\n";
                }
                else
                    a += "[" + deck[i] + "] ";
            }
            a += "\n" + deck.Length + " total cards in deck.\n";
            return a;
        }
    }

    // ====================================================================================
    public class Hand
    {

        protected Card[] deck;

        public Hand()
        {
            deck = new Card[0];
        }


        public void AddCard(Card card)
        {
            Array.Resize(ref deck, deck.Length + 1);
            deck[deck.Length - 1] = card;
        }

        public Card ViewCard(int index)
        {
            return deck[index];
        }


        public Card RemoveCard(Card card)
        {
            Card[] newDeck = new Card[deck.Length - 1];

            int i = 0;
            foreach (Card c in deck)
            {
                if (!c.Equals(card)) newDeck[i++] = c;
            }
            deck = newDeck;
            return card;
        }

        public Card RemoveCard(int index)
        {
            Card[] newDeck = new Card[deck.Length - 1];
            var cardtoReturn = deck[index];
            var j = 0;
            for(var i = 0; i < deck.Length; i++)
            {
                if(i != index) { newDeck[j++] = deck[i]; }
            }
            deck = newDeck;
            return cardtoReturn;
        }

        public int HandSize()
        {
            return deck.Length;
        }

        public override string ToString()
        {
            string array = "";
            for (int i = 0; i < deck.Length; i++)
            {
                array += "[" + deck[i] + "] ";
            }
            return array;
        }

        public bool HasTrick(Face face)
        {
            if (deck.Length < 4) { return false; }
            var faceCount = 0;
            foreach (var card in deck)
            {
                if (card.Face == face)
                {
                    faceCount++;
                    if (faceCount == 4) { return true; }
                }
            }
            return false;
        }

        public bool PlayTrick(Face face)
        {
            var cardsToRemove = new Card[4];
            var count = 0;
            foreach (var card in deck)
            {
                if (card.Face == face)
                {
                    cardsToRemove[count++] = card;
                }
            }
            foreach (var card in cardsToRemove)
            {
                RemoveCard(card);
            }
            return deck.Length == 0;
        }

        public int CardCountForFace(Face face)
        {
            var count = 0;
            foreach (var card in deck)
            {
                if (card.Face == face)
                {
                    count++;
                }
            }
            return count;
        }

        public Card[] GiveCards(int count, Face face)
        {
            var cards = new Card[count];
            var i = 0;
            foreach (var card in deck)
            {
                if (card.Face == face)
                {
                    cards[i++] = card;
                }
            }
            foreach(var card in cards)
            {
                RemoveCard(card);
            }
            return cards;
        }
    }

    // ====================================================================================

    public abstract class Player
    {
        private const int handSize = 5;
        public int Points { get; private set; } = 0;
        public string Name { get; private set; }
        public Hand Hand { get; private set; }
        // other stuff here


        public Player(string name)
        {
            this.Name = name;
            this.Hand = new Hand();
        }



        public abstract Player ChoosePlayerToAsk(Player[] players);
        public abstract Face ChooseRankToAskFor();

        public void DealHand(Deck deck)
        {
            for(var i = 0; i < handSize; i++)
            {
                if (deck.DeckSize() > 0)
                { Hand.AddCard(deck.DealCard()); }
            }
        }

        public bool AttemptToPlayTrick(Deck deck, Face face)
        {
            if (Hand.HasTrick(face))
            {
                if (Hand.PlayTrick(face))
                {
                    DealHand(deck);
                }
                Points++;
                return true;
            }
            return false;
        }

        //other methods here

        public override string ToString()
        {
            string s = Name + "'s Hand:\n";
            s += Hand.ToString();
            return s;
        }
        public override bool Equals(object obj)
        {
            Player p = (Player)obj;
            return p.Name == Name;
        }
    }

    // ======================================================================

    public class RandomPlayer : Player
    {
        protected readonly Random random = new Random();
        public RandomPlayer(string name) : base(name) { }
        public override Player ChoosePlayerToAsk(Player[] players)
        {
            Player playerToReturn = players[random.Next(players.Length)];
            while(playerToReturn.Equals(this))
            {
                playerToReturn = players[random.Next(players.Length)];
            }
            return playerToReturn;
        }

        public override Face ChooseRankToAskFor()
        {
            return Hand.ViewCard(random.Next(Hand.HandSize())).Face;
        }
    }

    public class FirstCardPersonOnRightPlayer : Player
    {
        public FirstCardPersonOnRightPlayer(string name) : base(name) { }
        public override Player ChoosePlayerToAsk(Player[] players)
        {
            int index = 0;
            for(var i = 0; i < players.Length; i++)
            {
                if (players[i].Equals(this)) { index = i; }
            }
            if(index == players.Length - 1) { index = 0; }
            else { index++; }
            return players[index];
        }

        public override Face ChooseRankToAskFor()
        {
            return Hand.ViewCard(0).Face;
        }
    }

    public class LastCardPersonOnLeftPlayer : Player
    {
        public LastCardPersonOnLeftPlayer(string name) : base(name) { }
        public override Player ChoosePlayerToAsk(Player[] players)
        {
            int index = 0;
            for (var i = 0; i < players.Length; i++)
            {
                if (players[i].Equals(this)) { index = i; }
            }
            if (index == 0) { index = players.Length - 1; }
            else { index--; }
            return players[index];
        }

        public override Face ChooseRankToAskFor()
        {
            return Hand.ViewCard(Hand.HandSize()-1).Face;
        }
    }

    public class LastCardRandomPersonPlayer : RandomPlayer
    {
        public LastCardRandomPersonPlayer(string name) : base(name) { }

        public override Face ChooseRankToAskFor()
        {
            return Hand.ViewCard(Hand.HandSize() - 1).Face;
        }
    }

    public class PlayerWithMemory : RandomPlayer
    {
        public KeyValuePair<string, Face>[] OtherPlayerChoices { get; private set; }
        public PlayerWithMemory(string name, string[] names) : base(name)
        {
            OtherPlayerChoices = new KeyValuePair<string, Face>[names.Length];
            for(var i = 0; i< names.Length; i++)
            {
                OtherPlayerChoices[i] = new KeyValuePair<string, Face>(names[i], Face.Ace);
            }
        }

        public void RememberChoice(string name, Face face)
        {
            for(var i = 0; i < OtherPlayerChoices.Length; i++)
            {
                if (OtherPlayerChoices[i].Key.Equals(name))
                {
                    OtherPlayerChoices[i] = new KeyValuePair<string, Face>(name, face);
                }
            }
        }

        public override Player ChoosePlayerToAsk(Player[] players)
        {
            for (var i = 0; i < Hand.HandSize(); i++)
            {
                for(var j = 0; j < OtherPlayerChoices.Length; j++)
                {
                    if(OtherPlayerChoices[j].Value == Hand.ViewCard(i).Face)
                    {
                        for(int k = 0; k < players.Length; k++)
                        {
                            if(players[k].Name == OtherPlayerChoices[j].Key)
                            {
                                return players[k];
                            }
                        }
                    }
                }
            }
            return base.ChoosePlayerToAsk(players);
        }

        public override Face ChooseRankToAskFor()
        {
            for (var i = 0; i < Hand.HandSize(); i++)
            {
                for (var j = 0; j < OtherPlayerChoices.Length; j++)
                {
                    if (OtherPlayerChoices[j].Value == Hand.ViewCard(i).Face)
                    {
                        return OtherPlayerChoices[j].Value;
                    }
                }
            }
            return base.ChooseRankToAskFor();
        }
    }

    public class Cheater : RandomPlayer
    {
        public Cheater(string name) : base(name) { }

        public override Face ChooseRankToAskFor()
        {
            var faces = Enum.GetValues(typeof(Face)).Cast<Face>().ToArray();
            var r = random.Next(faces.Length);
            return faces[r];
        }
    }
}
