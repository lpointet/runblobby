﻿using UnityEngine;

public class TouchManager : MonoBehaviour {

	public static TouchManager current;

	private enum FingerSide { left, right }

	private float screenMidWidth; // Le milieu de l'écran
	private float screenMaxHeight; // Le haut de l'écran, en tenant compte de la hauteur maximale de saut

	public int rightTouchId { get; private set; }
	public Vector2 rightTouchPosition { get; private set; }
	public int leftTouchId { get; private set; }
	public Vector2 leftTouchPosition { get; private set; }

	private RaycastHit2D hit; // Sert à vérifier qu'on ne touche pas une zone "cliquable"
	//public Vector2 clickableTouchPosition  { get; private set; }

	void Awake () {
		if (current == null) {
			current = this;
			DontDestroyOnLoad (current);
		} else
			DestroyObject (this);
	}

	void Start() {
		int screenHeightForbidden; // La hauteur du bouton "Pause" | Nombre de pixels de l'écran du menu
		screenHeightForbidden = 64 * Screen.height / 588; // Par défaut, dans l'éditeur, le canvas fait 588 pixels de haut, et le bouton 64
		screenMidWidth = Screen.width / 2;
		screenMaxHeight = Screen.height - screenHeightForbidden;

		rightTouchId = -1;
		leftTouchId = -1;
	}

	void Update () {
		// On désactive ces touches si le joueur est mort ou si le jeu est en pause
		if (TimeManager.paused || LevelManager.player.IsDead ())
			return;
		
		// Si l'appareil est touch et multi-touch
		if (Input.touchSupported && Input.multiTouchEnabled) {
			int nbTouches = Input.touchCount;

			if (nbTouches > 0) {
				for (int i = 0; i < nbTouches; i++) {
					Touch touch = Input.GetTouch (i);
					TouchPhase phase = touch.phase;

					switch (phase) {
					// Lorsqu'un nouveau contact est effectué, on regarde s'il est à droite ou à gauche
					case TouchPhase.Began:
						// Gauche
						if (touch.position.x < screenMidWidth) {
							leftTouchId = touch.fingerId;
							leftTouchPosition = touch.position;
							Mediator.current.Publish<TouchLeft> (new TouchLeft () {
								leftTouchPosition = leftTouchPosition,
								leftId = leftTouchId
							});
						}
						// Droite
						else {
							// On ne dépasse pas la hauteur du "menu" en haut (touche "Pause")
							if (touch.position.y < screenMaxHeight) {
								rightTouchId = touch.fingerId;
								rightTouchPosition = touch.position;
								Mediator.current.Publish<TouchRight> (new TouchRight () {
									rightTouchPosition = rightTouchPosition,
									rightId = rightTouchId
								});
							}
						}
						break;
					// Tant qu'on est en contact (qu'on bouge ou non), on met à jour la position du contact courant gauche et/ou droite
					case TouchPhase.Moved:
					case TouchPhase.Stationary:
						// Gauche
						if (IsExistingFinger(touch.fingerId, FingerSide.left)) {
							leftTouchPosition = touch.position;
						}
						// Droite
						else if (IsExistingFinger(touch.fingerId, FingerSide.right)) {
							rightTouchPosition = touch.position;
						}
						break;
					// Lorsqu'on enlève un contact, on supprime les propriétés gauche ou droite de celui-ci
					case TouchPhase.Ended:
						// Gauche
						if (IsExistingFinger(touch.fingerId, FingerSide.left)) {
							leftTouchId = -1;
						}
						// Droite
						else if (IsExistingFinger(touch.fingerId, FingerSide.right)) {
							rightTouchId = -1;
						}
						Mediator.current.Publish<EndTouch> (new EndTouch () { fingerId = touch.fingerId });
						break;
					}
						
				}
			}
		}
		// Si l'appareil n'est pas touch ou multi-touch
		else {
			if (Input.GetMouseButton (0)) {
				// La première fois qu'on appuie, on regarde de quel côté on est pour "enregistrer" le bouton
				if (Input.GetMouseButtonDown (0)) {
					// On intercepte si on est sur une zone "cliquable" et que le joueur peut cliquer
					// On n'intercepte pas autrement pour ne pas perdre des zones pour sauter/tirer
					LayerMask layerHit = LayerMask.NameToLayer("TouchLayer"); // TODO garder cette layer ? elle ne fonctionne pas pour l'instant

					hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero, Mathf.Infinity, layerHit.value);

					// Lorsque l'on clique sur une zone que l'on peut casser
					if (LevelManager.player.canBreakByClick > 0 && hit.collider != null) {
						Mediator.current.Publish<TouchClickable> (new TouchClickable () {
							touchPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition),
							objectId = hit.collider.gameObject.GetInstanceID ()
						});
						return;
					}

					// Lorsque l'on clique sur les anges pendant que LastWish est actif
					if (LevelManager.player.canCollectAngel && hit.collider != null) {
						Mediator.current.Publish<TouchClickable> (new TouchClickable () {
							touchPosition = Camera.main.ScreenToWorldPoint (Input.mousePosition),
							objectId = hit.collider.gameObject.GetInstanceID ()
						});
						return;
					}

					// Lorsque l'on clique sur le joueur
					// On prend en compte la distance du clic par rapport au joueur, vu qu'il peut y avoir de nombreux obstacles sur le chemin du joueur pour détecter un collider
					if (LevelManager.player.CanShieldAttract() && Vector2.Distance(LevelManager.player.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < 1.5f) {
						Mediator.current.Publish<TouchPlayer> (new TouchPlayer () {	});
						return;
					}

					// Gauche
					if (Input.mousePosition.x < screenMidWidth) {
						leftTouchId = 0;
						leftTouchPosition = Input.mousePosition;
						Mediator.current.Publish<TouchLeft> (new TouchLeft () {
							leftTouchPosition = leftTouchPosition,
							leftId = leftTouchId
						});
					}
					// Droite
					else {
						// On ne dépasse pas la hauteur du "menu" en haut (touche "Pause")
						if (Input.mousePosition.y < screenMaxHeight) {
							rightTouchId = 1;
							rightTouchPosition = Input.mousePosition;
							Mediator.current.Publish<TouchRight> (new TouchRight () {
								rightTouchPosition = rightTouchPosition,
								rightId = rightTouchId
							});
						}
					}
				}
				// Si ce n'est pas la première fois qu'on appuie, on met à jour le bouton actuellement actif
				else {
					// Gauche
					if (leftTouchId != -1) {
						leftTouchPosition = Input.mousePosition;
					}
					// Droite
					else if (rightTouchId != -1) {
						rightTouchPosition = Input.mousePosition;
					}
				}
			} else if (Input.GetMouseButtonUp (0)) {
				// Gauche
				if (leftTouchId != -1) {
					Mediator.current.Publish<EndTouch> (new EndTouch () { fingerId = leftTouchId });
					leftTouchId = -1;
				}
				// Droite
				else if (rightTouchId != -1) {
					Mediator.current.Publish<EndTouch> (new EndTouch () { fingerId = rightTouchId });
					rightTouchId = -1;
				}
			}
		}
	}

	private bool IsExistingFinger(int currentId, FingerSide side) {
		switch (side) {
		case FingerSide.left:
			if (currentId == leftTouchId)
				return true;
			else
				return false;
		case FingerSide.right:
			if (currentId == rightTouchId)
				return true;
			else
				return false;
		}
		return false;
	}
}