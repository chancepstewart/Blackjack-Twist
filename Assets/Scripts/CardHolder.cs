using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An area that will automatically organize cards that are sent to it. Can also count a total of the cards
/// within it for the GameHandler to use, and handles clearing cards by sending them to the discard pile.
/// </summary>

public class CardHolder : MonoBehaviour {
	// State Variables
	[SerializeField] private float overlapOffset = 0.8f;
	[SerializeField] private float cardLayer = 2f;
	[SerializeField] private bool hasTotal = true;
	private int total = 0;
	private bool softTotal = false;

	// Object References
	[SerializeField] private GameObject[] cardLocations;
	[SerializeField] private GameObject totalText;
	private List<PlayingCard> cards = new List<PlayingCard>();


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// When a card is being dealt, this method can be called and it will determine which position the card will be moved to and return coordinates.
	// Cards that overflow the capacity of the CardHolder will be set at an offset below (y coord) and in front of (z coord) the first row of cards.
	public Vector3 GetLocation()
    {
		Vector3 sendLocation = cardLocations[cards.Count % cardLocations.Length].transform.position;
		return new Vector3(sendLocation.x, sendLocation.y - overlapOffset * (int)(cards.Count / cardLocations.Length), -cardLayer - (int)(cards.Count / cardLocations.Length));
    }

	// Adds a card to the list of cards that this CardHolder currently contains
	public void AddCard(PlayingCard card)
    {
		cards.Add(card);
    }

	// Clears all cards from this CardHolder
	public void DiscardCards()
    {
		softTotal = false;

		foreach(PlayingCard card in cards)
        {
            if (card.IsFaceUp())
            {
				card.FlipCard();
            }
        }

		StartCoroutine(ClearCards());
    }

	// Waits for cards to finish flipping animation, then starts moving them towards the discard pile.
	// The discard function means that the card will destroy itself upon reaching its destination.
	private IEnumerator ClearCards()
    {
		yield return new WaitForSeconds(1);
		foreach(PlayingCard card in cards)
        {
			card.Discard();
        }
		cards.Clear();
    }

	// Calculate the total of the cards within the CardHolder area
	public void CalculateTotal()
	{
		// Prevents total calculation for card holding areas that do not use a total.
		if (!hasTotal)
		{
			return;
		}

		// Reset soft total in case new cards force a hard total.
		softTotal = false;
		total = 0;
		int addValue = 0;
		bool hasAce = false;

		// Add the value of each playing card (J,Q,K are counted as 10, A counted as 1)
		foreach (PlayingCard card in cards)
		{
			if(card.IsFaceUp()){
				addValue = card.GetCardValue();
				if(addValue > 10)
				{
					addValue = 10;
				}
				if(addValue == 1)
				{
					hasAce = true;
				}

				total += addValue;
			}
		}

		// If there is an ace, it can be counted as 11 (1 + 10) as long as that would not put the current total over 21
		if(hasAce && total + 10 <= 21)
		{
			total += 10;
			softTotal = true;
		}

		totalText.GetComponent<UnityEngine.UI.Text>().text = "Card Total: " + total;
	}

	// Returns whether or not the most recently calculated total is using the "11" value of an Ace.
	public bool IsSoftTotal()
	{
		return softTotal;
	}

	// Returns the current total of this card location.
	public int GetTotal()
	{
		return total;
	}

	// Removes a card from this CardHolder's list by comparing it to the cards in the list.
	public void RemoveCard(PlayingCard card)
	{
		for(int i = 0; i < cards.Count; i++)
		{
			if(cards[i] == card)
			{
				cards.RemoveAt(i);
			}
		}
	}
}
