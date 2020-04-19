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

    [PipelineDisplayName("Sitecore.Commerce.Plugin.Categories.Pipelines.Blocks.DoActionAssociateCategoryToSellableItemBlock")]
    public class DoActionAssociateCategoryToSellableItemBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public DoActionAssociateCategoryToSellableItemBlock(CommerceCommander commander)
            : base(null)
        {
            this.Commander = commander;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null");

            var textStrings = new KnownParentCategoriesViewPolicy();
            if (string.IsNullOrEmpty(arg.Action) || !arg.Action.Equals(KnownParentCategoriesViewActionsPolicy.AssociateCategoryToSellableItem, StringComparison.OrdinalIgnoreCase))
            {
                return arg;
            }

            var entity = context.CommerceContext.GetObject<CommerceEntity>(p => p.Id.Equals(arg.EntityId, StringComparison.OrdinalIgnoreCase));

            var categoryToAssociate = arg.Properties.OfType<ViewProperty>().FirstOrDefault(p => p.Name.Equals(textStrings.CategoryToAssociate, StringComparison.OrdinalIgnoreCase));

            if (entity == null)
            {
                return arg;
            }

            if (!categoryToAssociate.Value.StartsWith(CommerceEntity.IdPrefix<Category>(), StringComparison.InvariantCulture))
            {
                context.Abort(
                    await context.CommerceContext.AddMessage(
                        context.GetPolicy<KnownResultCodes>().ValidationError,
                        "InvalidType",
                        new object[] { categoryToAssociate.Value, CommerceEntity.IdPrefix<Category>() },
                        "{0} is not a Category Item").ConfigureAwait(false),
                    context);
                return null;
            }

            var findCategoryArgs = new FindEntityArgument(typeof(Category), categoryToAssociate.Value);
            var foundCategory = await Commander.Pipeline<IFindEntityPipeline>().RunWithResult(findCategoryArgs, context).ConfigureAwait(false);

            if (foundCategory == null || foundCategory.Value == null)
            {
                context.Abort(
                 await context.CommerceContext.AddMessage(
                     context.GetPolicy<KnownResultCodes>().ValidationError,
                     "EntityNotFound",
                     new object[] { categoryToAssociate.Value },
                     "{0} was not found").ConfigureAwait(false),
                 context);
                return null;
            }

            var catalogName = categoryToAssociate.Value.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries)[2];
            var entityCatalogId = CommerceEntity.IdPrefix<Catalog>() + catalogName;

            if (foundCategory != null && foundCategory.Value != null)
            {
                await Commander.Command<AssociateSellableItemToParentCommand>().Process(
                    context.CommerceContext,
                    entityCatalogId,
                    categoryToAssociate.Value,
                    arg.EntityId).ConfigureAwait(false);
            }


            return arg;

        }
    }
}