namespace Sitecore.Commerce.Plugin.Categories.Policies
{
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using System.Threading.Tasks;

    public class KnownParentCategoriesViewActionsPolicy : Policy
    {
        public const string AssociateCategoryToSellableItem = "AssociateCategoryToSellableItem";
        public const string DisassociateCategoryFromSellableItem = "DisassociateCategoryFromSellableItem";
    }
}