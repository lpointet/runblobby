using UnityEngine;

public class CoinDrop {
	private CoinPickup coin;
	private int poissonValue;
	private float esperance = 0;
	private int coinValue;

	public CoinDrop(CoinPickup coin, int poissonValue, float esperance) {
		this.coin = coin;
		this.poissonValue = poissonValue;
		this.esperance = esperance;
		coinValue = coin.pointToAdd;
	}

	public void SetEsperance(float value) {
		esperance = value;
	}

	public string GetCoinName() {
		return coin.name;
	}

	public int GetCoinValue() {
		return coinValue;
	}

	public int PourcentPoisson () {
		if (poissonValue > 0)
			return Mathf.Max(0, Mathf.RoundToInt (_StaticFunction.LoiPoisson (poissonValue, esperance, true) * 100));
		else // Si on est sur la dernière valeur du tableau, et donc que poissonValue = 0, la limite haute est toujours 100
			return 100;
	}
}