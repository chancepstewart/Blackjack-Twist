using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to manage the flow of the BlackJack game. It also manages UI elements, making them appear or disappear
/// depending on whether or not the player is currently allowed access to their functions.
/// </summary>

public class GameHandler : MonoBehaviour {

	// State variables
	[SerializeField] private int startMoney = 2000;
	[SerializeField] private int riskThreshold = 18;
	private int bet = 0;
	private float money = 0;
	private float odds = 1f;
	private float riskOdds = 1f;
	private bool shuffleNext = false;
	private bool riskHit = false;
	private bool stand = false;

	//Object References
	[SerializeField] private Deck deck;
	[SerializeField] private CardHolder playerHand;
	[SerializeField] private CardHolder dealerHand;
	[SerializeField] private CardHolder riskCards;
	[SerializeField] private UnityEngine.UI.Button hitButton;
	[SerializeField] private UnityEngine.UI.Button standButton;
	[SerializeField] private UnityEngine.UI.Button riskButton;
	[SerializeField] private UnityEngine.UI.Text infoText;
	[SerializeField] private UnityEngine.UI.Text moneyText;
	[SerializeField] private UnityEngine.UI.Text betText;
	[SerializeField] private UnityEngine.UI.Text oddsText;
	[SerializeField] private UnityEngine.UI.InputField betTextBox;
	private PlayingCard secretCard;

