using UnityEngine;

namespace Editor
{
    public class ConditionalReadOnlyAttribute : PropertyAttribute
    {
        public string conditionField;

        public ConditionalReadOnlyAttribute(string conditionField)
        {
            this.conditionField = conditionField;
        }
    }
}