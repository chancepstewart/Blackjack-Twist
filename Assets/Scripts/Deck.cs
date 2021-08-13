using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// This object contains a list of cards, and is primarily used for its public function which allows it 
/// to deal random cards to players.
/// </summary>

public class Deck : MonoBehaviour {

	public struct CardInfo
	{
		public PlayingCard.Suit suit;
		public int value;
	}

	// State variables
	[SerializeField] private int numDecks = 3;
	[SerializeField] private float cardLayer = 2;
	private List<CardInfo> cards = new List<CardInfo>();

	// Object References
	[SerializeField] private GameObject cardPrefab;
	[SerializeField] private Mesh[] cardMeshes;
	[SerializeField] private CardHolder player;
	private GameHandler gameHandler;

	// Use this for initialization
	void Start () {
		ShuffleDeck();
		gameHandler = FindObjectOfType<GameHandler>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Draws a card and sends it to a provided recipient, flipping the card over once it reaches the revipient unless specified not to
	public PlayingCard DrawCard(CardHolder recipient, bool flip = true)
	{
		// Get card information
		CardInfo info = cards[0];
		cards.RemoveAt(0);
		PlayingCard.Suit suit = info.suit;
		int value = info.value;
		// Meshes are indexed by suit (such that suit 0 is the first set of meshes, 1 is the second, etc.) and value so those numbers can be used to find the mesh
		Mesh cardMesh = cardMeshes[(int)suit * 13 + value - 1];

		// Instantiate card at correct position
		Vector3 pos = new Vector3(this.transform.position.x, this.transform.position.y, -cardLayer);
		var newCard = Instantiate(cardPrefab, pos, Quaternion.identity);
		PlayingCard card = newCard.GetComponent<PlayingCard>();

		// Set the card value and send it to the recipient
		card.SetCardValue(suit, value, cardMesh);
		card.SendToLocation(recipient, flip);

		// Check if deck needs to be shuffled (if it is less than 1/8th of the original size)
		if(cards.Count <= 52 * numDecks / 8)
		{
			gameHandler.ShuffleNext();
		}

		return card;
	}

	// Deletes current deck and generates a new shuffled deck
	public void ShuffleDeck()
	{
		// Delete any cards currently in the deck
		cards.Clear();

		// newCard will be used to add new cards to the deck
		CardInfo newCard;

		// First loop is for suits
		foreach(PlayingCard.Suit suit in (PlayingCard.Suit[]) Enum.GetValues(typeof(PlayingCard.Suit)))
		{
			newCard.suit = suit;

			// Second loop is for cards values, 1 is ace and 13 is King
			for(int j = 1; j < 14; j++)
			{
				newCard.value = j;

				// Add each card numDecks times
				for(int k = 0; k < numDecks; k++)
				{
					cards.Add(newCard);
				}
			}
		}

		for(int i = 0; i < cards.Count; i++)
		{
			CardInfo temp = cards[i];
			int randomIndex = UnityEngine.Random.Range(i, cards.Count);
			cards[i] = cards[randomIndex];
			cards[randomIndex] = temp;
		}

	}
}