	// Use this for initialization
	void Start () {
		SetMoney(startMoney);
		hitButton.gameObject.SetActive(false);
		standButton.gameObject.SetActive(false);
		infoText.gameObject.SetActive(false);
		riskButton.gameObject.SetActive(false);
		StartCoroutine(StartRound());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Starts a new round of BlackJack, from the initial betting phase
	private IEnumerator StartRound()
	{
		// pause for a moment before starting a new round
		yield return new WaitForSeconds(1);

		// Reset relevant state variables
		ResetState();

		// Get a bet from the player
		betTextBox.gameObject.SetActive(true);
		while (bet == 0)
		{
			yield return null;
		}

		// The play starts by drawing cards for the player and the dealer
		deck.DrawCard(playerHand, true);
		deck.DrawCard(playerHand, true);
		deck.DrawCard(dealerHand, true);
		secretCard = deck.DrawCard(dealerHand, false);

		yield return new WaitForSeconds(2);

		// Check for naturals.
		if (playerHand.GetTotal() == 21)
		{
			secretCard.FlipCard();
			yield return new WaitForSeconds(0.5f);

			// If the dealer has a natural, the player gains nothing, reset the game
			if (dealerHand.GetTotal() == 21)
			{
				// Show the result of the round for several seconds
				infoText.text = "Tied Naturals!";
				infoText.gameObject.SetActive(true);
				yield return new WaitForSeconds(2f);
				infoText.gameObject.SetActive(false);

				// Start a new round of BlackJack
				ResetGame();
				yield break;
			}

			// If the dealer has no natural, change the odds to 1.5x and give the player money; reset the game.
			else
			{
				infoText.text = "Player Natural!";
				infoText.gameObject.SetActive(true);
				yield return new WaitForSeconds(2f);

				// The payout is 1.5x for a natural
				ChangeOdds(1.5f);
				SetMoney(money + odds * bet);

				infoText.gameObject.SetActive(false);
				ResetGame();
				yield break;
			}
		}

		// Show the Hit and Stand buttons, then wait for the player to click one of them.
		ActiveButtons(true);
	}

	// Resets state variables to their starting values
	private void ResetState()
	{
		ChangeOdds(1f);
		bet = 0;
		stand = false;
		riskHit = false;
	}

	// Changes the amount of money the player has to amount.
	private void SetMoney(float amount)
	{
		money = amount;
		moneyText.text = "Money: " + money;
	}

	// Changes the odds to amount.
	private void ChangeOdds(float amount)
	{
		odds = amount;
		oddsText.text = "Payout: " + amount + "x";
	}

	// Clear the table and start a new round, as long as the player has money.
	private void ResetGame()
	{
		if(money <= 5)
		{
			GameOver();
		}
		else
		{
			playerHand.DiscardCards();
			dealerHand.DiscardCards();
			if (shuffleNext)
			{
				deck.ShuffleDeck();
				shuffleNext = false;
			}
			StartCoroutine(StartRound());
		}
	}

	// Set a flag to shuffle the deck before the next round starts.
	public void ShuffleNext()
	{
		shuffleNext = true;
	}


	// Kick the player back to the title screen when they run out of money.
	private void GameOver()
	{
		FindObjectOfType<GameSession>().TitleScreen();
	}

	// takes the string from the bet textbox and sets the current bet with it
	public void SetBet()
	{
		// get str from textbox
		string str = betTextBox.text;

		// attempt to convert it into an integer
		try
		{
			int amount = Int32.Parse(str);
			// bets must be between 5 and 500.
			if (amount > 500 || amount < 5)
			{
				Debug.Log("Incorrect amount: " + amount);
				return;
			}

			// once the input has been validated, set the current bet and continue.
			betTextBox.gameObject.SetActive(false);
			bet = amount;
			betText.text = "Bet: " + bet;
		}
		catch (FormatException)
		{
			Debug.Log("Str cannot be converted to Int");
			return;
		}
	}

	// Runs Coroutines for the hit button
	public void Hit()
	{
		// When the player hits a button, the buttons should always disappear until the results of their decision are finalized.
		ActiveButtons(false);

		// The "special twist" rule is that hits work differently when you hit above a threshold (by default 18).
		if(playerHand.GetTotal() >= riskThreshold && !playerHand.IsSoftTotal())
		{
			StartCoroutine(RunRiskHit());
		}
		else
		{
			StartCoroutine(RunHit());
		}
	}

	// The "risk hit" is a hit above the risk threshold. Two cards will be shown, and the higher card will be revealed.
	// If the player chooses to hit, then the odds go up based on how high the revealed card was.
	// The idea is to encourage the player to make high-risk decisions, although in some cases there is little risk.
	// This problem could be mitigated with 3-4 cards that have the highest one revealed (increasing the odds of a higher card appearing), then allowing the player to choose one of the remaining cards.
	private IEnumerator RunRiskHit()
	{
		// draw 2 cards and put them in the "risk card" area
		PlayingCard card1 = deck.DrawCard(riskCards, false);
		PlayingCard card2 = deck.DrawCard(riskCards, false);

		PlayingCard highCard;
		PlayingCard hiddenCard;

		yield return new WaitForSeconds(1.5f);

		// determine the high card
		if(card1.GetCardValue() > card2.GetCardValue())
		{
			highCard = card1;
			hiddenCard = card2;
		}
		else
		{
			highCard = card2;
			hiddenCard = card1;
		}

		// reveal the higher card, then set the odds based on that card's value.
		highCard.FlipCard();
		riskOdds = 1 + 0.1f * highCard.GetCardValue();

		// show the risk hit button and the stand button, then wait for the player to press one of them.
		riskButton.gameObject.SetActive(true);
		standButton.gameObject.SetActive(true);

		while(!riskHit && !stand)
		{
			yield return null;
		}

		// a button has been pressed, so deactivate all buttons until the result is done.
		ActiveButtons(false);
		riskButton.gameObject.SetActive(false);

		
		if (riskHit)
		{
			// Only change the odds if the player chose to hit.
			ChangeOdds(riskOdds);

			// Send the face-down card to the player's hand, discard the face-up card.
			hiddenCard.SendToLocation(playerHand, true);
			riskCards.RemoveCard(hiddenCard);
			riskCards.DiscardCards();
			yield return new WaitForSeconds(1);

			// Check for bust
			if (playerHand.GetTotal() > 21)
			{
				infoText.text = "Bust!";
				infoText.gameObject.SetActive(true);
				yield return new WaitForSeconds(2);
				SetMoney(money - bet);
				infoText.gameObject.SetActive(false);
				ResetGame();
				yield break;
			}
		}

		// If the player stands, stop this function, and the stand coroutine will do the rest.
		else
		{
			riskCards.DiscardCards();
			yield break;
		}

		// Buttons are activated again if the player did not stand or bust, in case they want to hit again for some reason.
		ActiveButtons(true);
	}

	// Trigger used by the risk button which will break the yield loop in the RiskHit coroutine.
	public void RiskButton()
	{
		riskHit = true;
	}

	// Gives the player a card and checks for busts. Returns to hit/stand selection if the player did not bust.
	private IEnumerator RunHit()
	{
		// Give the player their new card
		deck.DrawCard(playerHand, true);

		yield return new WaitForSeconds(2);

		// Check for bust
		if(playerHand.GetTotal() > 21)
		{
			infoText.text = "Bust!";
			infoText.gameObject.SetActive(true);
			yield return new WaitForSeconds(2);
			SetMoney(money - bet);
			infoText.gameObject.SetActive(false);
			ResetGame();
			yield break;
		}

		ActiveButtons(true);
	}

	// Starts the stand coroutine, as well as breaks the loop in the risk hit coroutine.
	public void Stand()
	{
		ActiveButtons(false);
		stand = true;
		StartCoroutine(RunStand());
	}

	// Give the dealer cards until their total is greater than 16, then calculate whether the dealer or player wins the round.
	private IEnumerator RunStand()
	{
		// flip the dealer's second, face-down card from the initial deal.
		secretCard.FlipCard();

		yield return new WaitForSeconds(0.5f);

		// Add cards until the dealer's total is greater than 16.
		while(dealerHand.GetTotal() <= 16)
		{
			deck.DrawCard(dealerHand, true);
			yield return new WaitForSeconds(0.5f);
		}

		// If the dealer busts or has less cards than the player, the player wins.
		if(dealerHand.GetTotal() > 21 || dealerHand.GetTotal() < playerHand.GetTotal())
		{
			// Display the result
			infoText.text = "Player Win!";
			infoText.gameObject.SetActive(true);
			yield return new WaitForSeconds(2);

			// Add money to the player's total
			SetMoney(money + bet * odds);
			infoText.gameObject.SetActive(false);

			//Start the round again.
			ResetGame();
		}
		// If the dealer has a higher total than the player, the dealer wins.
		else if(dealerHand.GetTotal() > playerHand.GetTotal())
		{
			infoText.text = "Dealer Win!";
			infoText.gameObject.SetActive(true);
			yield return new WaitForSeconds(2);

			// money is taken from the player
			SetMoney(money - bet);
			infoText.gameObject.SetActive(false);
			ResetGame();
		}
		// Equal totals are a stand-off.
		else
		{
			infoText.text = "Stand-off!";
			infoText.gameObject.SetActive(true);
			yield return new WaitForSeconds(2);
			infoText.gameObject.SetActive(false);
			ResetGame();
		}
	}

	// Shows the player the hit and stand buttons when set is true, and hides the buttons if it is false.
	private void ActiveButtons(bool set)
	{
		hitButton.gameObject.SetActive(set);
		standButton.gameObject.SetActive(set);
	}
}
