using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card : MonoBehaviour {
	public string suit; //Suit of the card
	public int rank;
	public Color color = Color.black; //color to tint pips
	public string colS = "Black"; //or "Red" name of the color
	//This list holds lal of the Decorator GameObjects
	public List<GameObject> decoGOs = new List<GameObject>();
	//This list holds lal of the Pip GameObjects
	public List<GameObject> pipGOs = new List<GameObject>();

	public GameObject back; //The GamEObject of the back of the card

	public CardDefinition def; //Parsed from DeckXML.xml

	//list of the SpriteRenderer components of this GameObject and its children
	public SpriteRenderer[] spriteRenderers;

	void Start() {
		SetSortOrder(0); //ensures that the card starts properly depth sorted
	}

	public bool faceUp {
		get {
			return (!back.activeSelf);
		}
		set {
			back.SetActive (!value);
		}
	}

	//If spriteRendersrs is not yet defined, this function deifines it
	public void PopulateSpriteRenderers() {
		//if spriteRenderers is null or empty
		if (spriteRenderers == null || spriteRenderers.Length == 0) {
			//Get SpriteRendere Components of this gameObject and its children
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}
	}
	//Sets the sortingLayerName on all SpriteRenderer Componenents
	public void SetSortingLayerName(string tSLN) {
		PopulateSpriteRenderers();

		foreach (SpriteRenderer tSR in spriteRenderers) {
			tSR.sortingLayerName = tSLN;
		}
	}

	//Sets the sortingOrder of all SpriteREnderer Components
	public void SetSortOrder(int sOrd) {
		PopulateSpriteRenderers();

		//the white bg of the card is on the bottom (sOrd)
		//on top of that are all the pips, decorators, face, etc. (sOrd+1)
		//The back is on the top so that wehn visible, it covers the rest (sord+2)
		//Iterate through all the sprite renderesr as tSR
		foreach (SpriteRenderer tSR in spriteRenderers) {
			if (tSR.gameObject == this.gameObject) {
				//If the gameobject is this.gameObject, its the bg
				tSR.sortingOrder = sOrd; //set its order to sOrd
				continue; //and continue to the next iteration of the loop
			}

			//Each of the children of this GameObject are named switch based ont he names
			switch (tSR.gameObject.name) {
			case "back": //if the name is "bacl"
				tSR.sortingOrder = sOrd+2;
				//^Set it to highest layer to cover everything else
				break;
			case "face": //if the name is "face"
			default: //or antyhing else
				tSR.sortingOrder = sOrd+1;
				//^Set it tot he middle layer above the card bg
				break;
			}
		}
	}

	//Virtual methods can be overridden by subclass methods witht he same name
	virtual public void OnMouseUpAsButton() {
		print (name);
	}
}

[System.Serializable]
public class Decorator {
	//this class stores information about each decorator or pip from DeckXML
	public string type; //for card pips, type = "pip"
	public Vector3 loc; //the location of the Sprite on the card
	public bool flip = false; //whether to flip the Sprite vertically
	public float scale =1f; //scale of sprite
}

[System.Serializable]
public class CardDefinition {
	//this class stores infromation for each rank of card
	public string face; //Sprite to use for each face card
	public int rank; //the rank of this card (1-13)
	public List<Decorator> pips = new List<Decorator>(); //pips used 
	//because decorators from the XML are used the same way on every card
	//in the deck, pips only stores information about the pips on the numbered card
}

