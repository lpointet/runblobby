using UnityEngine;



public class TouchManager : MonoBehaviour {

	public static TouchManager current;

	private enum FingerSide { left, right }

	private float screenMidWidth; // Le milieu de l'écran
	private float screenMaxHeight; // Le haut de l'écran, en tenant compte de la hauteur maximale de saut
	private float screenHeightForbidden = 64; // TODO : calculer en fonction de la hauteur du bouton "Pause" | Nombre de pixels de l'écran du menu

	public int rightTouchId { get; private set; }
	public Vector2 rightTouchPosition { get; private set; }
	public int leftTouchId { get; private set; }
	public Vector2 leftTouchPosition { get; private set; }

	void Awake () {
		if (current == null) {
			current = this;
			DontDestroyOnLoad (current);
		} else
			DestroyObject (this);
	}

	void Start() {
		// Si l'appareil n'est pas tactile, on affiche un message d'excuse et on ferme le jeu au bout d'un certain temps
		if (!Input.touchSupported) {Debug.Log("touch");
			// TODO
		}
		// Si l'appareil ne supporte pas le multi-touch, on affiche un message expliquant que le jeu ne sera pas aussi agréable à jouer, mais quand même jouable
		if (!Input.multiTouchEnabled) {Debug.Log("multi");
			// TODO
		}

		screenMidWidth = Screen.width / 2;
		screenMaxHeight = Screen.height - screenHeightForbidden;

		rightTouchId = -1;
		leftTouchId = -1;
	}

	void Update () {
		// On désactive ces touches si le joueur est mort ou si le jeu est en pause
		if (TimeManager.paused || LevelManager.GetPlayer().IsDead ())
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
		// TODO problème quand on change de moitié d'écran avec un bouton appuyé (sur le vol permanent c'est bien visible, d'ailleurs comment faire pour le maintenir appuyé ? GetMouseButton fait sauter en chaine)
		else {
			if (Input.GetMouseButtonDown (0)) {
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
					if (Input.mousePosition.y < screenMaxHeight) {
						rightTouchId = 1;
						rightTouchPosition = Input.mousePosition;
						Mediator.current.Publish<TouchRight> (new TouchRight () {
							rightTouchPosition = rightTouchPosition,
							rightId = rightTouchId
						});
					}
				}
			} else if (Input.GetMouseButtonUp (0)) {
				// Gauche
				if (Input.mousePosition.x < screenMidWidth) {
					Mediator.current.Publish<EndTouch> (new EndTouch () { fingerId = leftTouchId });
				}
				// Droite
				else {
					Mediator.current.Publish<EndTouch> (new EndTouch () { fingerId = rightTouchId });
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