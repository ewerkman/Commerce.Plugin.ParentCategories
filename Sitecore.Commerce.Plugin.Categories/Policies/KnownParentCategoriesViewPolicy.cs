namespace Sitecore.Commerce.Plugin.Categories.Policies
{
    using Sitecore.Commerce.Core;

    public class KnownParentCategoriesViewPolicy : Policy
    {
        public string CategoryToAssociate { get; set; } = "CategoryToAssociate";
    }
}
