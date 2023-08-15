namespace Greed.Models.JsonSource.Entities
{
    public class Entity : Source
    {
        public Entity(string path) : base(path)
        {
            // Intentionally left empty.
        }

        public static Entity Create(string path)
        {
            if (path.EndsWith("._entity_manifest"))
            {
                return new EntityManifest(path);
            }
            return new Entity(path);
        }
    }
}
