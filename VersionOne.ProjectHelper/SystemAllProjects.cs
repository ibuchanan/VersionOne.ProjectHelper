using System;
using System.Collections.Generic;
using System.Globalization;
using VersionOne.SDK.APIClient;

namespace VersionOne.ProjectHelper
{
    public class SystemAllProjects
    {
        private readonly IServices _services;
        private readonly IMetaModel _meta;
        private DateTime? _mostRecentChangeDateTime;
        private IDictionary<string, Project> _allProjects;

        public SystemAllProjects(IServices services, IMetaModel meta)
        {
            _services = services;
            _meta = meta;
        }

        /// <summary>
        /// Constructs an API request on Scopes (Projects) selecting only the 
        /// Name, Parent.ID, and ChangeDateUTC attributes, and sorting on the
        /// Parent.ID attribute. The results are iterated to populate the 
        /// Dictionary that maps a project ID to a textual representation of its
        /// project path.
        /// </summary>
        public void Reload()
        {
            // Build API Request
            _allProjects = new Dictionary<string, Project>();
            var projectType = _meta.GetAssetType("Scope");
            var query = new Query(projectType);
            var nameAttribute = projectType.GetAttributeDefinition("Name");
            var parentAttribute = projectType.GetAttributeDefinition("Parent.ID");
            var changeAttribute = projectType.GetAttributeDefinition("ChangeDateUTC");
            query.Selection.Add(nameAttribute);
            query.Selection.Add(parentAttribute);
            query.Selection.Add(changeAttribute);
            query.OrderBy.MajorSort(parentAttribute, OrderBy.Order.Ascending);
            var result = _services.Retrieve(query);
            var assetQueue = new Queue<Asset>(result.Assets);
            // Iterate results to build mapping
            while (assetQueue.Count > 0)
            {
                var asset = assetQueue.Dequeue();
                Project project;
                if (Oid.Null.Equals(asset.GetAttribute(parentAttribute).Value))
                {
                    project = new Project(asset.Oid.Token)
                                   {
                                       Name = asset.GetAttribute(nameAttribute).Value.ToString(),
                                       ParentScopeId = asset.GetAttribute(parentAttribute).Value.ToString() != "NULL"
                                                           ? asset.GetAttribute(parentAttribute).Value.ToString()
                                                           : null,
                                       PathName = asset.GetAttribute(nameAttribute).Value.ToString()
                                   };
                    _allProjects.Add(project.ScopeId, project);
                    continue;
                }

                // Defer processing a project if the parent has not yet been mapped
                if (!_allProjects.ContainsKey(asset.GetAttribute(parentAttribute).Value.ToString()))
                {
                    assetQueue.Enqueue(asset);
                    continue;
                }

                project = new Project(asset.Oid.Token)
                {
                    Name = asset.GetAttribute(nameAttribute).Value.ToString(),
                    ParentScopeId = asset.GetAttribute(parentAttribute).Value.ToString() != "NULL"
                                        ? asset.GetAttribute(parentAttribute).Value.ToString()
                                        : null,
                    PathName = _allProjects[asset.GetAttribute(parentAttribute).Value.ToString()].PathName + "\\" +
                                        asset.GetAttribute(nameAttribute).Value.ToString()
                };
                _allProjects[project.ParentScopeId].Children.Add(project);
                _allProjects.Add(project.ScopeId, project);

                // Remember the most recent change to VersionOne for checking dirty state
                var projectChangeDateTime = DB.DateTime(asset.GetAttribute(changeAttribute).Value);
                if ((!_mostRecentChangeDateTime.HasValue) || (projectChangeDateTime > _mostRecentChangeDateTime))
                {
                    _mostRecentChangeDateTime = projectChangeDateTime;
                }
            }
        }

        /// <summary>
        /// Constructs an API request on Scopes (Projects) selecting no 
        /// attributes, and filtering on changes since the last reload. If any 
        /// assets are returned in the result, then the cached paths are dirty.
        /// </summary>
        public bool IsDirty()
        {
            if (!_mostRecentChangeDateTime.HasValue)
            {
                return true;
            }
            var projectType = _meta.GetAssetType("Scope");
            var query = new Query(projectType);
            var changeAttribute = projectType.GetAttributeDefinition("ChangeDateUTC");
            var term = new FilterTerm(changeAttribute);
            term.Greater(_mostRecentChangeDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
            query.Filter = term;
            var result = _services.Retrieve(query);
            return result.TotalAvaliable > 0;
        }

        public IDictionary<string, Project> GetProjects()
        {
            if (IsDirty())
            {
                Reload();
            }
            return _allProjects;
        }

    }
}
