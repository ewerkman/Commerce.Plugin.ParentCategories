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

    [PipelineDisplayName("Sitecore.Commerce.Plugin.Categories.Pipelines.Blocks.DoActionDisassociateCategoryToSellableItemBlock")]
    public class DoActionDisassociateCategoryFromSellableItemBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public DoActionDisassociateCategoryFromSellableItemBlock(CommerceCommander commander)
            : base(null)
        {
            this.Commander = commander;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null");

            var textStrings = new KnownParentCategoriesViewPolicy();
            if (string.IsNullOrEmpty(arg.Action) || !arg.Action.Equals(KnownParentCategoriesViewActionsPolicy.DisassociateCategoryFromSellableItem, StringComparison.OrdinalIgnoreCase))
            {
                return arg;
            }

            var entity = context.CommerceContext.GetObject<CommerceEntity>(p => p.Id.Equals(arg.EntityId, StringComparison.OrdinalIgnoreCase));

            await Commander.Command<DeleteRelationshipCommand>().Process(context.CommerceContext, arg.ItemId, arg.EntityId, CatalogConstants.CategoryToSellableItem);

            return arg;

        }
    }
}