using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The run's randomness: every gameplay draw comes from one of its streams, derived from the run's seed, so that a run
/// replays the same from its seed and the player's inputs (map, loot, damage rolls, the AI's choices). Each domain has its
/// own stream: a damage roll more doesn't shift the loot that follows. The cosmetic draws (grass, wind, blood, fidgets) stay
/// on <see cref="Random"/> and never touch these. <see cref="Map"/> seeds it as it wakes, before generating; a suspended run
/// (<see cref="RunSave"/>) restores the streams where they were
/// </summary>
public static class GameRandom
{
    /// <summary>
    /// The run's seed, never 0
    /// </summary>
    public static int Seed { get; private set; }

    /// <summary>
    /// The seed was chosen (by the player at launch, a test, the generator's seed field) rather than drawn: the run is
    /// replayable at will, so it doesn't count for the scores (<see cref="global::Map.IsRanked"/>)
    /// </summary>
    public static bool Chosen { get; private set; }

    /// <summary>
    /// The map's layout and the rooms and layouts picked
    /// </summary>
    public static RandomStream Map { get; private set; }

    /// <summary>
    /// The artifacts of the chests and rewards
    /// </summary>
    public static RandomStream Loot { get; private set; }

    /// <summary>
    /// The damage rolls
    /// </summary>
    public static RandomStream Combat { get; private set; }

    /// <summary>
    /// The enemies' choices (Drareg's first patterns)
    /// </summary>
    public static RandomStream AI { get; private set; }

    /// <summary>
    /// The rest of the run (the player's name)
    /// </summary>
    public static RandomStream Run { get; private set; }

    // A seed asked for the next run (a test, a seed chosen at launch); 0 draws one
    private static int requested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        requested = 0;
        Begin(0);
    }

    static GameRandom() => Begin(0);

    /// <summary>
    /// The next run (<see cref="Begin"/> with no seed of its own) uses this seed
    /// </summary>
    public static void Request(int seed) => requested = seed;

    /// <summary>
    /// Starts the run's streams from <paramref name="seed"/>; 0 takes the requested seed, or draws a new one
    /// </summary>
    /// <param name="chosen">The seed given was chosen (<see cref="Chosen"/>); a requested one always is</param>
    public static void Begin(int seed, bool chosen = false) {
        if (seed == 0 && requested != 0) {
            seed = requested;
            chosen = true;
        }
        requested = 0;
        if (seed == 0) {
            seed = new System.Random().Next(1, int.MaxValue);
            chosen = false;
        }
        Seed = seed;
        Chosen = chosen;
        Map = new RandomStream(Derive(1));
        Loot = new RandomStream(Derive(2));
        Combat = new RandomStream(Derive(3));
        AI = new RandomStream(Derive(4));
        Run = new RandomStream(Derive(5));
    }

    /// <summary>
    /// A seed of its own for one thing of the run (a room at a position), stable whatever was drawn before
    /// </summary>
    public static int Derive(int key, int salt = 0) {
        ulong mixed = RandomStream.Mix((ulong)(uint)Seed << 32 ^ (uint)key ^ (ulong)(uint)salt << 16);
        return (int)(mixed >> 33);
    }

    private static IEnumerable<RandomStream> Streams => new[] { Map, Loot, Combat, AI, Run };

    /// <summary>
    /// Where each stream is, to save and restore (<see cref="RunSave"/>)
    /// </summary>
    public static List<long> Save() {
        var states = new List<long>();
        foreach (RandomStream stream in Streams) states.Add((long)stream.State);
        return states;
    }

    /// <summary>
    /// Puts the streams back where <see cref="Save"/> found them; a list of another length is ignored
    /// </summary>
    public static void Restore(List<long> states) {
        var streams = new List<RandomStream>(Streams);
        if (states == null || states.Count != streams.Count) return;
        for (int i = 0; i < streams.Count; i++) streams[i].State = (ulong)states[i];
    }
}

/// <summary>
/// A seeded random sequence whose state is one number, saved and restored at will (SplitMix64)
/// </summary>
public class RandomStream
{
    public ulong State { get; set; }

    public RandomStream(int seed) => State = (ulong)(uint)seed * 0x9E3779B97F4A7C15UL + 0x632BE59BD9B4E019UL;

    public static ulong Mix(ulong z) {
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    private ulong Next() {
        State += 0x9E3779B97F4A7C15UL;
        return Mix(State);
    }

    /// <summary>
    /// A float in [0, 1)
    /// </summary>
    public float Value => (Next() >> 40) * (1f / (1 << 24));

    /// <summary>
    /// An int in [min, max), min if the range is empty
    /// </summary>
    public int Range(int min, int max) => max <= min ? min : min + (int)(Next() % (ulong)(max - min));

    /// <summary>
    /// A float in [min, max)
    /// </summary>
    public float Range(float min, float max) => min + (max - min) * Value;

    /// <summary>
    /// An element of the list
    /// </summary>
    public T Pick<T>(IReadOnlyList<T> list) => list[Range(0, list.Count)];

    /// <summary>
    /// Shuffles the list in place (Fisher-Yates)
    /// </summary>
    public void Shuffle<T>(IList<T> list) {
        for (int i = list.Count - 1; i > 0; --i) {
            int j = Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
