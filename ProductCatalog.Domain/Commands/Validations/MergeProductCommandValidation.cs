namespace ProductCatalog.Domain.Commands.Validations
{
    public class MergeProductCommandValidation : ProductValidation<MergeProductCommand>
    {
        public MergeProductCommandValidation()
        {
            ValidateId();
        }
    }
}
