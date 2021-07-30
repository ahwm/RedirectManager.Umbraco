using NPoco;
using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Web;
using Umbraco.Web.Sections;

namespace RedirectManager
{
    public class RedirectUserComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Sections().InsertBefore<PackagesSection, RedirectSection>();
            composition.Register<RedirectService>();
        }
    }

    public class RedirectComposer : ComponentComposer<RedirectComponent>
    { }

    [RuntimeLevel(MinLevel = Umbraco.Core.RuntimeLevel.Run)]
    public class RedirectComponent : IComponent
    {
        private readonly IScopeProvider _scopeProvider;
        private readonly IMigrationBuilder _migrationBuilder;
        private readonly IKeyValueService _keyValueService;
        private readonly ILogger _logger;
        private readonly RedirectService _redirectService;

        public RedirectComponent(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger, RedirectService redirectService)
        {
            _scopeProvider = scopeProvider;
            _migrationBuilder = migrationBuilder;
            _keyValueService = keyValueService;
            _logger = logger;
            _redirectService = redirectService;
        }

        public void Initialize()
        {
            UmbracoApplicationBase.ApplicationInit += UmbracoApplicationBase_ApplicationInit;
            // Create a migration plan for a specific project/feature
            // We can then track that latest migration state/step for this project/feature
            var migrationPlan = new MigrationPlan("Redirects");

            // This is the steps we need to take
            // Each step in the migration adds a unique value
            migrationPlan.From(string.Empty).To<AddRedirectsTable>("redirects-db");

            // Go and upgrade our site (Will check if it needs to do the work or not)
            // Based on the current/latest step
            var upgrader = new Upgrader(migrationPlan);
            upgrader.Execute(_scopeProvider, _migrationBuilder, _keyValueService, _logger);
        }

        private void UmbracoApplicationBase_ApplicationInit(object sender, EventArgs e)
        {
            if (!(sender is HttpApplication app)) return;

            app.BeginRequest += App_BeginRequest;
        }

        private void App_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                HttpContext ctx = HttpContext.Current;
                if (ctx != null)
                {
                    string url = _redirectService.GetRedirectByUrl(ctx.Request.RawUrl);
                    if (!String.IsNullOrWhiteSpace(url))
                    {
                        ctx.Response.RedirectPermanent(url, true);
                    }
                }
            }
            catch { }
        }

        public void Terminate()
        {
            UmbracoApplicationBase.ApplicationInit -= UmbracoApplicationBase_ApplicationInit;
        }
    }

    public class AddRedirectsTable : MigrationBase
    {
        public AddRedirectsTable(IMigrationContext context) : base(context)
        { }

        public override void Migrate()
        {
            Logger.Debug<AddRedirectsTable>("Running migration {MigrationStep}", "AddAkismetCommentsTable");

            // Lots of methods available in the MigrationBase class - discover with this.
            if (TableExists("AkismetComments") == false)
            {
                Create.Table<RedirectSchema>().Do();
            }
            else
            {
                Logger.Debug<AddRedirectsTable>("The database table {DbTable} already exists, skipping", "AkismetComments");
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