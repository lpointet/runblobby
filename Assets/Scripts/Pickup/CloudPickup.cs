﻿using UnityEngine;
using System.Collections;

public class CloudPickup : Pickup {

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}
	
	protected override void OnPick() {
		base.OnPick ();

		CloudBlock.nuageActif = true;
		// TODO : jouer un son de nuage qui pop, genre POP. Pas mal.
	}
	
	protected override void DespawnEffect() {
		// TODO : faire diminuer la taille des nuages progressivement ? Les faire clignoter ?
	}
	
	protected override void OnDespawn() {
		CloudBlock.nuageActif = false;
	}
	
	protected override void Update() {
		base.Update();
		
		if( !picked ) {
			return;
		}
	}
}