using System.Windows;
using System.Windows.Markup;

namespace PresentationFramework.System.Windows;

public class DoubleValueTrigger : DataTrigger
{
    protected new double _value;

    public LogicalOp Relation { get; set; } = LogicalOp.Equals;


    [DependsOn("Binding")]
    [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)] // Not localizable by-default
    public new double Value
    {
        get
        {
            // Verify Context Access
            VerifyAccess();

            return _value;
        }
        set
        {
            // Verify Context Access
            VerifyAccess();

            if (IsSealed)
            {
                throw new InvalidOperationException(SR.Format(SR.CannotChangeAfterSealed, nameof(IntValueTrigger)));
            }

            if (value is MarkupExtension)
            {
                throw new ArgumentException(SR.Format(SR.ConditionValueOfMarkupExtensionNotSupported,
                    value.GetType().Name));
            }

            if (value is Expression)
            {
                throw new ArgumentException(SR.ConditionValueOfExpressionNotSupported);
            }

            _value = value;
        }
    }

    protected internal override void Seal()
    {
        if (IsSealed)
        {
            return;
        }

        // Process the _setters collection: Copy values into PropertyValueList and seal the Setter objects.
        ProcessSettersCollection(_setters);

        // Freeze the value for the trigger
        StyleHelper.SealIfSealable(_value);

        // Build conditions array from collection
        TriggerConditions =
        [
            new DoubleValueTriggerCondition(
                _binding,
                Relation,
                _value)
        ];

        // Set Condition for all data triggers
        for (int i = 0; i < PropertyValues.Count; i++)
        {
            PropertyValue propertyValue = PropertyValues[i];

            propertyValue.Conditions = TriggerConditions;
            switch (propertyValue.ValueType)
            {
                case PropertyValueType.Trigger:
                    propertyValue.ValueType = PropertyValueType.DataTrigger;
                    break;
                case PropertyValueType.PropertyTriggerResource:
                    propertyValue.ValueType = PropertyValueType.DataTriggerResource;
                    break;
                default:
                    throw new InvalidOperationException(SR.Format(SR.UnexpectedValueTypeForDataTrigger,
                        propertyValue.ValueType));
            }

            // Put back modified struct
            PropertyValues[i] = propertyValue;
        }
    }
}
