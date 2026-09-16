using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
/// Plays a sound at this entity's coordinates.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class PlaySoundEntityEffectSystem : EntityEffectSystem<TransformComponent, PlaySoundEffect>
{
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<PlaySoundEffect> args)
    {
        if (_net.IsClient)
            return;

        // Coordinates are used so the sound still plays if the entity is deleted by another effect.
        _audio.PlayPvs(args.Effect.Sound, entity.Comp.Coordinates, args.Effect.Params);
    }
}

/// <inheritdoc cref="EntityEffect"/>
public sealed partial class PlaySoundEffect : EntityEffectBase<PlaySoundEffect>
{
    /// <summary>
    /// The sound that gets played.
    /// </summary>
    [DataField(required: true)]
    public SoundSpecifier Sound = default!;

    /// <summary>
    /// Playback parameters, such as volume.
    /// </summary>
    [DataField]
    public AudioParams? Params;
}
