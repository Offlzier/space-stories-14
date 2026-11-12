using Content.Shared.Damage.Systems;
using Content.Shared.Emag.Systems;
using Content.Shared.Forensics.Components;
using Content.Shared.Hands;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Trigger.Components;
using Content.Shared.Trigger.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Containers;

namespace Content.Server._Stories.BioCode;

public sealed partial class StoriesBioCodeSystem : EntitySystem
{
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private EmagSystem _emag = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private TriggerSystem _trigger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BioCodeComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<BioCodeComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<BioCodeComponent, GotEquippedHandEvent>(OnHandEquipped);
        SubscribeLocalEvent<BioCodeComponent, GotUnequippedHandEvent>(OnHandUnequipped);
        SubscribeLocalEvent<BioCodeComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<BioCodeComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEquipped(Entity<BioCodeComponent> ent, ref GotEquippedEvent args)
    {
        CheckWearer(ent, args.EquipTarget);
    }

    private void OnUnequipped(Entity<BioCodeComponent> ent, ref GotUnequippedEvent args)
    {
        _trigger.StopTimerTrigger(ent.Owner);
    }

    private void OnHandEquipped(Entity<BioCodeComponent> ent, ref GotEquippedHandEvent args)
    {
        if (ent.Comp.ItemBioCode)
            CheckWearer(ent, args.User);
    }

    private void OnHandUnequipped(Entity<BioCodeComponent> ent, ref GotUnequippedHandEvent args)
    {
        _trigger.StopTimerTrigger(ent.Owner);
    }

    private void CheckWearer(Entity<BioCodeComponent> ent, EntityUid user)
    {
        var comp = ent.Comp;

        if (!TryComp<DnaComponent>(user, out var dna) || dna.DNA == null)
            return;

        if (comp.AutoRecordDNA && comp.RecordedDNA == null)
        {
            RecordDna(ent, user, dna.DNA);
            return;
        }

        if (comp.RecordedDNA == null || comp.RecordedDNA == dna.DNA)
            return;

        if (comp.HardDefense)
            EnsureComp<UnremoveableComponent>(ent);

        var message = comp.HardDefense ? "stories-biocode-hard-defense" : "stories-biocode-dna-not-match";
        _popup.PopupEntity(Loc.GetString(message), user, user, PopupType.LargeCaution);

        var timer = EnsureComp<TimerTriggerComponent>(ent);
        timer.Delay = TimeSpan.FromSeconds(comp.Delay);
        timer.BeepInterval = TimeSpan.FromSeconds(comp.BeepInterval);
        timer.InitialBeepDelay = comp.InitialBeepDelay is { } initial ? TimeSpan.FromSeconds(initial) : null;
        timer.BeepSound = comp.BeepSound;

        _trigger.ActivateTimerTrigger(ent.Owner, user);
    }

    private void RecordDna(Entity<BioCodeComponent> ent, EntityUid user, string dna)
    {
        _damageable.TryChangeDamage(user, ent.Comp.DamagePerRecord, true, true);
        ent.Comp.RecordedDNA = dna;
        _popup.PopupEntity(Loc.GetString("stories-biocode-successful-recording"), ent);
    }

    private void OnGetVerbs(Entity<BioCodeComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        var comp = ent.Comp;
        var user = args.User;

        if (comp.RecordedDNA != null
            && TryComp<DnaComponent>(user, out var userDna)
            && comp.RecordedDNA != userDna.DNA)
            return;

        if (!_container.TryGetContainingContainer(ent.Owner, out var container) || container.Owner != user)
            return;

        if (comp.RecordedDNA != null && comp.AllowOverwriting)
        {
            args.Verbs.Add(new Verb
            {
                Text = Loc.GetString("stories-biocode-delete-dna-verb"),
                Act = () =>
                {
                    comp.RecordedDNA = null;
                    _popup.PopupEntity(Loc.GetString("stories-biocode-successful-deletion"), ent);
                },
            });
        }

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("stories-biocode-record-dna-verb"),
            Act = () =>
            {
                if (!TryComp<DnaComponent>(user, out var dna) || dna.DNA == null)
                {
                    _popup.PopupEntity(Loc.GetString("stories-biocode-dna-not-found"), ent);
                    return;
                }

                RecordDna(ent, user, dna.DNA);
            },
        });
    }

    private void OnEmagged(Entity<BioCodeComponent> ent, ref GotEmaggedEvent args)
    {
        if (!_emag.CompareFlag(args.Type, EmagType.Interaction))
            return;

        _popup.PopupEntity(Loc.GetString("stories-biocode-defense-emagged"), ent);
        RemComp<BioCodeComponent>(ent);
        args.Handled = true;
    }
}
