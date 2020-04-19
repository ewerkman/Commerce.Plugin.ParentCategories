// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.Commerce.Plugin.Categories
{
    using System.Reflection;

    using Microsoft.Extensions.DependencyInjection;

    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Categories.Pipelines.Blocks;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IGetEntityViewPipeline>(p => p
                    .Add<GetParentCategoriesViewBlock>().After<GetSellableItemsViewBlock>()
                    .Add<GetAssociateCategoryToSellableItemViewBlock>().After<GetParentCategoriesViewBlock>()
                    )
                .ConfigurePipeline<IPopulateEntityViewActionsPipeline>( p => p
                    .Add<PopulateParentCategoriesViewActionsBlock>()
                    )
                .ConfigurePipeline<IDoActionPipeline>( p => p
                    .Add<DoActionAssociateCategoryToSellableItemBlock>().Before<IFormatEntityViewPipeline>()
                    .Add<DoActionDisassociateCategoryFromSellableItemBlock>().Before<IFormatEntityViewPipeline>()
                    )
                );                

            services.RegisterAllCommands(assembly);
        }
    }
}