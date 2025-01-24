using Godot;
using System.Collections.Generic;
using System.Linq;

public static class RandomUtil {
  public static RandomNumberGenerator Rng = new RandomNumberGenerator();

  /// <summary>
  /// Pick random from given list
  /// </summary>
  public static T FromList<T>(IEnumerable<T> list) {
    var i = Rng.RandiRange(0, list.Count() - 1);
    return list.Skip(i).First();
  }

  /// <summary>
  /// Generate random vec2 inside circle
  /// </summary>
  /// <param name="radius">Radius of point</param>
  public static Vector2 RandomPointInCircle(float radius) {
    RandomNumberGenerator rng = new RandomNumberGenerator();
    var theta = rng.Randf() * 2 * Mathf.Pi;
    return new Vector2(
            Mathf.Cos(theta),
            Mathf.Sin(theta))
        * Mathf.Sqrt(Rng.Randf())
        * radius;
  }

  public static void Shuffle<T>(this IList<T> list) {
    int n = list.Count;
    while (n > 1) {
      n--;
      int k = Rng.RandiRange(0, n);
      T value = list[k];
      list[k] = list[n];
      list[n] = value;
    }
  }
}
