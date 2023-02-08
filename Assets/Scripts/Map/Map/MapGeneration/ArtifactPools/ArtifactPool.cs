using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactPool", menuName = "TTU/ArtifactPool")]
public class ArtifactPool : ScriptableObject
{
    public List<ArtifactPoolElement> poolElements;

	private float GetTotalWeight() {
		float totalWeight = 0;
		foreach(ArtifactPoolElement poolElement in poolElements) {
			totalWeight += poolElement.weight;
		}
		return totalWeight;
	}

	public List<Artifact> GetRandomElement() {
		List<Artifact> artifacts = new List<Artifact>();
		float pickedWeight = Random.Range(0, GetTotalWeight());
		int index = 0;

		pickedWeight -= poolElements[index].weight;
		while (pickedWeight > 0) {
			pickedWeight -= poolElements[++index].weight;
		}

		ArtifactPoolElement element = poolElements[index];
		foreach(string artifactName in element.artifactNames) {
			artifacts.Add((Artifact)System.Activator.CreateInstance(System.Type.GetType(artifactName)));
		}

		return artifacts;
	}
}
