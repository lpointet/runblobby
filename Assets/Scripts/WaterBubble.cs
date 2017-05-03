using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterBubble : MonoBehaviour {

	public static WaterBubble waterBubble;

	private ParticleSystem myParticle;
	private ParticleSystem.ShapeModule particleShape;
	List<ParticleSystem.Particle> listParticle = new List<ParticleSystem.Particle> (); // La liste qui contient l'ensemble des particules qui vont être trigger

	[Header("Breathing")]
	[SerializeField] private float normalMaxBreath;
	[SerializeField] private float hardMaxBreath;
	[SerializeField] private float hellMaxBreath;
	private float maxBreath; // La quantité d'air maximum que le héros supporte
	private float currentBreath = 0;
	[SerializeField] private float normalDecayBreath;
	[SerializeField] private float hardDecayBreath;
	[SerializeField] private float hellDecayBreath;
	private float decayBreath; // La quantité d'air perdue par seconde par le héros
	private float timeBeforeHurt; // Permet de compter 1sec entre chaque blessure de noyade au héros
	private bool preventNextBreath = false; // Empêche la prochaine bulle de fournir de l'oxygène
	[SerializeField] private Color preventColor;

	[Header("Drowning")]
	[SerializeField] private SpriteRenderer drowningCircle; // Cercle représentant l'étouffement du héros
	private Transform drowningTransform;
	private float drowningScale; // Nombre d'unités du rayon du cercle (taille du sprite)
	[SerializeField] private float minDrowningSize; // Taille minimale du cercle
	[SerializeField] private float maxDrowningSize; // Taille maximale du cercle
	[SerializeField] private Color maxBreathColor;
	private float currentDrowningSize;
	private float breathAmplitude = 0.25f;
	private float initBreathFrequence = 1.5f;
	private float breathFrequence; // Accélère quand le joueur est proche de la noyade
	private float breathTime = 0; // On ne peut utiliser TimeManager.time car cette valeur augmente en permanence

	void Awake () {
		if (waterBubble == null)
			waterBubble = GameObject.FindObjectOfType<WaterBubble> ();
		
		myParticle = GetComponent<ParticleSystem> ();
		particleShape = myParticle.shape;

		drowningTransform = drowningCircle.transform;
		drowningScale = drowningCircle.bounds.extents.x;
	}

	void Start () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				gameObject.SetActive (false);
				return;
			// Hard
			case 1:
				maxBreath = hardMaxBreath;
				decayBreath = hardDecayBreath;
				currentBreath = maxBreath * 0.75f;
				break;
			// Hell
			case 2:
				maxBreath = hellMaxBreath;
				decayBreath = hellDecayBreath;
				currentBreath = maxBreath * 0.5f;
				break;
			}
		} else
			return;

		// Taille par rapport à l'écran
		particleShape.radius = Camera.main.orthographicSize * Camera.main.aspect + Camera.main.orthographicSize;
		// On le place au milieu
		transform.position = new Vector2 (CameraManager.cameraManager.xOffset, CameraManager.cameraDownPosition);
		// On le décale en considérant l'angle des bulles 45°
		transform.Translate (Vector2.right * Camera.main.orthographicSize);

		// Taille du cercle
		ResizeDrowning ();

		drowningCircle.color = maxBreathColor;
	}

	void Update () {
		if (LevelManager.player.IsDead () || TimeManager.paused)
			return;

		// Contrôle du souffle
		ReduceBreath (decayBreath * TimeManager.deltaTime);

		// Contrôle du cercle de noyade
		DrownFollowPlayer ();
		ResizeDrowning ();

		// Blessure aux héros
		PlayerDrowning ();
	}

	void OnParticleTrigger () {
		// Place dans "listParticle" la liste des particules qui ont rencontré le héros
		int numEnter = myParticle.GetTriggerParticles (ParticleSystemTriggerEventType.Enter, listParticle);

		// Itération dans cette liste de particules
		for (int i = 0; i < numEnter; i++)
		{
			ParticleSystem.Particle p = listParticle [i];
			p.remainingLifetime = 0; // Disparition de la particule
			// TODO sound
			if (preventNextBreath)
				preventNextBreath = false;
			else
				GainBreath (10.0f * p.startSize); // 10 fois la durée de vie par rapport à la taille
			listParticle [i] = p;
		}

		// Replace cette liste de particules dans "listParticle"
		myParticle.SetTriggerParticles (ParticleSystemTriggerEventType.Enter, listParticle);
	}

	public void ReduceBreath (float value) {
		currentBreath = Mathf.Max (currentBreath - value, 0);
	}

	public void GainBreath (float value) {
		drowningCircle.color = maxBreathColor;
		currentBreath = Mathf.Min (currentBreath + value, maxBreath);
	}

	public void PreventNextBreath () {
		drowningCircle.color = preventColor;
		preventNextBreath = true;
	}

	private void PlayerDrowning () {
		if (currentBreath <= 0 && TimeManager.time > timeBeforeHurt) {
			// On blesse le héros
			LevelManager.player.Hurt (1, 0, true);
			timeBeforeHurt = TimeManager.time + 1.0f; // Chaque seconde
		}
	}

	private void ResizeDrowning () {
		// Valeur "réelle" que doit avoir le cercle
		currentDrowningSize = Mathf.Lerp (minDrowningSize, maxDrowningSize, currentBreath / maxBreath);

		// Pour rendre l'effet plus réelle, on fait un effet de respiration
		BreathDrowning ();

		drowningTransform.localScale = Vector3.one * currentDrowningSize / drowningScale;
	}

	private void DrownFollowPlayer () {
		drowningTransform.position = LevelManager.player.transform.position;
	}


	private void BreathDrowning () {
		// Fonction de l'oxygène disponible : moins y en a plus ça va vite
		breathFrequence = Mathf.Lerp (3 * initBreathFrequence, initBreathFrequence, currentBreath * currentBreath / maxBreath / maxBreath);

		// Permet d'ajouter plus ou moins de "temps";
		breathTime += TimeManager.deltaTime * breathFrequence;

		// Permet d'avoir toujours un nombre positif à ajouter, ce qui fait que le cercle est "juste" quand il est au minimum
		currentDrowningSize += breathAmplitude * (1.0f + Mathf.Sin (breathTime));
	}
}
