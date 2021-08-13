using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A playing card, which contains a suit (unused in this game), value, and a face-up state.
/// Can be moved by changing its destination.
/// </summary>

public class PlayingCard : MonoBehaviour {

	public enum Suit
	{
		Club = 0,
		Diamond = 1,
		Heart = 2,
		Spade = 3
	}

	// State Variables
	[SerializeField] float moveSpeed = 10;
	private Suit suit = Suit.Club;
	private int value = 1;
	private bool faceUp = false;
	private bool discarded = false;
	private bool flip = false;
	private Vector3 destination;
	

	// Component References
	Animator animator;
	MeshFilter meshfilter;

	// Object References
	CardHolder currentHolder;
	DiscardLocation discardLocation;


	void Awake () {
		animator = GetComponent<Animator>();
		meshfilter = GetComponent<MeshFilter>();
		destination = transform.position;
		discardLocation = FindObjectOfType<DiscardLocation>();
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		MoveCard();
	}

	// Moves the card towards a destination location, then flips it if the "flip" flag is set, or destroys it if the "discarded" flag is set.
	private void MoveCard()
	{
		// Move the card towards its destination until it is there
		if (destination != transform.position)
		{
			transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed);
		}
		// Once the card is no longer moving, flip or destroy it if the flags are set
		else if (flip)
		{
			animator.SetTrigger("Flip");
			faceUp = !faceUp;
			flip = false;
			currentHolder.CalculateTotal();
		}
		else if (discarded)
		{
			Destroy(gameObject);
		}
	}

	// Set the suit, value, and mesh for this card.
	public void SetCardValue(Suit suit, int value, Mesh mesh)
	{
		this.suit = suit;
		this.value = value;
		meshfilter.mesh = mesh;
	}

	// Sets the "flip" flag so that the card will flip as long as it is not moving
	public void FlipCard()
	{
		flip = true;
	}

	// Returns true if this card is currently face-up.
	public bool IsFaceUp()
	{
		return faceUp;
	}

	// Retrieve a new destination from a Cardholder, then flip this card over when it reaches the destination unelss otherwise specified.
	public void SendToLocation(CardHolder recipient, bool flip = true)
	{
		destination = recipient.GetLocation();
		currentHolder = recipient;
		this.flip = flip;
		recipient.AddCard(this);
	}

	// Send this card to the discard pile then delete it.
	public void Discard()
	{
		destination = discardLocation.GetPosition();
		discarded = true;
	}

	// Returns the numerical value of this card.
	public int GetCardValue()
	{
		return value;
	}
}
