using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactPool : ScriptableObject
{
    public List<Pair<string, float>> artifacts;

	private float GetTotalWeight() {
		float totalWeight = 0;
		foreach(Pair<string, float> artifact in artifacts) {
			totalWeight += artifact.second;
		}
		return totalWeight;
	}

	public string GetRandomArtifact() {
		float pickedWeight = Random.Range(0, GetTotalWeight());
		int index = 0;

		pickedWeight -= artifacts[index].second;
		while (pickedWeight > 0) {
			pickedWeight -= artifacts[++index].second;
		}

		return artifacts[index].first;
	}
}
