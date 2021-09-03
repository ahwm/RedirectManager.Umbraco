using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;

namespace RedirectManager
{
    public class RedirectService
    {
        private readonly IScopeProvider scopeProvider;

        public RedirectService(IScopeProvider provider)
        {
            scopeProvider = provider;
        }

        internal string GetPrimaryDomain()
        {
            string appData = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Plugins/Redirects");
            if (!File.Exists(Path.Combine(appData, "primaryDomain.json")))
                return "";
            Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Path.Combine(appData, "primaryDomain.json")));
            if (config == null)
                return "";

            return config["Domain"];
        }

        internal void IncrementCD()
        {
            ClientDependency.Core.Config.ClientDependencySettings.Instance.Version = ClientDependency.Core.Config.ClientDependencySettings.Instance.Version + 1;
        }

        internal void SetPrimaryDomain(string domain)
        {
            string appData = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Plugins/Redirects");
            var config = new Dictionary<string, string> { { "Domain", domain } };
            File.WriteAllText(Path.Combine(appData, "primaryDomain.json"), JsonConvert.SerializeObject(config));
        }

        internal int GetRedirectPageCount()
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount<Redirect>(x => x.Id).From("Redirect");

                return (int)Math.Ceiling(scope.Database.ExecuteScalar<int>(sql) / 20m);
            }
        }

        internal IEnumerable<Redirect> ListRedirects(int page = 1)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("Redirect").OrderBy<Redirect>(x => x.OldUrl);

                return scope.Database.Query<Redirect>(sql).Skip((page - 1) * 20).Take(20);
            }
        }

        internal int GetFilterRedirectPageCount(string searchTerm)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount<Redirect>(x => x.Id).From("Redirect").Where<Redirect>(x => searchTerm != null && searchTerm != "" && (x.NewUrl.Contains(searchTerm) || x.OldUrl.Contains(searchTerm)));

                return (int)Math.Ceiling(scope.Database.ExecuteScalar<int>(sql) / 20m);
            }
        }

        internal IEnumerable<Redirect> FilterRedirects(string searchTerm, int page = 1)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("Redirect").Where<Redirect>(x => searchTerm != null && searchTerm != "" && (x.NewUrl.Contains(searchTerm) || x.OldUrl.Contains(searchTerm))).OrderBy<Redirect>(x => x.OldUrl);

                return scope.Database.Query<Redirect>(sql).Skip((page - 1) * 20).Take(20);
            }
        }

        internal void DeleteRedirect(string id)
        {
            List<int> ids = id.Split(',').Select(x => Convert.ToInt32(x)).ToList();
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Delete().From("Redirect").Where<Redirect>(x => ids.Contains(x.Id));

                scope.Database.Execute(sql);

                scope.Complete();
            }
        }

        internal string GetRedirectByUrl(string url)
        {
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .Select("*").From("Redirect").Where<Redirect>(x => x.OldUrl == url);

                Redirect redirect = scope.Database.Query<Redirect>(sql).FirstOrDefault();
                if (redirect != null)
                    return redirect.NewUrl;

                return "";
            }
        }

        internal void AddRedirect(string oldUrl, string newUrl)
        {
            if (!oldUrl.StartsWith("/"))
                oldUrl = "/" + oldUrl;
            int num = 0;
            using (var scope = scopeProvider.CreateScope(autoComplete: true))
            {
                var sql = scope.SqlContext.Sql()
                    .SelectCount("*").From("Redirect").Where<Redirect>(x => x.OldUrl == oldUrl);
                
                num = scope.Database.ExecuteScalar<int>(sql);
            }
            if (num == 0)
            {
                using (var scope = scopeProvider.CreateScope())
                {
                    var sql = scope.Database.Insert(new Redirect
                    {
                        OldUrl = oldUrl,
                        NewUrl = newUrl
                    });

                    scope.Complete();
                }
            }
        }
    }
}