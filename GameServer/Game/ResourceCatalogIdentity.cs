using Newtonsoft.Json.Linq;

namespace MollyPS.GameServer.Game;

internal static class ResourceCatalogIdentity
{
    public static void Validate(JObject catalog, string directory)
    {
        var manifest = Path.Combine(directory, "ProfileManifest.json");
        if (File.Exists(manifest))
        {
            var expected = JObject.Parse(File.ReadAllText(manifest)).Value<string>("clientFingerprint");
            if (string.IsNullOrEmpty(expected) || catalog.Value<string>("clientFingerprint") != expected)
                throw new InvalidDataException("Feature catalog fingerprint does not match the selected resource profile.");
        }
        else if (catalog.Value<string>("clientVersion") != "1.5.3")
            throw new InvalidDataException("Versioned feature catalogs require a matching resource profile manifest.");
    }
}
