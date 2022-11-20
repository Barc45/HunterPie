using HunterPie.Core.Domain.Interfaces;
using HunterPie.Core.Extensions;
using HunterPie.Core.Game.Entity.Party;
using HunterPie.Core.Native.IPC.Models.Common;
using HunterPie.Integrations.Datasources.MonsterHunterWorld.Definitions;

namespace HunterPie.Integrations.Datasources.MonsterHunterWorld.Entity.Party;

public class MHWParty : IParty, IEventDispatcher
{
    public const int MaximumSize = 4;

    private readonly Dictionary<long, MHWPartyMember> _partyMembers = new(MaximumSize);

    public int Size
    {
        get
        {
            lock (_partyMembers)
                return _partyMembers.Count;
        }
    }

    public int MaxSize => MaximumSize;

    public List<IPartyMember> Members
    {
        get
        {
            lock (_partyMembers)
                return _partyMembers.Values.ToList<IPartyMember>();
        }
    }

    public event EventHandler<IPartyMember>? OnMemberJoin;
    public event EventHandler<IPartyMember>? OnMemberLeave;

    public void Update(long memberAddress, MHWPartyMemberData data)
    {
        lock (_partyMembers)
        {
            if (!_partyMembers.TryGetValue(memberAddress, out MHWPartyMember? member))
            {
                Add(memberAddress, data);
                return;
            }

            member.Update(data);
        }
    }

    public void Update(long memberAddress, EntityDamageData data)
    {
        lock (_partyMembers)
        {
            if (!_partyMembers.TryGetValue(memberAddress, out MHWPartyMember? member))
                return;

            member.Update(data);
        }
    }

    private void Add(long memberAddress, MHWPartyMemberData data)
    {
        var member = new MHWPartyMember();
        member.Update(data);

        _partyMembers.Add(memberAddress, member);

        this.Dispatch(OnMemberJoin, member);
    }

    public void Remove(long memberAddress)
    {
        MHWPartyMember? member;
        lock (_partyMembers)
            if (!_partyMembers.Remove(memberAddress, out member))
                return;

        this.Dispatch(OnMemberLeave, member);
    }
}
