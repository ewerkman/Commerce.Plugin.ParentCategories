namespace Sitecore.Commerce.Plugin.Categories.Pipelines.Blocks
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using System;
    using System.Linq;
    using System.Threading.Tasks;


    public class GetParentCategoriesViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CatalogCommander commander;

        public GetParentCategoriesViewBlock(CatalogCommander commander)
          : base(null)
        {
            this.commander = commander;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{Name}: The argument cannot be null.");

            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var request = context.CommerceContext.GetObject<EntityViewArgument>();

            if (string.IsNullOrEmpty(request?.ViewName) ||
                !request.ViewName.Equals(viewsPolicy.Master, StringComparison.OrdinalIgnoreCase) &&
                !request.ViewName.Equals(viewsPolicy.Details, StringComparison.OrdinalIgnoreCase) &&
                !request.ViewName.Equals(viewsPolicy.Variant, StringComparison.OrdinalIgnoreCase) &&
                !request.ViewName.Equals(viewsPolicy.AddSellableItemToBundle, StringComparison.OrdinalIgnoreCase) &&
                !request.ViewName.Equals(viewsPolicy.EditSellableItemBundleQuantity,
                    StringComparison.OrdinalIgnoreCase) &&
                !request.ViewName.Equals(viewsPolicy.ConnectSellableItem, StringComparison.OrdinalIgnoreCase))
            {
                return await Task.FromResult(arg).ConfigureAwait(false);
            }

            if (!(request.Entity is SellableItem) || !string.IsNullOrEmpty(request.ForAction))
            {
                return await Task.FromResult(arg).ConfigureAwait(false);
            }

            var sellableItem = request.Entity as SellableItem;

            if (sellableItem != null)
            {

                var parentCategoriesView = new EntityView
                {
                    EntityId = request.Entity?.Id ?? string.Empty,
                    EntityVersion = arg.EntityVersion,
                    Name = "ParentCategories",
                    UiHint = "Table"
                };

                arg.ChildViews.Add(parentCategoriesView);

                if (sellableItem.ParentCategoryList != null)
                {
                    var categorySitecoreIds = sellableItem.ParentCategoryList.Split('|');
                    var categoryEntityIds = await commander.Pipeline<IFindEntityIdsInSitecoreIdListPipeline>().Run(categorySitecoreIds.ToList(), context);
                    foreach (var categoryEntityId in categoryEntityIds)
                    {
                        var category = await commander.GetEntity<Category>(context.CommerceContext, categoryEntityId);

                        var parentCategoryView = new EntityView
                        {
                            Name = "Master",
                            EntityId = category.Id,
                            EntityVersion = category.EntityVersion,
                            ItemId = category.Id,
                            VersionedItemId = $"{category.Id}-{category.EntityVersion}"
                        };

                        parentCategoryView.Properties.Add(new ViewProperty
                        {
                            Name = "Id",
                            RawValue = category.Id,
                            IsReadOnly = true,
                            UiType = "ItemLink"
                        });

                        parentCategoryView.Properties.Add(new ViewProperty
                        {
                            Name = "DisplayName",
                            RawValue = category.DisplayName,
                            IsReadOnly = true
                        });

                        parentCategoryView.Properties.Add(new ViewProperty
                        {
                            Name = "Description",
                            RawValue = category.Description,
                            IsReadOnly = true
                        });
                        parentCategoriesView.ChildViews.Add(parentCategoryView);
                    }
                }
            }

            return arg;
        }

    }
}