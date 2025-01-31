﻿using System.Linq;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.SimpleStation14.Announcements.Systems;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.StationEvents.Events;

[UsedImplicitly]
public sealed class BreakerFlipRule : StationEventSystem<BreakerFlipRuleComponent>
{
    [Dependency] private readonly ApcSystem _apcSystem = default!;
    [Dependency] private readonly AnnouncerSystem _announcerSystem = default!;

    protected override void Added(EntityUid uid, BreakerFlipRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        var str = Loc.GetString("station-event-breaker-flip-announcement", ("data", Loc.GetString(Loc.GetString($"random-sentience-event-data-{RobustRandom.Next(1, 6)}"))));

        _announcerSystem.SendAnnouncement(args.RuleId, Filter.Broadcast(), str, colorOverride: Color.Gold);
    }

    protected override void Started(EntityUid uid, BreakerFlipRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation))
            return;

        var stationApcs = new List<ApcComponent>();
        foreach (var (apc, transform) in EntityQuery<ApcComponent, TransformComponent>())
        {
            if (apc.MainBreakerEnabled && CompOrNull<StationMemberComponent>(transform.GridUid)?.Station == chosenStation)
            {
                stationApcs.Add(apc);
            }
        }

        var toDisable = Math.Min(RobustRandom.Next(3, 7), stationApcs.Count);
        if (toDisable == 0)
            return;

        RobustRandom.Shuffle(stationApcs);

        for (var i = 0; i < toDisable; i++)
        {
            _apcSystem.ApcToggleBreaker(stationApcs[i].Owner, stationApcs[i]);
        }
    }
}
