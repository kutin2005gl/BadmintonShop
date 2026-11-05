using BadmintonShop.Core.Services;

internal static partial class Program
{
    private static void CacheChecks()
    {
        Test("C01 snapshots / exact expiry / refresh / owner and revision isolation / failures",()=>
        {
            var now=DateTimeOffset.UtcNow;int calls=0;
            var cache=new BoNhoPhien<int,int>(TimeSpan.FromMinutes(10),()=>now,2);
            int Read(int key=1,string scope="user1:r1",bool refresh=false)=>cache.Lay(key,scope,()=>++calls,refresh);
            Eq(1,Read());for(int i=0;i<10;i++)Eq(1,Read());Eq(1,calls);
            now=now.AddMinutes(10);Eq(2,Read());Eq(3,Read(refresh:true));
            Eq(4,Read(scope:"user2:r1"));Eq(5,Read(scope:"user2:r2"));
            Eq(6,Read(2,"user2:r2"));Eq(7,Read(3,"user2:r2"));Eq(8,Read(1,"user2:r2"));
            cache.Xoa();
            Reject(()=>cache.Lay(1,"user2:r2",()=>throw new InvalidOperationException("load failed")));
            Eq(9,Read(scope:"user2:r2"));
        });
    }
}
