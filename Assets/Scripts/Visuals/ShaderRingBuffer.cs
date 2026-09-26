using UnityEngine;

/// <summary>
/// The last few events of a kind (a hit wave, a drop, a splash) passed to the shaders as global vector arrays, one or
/// several in step, with their count: the newest overwrites the oldest once full. Keep the capacity in step with the
/// array size declared in the shader
/// </summary>
public class ShaderRingBuffer
{
    private readonly int[] arrayIds;
    private readonly Vector4[][] arrays;
    private readonly int countId;
    private int next;
    private int count;

    public ShaderRingBuffer(int capacity, string countName, params string[] arrayNames)
    {
        countId = Shader.PropertyToID(countName);
        arrayIds = new int[arrayNames.Length];
        arrays = new Vector4[arrayNames.Length][];
        for (int i = 0; i < arrayNames.Length; i++)
        {
            arrayIds[i] = Shader.PropertyToID(arrayNames[i]);
            arrays[i] = new Vector4[capacity];
        }
    }

    /// <summary>Adds an event, one value per array; <see cref="Pass"/> sends it</summary>
    public void Add(Vector4 value, Vector4 second = default)
    {
        arrays[0][next] = value;
        if (arrays.Length > 1) arrays[1][next] = second;
        next = (next + 1) % arrays[0].Length;
        count = Mathf.Min(count + 1, arrays[0].Length);
    }

    public void Pass()
    {
        for (int i = 0; i < arrays.Length; i++) Shader.SetGlobalVectorArray(arrayIds[i], arrays[i]);
        Shader.SetGlobalInt(countId, count);
    }

    public void Clear()
    {
        next = 0;
        count = 0;
        Shader.SetGlobalInt(countId, 0);
    }
}
