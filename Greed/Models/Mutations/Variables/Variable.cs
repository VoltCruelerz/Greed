namespace Greed.Models.Mutations.Variables
{
    public class Variable
    {
        public string Name { get; set; }
        public object? Value { get; set; }

        public Variable(string name, object? value)
        {
            Name = name;
            Value = value;
        }
    }
}
