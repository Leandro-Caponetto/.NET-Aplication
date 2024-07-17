using System.Collections.Immutable;

namespace Presentation.Site.Auth
{
    public class Helpers
    {
        public static ImmutableArray<PermissionInfo> Permissions = new[]
        {
            //new PermissionInfo(nameof(FindDessertsRequest), requiresRestaurant: true),
            new PermissionInfo(""),
        }.ToImmutableArray();
    }

    public class PermissionInfo
    {
        public PermissionInfo(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
