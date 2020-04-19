namespace Sitecore.Commerce.Plugin.Categories.Pipelines.Blocks
{
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Categories.Policies;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [PipelineDisplayName("Sitecore.Commerce.Plugin.Categories.Pipelines.Blocks.GetAssociateCategoryToSellableItemViewBlock")]
    public class GetAssociateCategoryToSellableItemViewBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        protected CommerceCommander Commander { get; set; }

        public GetAssociateCategoryToSellableItemViewBlock(CommerceCommander commander)
            : base(null)
        {

            this.Commander = commander;

        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            var request = context.CommerceContext.GetObject<EntityViewArgument>();
            if (request == null)
            {
                return arg;
            }

            var action = request.ForAction;

            if (string.Equals(action, KnownParentCategoriesViewActionsPolicy.AssociateCategoryToSellableItem, System.StringComparison.Ordinal))
            {
                var searchPolicy = context.CommerceContext.Environment.GetComponent<PolicySetsComponent>().GetPolicy<Plugin.Search.SearchScopePolicy>();

                var policies = new List<Policy>
                                {
                                    new Policy(new List<Model> { new Model { Name = "Category" } })
                                    { PolicyId = "EntityType" }, searchPolicy
                                };

                arg.Properties.Add(new ViewProperty(policies)
                {
                    Name = "CategoryToAssociate",
                    IsReadOnly = false,
                    IsRequired = true,
                    IsHidden = false,
                    OriginalType = string.Empty.GetType().FullName,
                    UiType = "Autocomplete"
                });

            }

            return await Task.FromResult(arg);
        }
    }
}