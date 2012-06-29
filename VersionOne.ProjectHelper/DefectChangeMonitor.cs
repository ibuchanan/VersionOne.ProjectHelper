using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VersionOne.SDK.APIClient;

namespace VersionOne.ProjectHelper
{
    public class DefectChangeMonitor
    {
        private readonly IServices _services;
        private readonly IMetaModel _meta;
        private DateTime? _mostRecentChangeDateTime;

        public IList<Project> MonitoredProjects { get; set; }

        public DefectChangeMonitor(IServices services, IMetaModel meta)
        {
            _services = services;
            _meta = meta;
            MonitoredProjects = new List<Project>();
        }

        public bool HasChangedSince(DateTime? lastCheckedDateTime)
        {
            if (!lastCheckedDateTime.HasValue)
            {
                return true;
            }
            var defectType = _meta.GetAssetType("Defect");
            var query = new Query(defectType);
            var changeAttribute = defectType.GetAttributeDefinition("ChangeDateUTC");
            var changeTerm = new FilterTerm(changeAttribute);
            changeTerm.Greater(lastCheckedDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
            var projectAttribute = defectType.GetAttributeDefinition("Scope");
            var projectTerm = new FilterTerm(projectAttribute);
            projectTerm.Equal(MonitoredProjects.Select(project => project.ScopeId).Cast<Object>().ToArray());
            query.Filter = new AndFilterTerm(changeTerm, projectTerm);
            var result = _services.Retrieve(query);
            return result.TotalAvaliable > 0;
        }

        public DateTime? GetMostRecentChangeDateTime()
        {
            var defectType = _meta.GetAssetType("Defect");
            var query = new Query(defectType);
            var projectAttribute = defectType.GetAttributeDefinition("Scope");
            var projectTerm = new FilterTerm(projectAttribute);
            projectTerm.Equal(MonitoredProjects.Select(project => project.ScopeId).Cast<Object>().ToArray());

            var changeAttribute = defectType.GetAttributeDefinition("ChangeDateUTC");
            query.Selection.Add(changeAttribute);
            if (_mostRecentChangeDateTime.HasValue)
            {
                var changeTerm = new FilterTerm(changeAttribute);
                changeTerm.Greater(_mostRecentChangeDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture));
                query.Filter = new AndFilterTerm(changeTerm, projectTerm);
            } else
            {
                query.Filter = projectTerm;
            }

            var result = _services.Retrieve(query);

            if (result.TotalAvaliable > 0)
            {
                foreach (var asset in result.Assets)
                {
                    var projectChangeDateTime = DB.DateTime(asset.GetAttribute(changeAttribute).Value);
                    if ((!_mostRecentChangeDateTime.HasValue) || (projectChangeDateTime > _mostRecentChangeDateTime))
                    {
                        _mostRecentChangeDateTime = projectChangeDateTime;
                    }
                }
            }
            return _mostRecentChangeDateTime;
        }
    }
}
