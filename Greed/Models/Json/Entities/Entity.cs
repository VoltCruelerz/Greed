namespace Greed.Models.Json.Entities
{
    public class Entity : JsonSource
    {
        public Entity(string path) : base(path)
        {
            // Intentionally left empty.
        }

        public static Entity Create(string path)
        {
            if (path.EndsWith(".entity_manifest"))
            {
                return new EntityManifest(path);
            }
            return new Entity(path);
        }

        public override JsonSource Clone()
        {
            return new Entity(SourcePath);
        }
    }
}
