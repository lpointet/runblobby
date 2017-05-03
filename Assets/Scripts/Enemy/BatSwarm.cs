using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class BatSwarm : MonoBehaviour {
	
	ParticleSystem myParticle;
	List<ParticleSystem.Particle> listParticle = new List<ParticleSystem.Particle> (); // La liste qui contient l'ensemble des particules qui vont être trigger

	void OnEnable () {
		myParticle = GetComponent<ParticleSystem> ();
	}

	void OnParticleTrigger () {
		// Place dans "listParticle" la liste des particules qui ont rencontré le héros
		int numEnter = myParticle.GetTriggerParticles (ParticleSystemTriggerEventType.Enter, listParticle);

		// Itération dans cette liste de particules
		for (int i = 0; i < numEnter; i++)
		{
			ParticleSystem.Particle p = listParticle [i];
			p.remainingLifetime = 0; // Disparition de la particule
			if (LevelManager.levelManager.GetEnemyEnCours () != null)
				LevelManager.player.Hurt (1, LevelManager.levelManager.GetEnemyEnCours ().sharp, false, LevelManager.levelManager.GetEnemyEnCours ());
			listParticle [i] = p;
		}

		// Replace cette liste de particules dans "listParticle"
		myParticle.SetTriggerParticles (ParticleSystemTriggerEventType.Enter, listParticle);
	}
}