using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AudioManager : Node {
  private static byte _sfxChannelCount = 8;

  public static AudioManager Instance { get; private set; }

  private List<AudioStreamPlayer2D> _sfxPlayers = new List<AudioStreamPlayer2D>();

  private void SfxFinished(Node player) {
    if (_sfxPlayers.Count > _sfxChannelCount)
      player.Free();
  }

  private AudioStreamPlayer2D GetPlayer(bool IsPriority) {
    var player = _sfxPlayers.Where(x => !x.IsPlaying()).FirstOrDefault();

    if (player == null && IsPriority) {
      player = new AudioStreamPlayer2D {
        Bus = "Sfx"
      };
      player.Finished += () => SfxFinished(player);
      AddChild(player);

    }
    return player;
  }

  /// <summary>
  /// Plays a stream to the Sfx Bus and ignores spatial sound.
  /// </summary>
  /// <param name="stream">Stream to play</param>
  /// <param name="PitchScale">Optionally adjust pitch</param>
  /// <param name="IsPriority">Force the sound to play, even if the channels are busy. </param>
  public static void PlaySFX(AudioStream stream, float PitchScale = 1.0f, bool IsPriority = false) {
    var player = Instance.GetPlayer(IsPriority);
    if (player != null) {
      player.PitchScale = PitchScale;
      player.Stream = stream;
      player.Play();
    }
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() {
    //Add 16 inital players for Sfx
    for (int i = 0; i < _sfxChannelCount; i++) {
      var player = new AudioStreamPlayer2D {
        Bus = "Sfx"
      };
      AddChild(player);
      _sfxPlayers.Add(player);
    }

    Instance = this;
  }
}
