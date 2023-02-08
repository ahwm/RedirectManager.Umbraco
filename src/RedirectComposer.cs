using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace RedirectManager
{
    public class RedirectUserComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Sections().InsertBefore<PackagesSection, RedirectSection>();
            builder.Services.AddTransient<RedirectService>();
            builder.Services.Configure<UmbracoPipelineOptions>(options => {
                options.AddFilter(new UmbracoPipelineFilter(
                    "RedirectManager",
                    _ => { },
                    applicationBuilder => {
                        applicationBuilder.UseMiddleware<RedirectsMiddleware>();
                    },
                    _ => { }
                ));
            });
        }
    }

    public class RedirectComposer : ComponentComposer<RedirectComponent>
    { }

    public class RedirectComponent : IComponent
    {
        private readonly ICoreScopeProvider _coreScopeProvider;
        private readonly IMigrationPlanExecutor _migrationPlanExecutor;
        private readonly IKeyValueService _keyValueService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IRuntimeState _runtimeState;
        private readonly RedirectService _redirectService;

        public RedirectComponent(IMigrationPlanExecutor migrationPlanExecutor, ICoreScopeProvider coreScopeProvider, IKeyValueService keyValueService, ILoggerFactory loggerFactory, IRuntimeState runtimeState, RedirectService redirectService)
        {
            _migrationPlanExecutor = migrationPlanExecutor;
            _coreScopeProvider = coreScopeProvider;
            _keyValueService = keyValueService;
            _loggerFactory = loggerFactory;
            _redirectService = redirectService;
            _runtimeState = runtimeState;
        }

        public void Initialize()
        {
            if (_runtimeState.Level < RuntimeLevel.Run)
            {
                return;
            }
            //UmbracoApplicationBase.ApplicationInit += UmbracoApplicationBase_ApplicationInit;
            // Create a migration plan for a specific project/feature
            // We can then track that latest migration state/step for this project/feature
            var migrationPlan = new MigrationPlan("Redirects");

            // This is the steps we need to take
            // Each step in the migration adds a unique value
            migrationPlan.From(string.Empty).To<AddRedirectsTable>("redirects-db");

            // Go and upgrade our site (Will check if it needs to do the work or not)
            // Based on the current/latest step
            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(_migrationPlanExecutor, _coreScopeProvider, _keyValueService);
        }

        public void Terminate() { }
    }

    public class AddRedirectsTable : MigrationBase
    {
        public AddRedirectsTable(IMigrationContext context) : base(context)
        { }

        protected override void Migrate()
        {
            Logger.LogDebug("Running migration {MigrationStep}", "AddRedirectsTable");

            // Lots of methods available in the MigrationBase class - discover with this.
            if (TableExists("Redirect") == false)
            {
                Create.Table<RedirectSchema>().Do();
            }
            else
            {
                Logger.LogDebug("The database table {DbTable} already exists, skipping", "Redirect");
            }
        }

        [TableName("Redirect")]
        [PrimaryKey("Id", AutoIncrement = true)]
        [ExplicitColumns]
        public class RedirectSchema
        {
            [PrimaryKeyColumn(AutoIncrement = true)]
            [Column("Id")]
            public int Id { get; set; }

            [Column("OldUrl")]
            public string OldUrl { get; set; }

            [Column("NewUrl")]
            public string NewUrl { get; set; }
        }
    }

    [TableName("Redirect")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class Redirect
    {
        [PrimaryKeyColumn(AutoIncrement = true)]
        [Column("Id")]
        public int Id { get; set; }

        [Column("OldUrl")]
        public string OldUrl { get; set; }

        [Column("NewUrl")]
        public string NewUrl { get; set; }
    }
}