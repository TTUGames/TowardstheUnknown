using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The settings of an edition (Assets/Data/Editions), read through <see cref="Edition.Profile"/> by the systems that differ
/// between the editions without being turned off as a whole. Only settings: a system that needs more is an
/// <see cref="EditionOnly"/> or a pair of the <see cref="EditionSkin"/>
/// </summary>
[CreateAssetMenu(fileName = "EditionProfile", menuName = "TTU/Edition Profile")]
public class EditionProfile : ScriptableObject
{
    [Tooltip("Set on the quality level while the edition is shown; none keeps the quality level's own")]
    public RenderPipelineAsset renderPipeline;
}
