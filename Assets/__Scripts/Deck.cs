using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {
	//Suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;
	
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;

	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;
	
	//prefabs
	public GameObject prefabSprite;
	public GameObject prefabCard;

	public bool ________________;

	public PT_XMLReader xmlr;
	public List<string> cardNames;
	public List<Card> cards;
	public List<Decorator> decorators;
	public List<CardDefinition> cardDefs;
	public Transform deckAnchor;
	public Dictionary<string,Sprite> dictSuits;



	//InitDeck is called by Prospector when it is ready
	public void InitDeck (string deckXMLText) {
		//This creates an anchor for all the Card GamEObjects in the Hierarchy
		if (GameObject.Find ("_Deck") == null) {
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}

		//initilaize the Dictionary of SuitSprites with necessary sprites
		dictSuits = new Dictionary<string, Sprite>() {
			{"C", suitClub},
			{"D", suitDiamond},
			{"H", suitHeart},
			{"S", suitSpade}
		};

		ReadDeck(deckXMLText);
		MakeCards();
	}

	//ReadDeck parses the XML file passed to it into CardDefinitions
	public void ReadDeck(string deckXMLText) {
		xmlr = new PT_XMLReader(); //Create a new PT_XMLReader
		xmlr.Parse(deckXMLText); //use that PT_XMLREeader to parse DeckXML

		//This prints a test line to show you how xmlr can be used

		string s = "xml[0] decorator[0] ";
		s += "type="+xmlr.xml["xml"][0]["decorator"][0].att ("type");
		s += " x="+xmlr.xml["xml"][0]["decorator"][0].att ("x");
		s += " y="+xmlr.xml["xml"][0]["decorator"][0].att ("y");
		s += " scale="+xmlr.xml["xml"][0]["decorator"][0].att ("scale");
		//print (s);

		//Read decorators for all Cards
		decorators = new List<Decorator>();
		//Grab a PT_XMLHasList of all <decorators> in the XML file
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;

		for (int i=0; i<xDecos.Count; i++) {
			deco = new Decorator(); //Make a new decorator
			//Copy the attributes of the <decorator> to the Decorator
			deco.type = xDecos[i].att("type");
			//Set the bool flip based on whether the text of the attribute is "1" or
			//something else. This is an atypical but perfectly fine use of the == comparison
			//operator. It will return a true or false, which will be assigned to deco.flip
			deco.flip = (xDecos[i].att ("flip") == "1");
			//floats need to be parsed from the attributes strings
			deco.scale = float.Parse (xDecos[i].att ("scale"));
			//Vector3 loc initalizes to [0,0,0], so we just need to modify it
			deco.loc.x = float.Parse (xDecos[i].att ("x"));
			deco.loc.y = float.Parse (xDecos[i].att ("y"));
			deco.loc.z = float.Parse (xDecos[i].att ("z"));
			//add the temp deco to the List decorators
			decorators.Add (deco);
		}

		//Read pip locations for each card number
		cardDefs = new List<CardDefinition>(); //init the list of cards
		//grab a PT_XMLHashList of all the <cards>s in the XML file
		PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

		for (int i=0; i<xCardDefs.Count; i++){
			//For each of the <card>s
			//Create a new CArdDefinition
			CardDefinition cDef = new CardDefinition();
			//Parse the attribute values and add them to cDef
			cDef.rank = int.Parse(xCardDefs[i].att ("rank"));
			//Grab a PT_XMLHashList of all the <pip>s on this <card>
			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {
				for (int j=0; j<xPips.Count; j++) {
					//Iterate through all the <pip>s
					deco = new Decorator();
					//<pip>s on the card> are handled via the DEcorator Class
					deco.type = "pip";
					deco.flip = (xPips[j].att ("flip") == "1");
					deco.loc.x = float.Parse(xPips[j].att ("x"));
					deco.loc.y = float.Parse(xPips[j].att ("y"));
					deco.loc.z = float.Parse(xPips[j].att ("z"));
					if (xPips[j].HasAtt("scale")){
						deco.scale = float.Parse(xPips[j].att ("scale"));
					}
					cDef.pips.Add(deco);
				}
			}
			//FAce cards have a face attribute
			//cDef.face is the base name of the face card Sprite
			//e.g. FAceCard_11 is the base name for the Jack face Sprites
			//the Jack of Clubs is FaceCard_11C, hearts is FaceCard_11H, etc.
			if (xCardDefs[i].HasAtt("face")) {
				cDef.face = xCardDefs[i].att ("face");
			}
			cardDefs.Add(cDef);
		}
	}

	//Get te Proper CardDefinition based on Rank (1 to 14) is ace to King
	public CardDefinition GetCardDefinitionByRank(int rnk) {
		//serach through all of the carddefinitions
		foreach (CardDefinition cd in cardDefs) {
			//if the rank is corect, return thsi definition
			if (cd.rank == rnk) {
				return (cd);
			}
		}
		return(null);
	}

	//Make the Card GameObjects
	public void MakeCards() {
		//cardNames will be the names of the cards to build
		// EAch suit goes from 1-13 (e.g. , C1 to C13 for Clubs)
		cardNames = new List<string>();
		string[] letters = new string[] {"C","D","H","S"};
		foreach (string s in letters) {
			for (int i=0; i<13; i++) {
				cardNames.Add(s+(i+1));
			}
		}

		//Make a List to hold all the cards
		cards = new List<Card>();
		//Several variables that will be reused several times
		Sprite tS = null;
		GameObject tGO = null;
		SpriteRenderer tSR = null;

		//Iterate through all of the card names that were just made
		for (int i=0; i<cardNames.Count; i ++) {
			//Create a new Card GameObject
			GameObject cgo = Instantiate(prefabCard) as GameObject;
			//set the transform.parent o the new card to the anchor
			cgo.transform.parent = deckAnchor;
			Card card = cgo.GetComponent<Card>(); //Get the card component

			//this just stacks the cards so that theyre alll in nice rows
			cgo.transform.localPosition = new Vector3  ((i%13)*3, i/13*4, 0);

			//Assign basic values to the card
			card.name = cardNames[i];
			card.suit = card.name[0].ToString();
			card.rank = int.Parse(card.name.Substring(1));
			if (card.suit == "D" || card.suit == "H") {
				card.colS = "Red";
				card.color = Color.red;
			}

			//pull the carddefinition for this card
			card.def = GetCardDefinitionByRank(card.rank);

			//add decorators
			foreach (Decorator deco in decorators) {
				if (deco.type == "suit") {
					//Instantiate a Sprite GameOBject
					tGO = Instantiate(prefabSprite) as GameObject;
					//get the SpriteRenderer component
					tSR = tGO.GetComponent<SpriteRenderer>();
					//Set the Sprite to the proper suit
					tSR.sprite = dictSuits[card.suit];
				} else { //iif its not a suit its a rank deco
					tGO = Instantiate(prefabSprite) as GameObject;
					tSR = tGO.GetComponent<SpriteRenderer>();
					//get the proper sprite tot show this rank
					tS = rankSprites[card.rank];
					//assign this rank Sprite tot he spriterenderer
					tSR.sprite = tS;
					tSR.color = card.color;
				}
				//Make the deco Sprites render above the Card
				tSR.sortingOrder = 1;
				//Make the decorator Sprite a child of the Card
				tGO.transform.parent = cgo.transform;
				//set the localPosition based on the location from DCKXML
				tGO.transform.localPosition = deco.loc;//Flip the decorator if needed
				if (deco.flip) {
					//an euler roatition of 180 around the z-axis will flip it
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				//Set the scale to keep decos from being too big
				if (deco.scale != 1) {
					tGO.transform.localScale = Vector3.one * deco.scale;
				}
				//name this GameObject so its easy to find
				tGO.name = deco.type;
				//Add this deco GameObject to the List card.decoGOs
				card.decoGOs.Add(tGO);
			}

			//Add Pips
			//For each of the pips in the definition
			foreach (Decorator pip in card.def.pips) {
				//Instantiate a Sprite GameObject 
				tGO = Instantiate (prefabSprite) as GameObject;
				//Set the parent to be the card GameObject
				tGO.transform.parent = cgo.transform;
				//Set the position to that specified in the XML
				tGO.transform.localPosition = pip.loc;
				//flip if necessary
				if(pip.flip) {
					tGO.transform.rotation = Quaternion.Euler (0,0,180);
				}
				//Scale it if necessary (only for the ACe)
				if (pip.scale != 1) {
					tGO.transform.localScale = Vector3.one * pip.scale;
				}
				//Give this GameObject a name
				tGO.name = "pip";
				tSR = tGO.GetComponent<SpriteRenderer>();
				//set the spriet to the proper suit
				tSR.sprite = dictSuits[card.suit];
				//Set the sortingOrder so the pip is rendered above the acrd_front
				tSR.sortingOrder = 1;
				//Add this to the Card's list of pips
				card.pipGOs.Add (tGO);
			}

			//Handle FAce Cards
			if (card.def.face != "") { //if this has a face in card.def
				tGO = Instantiate (prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				//Generate the right name a pass it to GetFace()
				tS = GetFace(card.def.face+card.suit);
				tSR.sprite = tS; //Assign this Sprite to tSR
				tSR.sortingOrder = 1; //set the sorting order
				tGO.transform.parent = card.transform;
				tGO.transform.localPosition = Vector3.zero;
				tGO.name = "face";
			}

			//Add the Card Back
			//The CArd_Back will be able to cover everything else on the CArd
			tGO = Instantiate (prefabSprite) as GameObject;
			tSR = tGO.GetComponent<SpriteRenderer>();
			tSR.sprite = cardBack;
			tGO.transform.parent = card.transform;
			tGO.transform.localPosition = Vector3.zero;
			//this is a higher sortingOrder than anything else
			tSR.sortingOrder = 2;
			tGO.name = "back";
			card.back = tGO;

			//Default to face=up
			card.faceUp = true; //use the property faceUp of Card

			//Add the card to the deck
			cards.Add (card);
		}
	}//closing bracket for MakeCards()

	// Find the proper face card Sprite
	public Sprite GetFace(string faceS) {
		foreach (Sprite tS in faceSprites) {
			//if this sprite has the right name
			if (tS.name == faceS) {
				return(tS);
			}
		}
		//if nothing found, return null
		return(null);
	}
	//Shuffle the cards in deck.cards
	static public void Shuffle (ref List<Card> oCards) {
		//Create a temporary List to hold the new shuffle order
		List<Card> tCards = new List<Card>();
		
		int ndx; //this will hold the index of the card to be moved
		tCards = new List<Card>(); //initalize the temporary List
		//repeat as long as tehre are cards in the original List
		while (oCards.Count > 0) {
			//Pick the index of a random card
			ndx = Random.Range (0,oCards.Count);
			//add that card to the temporary list
			tCards.Add (oCards[ndx]);
			//and remove that card from the original list
			oCards.RemoveAt(ndx);
		}
		//replace the oriignal List with thte temp list
		oCards = tCards;
		//because oCArds is  a reference variable, the original that was passed in is changed as well
	}
}



