namespace JsonGen
{
    public class Filter
    {
        public enum Operators
        {
            // Equals
            Eq,
            // Greater than
            G,
            // Less than
            L,
            // Greater than or equal to
            GE,
            // Less than or equal to
            LE,
            // In a set values. Value should be an array
            In,
            // Between. Value should be an array of 2 elements: Min, Max 
            Bw
        }
        public string FieldName { get; set; }

        public Operators Operator { get; set; } = Operators.Eq;

        public dynamic Value { get; set; }
    }
}
