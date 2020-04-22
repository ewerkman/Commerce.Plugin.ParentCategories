namespace Sitecore.Commerce.Plugin.Categories.Pipelines.Blocks
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Categories.Policies;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    [PipelineDisplayName("Change to <Project>Constants.Pipelines.Blocks.<Block Name>")]
    public class PopulateParentCategoriesViewActionsBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public PopulateParentCategoriesViewActionsBlock(CommerceCommander commander)
            : base(null)
        {

            this.Commander = commander;

        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            if (string.IsNullOrEmpty(arg?.Name) ||
                !(arg.Name.Equals("ParentCategories", StringComparison.OrdinalIgnoreCase)))
            {
                return arg;
            }

            var actionPolicy = arg.GetPolicy<ActionsPolicy>();

            var entity = context.CommerceContext.GetObjects<CommerceEntity>().FirstOrDefault(e => e.Id.Equals(arg.EntityId, StringComparison.Ordinal))
                ?? context.CommerceContext.GetObjects<EntityViewArgument>().FirstOrDefault()?.Entity;

            if (entity == null || !(entity is SellableItem))
            {
                return arg;
            }

            var sellableItem = entity as SellableItem;

            actionPolicy.Actions.Add(
                new EntityActionView
                {
                    Name = KnownParentCategoriesViewActionsPolicy.AssociateCategoryToSellableItem,
                    DisplayName = "Associate Category",
                    Description = "Associate to a Category",
                    IsEnabled = true,
                    EntityView = context.GetPolicy<KnownCatalogViewsPolicy>().Details,
                    RequiresConfirmation = false,
                    Icon = "link"
                });

            actionPolicy.Actions.Add(
                new EntityActionView
                {
                    Name = KnownParentCategoriesViewActionsPolicy.DisassociateCategoryFromSellableItem,
                    DisplayName = "Disassociate Category",
                    Description = "Disassociate category from sellabe item",
                    IsEnabled = !string.IsNullOrEmpty(sellableItem.ParentCategoryList),
                    EntityView = string.Empty,
                    ConfirmationMessageTerm  = string.Empty,
                    RequiresConfirmation = true,
                    Icon = "link_broken"
                });

            return await Task.FromResult(arg);
        }
    }
}