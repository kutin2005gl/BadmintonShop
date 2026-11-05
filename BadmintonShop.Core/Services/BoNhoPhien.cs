namespace BadmintonShop.Core.Services;

/// <summary>Bounded per-owner snapshots; clock injection permits expiry checks without sleeping.</summary>
public sealed class BoNhoPhien<TKey,TValue> where TKey:notnull
{
    private readonly Dictionary<TKey,(TValue Value,DateTimeOffset Loaded)> entries=[];
    private readonly Func<DateTimeOffset> clock;
    private readonly TimeSpan lifetime;
    private readonly int capacity;
    private string? scope;
    public BoNhoPhien(TimeSpan lifetime,Func<DateTimeOffset>? clock=null,int capacity=8)
    {
        if(lifetime<=TimeSpan.Zero || capacity<1)throw new ArgumentOutOfRangeException(nameof(lifetime));
        this.lifetime=lifetime;this.clock=clock??(()=>DateTimeOffset.UtcNow);this.capacity=capacity;
    }
    public static TimeSpan ThoiHanMacDinh =>
        int.TryParse(Environment.GetEnvironmentVariable("BadmintonShop__CacheMinutes"),out var minutes) && minutes is >0 and <=60
            ?TimeSpan.FromMinutes(minutes):TimeSpan.FromMinutes(10);
    public TValue Lay(TKey key,string currentScope,Func<TValue> load,bool refresh=false)
    {
        lock(entries)
        {
            if(scope!=currentScope) { entries.Clear();scope=currentScope; }
            var now=clock();
            if(!refresh && entries.TryGetValue(key,out var entry) && now-entry.Loaded<lifetime)return entry.Value;
            // Failed reads never replace the last successful snapshot or its timestamp.
            var value=load();
            if(entries.Count>=capacity && !entries.ContainsKey(key))entries.Remove(entries.MinBy(x=>x.Value.Loaded).Key);
            entries[key]=(value,now);return value;
        }
    }
    public void Xoa() { lock(entries)entries.Clear(); }
}
